// ReSharper disable MissingXmlDoc
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
			Invalidate();
		}
	}

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
			BeginUpdate();
			AddItem(new FileListFileItem(new FileInfo(@"C:\Windows\notepad.exe")));
			AddItem(new FileListFileItem(new FileInfo(@"C:\Windows\regedit.exe")) { Selected = true });
			AddItem(new FileListFileItem(new FileInfo(@"C:\Windows\win.ini")));
			AddItem(new FileListDirectoryItem(new DirectoryInfo(@"C:\Windows\assembly")));
			AddItem(new FileListDirectoryItem(new DirectoryInfo(@"C:\Windows\System32")));
			AddItem(new FileListDriveItem(new DriveInfo("C:")));
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

	public void BeginUpdate()
	{
		if (_updating)
			return;
		_updating = true;
		_scrollOffset = 0;
		_focusedIndex = -1;
		_anchorIndex = -1;
	}

	public void EndUpdate()
	{
		if (!_updating)
			return;
		_updating = false;
		_widths.UpdateName(Items, Font);
		Invalidate(false);
	}

	public void AddFile(FileInfo fileInfo) =>
		AddItem(new FileListFileItem(fileInfo));

	public void AddDirectory(DirectoryInfo dirInfo) =>
		AddItem(new FileListDirectoryItem(dirInfo));

	public void AddDrive(DriveInfo driveInfo) =>
		AddItem(new FileListDriveItem(driveInfo));

	private void AddItem(FileListItem item)
	{
		Items.Add(item);
		if (!_updating)
		{
			_widths.UpdateName(item, Font);
			Invalidate();
		}
	}

	public void ClearItems()
	{
		Items.Clear();
		_focusedIndex = -1;
		_anchorIndex = -1;
		_widths.ResetName();
		Invalidate();
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		if (_updating)
			return;

		base.OnPaint(e);

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

			item.DrawDetails(g, Font, rect, _widths, theme, i == _focusedIndex);
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

			item.DrawShort(g, Font, rect, theme, i == _focusedIndex);
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
					_anchorIndex = _focusedIndex;
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

			_focusedIndex = idx;
			Invalidate();
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

		if (_focusedIndex < 0)
			_focusedIndex = 0;

		var shift = (e.Modifiers & Keys.Shift) == Keys.Shift;
		var itemHeight = Font.Height + 6;
		var visibleRows = Math.Max(1, Height / itemHeight);
		var prevIndex = _focusedIndex;
		var newIndex = prevIndex;

		switch (e.KeyCode)
		{
			case Keys.Up:
				switch (_viewMode)
				{
					case FileListViewMode.LongList when _focusedIndex > 0:
						newIndex--;
						break;
					case FileListViewMode.ShortList when _focusedIndex - _shortColumns >= 0:
						newIndex -= _shortColumns;
						break;
				}
				break;
			case Keys.Down:
				switch (_viewMode)
				{
					case FileListViewMode.LongList when _focusedIndex < Items.Count - 1:
						newIndex++;
						break;
					case FileListViewMode.ShortList when _focusedIndex + _shortColumns < Items.Count:
						newIndex += _shortColumns;
						break;
				}
				break;
			case Keys.Left when _viewMode == FileListViewMode.ShortList && _focusedIndex > 0:
				newIndex--;
				break;
			case Keys.Right when _viewMode == FileListViewMode.ShortList && _focusedIndex < Items.Count - 1:
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
					if (_focusedIndex > firstVisibleIndex)
					{
						newIndex = firstVisibleIndex;
					}
					else
					{
						_scrollOffset = Math.Max(0, _scrollOffset - visibleRows * itemHeight);
						newIndex = _scrollOffset / itemHeight;
					}
				}
				else // ShortList
				{
					var firstVisibleRow = _scrollOffset / itemHeight;
					var firstVisibleIndex = firstVisibleRow * _shortColumns + (_focusedIndex % _shortColumns);
					if (_focusedIndex > firstVisibleIndex)
					{
						newIndex = firstVisibleIndex;
					}
					else
					{
						_scrollOffset = Math.Max(0, _scrollOffset - visibleRows * itemHeight);
						firstVisibleRow = _scrollOffset / itemHeight;
						newIndex = firstVisibleRow * _shortColumns + (_focusedIndex % _shortColumns);
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
					if (_focusedIndex >= lastVisibleIndex)
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
					var lastVisibleIndex = lastVisibleRow * _shortColumns + (_focusedIndex % _shortColumns);
					if (lastVisibleIndex >= Items.Count)
						lastVisibleIndex = Items.Count - 1;
					if (_focusedIndex < lastVisibleIndex)
					{
						newIndex = lastVisibleIndex;
					}
					else
					{
						var totalRows = (Items.Count + _shortColumns - 1) / _shortColumns;
						var maxOffset = Math.Max(0, totalRows * itemHeight - Height);
						_scrollOffset = Math.Min(maxOffset, _scrollOffset + visibleRows * itemHeight);
						lastVisibleRow = Math.Min(((_scrollOffset + Height) / itemHeight) - 1, (Items.Count - 1) / _shortColumns);
						newIndex = lastVisibleRow * _shortColumns + (_focusedIndex % _shortColumns);
						if (newIndex >= Items.Count)
							newIndex = Items.Count - 1;
					}
				}
				break;
			}
			case Keys.Space:
			case Keys.Insert:
			{
				var item = Items[_focusedIndex];
				item.Selected = !item.Selected;
				_anchorIndex = _focusedIndex;
				Invalidate();
				return;
			}
		}

		// Arrow key scrollOffset 보정
		if (newIndex != prevIndex && (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down || e.KeyCode == Keys.Left || e.KeyCode == Keys.Right))
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
			_focusedIndex = newIndex;
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
	public int NameWidth;
	public int ExtWidth;
	public int SizeWidth;
	public int DateWidth;
	public int TimeWidth;
	public int AttrWidth;

	public void UpdateFixed(Font font)
	{
		var dirWidth = TextRenderer.MeasureText("[디렉토리]", font).Width;
		var sizeWidth = TextRenderer.MeasureText("999.99 WB", font).Width;
		SizeWidth = Math.Max(dirWidth, sizeWidth) + 8;
		DateWidth = TextRenderer.MeasureText("9999-99-99", font).Width + 8;
		TimeWidth = TextRenderer.MeasureText("99:99", font).Width + 8;
		AttrWidth = TextRenderer.MeasureText("WWWW"/*"AHRS"*/, font).Width + 8;
	}

	public void UpdateName(List<FileListItem> items, Font font)
	{
		NameWidth = 0;
		ExtWidth = 0;
		foreach (var item in items)
		{
			item.NameWidth = TextRenderer.MeasureText(item.DisplayName, font).Width + 8;
			item.ExtWidth = TextRenderer.MeasureText(item.DisplayExtension, font).Width + 8;
			if (item.NameWidth > NameWidth) NameWidth = item.NameWidth;
			if (item.ExtWidth > ExtWidth) ExtWidth = item.ExtWidth;
		}
	}

	public void UpdateName(FileListItem item, Font font)
	{
		item.NameWidth = TextRenderer.MeasureText(item.DisplayName, font).Width + 8;
		item.ExtWidth = TextRenderer.MeasureText(item.DisplayExtension, font).Width + 8;
		if (item.NameWidth > NameWidth) NameWidth = item.NameWidth;
		if (item.ExtWidth > ExtWidth) ExtWidth = item.ExtWidth;
	}

	public void ResetName()
	{
		NameWidth = 0;
		ExtWidth = 0;
	}
}

public abstract class FileListItem
{
	protected const int MarkWidth = 8;

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
}

public class FileListFileItem : FileListItem
{
	public FileInfo Info { get; }
	public string FileName { get; }
	public string Extension { get; }
	public long Size { get; }
	public DateTime Creation { get; }
	public FileAttributes Attributes { get; }

	public FileListFileItem(FileInfo fileInfo)
	{
		Info = fileInfo;
		FileName = fileInfo.Name;
		Extension = fileInfo.Extension.TrimStart('.');
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
		DrawItemText(g, FileName, font, new Rectangle(x, bounds.Top, widths.NameWidth, bounds.Height), fileColor);
		x += widths.NameWidth;
		DrawItemText(g, Extension, font, new Rectangle(x, bounds.Top, widths.ExtWidth, bounds.Height), fileColor);
		x += widths.ExtWidth;
		DrawItemText(g, Size.FormatFileSize(), font, new Rectangle(x, bounds.Top, widths.SizeWidth, bounds.Height), otherColor, true);
		x += widths.SizeWidth;
		DrawItemText(g, Creation.ToString("yyyy-MM-dd"), font, new Rectangle(x, bounds.Top, widths.DateWidth, bounds.Height), otherColor);
		x += widths.DateWidth;
		DrawItemText(g, Creation.ToString("HH:mm"), font, new Rectangle(x, bounds.Top, widths.TimeWidth, bounds.Height), otherColor);
		x += widths.TimeWidth;
		DrawItemText(g, Attributes.FormatString(), font, new Rectangle(x, bounds.Top, widths.AttrWidth, bounds.Height), otherColor);
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

	public FileListDirectoryItem(DirectoryInfo dirInfo)
	{
		Info = dirInfo;
		DirName = dirInfo.Name;
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
		DrawItemText(g, DirName, font, new Rectangle(x, bounds.Top, widths.NameWidth, bounds.Height), dirColor);
		x += widths.NameWidth + widths.ExtWidth;
		DrawItemText(g, "[디렉토리]", font, new Rectangle(x, bounds.Top, widths.SizeWidth, bounds.Height), dirColor, true);
		x += widths.SizeWidth;
		DrawItemText(g, Creation.ToString("yyyy-MM-dd"), font, new Rectangle(x, bounds.Top, widths.DateWidth, bounds.Height), otherColor);
		x += widths.DateWidth;
		DrawItemText(g, Creation.ToString("HH:mm"), font, new Rectangle(x, bounds.Top, widths.TimeWidth, bounds.Height), otherColor);
		x += widths.TimeWidth;
		DrawItemText(g, Attributes.FormatString(), font, new Rectangle(x, bounds.Top, widths.AttrWidth, bounds.Height), otherColor);
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

	public FileListDriveItem(DriveInfo driveInfo)
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
		DrawItemText(g, VolumeLabel, font, new Rectangle(x, bounds.Top, widths.NameWidth, bounds.Height), driveColor);
	}

	internal override void DrawShort(Graphics g, Font font, Rectangle bounds, Theme theme, bool focused)
	{
		DrawCommon(g, bounds, theme, focused);
		var driveColor = focused ? theme.BackContent : Color;
		DrawItemText(g, DriveName, font, bounds, driveColor);
	}
}
