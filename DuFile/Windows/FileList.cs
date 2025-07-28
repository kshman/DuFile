using System;

namespace DuFile.Windows;

/// <summary>
/// 파일/디렉터리/드라이브 항목을 표시하고 관리하는 커스텀 리스트 컨트롤입니다.
/// </summary>
public sealed class FileList : ThemeControl
{
	// 파일 리스트 속성
	private readonly FileListProps _props;
	// 컬럼 너비 정보
	private readonly FileListWidths _widths;

	// 업데이트 중 여부
	private bool _updating;

	// 포커스된 인덱스
	private int _focusedIndex = -1;
	// 시프트키로 다중 선택 기준 인덱스
	private int _anchorIndex = -1;

	/// <summary>리스트에 표시되는 항목 컬렉션입니다.</summary>
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public List<FileListItem> Items { get; } = [];

	/// <summary>현재 디렉터리의 전체 경로입니다.</summary>
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public string FullName { get; set; } = string.Empty;

	/// <summary>숫리스트 모드의 컬럼 수입니다.</summary>
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int ColumnCount
	{
		get => _props.Columns;
		set
		{
			_props.SetColumns(value);
			Invalidate();
		}
	}

	/// <summary>스크롤 바 높이입니다.</summary>
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int ScrollBarSize => _props.ScrollBarSize;

	/// <summary>현재 포커스된 항목의 인덱스입니다.</summary>
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int FocusedIndex
	{
		get => _focusedIndex;
		set
		{
			if (_focusedIndex == value)
				return;
			_focusedIndex = value;
			FocusedIndexChanged?.Invoke(this, EventArgs.Empty);
			Invalidate();
		}
	}

	/// <summary>포커스 인덱스 변경 이벤트입니다.</summary>
	[Category("FileList")]
	public event EventHandler? FocusedIndexChanged;

	/// <summary>선택 항목 변경 이벤트입니다.</summary>
	[Category("FileList")]
	public event EventHandler? SelectionChanged;

	/// <summary>포커스 항목 선택 이벤트입니다.</summary>
	[Category("FileList")]
	public event EventHandler? ItemActivate;

	/// <summary>항목 누름 이벤트입니다.</summary>
	[Category("FileList")]
	public event EventHandler<FileListClickEventArgs>? ItemClicked;

	/// <summary>FileList 컨트롤을 초기화합니다.</summary>
	public FileList()
	{
		SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
				 ControlStyles.ResizeRedraw | ControlStyles.UserPaint | ControlStyles.Selectable, true);
		TabStop = true;

		_props = new FileListProps(this, 1);
		_widths = new FileListWidths();
	}

	/// <inheritdoc />
	protected override void OnUpdateTheme(Theme theme)
	{
		Font = new Font(theme.ContentFontFamily, theme.ContentFontSize, FontStyle.Regular, GraphicsUnit.Point);
		BackColor = theme.BackContent;
		ForeColor = theme.Foreground;
		_props.Refresh();
		_widths.UpdateFixed(Font);
		_widths.AdjustName(Width);
	}

	/// <inheritdoc />
	protected override void OnResize(EventArgs e)
	{
		base.OnResize(e);
		_props.Refresh();
		_widths.UpdateFixed(Font);
		_widths.AdjustName(Width);
		Invalidate();
	}

	/// <inheritdoc />
	protected override void OnPaint(PaintEventArgs e)
	{
		if (_updating)
			return;

		base.OnPaint(e);

		var g = e.Graphics;
		var theme = Settings.Instance.Theme;
		g.Clear(theme.BackContent);

		var scrollBarVisible = _props.IsScrollBarVisible;
		var height = Height;
		if (scrollBarVisible)
			height -= _props.ScrollBarSize;

		// TODO: 아이템 루프를 다 돌게 아니고 페이지를 얻어 아이템의 시작과 끝을 얻어 루프를 돌도록 개선할 수 있음
		// 그러면 if문 두개다 필요가 없게됨
		for (var i = 0; i < Items.Count; i++)
		{
			var rect = _props.GetItemRect(i);
			if (rect.Bottom <= 0) continue;
			if (rect.Top >= height) break;
			Items[i].Draw(g, Font, rect, _widths, theme, i == FocusedIndex);
		}

		if (scrollBarVisible)
			DrawScrollBar(g, theme);
	}

	private void DrawScrollBar(Graphics g, Theme theme)
	{
		if (!_props.IsScrollBarDragging)
		{
			// 드래그 중이 아닐 때 스크롤바 인디케이터 범위 계산
			_props.CalcScrollBarIndicator();
		}

		using var brushForground = new SolidBrush(theme.Foreground);
		using var brushContent = new SolidBrush(theme.BackContent);
		using var brushSelection = new SolidBrush(theme.BackSelection);
		using var brushActive = new SolidBrush(theme.BackActive);

		var rect = _props.ScrollBarBound;
		g.FillRectangle(brushContent, rect);

		// 지사자
		g.FillRectangle(_props.IsScrollBarDragging ? brushActive : brushSelection,
			_props.ScrollBarIndicatorRange.Left, rect.Top + 1,
			_props.ScrollBarIndicatorWidth, rect.Height - 1);

		// 좌/우 화살표
		var leftRange = _props.ScrollBarLeftRange;
		var rightRange = _props.ScrollBarRightRange;
		Point[] leftPts =
		[
			new(leftRange.Left + 10, rect.Top + 2),
			new(leftRange.Right - 8, rect.Top + rect.Height / 2),
			new(leftRange.Left + 10, rect.Bottom - 2)
		];
		Point[] rightPts =
		[
			new(rightRange.Right - 10, rect.Top + 2),
			new(rightRange.Left + 8, rect.Top + rect.Height / 2),
			new(rightRange.Right - 10, rect.Bottom - 2)
		];
		g.FillPolygon(brushForground, leftPts);
		g.FillPolygon(brushForground, rightPts);

		// 분리선
		using var pen = new Pen(theme.Border);
		g.DrawLine(pen, 0, rect.Top, Width, rect.Top);
	}

	/// <inheritdoc />
	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		if (_props.IsScrollBarDragging)
		{
			var indWidth = _props.ScrollBarIndicatorWidth;
			var trackWidth = _props.ScrollBarTrackRange.Length;
			var x = e.X - _props.ScrollBarTrackRange.Left - _props.ScrollBarOffset;
			var rx = Math.Max(0, Math.Min(trackWidth - indWidth, x));
			var page = (int)Math.Round((float)rx / (trackWidth - indWidth) * (Math.Max(1, _props.PageCount) - 1));
			_props.ScrollToPage(page);

			// 드래그 중일 때는 지시자의 위치를 업데이트하고 다시 그립니다.
			var left = _props.ScrollBarTrackRange.Left + x;
			var right = left + _props.ScrollBarIndicatorWidth;
			if (left < _props.ScrollBarTrackRange.Left)
				left = _props.ScrollBarTrackRange.Left;
			else if (right > _props.ScrollBarTrackRange.Right)
				left = _props.ScrollBarTrackRange.Right - _props.ScrollBarIndicatorWidth;
			_props.ScrollBarIndicatorRange = new MinMax(left, right);
			Invalidate();
		}
	}

	/// <inheritdoc />
	protected override void OnMouseDown(MouseEventArgs e)
	{
		base.OnMouseDown(e);
		Focus();

		if (_props.IsScrollBarVisible &&
			_props.ScrollBarBound.Contains(e.Location) &&
			e.Button is MouseButtons.Left or MouseButtons.Right)
		{
			if (_props.ScrollBarLeftRange.Contains(e.X))
			{
				ScrollBy(-1);
				return;
			}
			if (_props.ScrollBarLeftRange.Contains(e.X))
			{
				ScrollBy(1);
				return;
			}
			if (_props.ScrollBarIndicatorRange.Contains(e.X))
			{
				_props.IsScrollBarDragging = true;
				_props.ScrollBarOffset = e.X - _props.ScrollBarIndicatorRange.Left;
				return;
			}
			if (e.X < _props.ScrollBarIndicatorRange.Left)
			{
				ScrollBy(-1);
				return;
			}
			if (e.X > _props.ScrollBarIndicatorRange.Right)
			{
				ScrollBy(1);
				return;
			}
		}

		var index = FindIndexAt(e.X, e.Y);
		if (index < 0)
			return;

		switch (e.Button)
		{
			case MouseButtons.Left when (ModifierKeys & Keys.Shift) == Keys.Shift:
				if (_anchorIndex == -1)
					_anchorIndex = FocusedIndex;
				SelectRange(_anchorIndex, index);
				break;
			case MouseButtons.Left when (ModifierKeys & Keys.Control) == Keys.Control:
				_anchorIndex = index;
				SelectIndex(index);
				break;
			case MouseButtons.Left:
			case MouseButtons.Right:
			case MouseButtons.Middle:
				_anchorIndex = index;
				break;
			case MouseButtons.XButton1:     // 다음 버튼 클릭
			case MouseButtons.XButton2:     // 이전 버튼 클릭
			case MouseButtons.None:         // 잉? 이게 들어온다고?
			default:
				break;
		}

		FocusedIndex = index;
		Invalidate();

		ItemClicked?.Invoke(this, new FileListClickEventArgs(index, e.Button, e.Location));
	}

	/// <inheritdoc />
	protected override void OnMouseUp(MouseEventArgs e)
	{
		base.OnMouseUp(e);
		if (_props.IsScrollBarDragging)
		{
			_props.IsScrollBarDragging = false;
			Invalidate();
		}
	}

	/// <inheritdoc />
	protected override void OnMouseDoubleClick(MouseEventArgs e)
	{
		base.OnMouseDoubleClick(e);

		if (_props.IsScrollBarVisible &&
			_props.ScrollBarBound.Contains(e.Location))
			return;

		// OnMouseDown에서 FocusedIndex가 설정되므로 그냥 ㄱㄱ
		if (FocusedIndex >= 0 && FocusedIndex < Items.Count)
			ItemActivate?.Invoke(this, EventArgs.Empty);
	}

	/// <inheritdoc />
	protected override void OnMouseWheel(MouseEventArgs e)
	{
		base.OnMouseWheel(e);
		ScrollBy(-Math.Sign(e.Delta));
	}

	/// <inheritdoc />
	protected override bool IsInputKey(Keys keyData)
	{
		return keyData switch
		{
			Keys.Up or Keys.Down or Keys.Left or Keys.Right => true,
			_ => base.IsInputKey(keyData)
		};
	}

	/// <inheritdoc />
	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);

		if (Items.Count == 0)
			return;

		if (FocusedIndex < 0)
			FocusedIndex = 0;

		var shift = (e.Modifiers & Keys.Shift) == Keys.Shift;
		var prevIndex = FocusedIndex;
		var newIndex = prevIndex;
		var (row, col) = _props.GetRowCol(prevIndex);
		var page = _props.GetPage(prevIndex);
		var columns = _props.Columns;

		switch (e.KeyCode)
		{
			case Keys.Up:
				if (row > 0)
					newIndex -= columns;
				else if (page > 0)
				{
					// 이전 페이지 마지막 항목으로 이동
					var prevPage = page - 1;
					newIndex = _props.GetLastIndexOfPage(prevPage);
					_props.ScrollToPage(prevPage);
				}
				break;
			case Keys.Down:
				if (row < _props.Rows - 1 && newIndex + columns < Items.Count)
					newIndex += columns;
				else if (page < _props.PageCount - 1)
				{
					// 다음 페이지 첫 항목으로 이동
					var nextPage = page + 1;
					newIndex = _props.GetFirstIndexOfPage(nextPage);
					_props.ScrollToPage(nextPage);
				}
				break;
			case Keys.Left:
				if (col > 0)
					newIndex--;
				break;
			case Keys.Right:
				if (col < columns - 1 && newIndex + 1 < Items.Count)
					newIndex++;
				break;
			case Keys.Home:
				newIndex = 0;
				_props.ScrollOffset = 0;
				break;
			case Keys.End:
				newIndex = Items.Count - 1;
				_props.ScrollToPage(_props.GetPage(newIndex));
				break;
			case Keys.PageUp:
				ScrollBy(-1, focusMove: true);
				return;
			case Keys.PageDown:
				ScrollBy(1, focusMove: true);
				return;
			case Keys.Space:
			case Keys.Insert:
				_anchorIndex = FocusedIndex;
				SelectIndex(FocusedIndex);
				if (FocusedIndex < Items.Count - 1)
					FocusedIndex++;
				Invalidate();
				return;
			case Keys.Return:
				ItemActivate?.Invoke(this, EventArgs.Empty);
				return;
			case Keys.Back when Items.Count > 0:
				if (Items[0] is FileListFolderItem { DirName: ".." })
				{
					FocusedIndex = 0;
					ItemActivate?.Invoke(this, EventArgs.Empty);
				}
				return;
			case Keys.Apps when ItemClicked != null:
				// 메뉴키는 마우스 오른쪽 누름을 에뮬레이션
				if (FocusedIndex >= 0 && FocusedIndex < Items.Count)
				{
					var rect = _props.GetItemRect(FocusedIndex);
					ItemClicked.Invoke(this,
						new FileListClickEventArgs(FocusedIndex, MouseButtons.Right, rect.Location));
				}
				return;
		}

		if (newIndex != prevIndex)
		{
			// scrollOffset 보정
			_props.ScrollToPage(_props.GetPage(newIndex));

			// 포커스 인덱스 변경
			if (shift)
			{
				if (_anchorIndex == -1)
					_anchorIndex = prevIndex;
				SelectRange(_anchorIndex, newIndex);
			}

			FocusedIndex = newIndex;
			Invalidate();
		}
	}

	/// <summary>
	/// 컨트롤을 다시 그립니다.
	/// </summary>
	public new void Invalidate()
	{
		if (!_updating)
			Invalidate(false);
	}

	/// <summary>
	/// 항목 추가/삭제 등 대량 작업 전 호출합니다.
	/// </summary>
	public void BeginUpdate()
	{
		if (_updating)
			return;
		_updating = true;
	}

	/// <summary>
	/// BeginUpdate 이후 대량 작업 완료 시 호출합니다.
	/// </summary>
	public void EndUpdate()
	{
		if (!_updating)
			return;

		FocusedIndex = Items.Count > 0 ? 0 : -1;
		_anchorIndex = -1;
		_updating = false;

		_props.Refresh(true);
		_widths.UpdateName(Items, Font);
		_widths.AdjustName(Width);
		Invalidate(false);
	}

	/// <summary>
	/// 모든 항목을 삭제합니다.
	/// </summary>
	public void ClearItems()
	{
		Items.Clear();
		if (!_updating)
		{
			FocusedIndex = -1;
			_anchorIndex = -1;
			_props.Refresh(true);
		}
		_widths.ResetName();
		Invalidate();
	}

	/// <summary>
	/// 파일 항목을 추가합니다.
	/// </summary>
	public void AddFile(FileInfo fileInfo) =>
		AddItem(new FileListFileItem(this, fileInfo));

	/// <summary>
	/// 폴더 항목을 추가합니다.
	/// </summary>
	public void AddFolder(DirectoryInfo dirInfo) =>
		AddItem(new FileListFolderItem(this, dirInfo));

	/// <summary>
	/// 상위 폴더 항목을 추가합니다.
	/// </summary>
	public void AddParentFolder(DirectoryInfo dirInfo) =>
		AddItem(new FileListFolderItem(this, dirInfo, "..", true));

	/// <summary>
	/// 드라이브 항목을 추가합니다.
	/// </summary>
	public void AddDrive(DriveInfo driveInfo) =>
		AddItem(new FileListDriveItem(this, driveInfo));

	// 항목을 리스트에 추가합니다.
	private void AddItem(FileListItem item)
	{
		Items.Add(item);
		if (!_updating)
		{
			_props.Refresh();
			_widths.UpdateName(item, Font);
			_widths.AdjustName(Width);
			Invalidate(false);
		}
	}

	/// <summary>
	/// Retrieves the <see cref="FileListItem"/> at the specified index.
	/// </summary>
	/// <param name="index">The zero-based index of the item to retrieve. Must be within the range of the collection.</param>
	/// <returns>The <see cref="FileListItem"/> at the specified index, or <see langword="null"/> if the index is out of range.</returns>
	public FileListItem? GetItem(int index)
	{
		if (index < 0 || index >= Items.Count)
			return null;
		return Items[index];
	}

	/// <summary>
	/// Finds the index of the item at the specified coordinates.
	/// </summary>
	/// <param name="x">The x-coordinate to check.</param>
	/// <param name="y">The y-coordinate to check.</param>
	/// <returns>The zero-based index of the item located at the specified coordinates;  returns -1 if no item is found at the given
	/// position.</returns>
	public int FindIndexAt(int x, int y)
	{
		// TODO: 이거 성능 개선 필요. 루프를 다 돌 필요없이, 페이지에서 얻을 수 있음.
		for (var i = 0; i < Items.Count; i++)
		{
			var rect = _props.GetItemRect(i);
			if (rect.Contains(x, y))
				return i;
		}
		return -1;
	}

	/// <summary>
	/// Retrieves the index of the first item with the specified name.
	/// </summary>
	/// <param name="name">The full name of the item to locate. Cannot be null.</param>
	/// <returns>The zero-based index of the first item with the specified name, or -1 if no such item is found.</returns>
	public int FindIndexByName(string name) =>
		Items.FindIndex(item => item.FullName == name);

	/// <summary>
	/// Sets the focus to the specified item in the list and adjusts the scroll position if necessary.
	/// </summary>
	/// <remarks>If the specified <paramref name="index"/> is out of range, the method does nothing.  When the item
	/// is focused, the view is scrolled to ensure the item is visible within the current viewport.</remarks>
	/// <param name="index">The zero-based index of the item to focus. Must be within the range of available items.</param>
	public void EnsureFocus(int index)
	{
		if (index < 0 || index >= Items.Count)
			return;

		FocusedIndex = index;

		_props.CalcScrollOffset(index);
		Invalidate();
	}

	/// <summary>
	/// Ensures that the item with the specified name is focused, if it exists.
	/// </summary>
	/// <remarks>If the item with the specified name is not found, or if the collection of items is empty, the
	/// method does nothing.</remarks>
	/// <param name="name">The name of the item to focus. Can be <see langword="null"/> or empty, in which case no action is taken.</param>
	public void EnsureFocusByName(string? name)
	{
		if (string.IsNullOrEmpty(name) || Items.Count == 0)
			return;

		var index = FindIndexByName(name);
		if (index >= 0)
			EnsureFocus(index);
	}

	/// <summary>
	/// 선택된 항목의 개수를 반환합니다.
	/// </summary>
	public int GetSelectedCount() =>
		Items.Count(item => item.Selected);

	/// <summary>
	/// 선택된 파일의 총 크기를 반환합니다.
	/// </summary>
	public long GetSelectedSize()
	{
		var size = 0L;
		foreach (var item in Items)
		{
			if (item.Selected && item is FileListFileItem fileItem)
				size += fileItem.Size;
		}
		return size;
	}

	/// <summary>
	/// 선택된 파일의 전체 경로 리스트를 반환합니다.
	/// </summary>
	public List<string> GetSelectedFiles()
	{
		List<string> files = [];
		files.AddRange(from item in Items where item.Selected select item.FullName);
		return files;
	}

	/// <summary>
	/// 선택된 파일이 없으면 포커스된 항목의 경로를 반환합니다.
	/// </summary>
	public List<string> GetSelectedOrFocused()
	{
		var files = GetSelectedFiles();
		if (files.Count == 0 && FocusedIndex >= 0 && FocusedIndex < Items.Count)
		{
			var item = Items[FocusedIndex];
			if (item is not FileListFolderItem { IsParent: true })
				files.Add(item.FullName);
		}
		return files;
	}

	// 아이템 선택
	private void SelectIndex(int index)
	{
		var item = Items[index];
		if (item is FileListFileItem or FileListFolderItem { IsParent: false })
		{
			item.Selected = !item.Selected;
			SelectionChanged?.Invoke(this, EventArgs.Empty);
		}
	}

	// 범위 선택
	private void SelectRange(int from, int to)
	{
		if (from > to)
			(from, to) = (to, from);
		foreach (var item in Items)
			item.Selected = false;
		for (var i = from; i <= to; i++)
		{
			var item = Items[i];
			if (item is FileListFileItem or FileListFolderItem { IsParent: false })
				item.Selected = true;
		}
		SelectionChanged?.Invoke(this, EventArgs.Empty);
		Invalidate();
	}

	// 페이지 스크롤 및 포커스 이동 지원
	private void ScrollBy(int direction, bool focusMove = false)
	{
		if (!_props.IsScrollBarVisible || Items.Count == 0)
			return;

		var curPage = _props.CurrentPage;
		var newPage = Math.Clamp(curPage + direction, 0, _props.LastPage);

		if (focusMove)
		{
			if (direction < 0)
			{
				var first = _props.GetFirstIndexOfPage(curPage);
				if (FocusedIndex > first)
					FocusedIndex = first;
				else
				{
					FocusedIndex = _props.GetFirstIndexOfPage(newPage);
					_props.ScrollToPage(newPage);
				}
			}
			else
			{
				var last = _props.GetLastIndexOfPage(curPage);
				if (FocusedIndex < last)
					FocusedIndex = last;
				else
				{
					FocusedIndex = _props.GetLastIndexOfPage(newPage);
					_props.ScrollToPage(newPage);
				}
			}
			return;
		}

		_props.ScrollToPage(newPage);
		Invalidate();
	}
}

/// <summary>
/// 파일 리스트의 스크롤바 및 페이지 관련 정보를 저장하는 구조체입니다.
/// </summary>
internal class FileListProps(FileList fileList, int columnCount)
{
	// 파일 리스트 컨트롤 참조
	private readonly FileList _list = fileList;

	public int Columns { get; set; } = columnCount;

	public int Rows { get; set; }
	public int ViewRows { get; set; }

	public int ItemWidth { get; set; }
	public int ItemHeight { get; set; }

	public int ContentHeight { get; set; }
	public int ViewHeight { get; set; }

	public int PageCount { get; set; }
	public int PageSize { get; set; }
	public int PageStep => ViewRows * ItemHeight;
	public int CurrentPage => ScrollOffset / PageStep;
	public int LastPage => PageCount - 1;
	public int ScrollOffset { get; set; }

	public bool IsScrollBarVisible => ContentHeight > ViewHeight;
	public bool IsScrollBarDragging { get; set; }
	public int ScrollBarSize { get; set; } = 12;
	public int ScrollBarOffset { get; set; }
	public int ScrollBarMinIndicatorWidth { get; set; } = 24;
	public int ScrollBarIndicatorWidth { get; set; }
	public Rectangle ScrollBarBound { get; set; } = Rectangle.Empty;
	public MinMax ScrollBarLeftRange { get; set; } = MinMax.Empty;
	public MinMax ScrollBarRightRange { get; set; } = MinMax.Empty;
	public MinMax ScrollBarTrackRange { get; set; } = MinMax.Empty;
	public MinMax ScrollBarIndicatorRange { get; set; } = MinMax.Empty;

	public void SetColumns(int column)
	{
		Columns = Math.Clamp(column, 1, 4);
		Refresh();
	}

	public void Refresh(bool resetScrollOffset = false)
	{
		var size = _list.Size;
		var fontHeight = _list.Font.Height;
		var count = _list.Items.Count;

		Rows = (count + Columns - 1) / Columns;
		ViewHeight = size.Height - ScrollBarSize;

		ItemHeight = fontHeight + 6;
		ItemWidth = (size.Width - 4) / Columns;

		ViewRows = Math.Max(1, ViewHeight / ItemHeight);
		ContentHeight = Rows * ItemHeight;

		PageSize = ViewRows * Columns;
		PageCount = (count + PageSize - 1) / PageSize;

		if (resetScrollOffset)
			ScrollOffset = 0;
		else if (ScrollOffset > ContentHeight - ViewHeight)
			ScrollOffset = Math.Max(0, ContentHeight - ViewHeight);

		if (!IsScrollBarVisible)
		{
			ScrollBarBound = Rectangle.Empty;
			return;
		}

		ScrollBarBound = new Rectangle(0, size.Height - ScrollBarSize, size.Width, ScrollBarSize);
		ScrollBarLeftRange = new MinMax(0, ScrollBarSize);
		ScrollBarRightRange = new MinMax(size.Width - ScrollBarSize, size.Width);
		ScrollBarTrackRange = new MinMax(ScrollBarSize, size.Width - ScrollBarSize);
		ScrollBarIndicatorWidth = Math.Max(ScrollBarMinIndicatorWidth, ScrollBarTrackRange.Length / Math.Max(1, PageCount));
	}

	public (int row, int col) GetRowCol(int index) =>
		(index / Columns, index % Columns);

	public int GetRow(int index) =>
		index / Columns;

	public int GetColumn(int index) =>
		index % Columns;

	public int GetPage(int index) =>
		PageSize == 0 ? 0 : index / PageSize;

	public int GetIndexOfPage(int index) =>
		PageSize == 0 ? 0 : index % PageSize;

	public int GetFirstIndexOfPage(int page) =>
		page * PageSize;

	public int GetLastIndexOfPage(int page) =>
		Math.Min((page + 1) * PageSize, _list.Items.Count) - 1;

	public Rectangle GetItemRect(int index)
	{
		var (row, col) = GetRowCol(index);
		var x = col * ItemWidth;
		var y = row * ItemHeight - ScrollOffset;
		return new Rectangle(x, y, ItemWidth, ItemHeight);
	}

	public void CalcScrollOffset(int index)
	{
		var row = GetRow(index);
		var top = ScrollOffset / ItemHeight;
		var bottom = (ScrollOffset + ViewHeight) / ItemHeight - 1;
		if (row < top)
			ScrollOffset = row * ItemHeight;
		else if (row > bottom)
		{
			var newOffset = Math.Min((row + 1) * ItemHeight - ViewHeight, Math.Max(0, ContentHeight - ViewHeight));
			ScrollOffset = newOffset;
		}
	}

	public void CalcScrollBarIndicator()
	{
		if (!IsScrollBarVisible)
			return;

		var width = ScrollBarTrackRange.Length;
		var pageCount = Math.Max(1, PageCount);
		var curPage = Math.Clamp(CurrentPage, 0, pageCount - 1);
		var indPos = (int)((float)curPage / (pageCount - 1) * (width - ScrollBarIndicatorWidth));
		if (pageCount == 1 || indPos < 0) indPos = 0;
		if (indPos + ScrollBarIndicatorWidth > width) indPos = width - ScrollBarIndicatorWidth;
		var left = indPos + ScrollBarTrackRange.Left;
		var right = left + ScrollBarIndicatorWidth;
		ScrollBarIndicatorRange = new MinMax(left, right);
	}

	public void ScrollToPage(int page)
	{
		if (page < 0 || page >= PageCount)
			return;
		ScrollOffset = page * PageStep;
	}
}

/// <summary>
/// 파일 리스트의 항목 너비와 기타 정보를 저장하는 구조체입니다.
/// </summary>
internal class FileListWidths
{
	/// <summary>파일/디렉터리 이름 컬럼 너비</summary>
	public int Name { get; set; }
	/// <summary>확장자 컬럼 너비</summary>
	public int Extension { get; set; }
	/// <summary>확장자 최소 너비</summary>
	public int MinExtension { get; set; }

	/// <summary>크기 컬럼 너비</summary>
	public int Size { get; set; }
	/// <summary>날짜 컬럼 너비</summary>
	public int Date { get; set; }
	/// <summary>시간 컬럼 너비</summary>
	public int Time { get; set; }
	/// <summary>속성 컬럼 너비</summary>
	public int Attr { get; set; }

	/// <summary>모든 고정 컬럼 너비</summary>
	public int Fixed { get; set; }

	/// <summary>전체 컬럼 그릴지 여부</summary>
	public bool IsFixedVisible { get; set; }

	/// <summary>
	/// 폰트 기준으로 고정 컬럼 너비를 계산합니다.
	/// </summary>
	public void UpdateFixed(Font font)
	{
		var dirWidth = TextRenderer.MeasureText("[폴더]", font).Width;
		var sizeWidth = TextRenderer.MeasureText("999.99 WB", font).Width;
		MinExtension = TextRenderer.MeasureText("WWW", font).Width + 4;

		Size = Math.Max(dirWidth, sizeWidth) + 8;
		Date = TextRenderer.MeasureText("9999-99-99", font).Width + 4;
		Time = TextRenderer.MeasureText("99:99", font).Width + 4;
		Attr = TextRenderer.MeasureText("WWWW", font).Width + 4;
		Fixed = Size + Date + Time + Attr;
	}

	/// <summary>
	/// 항목 리스트 기준으로 이름/확장자 너비를 계산합니다.
	/// </summary>
	public void UpdateName(List<FileListItem> items, Font font)
	{
		Name = 0;
		Extension = 0;
		foreach (var item in items)
		{
			item.NameWidth = TextRenderer.MeasureText(item.DisplayName, font).Width + 8;
			item.ExtWidth = TextRenderer.MeasureText(item.DisplayExtension, font).Width + 8;
			if (item.NameWidth > Name) Name = item.NameWidth;
			if (item.ExtWidth > Extension) Extension = item.ExtWidth;
		}
	}

	/// <summary>
	/// 단일 항목 기준으로 이름/확장자 너비를 갱신합니다.
	/// </summary>
	public void UpdateName(FileListItem item, Font font)
	{
		item.NameWidth = TextRenderer.MeasureText(item.DisplayName, font).Width + 8;
		item.ExtWidth = TextRenderer.MeasureText(item.DisplayExtension, font).Width + 8;
		if (item.NameWidth > Name) Name = item.NameWidth;
		if (item.ExtWidth > Extension) Extension = item.ExtWidth;
	}

	/// <summary>
	/// 이름/확장자 너비를 초기화합니다.
	/// </summary>
	public void ResetName()
	{
		Name = 0;
		Extension = 0;
	}

	/// <summary>
	/// 컨트롤 너비에 맞게 이름/확장자 너비를 조정합니다.
	/// </summary>
	public bool AdjustName(int controlWidth)
	{
		IsFixedVisible = Fixed <= controlWidth * 0.6;

		// TODO: 아래 로직 말고 이름과 확장자와 크기만 표시할 때 크기 설정할 것
		var remainWidth = controlWidth - Fixed;
		if (remainWidth <= 0)
		{
			Name = 0;
			Extension = 0;
			return false;
		}

		var total = Name + Extension;

		if (Name + Extension == remainWidth)
			Extension = remainWidth - Name;
		else if (Name < Extension)
		{
			Name = remainWidth / 2;
			Extension = remainWidth - Name;
		}
		else
		{
			if (total > 0)
			{
				Name = remainWidth * Name / total;
				Extension = remainWidth - Name;
				if (Extension < MinExtension)
				{
					Name = remainWidth - MinExtension;
					Extension = MinExtension;
				}
			}
			else
			{
				Name = remainWidth;
				Extension = 0;
			}
		}

		return !((Name + Extension) > remainWidth * 1.5);
	}
}

/// <summary>
/// 파일 리스트의 항목을 나타내는 추상 클래스입니다.
/// </summary>
public abstract class FileListItem(FileList fileList)
{
	/// <summary> 선택 마크 너비</summary>
	protected const int MarkWidth = 8;

	/// <summary> 소속 리스트</summary>
	protected readonly FileList _list = fileList;

	/// <summary>선택 여부</summary>
	public bool Selected { get; set; }
	/// <summary>이름 컬럼 너비</summary>
	public int NameWidth { get; set; }
	/// <summary>확장자 컬럼 너비</summary>
	public int ExtWidth { get; set; }
	/// <summary>아이콘</summary>
	public Image? Icon { get; set; }
	/// <summary>색상</summary>
	public Color Color { get; set; }

	/// <summary>전체 경로</summary>
	internal abstract string FullName { get; }
	/// <summary>표시 이름</summary>
	internal abstract string DisplayName { get; }
	/// <summary>표시 확장자</summary>
	internal abstract string DisplayExtension { get; }

	/// <summary>
	/// Draws the detailed view of a file list item within the specified bounds.
	/// </summary>
	/// <remarks>파생 클래스에서 파일 리스트 항목의 상세 그리기 로직을 구현해야 합니다.</remarks>
	/// <param name="g">그리기에 사용되는 <see cref="Graphics"/> 객체입니다.</param>
	/// <param name="font">텍스트 렌더링에 사용되는 <see cref="Font"/>입니다.</param>
	/// <param name="bounds">상세 정보를 그릴 영역을 지정하는 <see cref="Rectangle"/>입니다.</param>
	/// <param name="widths">상세 정보의 컬럼 너비를 지정하는 <see cref="FileListWidths"/>입니다.</param>
	/// <param name="theme">상세 정보의 시각적 스타일을 결정하는 <see cref="Theme"/>입니다.</param>
	/// <param name="focused">항목이 현재 포커스 상태인지 여부입니다. <see langword="true"/>면 포커스됨, 아니면 <see langword="false"/>.</param>
	internal virtual void Draw(Graphics g, Font font, Rectangle bounds, FileListWidths widths, Theme theme, bool focused)
	{
		using (var backBrush = new SolidBrush(focused ? Color : Selected ? theme.BackSelection : theme.BackContent))
			g.FillRectangle(backBrush, bounds);

		if (Selected)
		{
			var markRect = new Rectangle(bounds.Left + 2, bounds.Top + (bounds.Height - MarkWidth) / 2, MarkWidth, MarkWidth);
			DrawRightArrowMark(g, markRect, theme);
		}

		var iconX = bounds.Left + MarkWidth + 4;
		if (Icon != null)
			g.DrawImage(Icon, iconX, bounds.Top + (bounds.Height - 16) / 2, 16, 16);
	}

	/// <summary>
	/// 항목 텍스트를 그립니다.
	/// </summary>
	protected static void DrawItemText(Graphics g, string text, Font font, Rectangle bounds, Color color,
		bool rightAlign = false)
	{
		var drawText = text;
		var textSize = TextRenderer.MeasureText(drawText, font);
		if (textSize.Width > bounds.Width)
		{
			for (var len = drawText.Length - 1; len > 0; len -= 3)
			{
				var t = drawText[..len] + "...";
				if (TextRenderer.MeasureText(t, font).Width <= bounds.Width)
				{
					drawText = t;
					break;
				}
			}
		}

		var flags = TextFormatFlags.VerticalCenter | (rightAlign ? TextFormatFlags.Right : TextFormatFlags.Left);
		TextRenderer.DrawText(g, drawText, font, bounds, color, flags);
	}

	// 선택 화살표 마크 그리기
	private static void DrawRightArrowMark(Graphics g, Rectangle rect, Theme theme)
	{
		var pts = new[]
		{
			new Point(rect.Left, rect.Top),
			new Point(rect.Right, rect.Top + rect.Height / 2),
			new Point(rect.Left, rect.Bottom)
		};
		using var brush = new SolidBrush(theme.Foreground);
		g.FillPolygon(brush, pts);
	}
}

/// <summary>
/// 파일 항목을 나타내는 클래스입니다.
/// </summary>
public class FileListFileItem : FileListItem
{
	/// <summary>파일 정보</summary>
	public FileInfo Info { get; }
	/// <summary>파일명</summary>
	public string FileName { get; }
	/// <summary>확장자</summary>
	public string Extension { get; }
	/// <summary>파일 크기</summary>
	public long Size => Info.Length;
	/// <summary>생성일</summary>
	public DateTime Creation => Info.CreationTime;
	/// <summary>파일 속성</summary>
	public FileAttributes Attributes => Info.Attributes;

	/// <summary>
	/// 파일 항목을 생성합니다. 파일명, 확장자, 아이콘, 색상 등 정보를 제공합니다.
	/// </summary>
	/// <param name="fileList">이 파일 항목이 속한 파일 리스트입니다.</param>
	/// <param name="fileInfo">이 파일 항목을 초기화하는 파일 정보입니다.</param>
	public FileListFileItem(FileList fileList, FileInfo fileInfo) :
		base(fileList)
	{
		Info = fileInfo;

		var name = fileInfo.Name;
		var lastDot = name.LastIndexOf('.');
		FileName = lastDot >= 0 ? name[..lastDot] : name;
		Extension = lastDot >= 0 ? name[(lastDot + 1)..] : string.Empty;

		Icon = IconCache.Instance.GetIcon(fileInfo.FullName, Extension);
		Color = Settings.Instance.Theme.GetColorExtension(Extension.ToUpperInvariant());
	}

	/// <inheritdoc />
	internal override string FullName => Info.FullName;
	/// <inheritdoc />
	internal override string DisplayName => FileName;
	/// <inheritdoc />
	internal override string DisplayExtension => Extension;

	/// <inheritdoc />
	internal override void Draw(Graphics g, Font font, Rectangle bounds, FileListWidths widths, Theme theme, bool focused)
	{
		base.Draw(g, font, bounds, widths, theme, focused);

		var (fileColor, otherColor) = focused ? (theme.BackContent, theme.BackContent) : (Color, theme.Foreground);
		var x = bounds.Left + 28;
		DrawItemText(g, FileName, font, new Rectangle(x, bounds.Top, widths.Name, bounds.Height), fileColor);
		x += widths.Name;
		DrawItemText(g, Extension, font, new Rectangle(x, bounds.Top, widths.Extension, bounds.Height), fileColor);
		x += widths.Extension;
		DrawItemText(g, Size.FormatFileSize(), font, new Rectangle(x, bounds.Top, widths.Size, bounds.Height), otherColor, true);
		x += widths.Size;

		if (widths.IsFixedVisible)
		{
			DrawItemText(g, Creation.FormatRelativeDate(), font, new Rectangle(x, bounds.Top, widths.Date, bounds.Height), otherColor);
			x += widths.Date;
			DrawItemText(g, Creation.ToString("HH:mm"), font, new Rectangle(x, bounds.Top, widths.Time, bounds.Height), otherColor);
			x += widths.Time;
			DrawItemText(g, Attributes.FormatString(), font, new Rectangle(x, bounds.Top, widths.Attr, bounds.Height), otherColor);
		}
	}
}

/// <summary>
/// 디렉터리 항목을 나타내는 클래스입니다.
/// </summary>
public class FileListFolderItem : FileListItem
{
	/// <summary>디렉터리 정보</summary>
	public DirectoryInfo Info { get; }
	/// <summary>디렉터리명</summary>
	public string DirName { get; }
	/// <summary>생성일</summary>
	public DateTime Creation => Info.CreationTime;
	/// <summary>디렉터리 속성</summary>
	public FileAttributes Attributes => Info.Attributes;
	/// <summary>부모 폴더 인가요?</summary>
	public bool IsParent { get; }

	/// <summary>
	/// 폴더 항목을 생성합니다. 폴더 정보와 관련 속성에 접근할 수 있습니다.
	/// </summary>
	/// <param name="fileList">이 디렉터리 항목이 속한 파일 리스트입니다.</param>
	/// <param name="dirInfo">디렉터리 정보를 담고 있는 <see cref="DirectoryInfo"/> 객체입니다.</param>
	public FileListFolderItem(FileList fileList, DirectoryInfo dirInfo) :
		base(fileList)
	{
		Info = dirInfo;

		DirName = dirInfo.Name;
		IsParent = false;

		Icon = IconCache.Instance.GetIcon(dirInfo.FullName, string.Empty, true);
		Color = Settings.Instance.Theme.Folder;
	}

	/// <summary>
	/// 디렉터리 항목을 생성합니다. 디렉터리 아이콘과 색상은 테마 설정에 따라 지정됩니다.
	/// </summary>
	/// <param name="fileList">이 디렉터리 항목이 속한 파일 리스트입니다.</param>
	/// <param name="dirInfo">이 항목에 연결된 디렉터리 정보입니다.</param>
	/// <param name="dirName">디렉터리 이름입니다.</param>
	/// <param name="isParent">부모 폴더 인가요?</param>
	public FileListFolderItem(FileList fileList, DirectoryInfo dirInfo, string dirName, bool isParent) :
		base(fileList)
	{
		Info = dirInfo;

		DirName = dirName;
		IsParent = isParent;

		Icon = IconCache.Instance.GetIcon(dirInfo.FullName, string.Empty, true);
		Color = Settings.Instance.Theme.Folder;
	}

	/// <inheritdoc />
	internal override string FullName => Info.FullName;
	/// <inheritdoc />
	internal override string DisplayName => DirName;
	/// <inheritdoc />
	internal override string DisplayExtension => string.Empty;

	/// <inheritdoc />
	internal override void Draw(Graphics g, Font font, Rectangle bounds, FileListWidths widths, Theme theme, bool focused)
	{
		base.Draw(g, font, bounds, widths, theme, focused);

		var (dirColor, otherColor) = focused ? (theme.BackContent, theme.BackContent) : (Color, theme.Foreground);
		var x = bounds.Left + 28;
		DrawItemText(g, DirName, font, new Rectangle(x, bounds.Top, widths.Name, bounds.Height), dirColor);
		x += widths.Name;
		DrawItemText(g, string.Empty, font, new Rectangle(x, bounds.Top, widths.Extension, bounds.Height), dirColor);
		x += widths.Extension;
		DrawItemText(g, "[폴더]", font, new Rectangle(x, bounds.Top, widths.Size, bounds.Height), dirColor, true);
		x += widths.Size;

		if (widths.IsFixedVisible)
		{
			DrawItemText(g, Creation.FormatRelativeDate(), font, new Rectangle(x, bounds.Top, widths.Date, bounds.Height), otherColor);
			x += widths.Date;
			DrawItemText(g, Creation.ToString("HH:mm"), font, new Rectangle(x, bounds.Top, widths.Time, bounds.Height), otherColor);
			x += widths.Time;
			DrawItemText(g, Attributes.FormatString(), font, new Rectangle(x, bounds.Top, widths.Attr, bounds.Height), otherColor);
		}
	}
}

/// <summary>
/// 드라이브 항목을 나타내는 클래스입니다.
/// </summary>
public class FileListDriveItem : FileListItem
{
	/// <summary>드라이브 정보</summary>
	public DriveInfo Info { get; }
	/// <summary>드라이브명</summary>
	public string DriveName { get; }
	/// <summary>볼륨 라벨</summary>
	public string VolumeLabel { get; }
	/// <summary>전체 크기</summary>
	public long Total => Info.TotalSize;
	/// <summary>사용 가능 크기</summary>
	public long Available => Info.AvailableFreeSpace;

	/// <summary>
	/// 드라이브 항목을 생성합니다. 드라이브명, 볼륨 라벨, 아이콘, 색상 등을 설정합니다.
	/// </summary>
	/// <remarks>드라이브명과 볼륨 라벨을 추출 및 포맷하고, 아이콘과 테마 색상을 적용합니다.</remarks>
	/// <param name="fileList">이 드라이브 항목이 속한 파일 리스트입니다.</param>
	/// <param name="driveInfo">드라이브 정보를 초기화에 사용합니다.</param>
	public FileListDriveItem(FileList fileList, DriveInfo driveInfo) :
		base(fileList)
	{
		Info = driveInfo;

		DriveName = driveInfo.Name.TrimEnd('\\');
		VolumeLabel = $"{DriveName} {driveInfo.VolumeLabel}";

		Icon = IconCache.Instance.GetIcon(DriveName, string.Empty, false, true);
		Color = Settings.Instance.Theme.Drive;
	}

	/// <inheritdoc />
	internal override string FullName => Info.Name;
	/// <inheritdoc />
	internal override string DisplayName => VolumeLabel;
	/// <inheritdoc />
	internal override string DisplayExtension => string.Empty;

	/// <inheritdoc />
	internal override void Draw(Graphics g, Font font, Rectangle bounds, FileListWidths widths, Theme theme, bool focused)
	{
		base.Draw(g, font, bounds, widths, theme, focused);

		var driveColor = focused ? theme.BackContent : Color;
		var x = bounds.Left + 28;
		var w = widths.Name + widths.Extension;
		DrawItemText(g, VolumeLabel, font, new Rectangle(x, bounds.Top, w, bounds.Height), driveColor);
		// 그래프 그려야함
	}
}

/// <summary>
/// 파일 리스트 클릭 이벤트 인자입니다.
/// </summary>
public class FileListClickEventArgs(int index, MouseButtons button, Point location) : EventArgs
{
	/// <summary>클릭한 항목 순번입니다.</summary>
	public int Index { get; } = index;

	/// <summary>클릭된 마우스 버튼입니다.</summary>
	public MouseButtons Button { get; } = button;

	/// <summary>클릭 위치(컨트롤 기준 좌표)입니다.</summary>
	public Point Location { get; } = location;
}
