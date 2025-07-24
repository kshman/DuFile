namespace DuFile.Windows;

/// <summary>
/// 파일/디렉터리/드라이브 항목을 표시하고 관리하는 커스텀 리스트 컨트롤입니다.
/// </summary>
public sealed class FileList : Control
{
	// 현재 뷰 모드 (롱리스트/숏리스트)
	private FileListViewMode _viewMode = FileListViewMode.LongList;
	// 숏리스트 모드의 컬럼 수
	private int _shortColumns = 1;
	// 컬럼 너비 정보
	private FileListWidths _widths;
	// 스크롤 오프셋
	private int _scrollOffset;
	// 포커스된 인덱스
	private int _focusedIndex = -1;
	// Shift 선택 기준 인덱스
	private int _anchorIndex = -1;
	// 업데이트 중 여부
	private bool _updating;

	/// <summary>리스트에 표시되는 항목 컬렉션입니다.</summary>
	[Browsable(false)]
	public List<FileListItem> Items { get; } = [];

	/// <summary>현재 디렉터리의 전체 경로입니다.</summary>
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public string FullName { get; set; } = string.Empty;

	/// <summary>리스트의 뷰 모드입니다.</summary>
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public FileListViewMode ViewMode
	{
		get => _viewMode;
		set
		{
			_viewMode = value;
			_scrollOffset = 0;
			Invalidate();
		}
	}

	/// <summary>숏리스트 모드의 컬럼 수입니다.</summary>
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int ShortListColumnCount
	{
		get => _shortColumns;
		set
		{
			_shortColumns = Math.Clamp(value, 1, 4);
			Invalidate();
		}
	}

	/// <summary>현재 포커스된 항목의 인덱스입니다.</summary>
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int FocusedIndex
	{
		get => _focusedIndex;
		set
		{
			_focusedIndex = value;
			FocusedIndexChanged?.Invoke(this,
				new FileListFocusChangedEventArgs(value >= 0 && value < Items.Count ? Items[value] : null));
			Invalidate();
		}
	}

	/// <summary>포커스 인덱스 변경 이벤트입니다.</summary>
	[Category("FileList")]
	public event EventHandler<FileListFocusChangedEventArgs>? FocusedIndexChanged;

	/// <summary>항목 더블클릭 이벤트입니다.</summary>
	[Category("FileList")]
	public event EventHandler<FileListDoubleClickEventArgs>? ItemDoubleClicked;

	/// <summary>항목 클릭 이벤트입니다.</summary>
	[Category("FileList")]
	public event EventHandler<FileListClickEventArgs>? ItemClicked;

	/// <summary>선택 항목 변경 이벤트입니다.</summary>
	[Category("FileList")]
	public event EventHandler<FileListSelectChangedEventArgs>? SelectChanged;

	// 디자인 모드 여부 확인
	private bool IsReallyDesignMode => LicenseManager.UsageMode == LicenseUsageMode.Designtime || (Site?.DesignMode ?? false);

	/// <summary>FileList 컨트롤을 초기화합니다.</summary>
	public FileList()
	{
		SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
				 ControlStyles.ResizeRedraw | ControlStyles.UserPaint | ControlStyles.Selectable, true);
		TabStop = true;
		var settings = Settings.Instance;
		var theme = settings.Theme;
		Font = new Font(settings.FileFontFamily, settings.FileFontSize);
		BackColor = theme.BackContent;
		ForeColor = theme.Foreground;
		_widths.UpdateFixed(Font);
	}

	/// <inheritdoc />
	protected override void OnCreateControl()
	{
		base.OnCreateControl();

		if (IsReallyDesignMode && Directory.Exists(@"C:\Windows"))
		{
			// 디자인 모드에서 기본 값 설정
			BeginUpdate();
			AddItem(new FileListFileItem(this, new FileInfo(@"C:\Windows\notepad.exe")));
			AddItem(new FileListFileItem(this, new FileInfo(@"C:\Windows\regedit.exe")) { Selected = true });
			AddItem(new FileListFileItem(this, new FileInfo(@"C:\Windows\win.ini")));
			AddItem(new FileListFolderItem(this, new DirectoryInfo(@"C:\Windows\assembly")));
			AddItem(new FileListFolderItem(this, new DirectoryInfo(@"C:\Windows\System32")));
			AddItem(new FileListDriveItem(this, new DriveInfo("C:")));
			EndUpdate();
		}
	}

	/// <inheritdoc />
	protected override void OnResize(EventArgs e)
	{
		base.OnResize(e);
		_widths.UpdateFixed(Font);
		Invalidate();
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
		_scrollOffset = 0;
		if (Items.Count > 0)
			FocusedIndex = 0;
		_widths.UpdateName(Items, Font);
		_updating = false;
		Invalidate(false);
	}

	/// <summary>
	/// 모든 항목을 삭제합니다.
	/// </summary>
	public void ClearItems()
	{
		Items.Clear();
		FocusedIndex = -1;
		_anchorIndex = -1;
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
			_widths.UpdateName(item, Font);
			Invalidate();
		}
	}

	/// <summary>
	/// DisplayName이 name과 같은 항목에 포커스를 주고, 해당 위치가 보이도록 스크롤합니다.
	/// </summary>
	public void SelectName(string? name)
	{
		if (string.IsNullOrEmpty(name) || Items.Count == 0)
			return;

		var idx = Items.FindIndex(item => item.FullName == name);
		if (idx < 0)
			return;

		FocusedIndex = idx;

		// 스크롤 오프셋 조정
		if (_viewMode == FileListViewMode.LongList)
		{
			var itemHeight = Font.Height + 6;
			var topIndex = _scrollOffset / itemHeight;
			var bottomIndex = (_scrollOffset + Height) / itemHeight - 1;
			if (idx < topIndex)
				_scrollOffset = idx * itemHeight;
			else if (idx > bottomIndex)
				_scrollOffset = Math.Min((idx + 1) * itemHeight - Height, Math.Max(0, Items.Count * itemHeight - Height));
		}
		else // ShortList
		{
			var itemHeight = Font.Height + 6;
			var row = idx / _shortColumns;
			var topRow = _scrollOffset / itemHeight;
			var bottomRow = (_scrollOffset + Height) / itemHeight - 1;
			if (row < topRow)
				_scrollOffset = row * itemHeight;
			else if (row > bottomRow)
			{
				var totalRows = (Items.Count + _shortColumns - 1) / _shortColumns;
				_scrollOffset = Math.Min((row + 1) * itemHeight - Height, Math.Max(0, totalRows * itemHeight - Height));
			}
		}
		Invalidate();
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

	/// <inheritdoc />
	protected override void OnPaint(PaintEventArgs e)
	{
		if (_updating)
			return;

		base.OnPaint(e);

		_widths.AdjustName(Width);

		var g = e.Graphics;
		var theme = Settings.Instance.Theme;
		g.Clear(theme.BackContent);

		if (_viewMode == FileListViewMode.LongList)
			DrawLongList(g, theme);
		else
			DrawShortList(g, theme);
	}

	// 롱리스트 모드 그리기
	private void DrawLongList(Graphics g, Theme theme)
	{
		var itemHeight = Font.Height + 6;
		var y = -_scrollOffset;
		for (var i = 0; i < Items.Count; i++)
		{
			var item = Items[i];
			var rect = new Rectangle(0, y, Width, itemHeight);
			if (rect.Bottom < 0)
			{
				y += itemHeight;
				continue;
			}

			if (rect.Top > Height)
				break;

			item.DrawDetails(g, Font, rect, _widths, theme, i == FocusedIndex);
			y += itemHeight;
		}
	}

	// 숏리스트 모드 그리기
	private void DrawShortList(Graphics g, Theme theme)
	{
		var itemHeight = Font.Height + 6;
		var itemWidth = (Width - 4) / _shortColumns;
		var xOffset = -_scrollOffset;
		for (var i = 0; i < Items.Count; i++)
		{
			var item = Items[i];
			var col = i % _shortColumns;
			var row = i / _shortColumns;
			var x = xOffset + col * itemWidth;
			var top = row * itemHeight;
			var rect = new Rectangle(x, top, itemWidth, itemHeight);
			if (rect.Right < 0 || rect.Left > Width) continue;
			if (rect.Bottom < 0 || rect.Top > Height) continue;

			item.DrawShort(g, Font, rect, theme, i == FocusedIndex);
		}

		DrawShortListScrollBar(g, theme);
	}

	// 숏리스트 스크롤바 그리기
	private void DrawShortListScrollBar(Graphics g, Theme theme)
	{
		var totalCols = (Items.Count + _shortColumns - 1) / _shortColumns;
		var totalWidth = totalCols * ((Width - 4) / _shortColumns);
		if (totalWidth <= Width)
			return;

		var barHeight = 12;
		var barRect = new Rectangle(0, Height - barHeight, Width, barHeight);
		g.FillRectangle(new SolidBrush(theme.BackContent), barRect);

		var ratio = (float)Width / totalWidth;
		var indicatorWidth = (int)(Width * ratio);
		var indicatorX = (int)((float)_scrollOffset / (totalWidth - Width) * (Width - indicatorWidth));
		var indicatorRect = new Rectangle(indicatorX, barRect.Top, indicatorWidth, barHeight);
		g.FillRectangle(new SolidBrush(theme.BackSelection), indicatorRect);

		var leftBtn = new Rectangle(0, barRect.Top, barHeight, barHeight);
		var rightBtn = new Rectangle(Width - barHeight, barRect.Top, barHeight, barHeight);
		g.FillRectangle(new SolidBrush(theme.BackSelection), leftBtn);
		g.FillRectangle(new SolidBrush(theme.BackSelection), rightBtn);

		DrawArrow(g, leftBtn, false, theme);
		DrawArrow(g, rightBtn, true, theme);
	}

	// 스크롤바 화살표 그리기
	private static void DrawArrow(Graphics g, Rectangle rect, bool right, Theme theme)
	{
		Point[] pts;
		if (right)
			pts =
			[
				new Point(rect.Left + 4, rect.Top + 3), new Point(rect.Right - 4, rect.Top + rect.Height / 2),
				new Point(rect.Left + 4, rect.Bottom - 3)
			];
		else
			pts =
			[
				new Point(rect.Right - 4, rect.Top + 3), new Point(rect.Left + 4, rect.Top + rect.Height / 2),
				new Point(rect.Right - 4, rect.Bottom - 3)
			];
		g.FillPolygon(new SolidBrush(theme.BackContent), pts);
	}

	/// <inheritdoc />
	protected override void OnMouseDown(MouseEventArgs e)
	{
		base.OnMouseDown(e);
		Focus();

		var idx = HitTest(e.Location);
		if (idx < 0)
			return;

		if (e.Button == MouseButtons.Left)
		{
			if ((ModifierKeys & Keys.Shift) == Keys.Shift)
			{
				if (_anchorIndex == -1)
					_anchorIndex = FocusedIndex;
				SelectRange(_anchorIndex, idx);
			}
			else if ((ModifierKeys & Keys.Control) == Keys.Control)
			{
				var item = Items[idx];
				if (item is FileListFileItem or FileListFolderItem { IsParent: false })
				{
					item.Selected = !item.Selected;
					SelectChanged?.Invoke(this, new FileListSelectChangedEventArgs(this));
				}
				_anchorIndex = idx;
			}
			else
			{
				_anchorIndex = idx;
			}

			FocusedIndex = idx;
			Invalidate();
		}

		ItemClicked?.Invoke(this, new FileListClickEventArgs(this, e.Button, e.Location));
	}

	/// <inheritdoc />
	protected override void OnMouseDoubleClick(MouseEventArgs e)
	{
		base.OnMouseDoubleClick(e);

		// OnMouseDown에서 FocusedIndex가 설정되므로 그냥 ㄱㄱ
		if (e.Button == MouseButtons.Left && FocusedIndex >= 0 && FocusedIndex < Items.Count)
		{
			var item = Items[FocusedIndex];
			ItemDoubleClicked?.Invoke(this, new FileListDoubleClickEventArgs(item, FullName));
		}
	}

	/// <inheritdoc />
	protected override void OnMouseWheel(MouseEventArgs e)
	{
		base.OnMouseWheel(e);

		if (_viewMode == FileListViewMode.LongList)
		{
			var itemHeight = Font.Height + 6;
			var maxOffset = Math.Max(0, Items.Count * itemHeight - Height);
			_scrollOffset -= e.Delta / 120 * itemHeight;
			if (_scrollOffset < 0) _scrollOffset = 0;
			if (_scrollOffset > maxOffset) _scrollOffset = maxOffset;
		}
		else
		{
			var itemWidth = (Width - 4) / _shortColumns;
			var totalCols = (Items.Count + _shortColumns - 1) / _shortColumns;
			var totalWidth = totalCols * itemWidth;
			var maxOffset = Math.Max(0, totalWidth - Width);
			_scrollOffset -= e.Delta / 120 * itemWidth;
			if (_scrollOffset < 0) _scrollOffset = 0;
			if (_scrollOffset > maxOffset) _scrollOffset = maxOffset;
		}

		Invalidate();
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
		var itemHeight = Font.Height + 6;
		var visibleRows = Math.Max(1, Height / itemHeight);
		var prevIndex = FocusedIndex;
		var newIndex = prevIndex;

		// 키보드 이동 처리
		switch (e.KeyCode)
		{
			case Keys.Up:
				switch (_viewMode)
				{
					case FileListViewMode.LongList when FocusedIndex > 0:
						newIndex--;
						break;
					case FileListViewMode.ShortList when FocusedIndex - _shortColumns >= 0:
						newIndex -= _shortColumns;
						break;
				}
				break;
			case Keys.Down:
				switch (_viewMode)
				{
					case FileListViewMode.LongList when FocusedIndex < Items.Count - 1:
						newIndex++;
						break;
					case FileListViewMode.ShortList when FocusedIndex + _shortColumns < Items.Count:
						newIndex += _shortColumns;
						break;
				}
				break;
			case Keys.Left when _viewMode == FileListViewMode.ShortList && FocusedIndex > 0:
				newIndex--;
				break;
			case Keys.Right when _viewMode == FileListViewMode.ShortList && FocusedIndex < Items.Count - 1:
				newIndex++;
				break;
			case Keys.Home:
				newIndex = 0;
				_scrollOffset = 0;
				break;
			case Keys.End:
				newIndex = Items.Count - 1;
				if (_viewMode == FileListViewMode.LongList)
				{
					_scrollOffset = Math.Max(0, Items.Count * itemHeight - Height);
				}
				else // ShortList
				{
					var totalRows = (Items.Count + _shortColumns - 1) / _shortColumns;
					_scrollOffset = Math.Max(0, totalRows * itemHeight - Height);
				}
				break;
			case Keys.PageUp:
			{
				if (_viewMode == FileListViewMode.LongList)
				{
					var firstVisibleIndex = _scrollOffset / itemHeight;
					if (FocusedIndex > firstVisibleIndex)
						newIndex = firstVisibleIndex;
					else
					{
						_scrollOffset = Math.Max(0, _scrollOffset - visibleRows * itemHeight);
						// itemHeight 배수로 보정
						_scrollOffset = (_scrollOffset / itemHeight) * itemHeight;
						newIndex = _scrollOffset / itemHeight;
					}
				}
				else // ShortList
				{
					var firstVisibleRow = _scrollOffset / itemHeight;
					var firstVisibleIndex = firstVisibleRow * _shortColumns + (FocusedIndex % _shortColumns);
					if (FocusedIndex > firstVisibleIndex)
						newIndex = firstVisibleIndex;
					else
					{
						_scrollOffset = Math.Max(0, _scrollOffset - visibleRows * itemHeight);
						// itemHeight 배수로 보정
						_scrollOffset = (_scrollOffset / itemHeight) * itemHeight;
						firstVisibleRow = _scrollOffset / itemHeight;
						newIndex = firstVisibleRow * _shortColumns + (FocusedIndex % _shortColumns);
						if (newIndex >= Items.Count)
							newIndex = Items.Count - 1;
					}
				}
				break;
			}
			case Keys.PageDown:
			{
				if (_viewMode == FileListViewMode.LongList)
				{
					var lastVisibleIndex = Math.Min(Items.Count - 1, (_scrollOffset + Height) / itemHeight - 1);
					if (FocusedIndex >= lastVisibleIndex)
					{
						var maxOffset = Math.Max(0, Items.Count * itemHeight - Height);
						_scrollOffset = Math.Min(maxOffset, _scrollOffset + visibleRows * itemHeight);
						lastVisibleIndex = Math.Min(Items.Count - 1, (_scrollOffset + Height) / itemHeight - 1);
					}
					newIndex = lastVisibleIndex;
				}
				else // ShortList
				{
					var lastVisibleRow = Math.Min(((_scrollOffset + Height) / itemHeight) - 1, (Items.Count - 1) / _shortColumns);
					var lastVisibleIndex = lastVisibleRow * _shortColumns + (FocusedIndex % _shortColumns);
					if (lastVisibleIndex >= Items.Count)
						lastVisibleIndex = Items.Count - 1;
					if (FocusedIndex < lastVisibleIndex)
						newIndex = lastVisibleIndex;
					else
					{
						var totalRows = (Items.Count + _shortColumns - 1) / _shortColumns;
						var maxOffset = Math.Max(0, totalRows * itemHeight - Height);
						_scrollOffset = Math.Min(maxOffset, _scrollOffset + visibleRows * itemHeight);
						lastVisibleRow = Math.Min(((_scrollOffset + Height) / itemHeight) - 1, (Items.Count - 1) / _shortColumns);
						newIndex = lastVisibleRow * _shortColumns + (FocusedIndex % _shortColumns);
						if (newIndex >= Items.Count)
							newIndex = Items.Count - 1;
					}
				}
				break;
			}
			case Keys.Space:
			case Keys.Insert:
			{
				var item = Items[FocusedIndex];
				if (item is FileListFileItem or FileListFolderItem { IsParent: false })
				{
					item.Selected = !item.Selected;
					SelectChanged?.Invoke(this, new FileListSelectChangedEventArgs(this));
				}
				_anchorIndex = FocusedIndex;
				if (FocusedIndex < Items.Count - 1)
					FocusedIndex++;
				Invalidate();
				return;
			}
			case Keys.Return:
			{
				var item = Items[FocusedIndex];
				ItemDoubleClicked?.Invoke(this, new FileListDoubleClickEventArgs(item, FullName));
				return;
			}
			case Keys.Back when Items.Count > 0:
			{
				if (Items[0] is FileListFolderItem { DirName: ".." } item)
					ItemDoubleClicked?.Invoke(this, new FileListDoubleClickEventArgs(item, FullName));
				return;
			}
		}

		// Arrow key scrollOffset 보정
		if (newIndex != prevIndex)
		{
			if (_viewMode == FileListViewMode.LongList)
			{
				var topIndex = _scrollOffset / itemHeight;
				var bottomIndex = (_scrollOffset + Height) / itemHeight - 1;
				if (newIndex < topIndex)
					_scrollOffset = newIndex * itemHeight;
				else if (newIndex > bottomIndex)
					_scrollOffset = Math.Min((newIndex + 1) * itemHeight - Height, Math.Max(0, Items.Count * itemHeight - Height));
			}
			else // ShortList
			{
				var row = newIndex / _shortColumns;
				var topRow = _scrollOffset / itemHeight;
				var bottomRow = (_scrollOffset + Height) / itemHeight - 1;
				if (row < topRow)
					_scrollOffset = row * itemHeight;
				else if (row > bottomRow)
				{
					var totalRows = (Items.Count + _shortColumns - 1) / _shortColumns;
					_scrollOffset = Math.Min((row + 1) * itemHeight - Height, Math.Max(0, totalRows * itemHeight - Height));
				}
			}
		}

		if (newIndex != prevIndex)
		{
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

	// from~to 범위 선택
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
		SelectChanged?.Invoke(this, new FileListSelectChangedEventArgs(this));
		Invalidate();
	}

	// 마우스 위치에 해당하는 인덱스 반환
	private int HitTest(Point pt)
	{
		if (_viewMode == FileListViewMode.LongList)
		{
			var itemHeight = Font.Height + 6;
			var y = -_scrollOffset;
			for (var i = 0; i < Items.Count; i++)
			{
				var rect = new Rectangle(0, y, Width, itemHeight);
				if (rect.Contains(pt)) return i;
				y += itemHeight;
			}
		}
		else
		{
			var itemHeight = Font.Height + 6;
			var itemWidth = (Width - 4) / _shortColumns;
			var x0 = -_scrollOffset;
			for (var i = 0; i < Items.Count; i++)
			{
				var col = i % _shortColumns;
				var row = i / _shortColumns;
				var x = x0 + col * itemWidth;
				var top = row * itemHeight;
				var rect = new Rectangle(x, top, itemWidth, itemHeight);
				if (rect.Contains(pt)) return i;
			}
		}

		return -1;
	}
}

/// <summary>
/// 파일 리스트의 뷰 모드입니다.
/// </summary>
public enum FileListViewMode
{
	/// <summary>세로로 긴 리스트</summary>
	LongList,
	/// <summary>여러 컬럼의 짧은 리스트</summary>
	ShortList
}

/// <summary>
/// 파일 리스트의 각 컬럼 너비 정보를 저장하는 구조체입니다.
/// </summary>
internal struct FileListWidths
{
	/// <summary>파일/디렉터리 이름 컬럼 너비</summary>
	public int Name;
	/// <summary>확장자 컬럼 너비</summary>
	public int Extension;
	/// <summary>크기 컬럼 너비</summary>
	public int Size;
	/// <summary>날짜 컬럼 너비</summary>
	public int Date;
	/// <summary>시간 컬럼 너비</summary>
	public int Time;
	/// <summary>속성 컬럼 너비</summary>
	public int Attr;
	/// <summary>확장자 최소 너비</summary>
	public int MinExtension;

	/// <summary>
	/// 폰트 기준으로 고정 컬럼 너비를 계산합니다.
	/// </summary>
	public void UpdateFixed(Font font)
	{
		var dirWidth = TextRenderer.MeasureText("[폴더]", font).Width;
		var sizeWidth = TextRenderer.MeasureText("999.99 WB", font).Width;
		Size = Math.Max(dirWidth, sizeWidth) + 8;
		Date = TextRenderer.MeasureText("9999-99-99", font).Width + 4;
		Time = TextRenderer.MeasureText("99:99", font).Width + 4;
		Attr = TextRenderer.MeasureText("WWWW", font).Width + 4;
		MinExtension = TextRenderer.MeasureText("WWW", font).Width + 4;
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
		var remainWidth = controlWidth - Size - Date - Time - Attr;
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
	/// <summary>선택 마크 너비</summary>
	protected const int MarkWidth = 8;

	/// <summary>소속 리스트</summary>
	public FileList FileList { get; set; } = fileList;
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
	internal abstract void DrawDetails(Graphics g, Font font, Rectangle bounds, FileListWidths widths, Theme theme, bool focused);

	/// <summary>
	/// 항목을 지정된 영역에 간략하게 그립니다.
	/// </summary>
	/// <remarks>공간이 제한된 곳에 항목을 간단히 표시할 때 사용합니다.</remarks>
	/// <param name="g">그리기에 사용되는 <see cref="Graphics"/> 객체입니다.</param>
	/// <param name="font">텍스트 렌더링에 사용되는 <see cref="Font"/>입니다.</param>
	/// <param name="bounds">그릴 영역을 지정하는 <see cref="Rectangle"/>입니다.</param>
	/// <param name="theme">적용할 시각적 스타일을 지정하는 <see cref="Theme"/>입니다.</param>
	/// <param name="focused">항목이 포커스 상태인지 여부입니다. <see langword="true"/>면 포커스됨, 아니면 <see langword="false"/>.</param>
	internal abstract void DrawShort(Graphics g, Font font, Rectangle bounds, Theme theme, bool focused);

	/// <summary>
	/// 공통 항목 그리기(배경, 아이콘, 선택 마크 등)
	/// </summary>
	internal void DrawCommon(Graphics g, Rectangle bounds, Theme theme, bool focused)
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
			for (var len = drawText.Length - 1; len > 0; len--)
			{
				var t = drawText.Substring(0, len) + "...";
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

	// 선택 마크(화살표) 그리기
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
	internal override void DrawDetails(Graphics g, Font font, Rectangle bounds, FileListWidths widths, Theme theme, bool focused)
	{
		DrawCommon(g, bounds, theme, focused);
		var (fileColor, otherColor) = focused ? (theme.BackContent, theme.BackContent) : (Color, theme.Foreground);
		var x = bounds.Left + 28;
		DrawItemText(g, FileName, font, new Rectangle(x, bounds.Top, widths.Name, bounds.Height), fileColor);
		x += widths.Name;
		DrawItemText(g, Extension, font, new Rectangle(x, bounds.Top, widths.Extension, bounds.Height), fileColor);
		x += widths.Extension;
		DrawItemText(g, Size.FormatFileSize(), font, new Rectangle(x, bounds.Top, widths.Size, bounds.Height), otherColor, true);
		x += widths.Size;
		DrawItemText(g, Creation.FormatRelativeDate(), font, new Rectangle(x, bounds.Top, widths.Date, bounds.Height), otherColor);
		x += widths.Date;
		DrawItemText(g, Creation.ToString("HH:mm"), font, new Rectangle(x, bounds.Top, widths.Time, bounds.Height), otherColor);
		x += widths.Time;
		DrawItemText(g, Attributes.FormatString(), font, new Rectangle(x, bounds.Top, widths.Attr, bounds.Height), otherColor);
	}

	/// <inheritdoc />
	internal override void DrawShort(Graphics g, Font font, Rectangle bounds, Theme theme, bool focused)
	{
		DrawCommon(g, bounds, theme, focused);
		DrawItemText(g, FileName, font, bounds, focused ? theme.BackContent : Color);
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
	internal override void DrawDetails(Graphics g, Font font, Rectangle bounds, FileListWidths widths, Theme theme, bool focused)
	{
		DrawCommon(g, bounds, theme, focused);
		var (dirColor, otherColor) = focused ? (theme.BackContent, theme.BackContent) : (Color, theme.Foreground);
		var x = bounds.Left + 28;
		DrawItemText(g, DirName, font, new Rectangle(x, bounds.Top, widths.Name, bounds.Height), dirColor);
		x += widths.Name + widths.Extension;
		DrawItemText(g, "[폴더]", font, new Rectangle(x, bounds.Top, widths.Size, bounds.Height), dirColor, true);
		x += widths.Size;
		DrawItemText(g, Creation.FormatRelativeDate(), font, new Rectangle(x, bounds.Top, widths.Date, bounds.Height), otherColor);
		x += widths.Date;
		DrawItemText(g, Creation.ToString("HH:mm"), font, new Rectangle(x, bounds.Top, widths.Time, bounds.Height), otherColor);
		x += widths.Time;
		DrawItemText(g, Attributes.FormatString(), font, new Rectangle(x, bounds.Top, widths.Attr, bounds.Height), otherColor);
	}

	/// <inheritdoc />
	internal override void DrawShort(Graphics g, Font font, Rectangle bounds, Theme theme, bool focused)
	{
		DrawCommon(g, bounds, theme, focused);
		DrawItemText(g, DirName, font, bounds, focused ? theme.BackContent : Color);
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
	internal override void DrawDetails(Graphics g, Font font, Rectangle bounds, FileListWidths widths, Theme theme, bool focused)
	{
		DrawCommon(g, bounds, theme, focused);
		var driveColor = focused ? theme.BackContent : Color;
		var x = bounds.Left + 28;
		DrawItemText(g, VolumeLabel, font, new Rectangle(x, bounds.Top, widths.Name, bounds.Height), driveColor);
	}

	/// <inheritdoc />
	internal override void DrawShort(Graphics g, Font font, Rectangle bounds, Theme theme, bool focused)
	{
		DrawCommon(g, bounds, theme, focused);
		var driveColor = focused ? theme.BackContent : Color;
		DrawItemText(g, DriveName, font, bounds, driveColor);
	}
}

/// <summary>
/// 포커스 변경 이벤트 인자입니다.
/// </summary>
public class FileListFocusChangedEventArgs(FileListItem? item) : EventArgs
{
	/// <summary>포커스된 항목입니다.</summary>
	public FileListItem? Item { get; } = item;
}

/// <summary>
/// 더블클릭 이벤트 인자입니다.
/// </summary>
public class FileListDoubleClickEventArgs(FileListItem? item, string fullName) : EventArgs
{
	/// <summary>더블클릭된 항목입니다.</summary>
	public FileListItem? Item { get; } = item;
	/// <summary>항목의 전체 경로입니다.</summary>
	public string FullName { get; } = fullName;
}

/// <summary>
/// 파일 리스트 클릭 이벤트 인자입니다.
/// </summary>
public class FileListClickEventArgs : EventArgs
{
	/// <summary>클릭이 발생한 파일 리스트 컨트롤입니다.</summary>
	public FileList FileList { get; }
	/// <summary>클릭된 마우스 버튼입니다.</summary>
	public MouseButtons Button { get; }
	/// <summary>클릭 위치(컨트롤 기준 좌표)입니다.</summary>
	public Point Location { get; }
	/// <summary>클릭 위치(스크린 기준 좌표)입니다.</summary>
	public Point ScreenLocation { get; }

	/// <summary>
	/// 파일 리스트 클릭 이벤트 인자를 생성합니다.
	/// </summary>
	/// <param name="fileList">클릭이 발생한 파일 리스트 컨트롤입니다.</param>
	/// <param name="button">클릭된 마우스 버튼입니다.</param>
	/// <param name="location">클릭 위치(컨트롤 기준 좌표)입니다.</param>
	public FileListClickEventArgs(FileList fileList, MouseButtons button, Point location)
	{
		FileList = fileList;
		Button = button;
		Location = location;
		ScreenLocation = fileList.PointToScreen(location);
	}
}

/// <summary>
/// 파일 리스트 선택 항목 변경 이벤트 인자입니다.
/// </summary>
public class FileListSelectChangedEventArgs(FileList fileList) : EventArgs
{
	/// <summary>파일 리스트 컨트롤입니다.</summary>
	public FileList FileList { get; } = fileList;
}
