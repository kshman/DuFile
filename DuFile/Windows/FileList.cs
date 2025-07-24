// ReSharper disable MissingXmlDoc
using System.Xml.Linq;

namespace DuFile.Windows;

public sealed class FileList : Control
{
	private FileListViewMode _viewMode = FileListViewMode.LongList;
	private int _shortColumns = 1;

	private FileListWidths _widths;
	private int _scrollOffset;
	private int _focusedIndex = -1;
	private int _anchorIndex = -1;

	private bool _updating;
	private string _myName = string.Empty;

	[Browsable(false)]
	public List<FileListItem> Items { get; } = [];

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

	[Category("FileList")]
	public event EventHandler<FileListFocusChangedEventArgs>? FocusedIndexChanged;

	[Category("FileList")]
	public event EventHandler<FileListDoubleClickEventArgs>? ItemDoubleClicked;

	// 디자인 모드 확인
	private bool IsReallyDesignMode => LicenseManager.UsageMode == LicenseUsageMode.Designtime || (Site?.DesignMode ?? false);

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
			BeginUpdate(string.Empty);
			AddItem(new FileListFileItem(this, new FileInfo(@"C:\Windows\notepad.exe")));
			AddItem(new FileListFileItem(this, new FileInfo(@"C:\Windows\regedit.exe")) { Selected = true });
			AddItem(new FileListFileItem(this, new FileInfo(@"C:\Windows\win.ini")));
			AddItem(new FileListDirectoryItem(this, new DirectoryInfo(@"C:\Windows\assembly")));
			AddItem(new FileListDirectoryItem(this, new DirectoryInfo(@"C:\Windows\System32")));
			AddItem(new FileListDriveItem(this, new DriveInfo("C:")));
			EndUpdate();
		}
	}

	protected override void OnResize(EventArgs e)
	{
		base.OnResize(e);
		_widths.UpdateFixed(Font);
		Invalidate();
	}

	public new void Invalidate()
	{
		if (!_updating)
			Invalidate(false);
	}

	public void BeginUpdate(string myName)
	{
		if (_updating)
			return;
		_updating = true;
		_myName = myName;
	}

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

	public void ClearItems()
	{
		Items.Clear();
		FocusedIndex = -1;
		_anchorIndex = -1;
		_widths.ResetName();
		Invalidate();
	}

	public void AddFile(FileInfo fileInfo) =>
		AddItem(new FileListFileItem(this, fileInfo));

	public void AddDirectory(DirectoryInfo dirInfo) =>
		AddItem(new FileListDirectoryItem(this, dirInfo));

	public void AddParentDirectory(DirectoryInfo dirInfo) =>
		AddItem(new FileListDirectoryItem(this, dirInfo, ".."));

	public void AddDrive(DriveInfo driveInfo) =>
		AddItem(new FileListDriveItem(this, driveInfo));

	private void AddItem(FileListItem item)
	{
		Items.Add(item);
		if (!_updating)
		{
			_widths.UpdateName(item, Font);
			Invalidate();
		}
	}

	public void SelectName(string? name)
	{
		if (string.IsNullOrEmpty(name) || Items.Count == 0)
			return;

		var idx = Items.FindIndex(item => item.DisplayName == name);
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
				item.Selected = !item.Selected;
				_anchorIndex = idx;
			}
			else
			{
				_anchorIndex = idx;
			}

			FocusedIndex = idx;
			Invalidate();
		}
	}

	protected override void OnMouseDoubleClick(MouseEventArgs e)
	{
		base.OnMouseDoubleClick(e);

		// OnMouseDown에서 FocusedIndex가 설정되므로 그냥 ㄱㄱ
		if (e.Button == MouseButtons.Left && FocusedIndex >= 0 && FocusedIndex < Items.Count)
		{
			var item = Items[FocusedIndex];
			var myName = string.Empty;
			if (item is FileListDirectoryItem { DirName: ".." })
				myName = _myName;
			ItemDoubleClicked?.Invoke(this, new FileListDoubleClickEventArgs(item, myName));
		}
	}

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

	protected override bool IsInputKey(Keys keyData)
	{
		return keyData switch
		{
			Keys.Up or Keys.Down or Keys.Left or Keys.Right => true,
			_ => base.IsInputKey(keyData)
		};
	}

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
				item.Selected = !item.Selected;
				_anchorIndex = FocusedIndex;
				if (FocusedIndex < Items.Count - 1)
					FocusedIndex++;
				Invalidate();
				return;
			}
			case Keys.Return:
			{
				var item = Items[FocusedIndex];
				var myName = string.Empty;
				if (item is FileListDirectoryItem { DirName: ".." })
					myName = _myName;
				ItemDoubleClicked?.Invoke(this, new FileListDoubleClickEventArgs(item, myName));
				return;
			}
			case Keys.Back when Items.Count > 0:
			{
				if (Items[0] is FileListDirectoryItem { DirName: ".." } item)
					ItemDoubleClicked?.Invoke(this, new FileListDoubleClickEventArgs(item, _myName));
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

	private void SelectRange(int from, int to)
	{
		if (from > to)
			(from, to) = (to, from);
		foreach (var item in Items)
			item.Selected = false;
		for (var i = from; i <= to; i++)
			Items[i].Selected = true;
		Invalidate();
	}

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

public enum FileListViewMode
{
	LongList,
	ShortList
}

internal struct FileListWidths
{
	public int Name;
	public int Extension;
	public int Size;
	public int Date;
	public int Time;
	public int Attr;
	public int MinExtension;

	public void UpdateFixed(Font font)
	{
		var dirWidth = TextRenderer.MeasureText("[디렉토리]", font).Width;
		var sizeWidth = TextRenderer.MeasureText("999.99 WB", font).Width;
		Size = Math.Max(dirWidth, sizeWidth) + 8;
		Date = TextRenderer.MeasureText("9999-99-99", font).Width + 4;
		Time = TextRenderer.MeasureText("99:99", font).Width + 4;
		Attr = TextRenderer.MeasureText("WWWW"/*"AHRS"*/, font).Width + 4;
		MinExtension = TextRenderer.MeasureText("WWW", font).Width + 4; // 최소 확장자 너비
	}

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

	public void UpdateName(FileListItem item, Font font)
	{
		item.NameWidth = TextRenderer.MeasureText(item.DisplayName, font).Width + 8;
		item.ExtWidth = TextRenderer.MeasureText(item.DisplayExtension, font).Width + 8;
		if (item.NameWidth > Name) Name = item.NameWidth;
		if (item.ExtWidth > Extension) Extension = item.ExtWidth;
	}

	public void ResetName()
	{
		Name = 0;
		Extension = 0;
	}

	public bool AdjustName(int controlWidth)
	{
		// 고정 컬럼 너비 제외한 남은 너비 계산
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

public abstract class FileListItem(FileList fileList)
{
	protected const int MarkWidth = 8;

	public FileList FileList { get; set; } = fileList;

	public bool Selected { get; set; }
	public int NameWidth { get; set; }
	public int ExtWidth { get; set; }

	public Image? Icon { get; set; }
	public Color Color { get; set; }

	internal abstract string DisplayName { get; }
	internal abstract string DisplayExtension { get; }

	internal abstract void DrawDetails(Graphics g, Font font, Rectangle bounds, FileListWidths widths, Theme theme, bool focused);
	internal abstract void DrawShort(Graphics g, Font font, Rectangle bounds, Theme theme, bool focused);

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

	protected static string ConvertDate(DateTime creation)
	{
		var days = (DateTime.Now - creation).TotalDays;
		return days switch
		{
			> 30 => creation.ToString("yyyy-MM-dd"),
			0 => "오늘",
			_ => $"{(int)days}일전"
		};
	}
}

public class FileListFileItem : FileListItem
{
	public FileInfo Info { get; }
	public string FileName { get; }
	public string Extension { get; }
	public long Size { get; }
	public DateTime Creation { get; }
	public FileAttributes Attributes { get; }

	public FileListFileItem(FileList fileList, FileInfo fileInfo) :
		base(fileList)
	{
		var name = fileInfo.Name;
		var lastDot = name.LastIndexOf('.');

		Info = fileInfo;
		FileName = lastDot >= 0 ? name[..lastDot] : name;
		Extension = lastDot >= 0 ? name[(lastDot + 1)..] : string.Empty;
		Size = fileInfo.Length;
		Creation = fileInfo.CreationTime;
		Attributes = fileInfo.Attributes;
		Icon = IconCache.Instance.GetIcon(fileInfo.FullName, Extension);
		Color = Settings.Instance.Theme.GetColorExtension(Extension.ToUpperInvariant());
	}

	internal override string DisplayName => FileName;
	internal override string DisplayExtension => Extension;

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
		DrawItemText(g, ConvertDate(Creation), font, new Rectangle(x, bounds.Top, widths.Date, bounds.Height), otherColor);
		x += widths.Date;
		DrawItemText(g, Creation.ToString("HH:mm"), font, new Rectangle(x, bounds.Top, widths.Time, bounds.Height), otherColor);
		x += widths.Time;
		DrawItemText(g, Attributes.FormatString(), font, new Rectangle(x, bounds.Top, widths.Attr, bounds.Height), otherColor);
	}

	internal override void DrawShort(Graphics g, Font font, Rectangle bounds, Theme theme, bool focused)
	{
		DrawCommon(g, bounds, theme, focused);
		DrawItemText(g, FileName, font, bounds, focused ? theme.BackContent : Color);
	}
}

public class FileListDirectoryItem : FileListItem
{
	public DirectoryInfo Info { get; }
	public string DirName { get; }
	public DateTime Creation { get; }
	public FileAttributes Attributes { get; }

	public FileListDirectoryItem(FileList fileList, DirectoryInfo dirInfo) :
		base(fileList)
	{
		Info = dirInfo;
		DirName = dirInfo.Name;
		Creation = dirInfo.CreationTime;
		Attributes = dirInfo.Attributes;
		Icon = IconCache.Instance.GetIcon(dirInfo.FullName, string.Empty, true);
		Color = Settings.Instance.Theme.Directory;
	}

	public FileListDirectoryItem(FileList fileList, DirectoryInfo dirInfo, string dirName) :
		base(fileList)
	{
		Info = dirInfo;
		DirName = dirName;
		Creation = dirInfo.CreationTime;
		Attributes = dirInfo.Attributes;
		Icon = IconCache.Instance.GetIcon(dirInfo.FullName, string.Empty, true);
		Color = Settings.Instance.Theme.Directory;
	}

	internal override string DisplayName => DirName;
	internal override string DisplayExtension => string.Empty;

	internal override void DrawDetails(Graphics g, Font font, Rectangle bounds, FileListWidths widths, Theme theme, bool focused)
	{
		DrawCommon(g, bounds, theme, focused);
		var (dirColor, otherColor) = focused ? (theme.BackContent, theme.BackContent) : (Color, theme.Foreground);
		var x = bounds.Left + 28;
		DrawItemText(g, DirName, font, new Rectangle(x, bounds.Top, widths.Name, bounds.Height), dirColor);
		x += widths.Name + widths.Extension;
		DrawItemText(g, "[디렉토리]", font, new Rectangle(x, bounds.Top, widths.Size, bounds.Height), dirColor, true);
		x += widths.Size;
		DrawItemText(g, ConvertDate(Creation), font, new Rectangle(x, bounds.Top, widths.Date, bounds.Height), otherColor);
		x += widths.Date;
		DrawItemText(g, Creation.ToString("HH:mm"), font, new Rectangle(x, bounds.Top, widths.Time, bounds.Height), otherColor);
		x += widths.Time;
		DrawItemText(g, Attributes.FormatString(), font, new Rectangle(x, bounds.Top, widths.Attr, bounds.Height), otherColor);
	}

	internal override void DrawShort(Graphics g, Font font, Rectangle bounds, Theme theme, bool focused)
	{
		DrawCommon(g, bounds, theme, focused);
		DrawItemText(g, DirName, font, bounds, focused ? theme.BackContent : Color);
	}
}

public class FileListDriveItem : FileListItem
{
	public DriveInfo Info { get; }
	public string DriveName { get; }
	public string VolumeLabel { get; }
	public long Total { get; }
	public long Available { get; }

	public FileListDriveItem(FileList fileList, DriveInfo driveInfo) :
		base(fileList)
	{
		Info = driveInfo;
		DriveName = driveInfo.Name.TrimEnd('\\');
		VolumeLabel = $"{DriveName} {driveInfo.VolumeLabel}";
		Total = driveInfo.TotalSize;
		Available = driveInfo.AvailableFreeSpace;
		Icon = IconCache.Instance.GetIcon(DriveName, string.Empty, false, true);
		Color = Settings.Instance.Theme.Drive;
	}

	internal override string DisplayName => VolumeLabel;
	internal override string DisplayExtension => string.Empty;

	internal override void DrawDetails(Graphics g, Font font, Rectangle bounds, FileListWidths widths, Theme theme, bool focused)
	{
		DrawCommon(g, bounds, theme, focused);
		var driveColor = focused ? theme.BackContent : Color;
		var x = bounds.Left + 28;
		DrawItemText(g, VolumeLabel, font, new Rectangle(x, bounds.Top, widths.Name, bounds.Height), driveColor);
	}

	internal override void DrawShort(Graphics g, Font font, Rectangle bounds, Theme theme, bool focused)
	{
		DrawCommon(g, bounds, theme, focused);
		var driveColor = focused ? theme.BackContent : Color;
		DrawItemText(g, DriveName, font, bounds, driveColor);
	}
}

public class FileListFocusChangedEventArgs(FileListItem? item) : EventArgs
{
	public FileListItem? Item { get; } = item;
}

public class FileListDoubleClickEventArgs(FileListItem? item, string myName) : EventArgs
{
	public FileListItem? Item { get; } = item;
	public string MyName { get; } = myName;
}
