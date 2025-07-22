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

	public FileList()
	{
		SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
				 ControlStyles.ResizeRedraw | ControlStyles.UserPaint | ControlStyles.Selectable, true);
		var settings = Settings.Instance;
		var theme = settings.Theme;
		Font = new Font(settings.FileFontFamily, settings.FileFontSize);
		BackColor = theme.BackContent;
		ForeColor = theme.Foreground;
		_widths.UpdateFixed(Font);
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

			using (var backBrush = new SolidBrush(item.Selected ? theme.BackSelection : theme.BackContent))
				g.FillRectangle(backBrush, rect);

			if (_focusedIndex == i)
			{
				var markRect = new Rectangle(rect.Left + 2, rect.Top + 2, 16, rect.Height - 4);
				DrawSelectMark(g, markRect, theme);
			}

			var tx = 22;
			if (item.Icon != null)
				g.DrawImage(item.Icon, tx, rect.Top + (rect.Height - 16) / 2, 16, 16);
			tx += 20;
			var itemRect = new Rectangle(tx, rect.Top, Width - tx, rect.Height);
			item.DrawDetails(g, Font, itemRect, _widths, ForeColor);

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

			using (var backBrush = new SolidBrush(item.Selected ? theme.BackSelection : theme.BackContent))
				g.FillRectangle(backBrush, rect);

			if (_focusedIndex == i)
			{
				var markRect = new Rectangle(rect.Left + 2, rect.Top + 2, 16, rect.Height - 4);
				DrawSelectMark(g, markRect, theme);
			}

			var tx = rect.Left + 22;
			if (item.Icon != null)
				g.DrawImage(item.Icon, tx, rect.Top + (rect.Height - 16) / 2, 16, 16);
			tx += 20;
			var itemRect = new Rectangle(tx, rect.Top, rect.Width - (tx - rect.Left) - 4, rect.Height);
			item.DrawShort(g, Font, itemRect, ForeColor);
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

	private static void DrawSelectMark(Graphics g, Rectangle rect, Theme theme)
	{
		var pts = new[]
		{
			new Point(rect.Left + 2, rect.Top + rect.Height / 2),
			new Point(rect.Right - 2, rect.Top + 4),
			new Point(rect.Right - 2, rect.Bottom - 4)
		};
		g.FillPolygon(new SolidBrush(theme.Foreground), pts);
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

	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);

		if (Items.Count == 0)
			return;

		if (_focusedIndex < 0)
			_focusedIndex = 0;

		// ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
		switch (e.KeyCode)
		{
			case Keys.Up:
			{
				// ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
				switch (_viewMode)
				{
					case FileListViewMode.LongList when _focusedIndex > 0:
						_focusedIndex--;
						break;
					case FileListViewMode.ShortList when _focusedIndex - _shortColumns >= 0:
						_focusedIndex -= _shortColumns;
						break;
				}
				Invalidate();
				break;
			}
			case Keys.Down:
			{
				// ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
				switch (_viewMode)
				{
					case FileListViewMode.LongList when _focusedIndex < Items.Count - 1:
						_focusedIndex++;
						break;
					case FileListViewMode.ShortList when _focusedIndex + _shortColumns < Items.Count:
						_focusedIndex += _shortColumns;
						break;
				}
				Invalidate();
				break;
			}
			case Keys.Left when _viewMode == FileListViewMode.ShortList && _focusedIndex > 0:
				_focusedIndex--;
				Invalidate();
				break;
			case Keys.Right when _viewMode == FileListViewMode.ShortList && _focusedIndex < Items.Count - 1:
				_focusedIndex++;
				Invalidate();
				break;
			case Keys.Space:
			case Keys.Insert:
			{
				var item = Items[_focusedIndex];
				item.Selected = !item.Selected;
				_anchorIndex = _focusedIndex;
				Invalidate();
				break;
			}
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

	internal static void DrawItemText(Graphics g, string text, Font font, Rectangle bounds, Color color,
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
	public bool Selected { get; set; }
	public int NameWidth { get; set; }
	public int ExtWidth { get; set; }

	public Image? Icon { get; set; }

	internal abstract string DisplayName { get; }
	internal abstract string DisplayExtension { get; }

	internal abstract void DrawDetails(Graphics g, Font font, Rectangle bounds, FileListWidths widths, Color color);
	internal abstract void DrawShort(Graphics g, Font font, Rectangle bounds, Color color);
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
	}

	internal override string DisplayName => FileName;
	internal override string DisplayExtension => Extension;

	internal override void DrawDetails(Graphics g, Font font, Rectangle bounds, FileListWidths widths, Color color)
	{
		var x = bounds.Left;
		FileList.DrawItemText(g, FileName, font, new Rectangle(x, bounds.Top, widths.NameWidth, bounds.Height), color);
		x += widths.NameWidth;
		FileList.DrawItemText(g, Extension, font, new Rectangle(x, bounds.Top, widths.ExtWidth, bounds.Height), color);
		x += widths.ExtWidth;
		FileList.DrawItemText(g, Size.FormatFileSize(), font, new Rectangle(x, bounds.Top, widths.SizeWidth, bounds.Height), color,
			true);
		x += widths.SizeWidth;
		FileList.DrawItemText(g, Creation.ToString("yyyy-MM-dd"), font,
			new Rectangle(x, bounds.Top, widths.DateWidth, bounds.Height), color);
		x += widths.DateWidth;
		FileList.DrawItemText(g, Creation.ToString("HH:mm"), font, new Rectangle(x, bounds.Top, widths.TimeWidth, bounds.Height),
			color);
		x += widths.TimeWidth;
		FileList.DrawItemText(g, Attributes.FormatString(), font, new Rectangle(x, bounds.Top, widths.AttrWidth, bounds.Height),
			color);
	}

	internal override void DrawShort(Graphics g, Font font, Rectangle bounds, Color color)
	{
		FileList.DrawItemText(g, FileName, font, bounds, color);
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
	}

	internal override string DisplayName => DirName;
	internal override string DisplayExtension => string.Empty;

	internal override void DrawDetails(Graphics g, Font font, Rectangle bounds, FileListWidths widths, Color color)
	{
		var x = bounds.Left;
		FileList.DrawItemText(g, DirName, font, new Rectangle(x, bounds.Top, widths.NameWidth, bounds.Height), color);
		x += widths.NameWidth + widths.ExtWidth;
		FileList.DrawItemText(g, "[디렉토리]", font, new Rectangle(x, bounds.Top, widths.SizeWidth, bounds.Height), color, true);
		x += widths.SizeWidth;
		FileList.DrawItemText(g, Creation.ToString("yyyy-MM-dd"), font,
			new Rectangle(x, bounds.Top, widths.DateWidth, bounds.Height), color);
		x += widths.DateWidth;
		FileList.DrawItemText(g, Creation.ToString("HH:mm"), font, new Rectangle(x, bounds.Top, widths.TimeWidth, bounds.Height),
			color);
		x += widths.TimeWidth;
		FileList.DrawItemText(g, Attributes.ToString(), font, new Rectangle(x, bounds.Top, widths.AttrWidth, bounds.Height),
			color);
	}

	internal override void DrawShort(Graphics g, Font font, Rectangle bounds, Color color)
	{
		FileList.DrawItemText(g, DirName, font, bounds, color);
	}
}

public class FileListDriveItem : FileListItem
{
	public DriveInfo Info { get; }
	public string DriveName { get; }
	public long Total { get; }
	public long Available { get; }

	public FileListDriveItem(DriveInfo driveInfo)
	{
		Info = driveInfo;
		DriveName = $"{driveInfo.Name.TrimEnd('\\')} {driveInfo.VolumeLabel}";
		Total = driveInfo.TotalSize;
		Available = driveInfo.AvailableFreeSpace;
		Icon = IconCache.Instance.GetIcon(DriveName, string.Empty, false, true);
	}

	internal override string DisplayName => DriveName;
	internal override string DisplayExtension => string.Empty;

	internal override void DrawDetails(Graphics g, Font font, Rectangle bounds, FileListWidths widths, Color color)
	{
		var x = bounds.Left;
		FileList.DrawItemText(g, DriveName, font, new Rectangle(x, bounds.Top, widths.NameWidth, bounds.Height), color);
	}

	internal override void DrawShort(Graphics g, Font font, Rectangle bounds, Color color)
	{
		FileList.DrawItemText(g, DriveName, font, bounds, color);
	}
}
