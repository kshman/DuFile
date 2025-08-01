using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace DuFile.Windows;

/// <summary>
/// 파일, 디렉터리, 드라이브 항목을 표시하고 관리하는 커스텀 리스트 컨트롤입니다.
/// </summary>
public sealed class FileList : ThemeControl
{
	// 스크롤바 크기 (픽셀)
	private const int SbSize = 12;
	// 스크롤바 지시자 최소 너비 (픽셀)
	private const int SbMinIndWidth = 24;

	// 컬럼 너비 정보 객체
	private readonly FileListWidths _widths = new();

	// 항목 추가/삭제 등 대량 작업 중 여부
	private bool _updating;
	// 레이아웃 재계산 필요 여부
	private bool _needRefresh;
	// 활성화 상태
	private bool _isActive;

	// 현재 포커스된 항목 인덱스
	private int _focusedIndex = -1;
	// Shift로 다중 선택 시 기준 인덱스
	private int _anchorIndex = -1;

	// 열(컬럼) 개수
	private int _columns = 1;
	// 전체 줄(행) 개수
	private int _rows;

	// 한 열에 보이는 줄 수
	private int _viewRows;
	// 아이템 1개의 너비
	private int _itemWidth;
	// 아이템 1개의 높이
	private int _itemHeight;
	// 전체 내용 높이
	private int _contentHeight;
	// 실제 보이는 높이
	private int _viewHeight;
	// 전체 페이지 개수
	private int _pageCount;
	// 한 페이지에 표시할 항목 수
	private int _pageSize;
	// 현재 페이지 번호
	private int _currentPage;
	// 현재 페이지의 첫 번째 항목 인덱스
	private int _firstIndex;
	// 현재 페이지의 마지막 항목 인덱스
	private int _lastIndex;

	// 스크롤바 드래그 중 여부
	private bool _sbDragging;
	// 스크롤바 드래그 오프셋
	private int _sbOffset;
	// 스크롤바 지시자(인디케이터) 너비
	private int _sbIndWidth;
	// 스크롤바 전체 영역
	private Rectangle _sbBound = Rectangle.Empty;
	// 스크롤바 트랙(이동 가능한 영역) 범위
	private MinMax _sbTrackRange = MinMax.Empty;
	// 스크롤바 지시자(인디케이터) 범위
	private MinMax _sbIndRange = MinMax.Empty;

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
		get => _columns;
		set
		{
			SetColumns(value);
			_needRefresh = true;
			Invalidate();
		}
	}

	/// <summary>스크롤 바 높이입니다.</summary>
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int ScrollBarSize { get; set; } = SbSize;

	/// <summary>세로 줄을 그립니다.</summary>
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool ShowVerticalLine { get; set; } = false;

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
			if (value < 0)
				_focusedIndex = 0;
			else if (value >= Items.Count)
				_focusedIndex = Items.Count - 1;
			else
				_focusedIndex = value;
			FocusedIndexChanged?.Invoke(this, EventArgs.Empty);
			Invalidate();
		}
	}

	/// <summary>
	/// 현재 컨트롤이 활성 상태인지 가져오거나 설정합니다.
	/// </summary>
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool IsActive
	{
		get => _isActive;
		set
		{
			_isActive = value;
			if (value)
				Focus();
			Invalidate();
		}
	}

	/// <summary>
	/// 파일 패널
	/// </summary>
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public FilePanel? FilePanel { get; set; }

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

	/// <summary>
	/// FileList 컨트롤을 초기화합니다.
	/// </summary>
	public FileList()
	{
		SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
				 ControlStyles.ResizeRedraw | ControlStyles.UserPaint | ControlStyles.Selectable, true);
		TabStop = true;
	}

	/// <inheritdoc/>
	protected override void OnUpdateTheme(Theme theme)
	{
		Font = new Font(theme.ContentFontFamily, theme.ContentFontSize, FontStyle.Regular, GraphicsUnit.Point);
		BackColor = theme.BackContent;
		ForeColor = theme.Foreground;

		_widths.UpdateFixed(Font);
		RefreshLayout();
	}

	/// <inheritdoc/>
	protected override void OnResize(EventArgs e)
	{
		base.OnResize(e);
		Invalidate();
		_widths.UpdateFixed(Font);
		_needRefresh = true;
	}

	/// <inheritdoc/>
	protected override void OnPaint(PaintEventArgs e)
	{
		if (_updating)
			return;

		base.OnPaint(e);

		var g = e.Graphics;
		var theme = Settings.Instance.Theme;
		g.Clear(theme.BackContent);

		if (_needRefresh)
		{
			_needRefresh = false;
			_widths.UpdateName(Items, Font);
			_widths.AdjustName(_itemWidth, _columns);
			Sort();
			RefreshLayout();
		}

		PrepareIndex();
		var prop = new FileListDrawProp(Font, theme, _isActive);
		var first = _firstIndex;
		var count = _lastIndex - first + 1;
		for (var i = 0; i < count; i++)
		{
			var index = first + i;
			prop.Reset(GetItemRect(i), index == FocusedIndex);
			Items[index].Draw(g, prop, _widths);
		}

		if (IsScrollBarVisible)
			DrawScrollBar(g, theme);

		if (ShowVerticalLine)
		{
			using var pen = new Pen(theme.DebugLine, 1);
			pen.DashStyle = DashStyle.Dash;
			var x = FileListDrawProp.LeadingWidth;
			g.DrawLine(pen, x, 0, x, Height);
			x += _widths.Name;
			g.DrawLine(pen, x, 0, x, Height);
			x += _widths.Extension;
			g.DrawLine(pen, x, 0, x, Height);
			x += _widths.Size;
			if (_widths.IsFixedVisible)
			{
				g.DrawLine(pen, x, 0, x, Height);
				x += _widths.Date;
				g.DrawLine(pen, x, 0, x, Height);
				x += _widths.Time;
				g.DrawLine(pen, x, 0, x, Height);
			}
		}
	}

	// 스크롤바를 그립니다.
	private void DrawScrollBar(Graphics g, Theme theme)
	{
		if (!_sbDragging)
		{
			// 드래그 중이 아닐 때 스크롤바 인디케이터 범위 계산
			CalcScrollBarIndicator();
		}

		using var brushForground = new SolidBrush(theme.Foreground);
		using var brushContent = new SolidBrush(theme.BackContent);
		using var brushSelection = new SolidBrush(theme.BackSelection);
		using var brushActive = new SolidBrush(theme.BackActive);

		var rect = _sbBound;
		g.FillRectangle(brushContent, rect);

		// 스크롤바 지시자(인디케이터) 그리기
		g.FillRectangle(_sbDragging ? brushActive : brushSelection,
			_sbIndRange.Left, rect.Top + 1, _sbIndWidth, rect.Height - 1);

		// 좌/우 화살표 그리기
		Point[] leftPts =
		[
			new(10, rect.Top + 2),
			new(SbSize - 8, rect.Top + rect.Height / 2),
			new(10, rect.Bottom - 2)
		];
		Point[] rightPts =
		[
			new(Width - 10, rect.Top + 2),
			new(Width - SbSize + 8, rect.Top + rect.Height / 2),
			new(Width - 10, rect.Bottom - 2)
		];
		g.FillPolygon(brushForground, leftPts);
		g.FillPolygon(brushForground, rightPts);

		// 스크롤바 상단 분리선
		using var pen = new Pen(theme.Border);
		g.DrawLine(pen, 0, rect.Top, Width, rect.Top);
	}

	/// <inheritdoc/>
	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		if (_sbDragging)
		{
			var trackWidth = _sbTrackRange.Length;
			var x = e.X - _sbTrackRange.Left - _sbOffset;
			var rx = Math.Max(0, Math.Min(trackWidth - _sbIndWidth, x));
			var page = (int)Math.Round((float)rx / (trackWidth - _sbIndWidth) * (Math.Max(1, _pageCount) - 1));
			SetPage(page);

			// 드래그 중일 때는 지시자의 위치를 업데이트하고 다시 그립니다.
			var left = _sbTrackRange.Left + x;
			var right = left + _sbIndWidth;
			if (left < _sbTrackRange.Left)
				left = _sbTrackRange.Left;
			else if (right > _sbTrackRange.Right)
				left = _sbTrackRange.Right - _sbIndWidth;
			_sbIndRange = new MinMax(left, right);
			Invalidate();
		}
	}

	/// <inheritdoc/>
	protected override void OnMouseDown(MouseEventArgs e)
	{
		base.OnMouseDown(e);
		Focus();

		if (IsScrollBarVisible &&
			_sbBound.Contains(e.Location) &&
			e.Button is MouseButtons.Left or MouseButtons.Right)
		{
			if (_sbIndRange.Contains(e.X))
			{
				_sbDragging = true;
				_sbOffset = e.X - _sbIndRange.Left;
				return;
			}
			if (e.X < _sbIndRange.Left)
			{
				SetPageDelta(-1);
				Invalidate();
				return;
			}
			if (e.X > _sbIndRange.Right)
			{
				SetPageDelta(1);
				Invalidate();
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
			case MouseButtons.XButton1:
			case MouseButtons.XButton2:
			case MouseButtons.None:
			default:
				break;
		}

		FocusedIndex = index;
		ItemClicked?.Invoke(this, new FileListClickEventArgs(index, e.Button, e.Location));
	}

	/// <inheritdoc/>
	protected override void OnMouseUp(MouseEventArgs e)
	{
		base.OnMouseUp(e);

		if (_sbDragging)
		{
			_sbDragging = false;
			Invalidate();
		}
	}

	/// <inheritdoc/>
	protected override void OnMouseDoubleClick(MouseEventArgs e)
	{
		base.OnMouseDoubleClick(e);

		if (IsScrollBarVisible &&
			_sbBound.Contains(e.Location))
			return;

		var index = FindIndexAt(e.X, e.Y);
		if (index < 0 || index >= Items.Count)
			return;

		FocusedIndex = index;
		ItemActivate?.Invoke(this, EventArgs.Empty);
	}

	/// <inheritdoc/>
	protected override void OnMouseWheel(MouseEventArgs e)
	{
		base.OnMouseWheel(e);
		SetPageDelta(-Math.Sign(e.Delta));
		Invalidate();
	}

	/// <inheritdoc/>
	protected override bool IsInputKey(Keys keyData)
	{
		return keyData switch
		{
			Keys.Up or Keys.Down or Keys.Left or Keys.Right => true,
			_ => base.IsInputKey(keyData)
		};
	}

	/// <inheritdoc/>
	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);

		if (Items.Count == 0)
			return;

		if (FocusedIndex < 0)
			FocusedIndex = 0;

		var curIndex = FocusedIndex;
		var newIndex = curIndex;

		switch (e.KeyCode)
		{
			case Keys.Up:
				newIndex--;
				break;
			case Keys.Down:
				newIndex++;
				break;
			case Keys.Left:
				newIndex -= _viewRows;
				break;
			case Keys.Right:
				newIndex += _viewRows;
				break;
			case Keys.Home:
				newIndex = 0;
				break;
			case Keys.End:
				newIndex = int.MaxValue;
				break;
			case Keys.PageUp:
				newIndex = curIndex == _firstIndex ? _firstIndex - _pageSize : _firstIndex;
				break;
			case Keys.PageDown:
				newIndex = curIndex == _lastIndex ? _lastIndex + _pageSize : _lastIndex;
				break;
			case Keys.Space:
			case Keys.Insert:
				_anchorIndex = curIndex;
				SelectIndex(curIndex);
				newIndex++;
				break;
			case Keys.Return:
				ItemActivate?.Invoke(this, EventArgs.Empty);
				return;
			case Keys.Back:
				FilePanel?.NavigateParent();
				return;
			case Keys.OemBackslash or Keys.Oem5:
				FilePanel?.NavigateRoot();
				return;
			case Keys.Apps when ItemClicked != null:
				// 메뉴키는 마우스 오른쪽 누름을 에뮬레이션
				if (curIndex >= 0 && curIndex < Items.Count)
				{
					var rect = GetIndexedItemRect(curIndex);
					ItemClicked.Invoke(this,
						new FileListClickEventArgs(curIndex, MouseButtons.Right, rect.Location));
				}
				return;
		}

		newIndex = Math.Clamp(newIndex, 0, Items.Count - 1);
		if (newIndex != curIndex)
		{
			if ((e.Modifiers & Keys.Shift) == Keys.Shift)
			{
				if (_anchorIndex == -1)
					_anchorIndex = curIndex;
				SelectRange(_anchorIndex, newIndex);
			}

			EnsureFocus(newIndex);
		}
	}

	/// <summary>
	/// 컨트롤을 다시 그립니다.
	/// </summary>
	public new void Invalidate()
	{
		if (_updating)
			return;
		base.Invalidate();
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

		_updating = false;
		_anchorIndex = -1;
		EnsureIndex(FocusedIndex = Items.Count > 0 ? 0 : -1);

		_needRefresh = true;
		Invalidate();
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
		}
		_widths.ResetName();
		_needRefresh = true;
		Invalidate();
	}

	/// <summary>
	/// 파일 항목을 추가합니다.
	/// </summary>
	public void AddFile(FileInfo fileInfo) =>
		AddItem(new FileListFileItem(fileInfo));

	/// <summary>
	/// 폴더 항목을 추가합니다.
	/// </summary>
	public void AddFolder(DirectoryInfo dirInfo) =>
		AddItem(new FileListFolderItem(dirInfo));

	/// <summary>
	/// 상위 폴더 항목을 추가합니다.
	/// </summary>
	public void AddParentFolder(DirectoryInfo dirInfo) =>
		AddItem(new FileListFolderItem(dirInfo, true));

	/// <summary>
	/// 드라이브 항목을 추가합니다.
	/// </summary>
	public void AddDrive(DriveInfo driveInfo) =>
		AddItem(new FileListDriveItem(driveInfo));

	// 항목을 리스트에 추가합니다.
	private void AddItem(FileListItem item)
	{
		Items.Add(item);
		if (!_updating)
		{
			_needRefresh = true;
			Invalidate();
		}
	}

	/// <summary>
	/// 항목을 리스트에 추가합니다.
	/// </summary>
	/// <param name="name"></param>
	/// <param name="moveFocus"></param>
	public void AddItem(string name, bool moveFocus = false)
	{
		FileListItem item;
		if (File.Exists(name))
			item = new FileListFileItem(new FileInfo(name));
		else if (Directory.Exists(name))
			item = new FileListFolderItem(new DirectoryInfo(name));
		else
			return; // 파일이나 폴더가 아닌 경우 추가하지 않음

		AddItem(item);

		if (moveFocus)
			FocusedIndex = FindIndexByName(name);
	}

	/// <summary>
	/// 항목을 삭제합니다.
	/// </summary>
	/// <param name="name"></param>
	public void DeleteItem(string name)
	{
		var index = FindIndexByName(name);
		var item = GetItem(index);
		if (item == null)
			return;

		Items.RemoveAt(index);

		_needRefresh = true;
		Invalidate();
	}

	/// <summary>
	/// 아이템 이름 바꾸기
	/// </summary>
	/// <param name="oldName"></param>
	/// <param name="newName"></param>
	public void RenameItem(string oldName, string newName)
	{
		var index = FindIndexByName(oldName);
		var item = GetItem(index);
		if (item == null)
			return;

		switch (item)
		{
			case FileListFileItem fi:
				fi.Renew(new FileInfo(newName));
				break;
			case FileListFolderItem di:
				di.Renew(new DirectoryInfo(newName));
				break;
			default:
				return; // 다른 타입은 지원하지 않음
		}

		_needRefresh = true;
		Invalidate();
	}

	/// <summary>
	/// 아이템 갱신
	/// </summary>
	/// <param name="name"></param>
	public void RefreshItem(string name)
	{
		var index = FindIndexByName(name);
		var item = GetItem(index);
		if (item == null)
			return;

		switch (item)
		{
			case FileListFileItem fi:
				fi.Renew(new FileInfo(name));
				break;
			case FileListFolderItem di:
				di.Renew(new DirectoryInfo(name));
				break;
			default:
				return; // 다른 타입은 지원하지 않음
		}

		_needRefresh = true;
		Invalidate();
	}

	/// <summary>
	/// 지정한 인덱스의 <see cref="FileListItem"/>을 반환합니다.
	/// </summary>
	/// <param name="index">가져올 항목의 0부터 시작하는 인덱스입니다.</param>
	/// <returns>해당 인덱스의 <see cref="FileListItem"/> 또는 범위를 벗어나면 <see langword="null"/>을 반환합니다.</returns>
	public FileListItem? GetItem(int index)
	{
		if (index < 0 || index >= Items.Count)
			return null;
		return Items[index];
	}

	/// <summary>
	/// 지정한 좌표에 위치한 항목의 인덱스를 찾습니다.
	/// </summary>
	/// <param name="x">확인할 x 좌표입니다.</param>
	/// <param name="y">확인할 y 좌표입니다.</param>
	/// <returns>해당 좌표에 위치한 항목의 인덱스, 없으면 -1을 반환합니다.</returns>
	public int FindIndexAt(int x, int y)
	{
		var first = _firstIndex;
		var count = _lastIndex - first + 1;
		for (var i = 0; i < count; i++)
		{
			var rect = GetItemRect(i);
			if (rect.Contains(x, y))
				return first + i;
		}
		return -1;
	}

	/// <summary>
	/// 지정한 이름을 가진 첫 번째 항목의 인덱스를 반환합니다.
	/// </summary>
	/// <param name="name">찾을 항목의 전체 이름입니다.</param>
	/// <returns>해당 이름을 가진 첫 번째 항목의 인덱스, 없으면 -1을 반환합니다.</returns>
	public int FindIndexByName(string name) =>
		Items.FindIndex(item => item.FullName == name);

	// 인덱스에 해당하는 페이지로 스크롤을 맞춥니다.
	private void EnsureIndex(int index)
	{
		if (index < 0 || index >= Items.Count)
			return;
		var page = GetPageNo(index);
		if (page != _currentPage)
			SetPage(page);
	}

	/// <summary>
	/// 지정한 인덱스의 항목에 포커스를 맞추고 필요시 스크롤을 조정합니다.
	/// </summary>
	/// <param name="index">포커스할 항목의 인덱스입니다.</param>
	public void EnsureFocus(int index)
	{
		if (index < 0 || index >= Items.Count)
			return;
		EnsureIndex(index);
		FocusedIndex = index;
	}

	/// <summary>
	/// 지정한 이름의 항목에 포커스를 맞춥니다.
	/// </summary>
	/// <param name="name">포커스할 항목의 이름입니다.</param>
	public void EnsureFocus(string? name)
	{
		if (string.IsNullOrEmpty(name) || Items.Count == 0)
			return;
		EnsureFocus(FindIndexByName(name));
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

	// 단일 항목 선택/해제
	private void SelectIndex(int index)
	{
		var item = Items[index];
		if (item is FileListFileItem or FileListFolderItem { IsParent: false })
		{
			item.Selected = !item.Selected;
			SelectionChanged?.Invoke(this, EventArgs.Empty);
			Invalidate();
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

	// 스크롤바가 보이는지 여부
	private bool IsScrollBarVisible => _contentHeight > _viewHeight;

	// 컬럼 수 설정
	private void SetColumns(int column)
	{
		_columns = Math.Clamp(column, 1, 4);
		RefreshLayout();
	}

	// 페이지 번호 설정
	private void SetPage(int page)
	{
		if (page >= 0 && page < _pageCount)
			_currentPage = page;
	}

	// 페이지 이동
	private void SetPageDelta(int delta)
	{
		var newPage = Math.Clamp(_currentPage + delta, 0, _pageCount - 1);
		_currentPage = newPage;
	}

	// 인덱스 준비하기
	private void PrepareIndex()
	{
		_firstIndex = _currentPage * _pageSize;
		_lastIndex = Math.Min(_firstIndex + _pageSize - 1, Items.Count - 1);
	}

	// 레이아웃 및 페이지 정보 갱신
	private void RefreshLayout()
	{
		var size = Size;
		var fontHeight = Font.Height;
		var count = Items.Count;

		_rows = (count + _columns - 1) / _columns;
		_viewHeight = size.Height - SbSize;

		_itemHeight = fontHeight + 4;
		_itemWidth = (size.Width - 4) / _columns;

		_viewRows = Math.Max(1, _viewHeight / _itemHeight);
		_contentHeight = _rows * _itemHeight;

		_pageSize = _viewRows * _columns;
		_pageCount = (count + _pageSize - 1) / _pageSize;

		if (!IsScrollBarVisible)
			_sbBound = Rectangle.Empty;
		else
		{
			_sbBound = new Rectangle(0, size.Height - SbSize, size.Width, SbSize);
			_sbTrackRange = new MinMax(SbSize, size.Width - SbSize);
			_sbIndWidth = Math.Max(SbMinIndWidth, _sbTrackRange.Length / Math.Max(1, _pageCount));
		}
	}

	// 인덱스으로부터 (row, col) 계산
	private (int row, int col) GetRowCol(int index) =>
		(index % _viewRows, index / _viewRows);

	// 인덱스에 해당하는 페이지 번호 반환
	private int GetPageNo(int index) =>
		index / _pageSize;

	// 페이지 내 인덱스에 해당하는 사각형 반환
	private Rectangle GetItemRect(int indexOfPage)
	{
		var (row, col) = GetRowCol(indexOfPage);
		return new Rectangle(col * _itemWidth, row * _itemHeight, _itemWidth, _itemHeight);
	}

	// 전체 인덱스에 해당하는 사각형 반환
	private Rectangle GetIndexedItemRect(int index)
	{
		var (row, col) = GetRowCol(index - GetPageNo(index) * _pageSize);
		return new Rectangle(col * _itemWidth, row * _itemHeight, _itemWidth, _itemHeight);
	}

	// 스크롤바 인디케이터 위치 계산
	private void CalcScrollBarIndicator()
	{
		if (!IsScrollBarVisible)
			return;
		var width = _sbTrackRange.Length;
		var pageCount = Math.Max(1, _pageCount);
		var curPage = Math.Clamp(_currentPage, 0, pageCount - 1);
		var indPos = (int)((float)curPage / (pageCount - 1) * (width - _sbIndWidth));
		if (pageCount == 1 || indPos < 0)
			indPos = 0;
		if (indPos + _sbIndWidth > width)
			indPos = width - _sbIndWidth;
		var left = indPos + _sbTrackRange.Left;
		var right = left + _sbIndWidth;
		_sbIndRange = new MinMax(left, right);
	}

	/// <summary>
	/// Sorts the collection based on the current settings.
	/// </summary>
	/// <remarks>This method sorts the collection using the sort order and direction specified in the application's
	/// settings. It retrieves the sort configuration from a singleton instance of the <c>Settings</c> class.</remarks>
	public void Sort()
	{
		var settings = Settings.Instance;
		Sort(settings.SortOrder, settings.SortDescending);
	}

	/// <summary>
	/// Sorts the items in the list based on the specified order and direction.
	/// </summary>
	/// <remarks>The method sorts different types of items (folders, files, drives) using specific criteria. Folders
	/// are prioritized to appear at the top if they are parent folders. Drives are always sorted by name.</remarks>
	/// <param name="order">Determines the sorting criteria: 0 for name, 1 for extension, 2 for size, 3 for date, and 4 for attributes.</param>
	/// <param name="desc">If <see langword="true"/>, sorts the items in descending order; otherwise, sorts in ascending order.</param>
	public void Sort(int order, bool desc)
	{
		if (Items.Count == 0)
			return;

		var sign = desc ? -1 : 1;
		Items.Sort((l, r) =>
		{
			var typeA = DetemineItemType(l);
			var typeB = DetemineItemType(r);
			if (typeA != typeB)
				return typeA.CompareTo(typeB);

			int cmp;
			switch (typeA)
			{
				case 0 when typeB == 0:
				{
					var a = (FileListFolderItem)l;
					var b = (FileListFolderItem)r;

					// 부모 폴더는 항상 맨 위에 오도록 정렬
					if (a.IsParent || b.IsParent)
					{
						switch (a.IsParent)
						{
							case true when !b.IsParent:
								return -1;
							case false when b.IsParent:
								return 1;
						}
					}

					if (order != 3)
						cmp = Alter.CompareNatualFilename(a.DirName, b.DirName);
					else
					{
						cmp = a.LastWrite.CompareTo(b.LastWrite);
						if (cmp == 0)
							cmp = Alter.CompareNatualFilename(a.DirName, b.DirName);
					}

					break;
				}
				case 2 when typeB == 2:
				{
					var a = (FileListDriveItem)l;
					var b = (FileListDriveItem)r;

					// 드라이브는 정렬 방법과 상관없이 항상 이름으로 정렬
					return string.Compare(a.Letter, b.Letter, StringComparison.OrdinalIgnoreCase);
				}
				case 1 when typeB == 1:
				{
					var a = (FileListFileItem)l;
					var b = (FileListFileItem)r;
					switch (order)
					{
						case 0: // 이름 -> 확장자
							cmp = Alter.CompareNatualFilename(a.FileName, b.FileName);
							if (cmp == 0)
								cmp = Alter.CompareNatualFilename(a.Extension, b.Extension);
							break;
						case 1: // 확장자 -> 이름
							cmp = Alter.CompareNatualFilename(a.Extension, b.Extension);
							if (cmp == 0)
								cmp = Alter.CompareNatualFilename(a.FileName, b.FileName);
							break;
						case 2: // 파일 크기 -> 이름 -> 확장자
							cmp = a.Size.CompareTo(b.Size);
							if (cmp == 0)
							{
								cmp = Alter.CompareNatualFilename(a.FileName, b.FileName);
								if (cmp == 0)
									cmp = Alter.CompareNatualFilename(a.Extension, b.Extension);
							}
							break;
						case 3: // 날짜 -> 이름 -> 확장자
							cmp = a.LastWrite.CompareTo(b.LastWrite);
							if (cmp == 0)
							{
								cmp = Alter.CompareNatualFilename(a.FileName, b.FileName);
								if (cmp == 0)
									cmp = Alter.CompareNatualFilename(a.Extension, b.Extension);
							}
							break;
						case 4: // 속성 -> 이름 -> 확장자
							cmp = a.Attributes.CompareTo(b.Attributes);
							if (cmp == 0)
							{
								cmp = Alter.CompareNatualFilename(a.FileName, b.FileName);
								if (cmp == 0)
									cmp = Alter.CompareNatualFilename(a.Extension, b.Extension);
							}
							break;
						default:
							cmp = Alter.CompareNatualFilename(a.FileName, b.FileName);
							break;
					}
					break;
				}
				default:
					cmp = Alter.CompareNatualFilename(l.FullName, r.FullName);
					break;
			}

			return cmp * sign;
		});
		return;

		int DetemineItemType(FileListItem item) => item switch
		{
			FileListFolderItem => 0,
			FileListFileItem => 1,
			FileListDriveItem => 2,
			_ => 3 // 기타
		};
	}
}

/// <summary>
/// 파일 리스트의 항목 너비와 기타 정보를 저장하는 클래스입니다.
/// </summary>
internal class FileListWidths
{
	/// <summary>파일/디렉터리 이름 컬럼 너비</summary>
	public int Name { get; set; }
	/// <summary>확장자 컬럼 너비</summary>
	public int Extension { get; set; }
	/// <summary>드라이브 이름 너비</summary>
	public int DriveName { get; set; }
	/// <summary>드라이브 그래프 너비</summary>
	public int DriveGraph { get; set; }

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
	/// <summary>전체 고정 컬럼 표시 여부</summary>
	public bool IsFixedVisible { get; set; }

	/// <summary>드라이브 정보</summary>
	public int DriveInfo { get; set; }
	/// <summary>드라이브 정보 표시 여부</summary>
	public bool IsDriveInfoVisible { get; set; }

	// 드라이브 그래프 너비
	private readonly MinMax rangeOfDriveGraph = new(40, 90);

	/// <summary>
	/// 폰트 기준으로 고정 컬럼 너비를 계산합니다.
	/// </summary>
	public void UpdateFixed(Font font)
	{
		MinExtension = TextRenderer.MeasureText("9W9", font).Width + 2;
		Size = TextRenderer.MeasureText("999.99 WB", font).Width + 4;
		Date = TextRenderer.MeasureText("9999-99-99", font).Width + 4;
		Time = TextRenderer.MeasureText("99:99", font).Width + 4;
		Attr = TextRenderer.MeasureText("ARHS", font).Width + 2;

		DriveInfo = TextRenderer.MeasureText("999.99 WB 남음", font).Width + 2;
	}

	/// <summary>
	/// 항목 리스트 기준으로 이름/확장자 너비를 계산합니다.
	/// </summary>
	public void UpdateName(List<FileListItem> items, Font font)
	{
		Name = 0;
		Extension = 0;
		DriveName = 0;

		foreach (var item in items)
		{
			int len;
			switch (item)
			{
				case FileListFileItem fileItem:
				{
					len = TextRenderer.MeasureText(fileItem.FileName, font).Width + 2;
					if (len > Name) Name = len;
					len = TextRenderer.MeasureText(fileItem.Extension, font).Width + 2;
					if (len > Extension) Extension = len;
					break;
				}
				case FileListDriveItem driveItem:
				{
					len = TextRenderer.MeasureText(driveItem.DisplayName, font).Width + 2;
					if (len > DriveName) DriveName = len;
					break;
				}
			}
		}
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
	/// <remarks>자동 열 너비를 구현하기 위해 <see cref="UpdateName"/>과 별도로 호출합니다.</remarks>
	public int AdjustName(int width, int columns)
	{
		var fixedSize = Date + Time + Attr;

		width -= FileListDrawProp.LeadingWidth; // 선두 너비 제외해야 한다!

		// 파일 이름 처리
		IsFixedVisible = columns == 1 && fixedSize <= width * 0.4f;
		if (Name + Extension > width * 0.8f)
		{
			// 이름과 확장자 합이 너무 크면 고정 컬럼을 표시하지 않음
			IsFixedVisible = false;
		}

		var remainWidth = (IsFixedVisible ? width - fixedSize : width) - Size;
		if (Name + Extension == 0)
		{
			// 이름과 확장이가 모두 0인 경우
			Name = remainWidth;
		}
		else if (Name == Extension)
		{
			Name = (int)(remainWidth * 0.5f);
			Extension = remainWidth - Name;
		}
		else if (Name < Extension)
		{
			if (Name == 0)
			{
				// 가끔 있다. 파일 이름이 없거나 비어있는 경우.
				Extension = remainWidth;
			}
			else if (Extension < MinExtension)
			{
				Name = remainWidth - MinExtension;
				Extension = MinExtension;
			}
			else
			{
				Name = (int)(remainWidth * 0.4f);
				Extension = remainWidth - Name;
			}
		}
		else
		{
			if (Extension == 0)
			{
				// 가끔 있다. 확장자가 없는 경우.
				Name = remainWidth;
			}
			else if (Extension < MinExtension)
			{
				Name = remainWidth - MinExtension;
				Extension = MinExtension;
			}
			else
			{
				Name = (int)(remainWidth * (float)Name / (Name + Extension));
				Extension = remainWidth - Name;
				if (Extension < MinExtension)
				{
					Name = remainWidth - MinExtension;
					Extension = MinExtension;
				}
			}
		}

		// 드라이브 처리
		DriveGraph = columns == 1 ? rangeOfDriveGraph.Max : rangeOfDriveGraph.Min;
		var drvWidth = (columns == 1 ? width - DriveInfo - DriveGraph : width - DriveGraph) + 2;
		if (DriveName > drvWidth)
		{
			// 드라이브 이름이 더 길면 드라이브 너비를 조정
			DriveName = drvWidth;
		}

		// 자동 컬럼을 해야하는데 귀찮아서 일단 패스
		return columns;
	}
}

// 파일 리스트 아이템을 그리기 속성 정의 클랫
internal class FileListDrawProp(Font font, Theme theme, bool isActive)
{
	// 선택 마크(화살표) 크기
	public const int MarkSize = 8;
	// 아이콘 크기
	public const int IconSize = 16;
	// 아이콘 마진
	public const int IconMargin = 4;
	// 아이템의 선두 너비
	public const int LeadingWidth = MarkSize + IconSize + IconMargin;

	// 그려질 영역의 왼쪽
	public int Left { get; private set; }
	// 그려질 영역의 윗쪽	 
	public int Top { get; private set; }
	// 그려질 영역의 너비
	public int Width { get; private set; }
	// 그려질 영역의 높이
	public int Height { get; private set; }
	// 기본 너비
	public int BaseWidth { get; private set; }
	// 포커스 여부
	public bool Focused { get; private set; }

	// 글꼴
	public Font Font { get; } = font;
	// 테마
	public Theme Theme { get; } = theme;
	// 활성 상태
	public bool IsActive { get; } = isActive;

	// 그려질 영역
	public Rectangle Bound => new(Left, Top, Width, Height);

	// 마크가 그려질 영역
	public Rectangle MarkRect
	{
		get
		{
			var x = Left + 2;
			var y = Top + (Height - MarkSize) / 2;
			return new Rectangle(x, y, MarkSize, MarkSize);
		}
	}

	// 아이콘을 그릴 위치
	public Rectangle IconRect
	{
		get
		{
			var x = Left + MarkSize + IconMargin / 2;
			var y = Top + (Height - IconSize) / 2;
			return new Rectangle(x, y, IconSize, IconSize);
		}
	}

	// 너비 설정
	public void SetWidth(int width) =>
		Width = width;

	// 기준점 이동
	public void Advance(int width) =>
		Left += width;

	// 기준점 최초 이동
	public void BaseAdvance() =>
		Left += LeadingWidth;

	// 재 사용
	public void Reset(Rectangle rect, bool focused)
	{
		Left = rect.Left;
		Top = rect.Top;
		BaseWidth = Width = rect.Width;
		Height = rect.Height;
		Focused = focused;
	}
}

/// <summary>
/// 파일 리스트의 항목을 나타내는 추상 클래스입니다.
/// </summary>
public abstract class FileListItem
{
	/// <summary>선택 여부</summary>
	public bool Selected { get; set; }
	/// <summary>아이콘</summary>
	public Image? Icon { get; set; }
	/// <summary>색상</summary>
	public Color Color { get; set; }

#nullable disable
	/// <summary>전체 경로</summary>
	public string FullName { get; private set; }
	/// <summary>마지막 수정일</summary>
	public DateTime LastWrite { get; private set; }
	/// <summary>파일 속성</summary>
	public FileAttributes Attributes { get; private set; }
#nullable restore

	/// <summary>
	/// 파일 리스트 아이템을 생성합니다.
	/// </summary>
	protected FileListItem()
	{
	}

	/// <summary>
	/// Updates the file metadata with the specified values.
	/// </summary>
	/// <remarks>This method updates the file's metadata properties, including its path, last modification time, 
	/// and attributes. Ensure that the provided values accurately reflect the desired state of the file.</remarks>
	/// <param name="fullName">The full path of the file, including its name and extension.</param>
	/// <param name="lastWrite">The date and time of the last modification to the file.</param>
	/// <param name="attributes">The file attributes, such as read-only or hidden, to be applied.</param>
	protected void SetFileInfomation(string fullName, DateTime lastWrite, FileAttributes attributes)
	{
		FullName = fullName;
		LastWrite = lastWrite;
		Attributes = attributes;
	}

	// 아이템을 그립니다.
	internal virtual void Draw(Graphics g, FileListDrawProp prop, FileListWidths widths)
	{
		var background =
			prop.Focused ? Color :
			Selected ? prop.Theme.BackSelection : prop.Theme.BackContent;

		if (prop.IsActive)
		{
			using var brush = new SolidBrush(background);
			g.FillRectangle(brush, prop.Bound);
		}
		else
		{
			using var brush = new SolidBrush(Color.FromArgb(30, background));
			using var pen = new Pen(Color.FromArgb(120, background));
			g.FillRectangle(brush, prop.Bound);
			g.DrawRectangle(pen, new Rectangle(prop.Left, prop.Top, prop.Width - 1, prop.Height - 1));
		}

		if (Selected)
		{
			var rect = prop.MarkRect;
			var pts = new[]
			{
				new Point(rect.Left, rect.Top),
				new Point(rect.Right, rect.Top + rect.Height / 2),
				new Point(rect.Left, rect.Bottom)
			};
			using var polyBrush = new SolidBrush(prop.Theme.Foreground);
			g.FillPolygon(polyBrush, pts);
		}

		if (Icon != null)
		{
			if (!Attributes.HasFlag(FileAttributes.Hidden))
				g.DrawImage(Icon, prop.IconRect);
			else
			{
				var m = new ColorMatrix
				{
					Matrix33 = 0.5f // 반투명하게
				};
				var attr = new ImageAttributes();
				attr.SetColorMatrix(m);
				g.DrawImage(Icon, prop.IconRect, 0, 0, Icon.Width, Icon.Height, GraphicsUnit.Pixel, attr);
			}
		}

		prop.BaseAdvance();
	}

	// 실제 표시할 텍스트를 얻습니다.
	private static string GetDisplayText(string input, FileListDrawProp prop)
	{
		var text = input;
		var width = TextRenderer.MeasureText(text, prop.Font).Width;
		if (width > prop.Width)
		{
			for (var len = text.Length - 1; len > 0; len -= 3)
			{
				var t = text[..len] + "...";
				if (TextRenderer.MeasureText(t, prop.Font).Width < prop.Width)
				{
					text = t;
					break;
				}
			}
		}

		return text;
	}

	// 강조 있는 아이템 텍스트를 그립니다.
	internal void DrawAccentText(Graphics g, FileListDrawProp prop, string text, int width, bool rightAlign = false)
	{
		prop.SetWidth(width);

		TextRenderer.DrawText(g, GetDisplayText(text, prop), prop.Font, prop.Bound,
			prop is { Focused: true, IsActive: true } ? prop.Theme.Focus : Color,
			TextFormatFlags.VerticalCenter | (rightAlign ? TextFormatFlags.Right : TextFormatFlags.Left));

		prop.Advance(width);
	}

	// 아이템 텍스트를 그립니다.
	internal void DrawText(Graphics g, FileListDrawProp prop, string text, int width, bool rightAlign = false)
	{
		prop.SetWidth(width);

		TextRenderer.DrawText(g, GetDisplayText(text, prop), prop.Font, prop.Bound,
			prop.Focused ? !prop.IsActive ? Color : prop.Theme.Focus : prop.Theme.File,
			TextFormatFlags.VerticalCenter | (rightAlign ? TextFormatFlags.Right : TextFormatFlags.Left));

		prop.Advance(width);
	}
}

/// <summary>
/// 파일 항목을 나타내는 클래스입니다.
/// </summary>
public class FileListFileItem : FileListItem
{
#nullable disable
	/// <summary>파일명</summary>
	public string FileName { get; private set; }
	/// <summary>확장자</summary>
	public string Extension { get; private set; }
	/// <summary>파일 크기</summary>
	public long Size { get; private set; }
#nullable restore

	/// <summary>
	/// 파일 항목을 생성합니다. 파일명, 확장자, 아이콘, 색상 등 정보를 제공합니다.
	/// </summary>
	/// <param name="fileInfo">이 파일 항목을 초기화하는 파일 정보입니다.</param>
	public FileListFileItem(FileInfo fileInfo) =>
		Renew(fileInfo);

	/// <summary>
	/// 파일 항목을 새로 설정합니다.
	/// </summary>
	/// <param name="fileInfo"></param>
	public void Renew(FileInfo fileInfo)
	{
		SetFileInfomation(fileInfo.FullName, fileInfo.LastWriteTime, fileInfo.Attributes);

		var name = fileInfo.Name;
		var dot = name.LastIndexOf('.');
		FileName = dot >= 0 ? name[..dot] : name;
		Extension = dot >= 0 ? name[(dot + 1)..] : string.Empty;
		Size = fileInfo.Length;

		Icon = IconCache.Instance.GetIcon(fileInfo.FullName, Extension);
		Color = Settings.Instance.Theme.GetColorExtension(Extension.ToUpperInvariant());
	}

	/// <inheritdoc/>
	internal override void Draw(Graphics g, FileListDrawProp prop, FileListWidths widths)
	{
		base.Draw(g, prop, widths);

		DrawAccentText(g, prop, FileName, widths.Name);
		DrawAccentText(g, prop, Extension, widths.Extension);
		DrawSize(g, prop, widths);

		if (widths.IsFixedVisible)
		{
			DrawText(g, prop, LastWrite.FormatRelativeDate(), widths.Date);
			DrawText(g, prop, LastWrite.ToString("HH:mm"), widths.Time);
			DrawText(g, prop, Attributes.FormatString(), widths.Attr);
		}
	}

	// 크기만 그리기
	private void DrawSize(Graphics g, FileListDrawProp prop, FileListWidths widths)
	{
		var size = Alter.FormatFileSize(Size, out var suffix);
		if (string.IsNullOrEmpty(suffix))
		{
			DrawText(g, prop, size, widths.Size, true);
			return;
		}

		var scolor = prop.Theme.GetColorSize(suffix);
		var slen = TextRenderer.MeasureText(suffix, prop.Font).Width;

		prop.SetWidth(widths.Size);
		TextRenderer.DrawText(g, suffix, prop.Font, prop.Bound,
			prop.Focused ? !prop.IsActive ? Color : prop.Theme.Focus : scolor,
			TextFormatFlags.VerticalCenter | TextFormatFlags.Right);

		prop.SetWidth(widths.Size - slen + 4);  // 4는 달라 붙는거 같아서 여백을 줄인것
		TextRenderer.DrawText(g, size, prop.Font, prop.Bound,
			prop.Focused ? !prop.IsActive ? Color : prop.Theme.Focus : prop.Theme.File,
			TextFormatFlags.VerticalCenter | TextFormatFlags.Right);

		prop.Advance(widths.Size);
	}
}

/// <summary>
/// 디렉터리 항목을 나타내는 클래스입니다.
/// </summary>
public class FileListFolderItem : FileListItem
{
#nullable disable
	/// <summary>디렉터리명</summary>
	public string DirName { get; private set; }
	/// <summary>부모 폴더 여부</summary>
	public bool IsParent { get; }
#nullable restore

	/// <summary>
	/// 폴더 항목을 생성합니다. 폴더 정보와 관련 속성에 접근할 수 있습니다.
	/// </summary>
	/// <param name="dirInfo">디렉터리 정보를 담고 있는 <see cref="DirectoryInfo"/> 객체입니다.</param>
	/// <param name="isParent">부모 폴더 여부입니다.</param>
	public FileListFolderItem(DirectoryInfo dirInfo, bool isParent = false)
	{
		Renew(dirInfo);
		IsParent = isParent;
	}

	/// <summary>
	/// 폴더 항목을 새로 설정합니다. 디렉터리명, 마지막 수정일, 속성 등을 갱신합니다.
	/// </summary>
	/// <param name="dirInfo"></param>
	public void Renew(DirectoryInfo dirInfo)
	{
		SetFileInfomation(dirInfo.FullName, dirInfo.LastWriteTime, dirInfo.Attributes);

		DirName = dirInfo.Name;

		Icon = IconCache.Instance.GetIcon(dirInfo.FullName, string.Empty, true);
		Color = Settings.Instance.Theme.Folder;
	}

	/// <inheritdoc/>
	internal override void Draw(Graphics g, FileListDrawProp prop, FileListWidths widths)
	{
		base.Draw(g, prop, widths);

		DrawAccentText(g, prop, IsParent ? ".." : DirName, widths.Name + widths.Extension);
		DrawAccentText(g, prop, "[폴더]", widths.Size, true);

		if (widths.IsFixedVisible)
		{
			DrawText(g, prop, LastWrite.FormatRelativeDate(), widths.Date);
			DrawText(g, prop, LastWrite.ToString("HH:mm"), widths.Time);
			DrawText(g, prop, Attributes.FormatString(), widths.Attr);
		}
	}
}

/// <summary>
/// 드라이브 항목을 나타내는 클래스입니다.
/// </summary>
public class FileListDriveItem : FileListItem
{
	/// <summary>드라이브 번호</summary>
	public string Letter { get; }
	/// <summary>표시 이름</summary>
	public string DisplayName { get; }
	/// <summary>드라이브명</summary>
	public string DriveName { get; }
	/// <summary>볼륨 라벨</summary>
	public string VolumeLabel { get; }
	/// <summary>전체 크기</summary>
	public long Total { get; }
	/// <summary>사용 가능 크기</summary>
	public long Available { get; }
	/// <summary>드라이브 유형</summary>
	public DriveType Type { get; }
	/// <summary>드라이브 포맷</summary>
	public string Format { get; }

	/// <summary>
	/// 드라이브 항목을 생성합니다. 드라이브명, 볼륨 라벨, 아이콘, 색상 등을 설정합니다.
	/// </summary>
	/// <remarks>드라이브명과 볼륨 라벨을 추출 및 포맷하고, 아이콘과 테마 색상을 적용합니다.</remarks>
	/// <param name="driveInfo">드라이브 정보를 초기화에 사용합니다.</param>
	public FileListDriveItem(DriveInfo driveInfo) 
	{
		SetFileInfomation(driveInfo.RootDirectory.FullName, DateTime.Now, FileAttributes.Directory);

		Letter = driveInfo.Name.TrimEnd('\\');
		DriveName = driveInfo.Name;
		VolumeLabel = driveInfo.VolumeLabel;
		DisplayName = $"{Letter} {VolumeLabel}";
		Total = driveInfo.TotalSize;
		Available = driveInfo.AvailableFreeSpace;
		Format = driveInfo.DriveFormat;
		Type = driveInfo.DriveType;

		Icon = IconCache.Instance.GetIcon(DriveName, string.Empty, false, true);
		Color = Settings.Instance.Theme.Drive;
	}

	/// <inheritdoc/>
	internal override void Draw(Graphics g, FileListDrawProp prop, FileListWidths widths)
	{
		base.Draw(g, prop, widths);

		DrawAccentText(g, prop, DisplayName, widths.DriveName);

		// 드라이브 용량 그래프 그리기
		if (Total > 0)
		{
			var width = widths.DriveGraph;
			var height = (int)(prop.Height * 0.5f);
			var top = prop.Top + (prop.Height - height) / 2;
			var left = prop.Left;

			var ratio = (Total - Available) / (float)Total;
			var used = (int)(width * ratio);

			using (var backBrush = new SolidBrush(prop.Theme.Background))
				g.FillRectangle(backBrush, left, top, used, height);

			using (var borderPen = new Pen(prop.Theme.Border))
				g.DrawRectangle(borderPen, left, top, width - 1, height - 1);

			prop.Advance(width);

			// 드라이브 정보 표시 (공간이 충분할 때만)
			if (prop.Width + widths.DriveInfo < prop.BaseWidth)
				DrawText(g, prop, $"{Available.FormatFileSize()} 남음", widths.DriveInfo, true);
		}
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
