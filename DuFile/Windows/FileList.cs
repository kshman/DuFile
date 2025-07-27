namespace DuFile.Windows;

/// <summary>
/// 파일/디렉터리/드라이브 항목을 표시하고 관리하는 커스텀 리스트 컨트롤입니다.
/// </summary>
public sealed class FileList : ThemeControl
{
	// 리스트의 컬럼 수
	private int _columns = 1;
	// 컬럼 너비 정보
	private FileListWidths _widths;

	// 업데이트 중 여부
	private bool _updating;

	// 포커스된 인덱스
	private int _focusedIndex = -1;
	// 시프트키로 다중 선택 기준 인덱스
	private int _anchorIndex = -1;
	// 스크롤 오프셋
	private int _scrollOffset;
	// 스크롤바 눌림
	private bool _sbarDragging;
	// 스크롤바 드래그 오프셋
	private int _sbarDragOffset;
	// 스크롤바 지시자 사각형
	private Rectangle _sbarIndicatorBound = Rectangle.Empty;

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
			_columns = Math.Clamp(value, 1, 4);
			Invalidate();
		}
	}

	/// <summary>스크롤 바 높이입니다.</summary>
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int ScrollBarHeight => 12;

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

	/// <summary>FileList 컨트롤을 초기화합니다.</summary>
	public FileList()
	{
		SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
				 ControlStyles.ResizeRedraw | ControlStyles.UserPaint | ControlStyles.Selectable, true);
		TabStop = true;
	}

	/// <inheritdoc />
	protected override void OnUpdateTheme(Theme theme)
	{
		Font = new Font(theme.ContentFontFamily, theme.ContentFontSize, FontStyle.Regular, GraphicsUnit.Point);
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
		_widths.UpdateName(Items, Font);
		FocusedIndex = Items.Count > 0 ? 0 : -1;
		_anchorIndex = -1;
		_scrollOffset = 0;
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
			Invalidate(false);
		}
	}

	/// <summary>
	/// DisplayName이 name과 같은 항목에 포커스를 주고, 해당 위치가 보이도록 스크롤합니다.
	/// </summary>
	public void FocusName(string? name)
	{
		if (string.IsNullOrEmpty(name) || Items.Count == 0)
			return;

		var idx = Items.FindIndex(item => item.FullName == name);
		if (idx < 0)
			return;

		FocusedIndex = idx;

		var itemHeight = Font.Height + 6;
		var row = idx / _columns;
		var topRow = _scrollOffset / itemHeight;
		var bottomRow = (_scrollOffset + Height) / itemHeight - 1;
		if (row < topRow)
			_scrollOffset = row * itemHeight;
		else if (row > bottomRow)
		{
			var rows = (Items.Count + _columns - 1) / _columns;
			_scrollOffset = Math.Min((row + 1) * itemHeight - Height, Math.Max(0, rows * itemHeight - Height));
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

		var showScrollBar = ShouldShowScrollBar();
		var clientHeight = Height;
		if (showScrollBar)
			clientHeight -= ScrollBarHeight;

		var itemHeight = Font.Height + 6;
		var itemWidth = (Width - 4) / _columns;
		for (var i = 0; i < Items.Count; i++)
		{
			var col = i % _columns;
			var row = i / _columns;
			var x = col * itemWidth;
			var y = row * itemHeight - _scrollOffset;
			var rect = new Rectangle(x, y, itemWidth, itemHeight);
			if (rect.Bottom <= 0) continue;
			if (rect.Top >= clientHeight) break;
			Items[i].Draw(g, Font, rect, _widths, theme, i == FocusedIndex);
		}

		if (showScrollBar)
			DrawScrollBar(g, theme);
	}

	// 스크롤바가 필요한지 미리 계산
	private bool ShouldShowScrollBar()
	{
		var itemHeight = Font.Height + 6;
		var rows = (Items.Count + _columns - 1) / _columns;
		var contentHeight = rows * itemHeight;
		return contentHeight > Height - ScrollBarHeight;
	}

	// 스크롤바 그리기
	private void DrawScrollBar(Graphics g, Theme theme)
	{
		var itemHeight = Font.Height + 6;
		var contentLength = Items.Count * itemHeight;
		var viewLength = Height - ScrollBarHeight;

		if (contentLength <= viewLength)
		{
			_sbarIndicatorBound = Rectangle.Empty;
			return;
		}

		var barRect = new Rectangle(0, Height - ScrollBarHeight, Width, ScrollBarHeight);
		g.FillRectangle(new SolidBrush(theme.BackContent), barRect);

		const int minIndicatorWidth = 24;
		var ratio = (float)viewLength / contentLength;
		var indicatorLength = Math.Max((int)(Width * ratio), minIndicatorWidth);
		var indicatorPos = (int)((float)_scrollOffset / (contentLength - viewLength) * (Width - indicatorLength));
		var indicatorRect = new Rectangle(indicatorPos, barRect.Top, indicatorLength, ScrollBarHeight);
		_sbarIndicatorBound = indicatorRect;

		// Draw rounded indicator (left 8, right 8)
		using (var path = new System.Drawing.Drawing2D.GraphicsPath())
		{
			const int radius = 8;
			path.AddArc(indicatorRect.Left, indicatorRect.Top, radius, ScrollBarHeight, 90, 180);
			path.AddArc(indicatorRect.Right - radius, indicatorRect.Top, radius, ScrollBarHeight, 270, 180);
			path.CloseFigure();
			g.FillPath(new SolidBrush(theme.BackSelection), path);
		}

		var leftBtn = new Rectangle(0, barRect.Top, ScrollBarHeight, ScrollBarHeight);
		var rightBtn = new Rectangle(Width - ScrollBarHeight, barRect.Top, ScrollBarHeight, ScrollBarHeight);
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
	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		if (_sbarDragging)
		{
			var indicatorWidth = _sbarIndicatorBound.Width;
			var barWidth = Width;
			var indicatorX = e.Location.X - _sbarDragOffset;
			indicatorX = Math.Max(0, Math.Min(barWidth - indicatorWidth, indicatorX));

			var itemHeight = Font.Height + 6;
			var contentLength = Items.Count * itemHeight;
			var viewLength = Height - ScrollBarHeight;

			if (contentLength <= viewLength)
				return;

			var ratio = (float)indicatorX / (barWidth - indicatorWidth);
			_scrollOffset = (int)((contentLength - viewLength) * ratio);
			Invalidate();
		}
	}

	/// <inheritdoc />
	protected override void OnMouseUp(MouseEventArgs e)
	{
		base.OnMouseUp(e);
		if (_sbarDragging)
			_sbarDragging = false;
	}

	/// <inheritdoc />
	protected override void OnMouseDown(MouseEventArgs e)
	{
		base.OnMouseDown(e);
		Focus();

		// 스크롤바 조작
		if (_sbarIndicatorBound.Contains(e.Location) && e.Button is MouseButtons.Left or MouseButtons.Right)
		{
			_sbarDragging = true;
			_sbarDragOffset = e.Location.X - _sbarIndicatorBound.Left;
			return;
		}

		// 스크롤바　좌/우 버튼 클릭
		if (e.Location.Y >= Height - ScrollBarHeight && e.Location.Y < Height)
		{
			if (e.Location.X < ScrollBarHeight)
			{
				ScrollBy(-1);
				return;
			}
			if (e.Location.X > Width - ScrollBarHeight)
			{
				ScrollBy(1);
				return;
			}
		}

		var idx = HitTest(e.Location);
		if (idx < 0)
			return;

		switch (e.Button)
		{
			case MouseButtons.XButton1:
			// 다음 버튼 클릭
			case MouseButtons.XButton2:
				// 이전 버튼 클릭
				return;
			case MouseButtons.Left when (ModifierKeys & Keys.Shift) == Keys.Shift:
			{
				if (_anchorIndex == -1)
					_anchorIndex = FocusedIndex;
				SelectRange(_anchorIndex, idx);
				break;
			}
			case MouseButtons.Left when (ModifierKeys & Keys.Control) == Keys.Control:
				SelectIndex(idx);
				_anchorIndex = idx;
				break;
			case MouseButtons.Left:
			case MouseButtons.Right:
			case MouseButtons.Middle:
				_anchorIndex = idx;
				break;
			case MouseButtons.None:
			// 잉? 이게 들어온다고?
			default:
				break;
		}

		FocusedIndex = idx;
		Invalidate();

		ItemClicked?.Invoke(this, new FileListClickEventArgs(this, e.Button, e.Location));
	}

	/// <inheritdoc />
	protected override void OnMouseDoubleClick(MouseEventArgs e)
	{
		base.OnMouseDoubleClick(e);

		// TODO: 스크롤 바 클릭 시 더블클릭 이벤트 무시

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
		ScrollBy(-Math.Sign(e.Delta), page: true);
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
		var prevIndex = FocusedIndex;
		var newIndex = prevIndex;
		var rows = (Items.Count + _columns - 1) / _columns;
		var viewRows = Math.Max(1, (Height - ScrollBarHeight) / itemHeight);
		var col = prevIndex % _columns;
		var row = prevIndex / _columns;

		switch (e.KeyCode)
		{
			case Keys.Up:
				if (row > 0)
					newIndex -= _columns;
				else if (_scrollOffset > 0)
				{
					// 페이지 위로 이동
					_scrollOffset = Math.Max(0, _scrollOffset - viewRows * itemHeight);
					newIndex = col;
				}
				break;
			case Keys.Down:
				if (row < rows - 1 && newIndex + _columns < Items.Count)
					newIndex += _columns;
				else if (_scrollOffset + viewRows * itemHeight < rows * itemHeight)
				{
					// 페이지 아래로 이동
					_scrollOffset = Math.Min((rows * itemHeight) - viewRows * itemHeight, _scrollOffset + viewRows * itemHeight);
					var newRow = Math.Min(rows - 1, row + 1);
					newIndex = newRow * _columns + col;
					if (newIndex >= Items.Count)
						newIndex = Items.Count - 1;
				}
				break;
			case Keys.Left:
				if (col > 0)
					newIndex--;
				break;
			case Keys.Right:
				if (col < _columns - 1 && newIndex + 1 < Items.Count)
					newIndex++;
				break;
			case Keys.Home:
				newIndex = 0;
				_scrollOffset = 0;
				break;
			case Keys.End:
				newIndex = Items.Count - 1;
				var lastRow = (Items.Count - 1) / _columns;
				_scrollOffset = Math.Max(0, lastRow * itemHeight - (viewRows - 1) * itemHeight);
				break;
			case Keys.PageUp:
				ScrollBy(-1, page: true, focusMove: true);
				return;
			case Keys.PageDown:
				ScrollBy(1, page: true, focusMove: true);
				return;
			case Keys.Space:
			case Keys.Insert:
				SelectIndex(FocusedIndex);
				_anchorIndex = FocusedIndex;
				if (FocusedIndex < Items.Count - 1)
					FocusedIndex++;
				Invalidate();
				return;
			case Keys.Return:
				ItemDoubleClicked?.Invoke(this, new FileListDoubleClickEventArgs(Items[FocusedIndex], FullName));
				return;
			case Keys.Back when Items.Count > 0:
				if (Items[0] is FileListFolderItem { DirName: ".." } item)
					ItemDoubleClicked?.Invoke(this, new FileListDoubleClickEventArgs(item, FullName));
				return;
			case Keys.Apps when ItemClicked != null:
			{
				var bound = GetIndexedBound(FocusedIndex);
				if (!bound.HasValue)
					return;
				ItemClicked.Invoke(this, new FileListClickEventArgs(this, MouseButtons.Right, bound.Value.Location));
				return;
			}
		}

		// scrollOffset 보정
		if (newIndex != prevIndex)
		{
			var newRow = newIndex / _columns;
			if (newRow < _scrollOffset / itemHeight)
				_scrollOffset = newRow * itemHeight;
			else if (newRow >= (_scrollOffset + viewRows * itemHeight) / itemHeight)
				_scrollOffset = Math.Min((rows * itemHeight) - viewRows * itemHeight, (newRow - viewRows + 1) * itemHeight);
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

	// 아이템 선택
	private void SelectIndex(int index)
	{
		var item = Items[index];
		if (item is FileListFileItem or FileListFolderItem { IsParent: false })
		{
			item.Selected = !item.Selected;
			SelectChanged?.Invoke(this, new FileListSelectChangedEventArgs(this));
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
		SelectChanged?.Invoke(this, new FileListSelectChangedEventArgs(this));
		Invalidate();
	}

	// 마우스 위치에 해당하는 인덱스 반환
	private int HitTest(Point pt)
	{
		var itemHeight = Font.Height + 6;
		var itemWidth = (Width - 4) / _columns;
		for (var i = 0; i < Items.Count; i++)
		{
			var col = i % _columns;
			var row = i / _columns;
			var x = col * itemWidth;
			var y = row * itemHeight - _scrollOffset;
			var rect = new Rectangle(x, y, itemWidth, itemHeight);
			if (rect.Contains(pt)) return i;
		}
		return -1;
	}

	// 인덱스에 해당하는 경계 사각형을 반환합니다.
	private Rectangle? GetIndexedBound(int index)
	{
		if (index < 0 || index >= Items.Count)
			return null;
		var itemHeight = Font.Height + 6;
		var itemWidth = (Width - 4) / _columns;
		var row = index / _columns;
		var col = index % _columns;
		var x = col * itemWidth;
		var y = row * itemHeight - _scrollOffset;
		return new Rectangle(x, y, itemWidth, itemHeight);
	}

	// 페이지 스크롤 및 포커스 이동 지원
	private void ScrollBy(int direction, bool page = false, bool focusMove = false)
	{
		var itemHeight = Font.Height + 6;
		var rows = (Items.Count + _columns - 1) / _columns;
		var viewRows = Math.Max(1, (Height - ScrollBarHeight) / itemHeight);
		var contentHeight = rows * itemHeight;
		var viewHeight = Height - ScrollBarHeight;
		var step = page ? viewRows * itemHeight : itemHeight;
		if (contentHeight <= viewHeight)
			return;

		if (page && focusMove && Items.Count > 0)
		{
			if (direction < 0)
			{
				// PageUp: 맨 위 항목으로 포커스 이동, 이미 맨 위면 스크롤 후 새 영역 맨 위로 포커스
				var topRow = _scrollOffset / itemHeight;
				var curRow = FocusedIndex / _columns;
				if (curRow <= topRow)
				{
					_scrollOffset = Math.Max(0, _scrollOffset - step);
					topRow = _scrollOffset / itemHeight;
				}
				FocusedIndex = Math.Max(0, (topRow) * _columns + FocusedIndex % _columns);
			}
			else
			{
				// PageDown: 맨 아래 항목으로 포커스 이동, 이미 맨 아래면 스크롤 후 새 영역 맨 아래로 포커스
				var bottomRow = (_scrollOffset + viewHeight) / itemHeight - 1;
				var curRow = FocusedIndex / _columns;
				var maxRow = (Items.Count - 1) / _columns;
				if (curRow >= bottomRow || bottomRow > maxRow)
				{
					_scrollOffset = Math.Min(contentHeight - viewHeight, _scrollOffset + step);
					bottomRow = (_scrollOffset + viewHeight) / itemHeight - 1;
				}
				FocusedIndex = Math.Min((bottomRow) * _columns + FocusedIndex % _columns, Items.Count - 1);
			}
			Invalidate();
			return;
		}

		// 일반 스크롤
		_scrollOffset += direction * step;
		if (_scrollOffset < 0) _scrollOffset = 0;
		if (_scrollOffset > contentHeight - viewHeight) _scrollOffset = contentHeight - viewHeight;
		Invalidate();
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
	/// <summary>확장자 최소 너비</summary>
	public int MinExtension;

	/// <summary>크기 컬럼 너비</summary>
	public int Size;
	/// <summary>날짜 컬럼 너비</summary>
	public int Date;
	/// <summary>시간 컬럼 너비</summary>
	public int Time;
	/// <summary>속성 컬럼 너비</summary>
	public int Attr;

	/// <summary>모든 고정 컬럼 너비</summary>
	public int Fixed;

	/// <summary>전체 컬럼 그릴지 여부</summary>
	public bool IsFixedVisible;

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
/// 포커스 변화 이벤트 인자입니다.
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
