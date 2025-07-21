// ReSharper disable MissingXmlDoc

namespace DuFile.Windows;

/// <summary>
/// 파일/드라이브/알수없음 항목을 다양한 모드로 커스텀 그리기하는 리스트뷰 컨트롤입니다.
/// </summary>
public sealed class FileSelection : ListView
{
	public FileSelection()
	{
		AllowDrop = true;
		OwnerDraw = true;
		FullRowSelect = true;
		View = View.Details;
		HideSelection = false;
		DoubleBuffered = true;

		var settings = Settings.Instance;
		var theme = settings.Theme;
		Font = new Font(settings.FileFontFamily, settings.FileFontSize, FontStyle.Regular, GraphicsUnit.Point);
		BackColor = theme.BackContent;
		ForeColor = theme.Foreground;

		ClearItems();
	}

	public void ClearItems()
	{
		Items.Clear();
		Columns.Clear();
		
		Columns.Add("", 22); // 아이콘
		Columns.Add("파일이름", 180);
		Columns.Add("확장자", 60);
		Columns.Add("파일크기", 80, HorizontalAlignment.Right);
		Columns.Add("만든날짜", 90);
		Columns.Add("만든시간", 70);
		Columns.Add("속성", 60);
	}

	public void AddFileItem(FileInfo fileinfo)
	{
		var item = new FileItem(fileinfo);
		Items.Add(item);
	}

	public void AddDirectoryItem(DirectoryInfo directoryInfo)
	{
		var item = new FileItem(directoryInfo);
		Items.Add(item);
	}

	public void AddDriveItem(DriveInfo driveInfo)
	{
		var item = new DriveItem(driveInfo);
		Items.Add(item);
	}

	public void AddUnknownItem(string unknownDesc)
	{
		var item = new UnknownItem(unknownDesc);
		Items.Add(item);
	}

	protected override void OnDrawColumnHeader(DrawListViewColumnHeaderEventArgs e)
	{
		using var b = new SolidBrush(Settings.Instance.Theme.Background);
		using var f = new SolidBrush(Settings.Instance.Theme.Foreground);
		e.Graphics.FillRectangle(b, e.Bounds);
		TextRenderer.DrawText(e.Graphics, e.Header?.Text, Font, e.Bounds, Settings.Instance.Theme.Foreground, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
	}

	protected override void OnDrawItem(DrawListViewItemEventArgs e)
	{
		if (View != View.Details && e.Item is FileSelectionItem item)
			DrawRowSmallIcon(e.Graphics, item, e.Bounds, e.Item.Selected);
	}

	protected override void OnDrawSubItem(DrawListViewSubItemEventArgs e)
	{
		if (View == View.Details && e is { Item: FileSelectionItem item, ColumnIndex: 0 })
			DrawRowDetails(e.Graphics, item, item.Bounds, item.Selected);
	}

	private void DrawRowDetails(Graphics g, FileSelectionItem item, Rectangle bounds, bool selected)
	{
		var theme = Settings.Instance.Theme;

		if (selected)
		{
			using var selBrush = new SolidBrush(theme.BackActive);
			g.FillRectangle(selBrush, bounds);
		}
		else
		{
			using var bgBrush = new SolidBrush(theme.BackContent);
			g.FillRectangle(bgBrush, bounds);
		}

		if (item.Icon != null)
			g.DrawImage(item.Icon, bounds.Left + 3, bounds.Top + (bounds.Height - 16) / 2, 16, 16);

		var textColor = selected ? theme.BackContent : theme.Foreground;
		var font = Font;
		var x = bounds.Left + 24;
		var y = bounds.Top + (bounds.Height - font.Height) / 2;

		switch (item)
		{
			case FileItem file:
				TextRenderer.DrawText(g, file.FileName, font, new Point(x, y), textColor, TextFormatFlags.Left);
				x += 180;
				TextRenderer.DrawText(g, file.Extension, font, new Point(x, y), textColor, TextFormatFlags.Left);
				x += 60;
				TextRenderer.DrawText(g, file.Size.FormatFileSize(), font, new Point(x + 70, y), textColor, TextFormatFlags.Right);
				x += 80;
				TextRenderer.DrawText(g, file.Creation.ToString("yyyy-MM-dd"), font, new Point(x, y), textColor, TextFormatFlags.Left);
				x += 90;
				TextRenderer.DrawText(g, file.Creation.ToString("HH:mm"), font, new Point(x, y), textColor, TextFormatFlags.Left);
				x += 70;
				TextRenderer.DrawText(g, file.Attributes.ToString(), font, new Point(x, y), textColor, TextFormatFlags.Left);
				break;
			case DriveItem drive:
			{
				TextRenderer.DrawText(g, drive.DriveName, font, new Point(x, y), textColor, TextFormatFlags.Left);
				x += 100;
				TextRenderer.DrawText(g, drive.Label, font, new Point(x, y), textColor, TextFormatFlags.Left);
				x += 100;
				var percent = drive.Total > 0 ? (float)drive.Available / drive.Total : 0f;
				var barRect = new Rectangle(x, y + font.Height / 4, 80, font.Height / 2);
				using (var barBg = new SolidBrush(theme.Border))
					g.FillRectangle(barBg, barRect);
				using (var barFg = new SolidBrush(theme.BackActive))
					g.FillRectangle(barFg, new Rectangle(barRect.Left, barRect.Top, (int)(barRect.Width * percent), barRect.Height));
				x += 90;
				TextRenderer.DrawText(g, drive.Available.FormatFileSize() + " 남음", font, new Point(x, y), textColor, TextFormatFlags.Right);
				break;
			}
			case UnknownItem unk:
				TextRenderer.DrawText(g, unk.UnknownDesc, font, new Point(x, y), textColor, TextFormatFlags.Left);
				break;
		}
	}

	private void DrawRowSmallIcon(Graphics g, FileSelectionItem item, Rectangle bounds, bool selected)
	{
		var theme = Settings.Instance.Theme;

		if (selected)
		{
			using var selBrush = new SolidBrush(theme.BackActive);
			g.FillRectangle(selBrush, bounds);
		}
		else
		{
			using var bgBrush = new SolidBrush(theme.BackContent);
			g.FillRectangle(bgBrush, bounds);
		}

		if (item.Icon != null)
			g.DrawImage(item.Icon, bounds.Left + 3, bounds.Top + (bounds.Height - 16) / 2, 16, 16);

		var textColor = selected ? theme.BackContent : theme.Foreground;
		var font = Font;
		var x = bounds.Left + 24;
		var y = bounds.Top + (bounds.Height - font.Height) / 2;

		switch (item)
		{
			case FileItem file:
			{
				var text = file.FileName;
				if (!string.IsNullOrEmpty(file.Extension))
					text += "." + file.Extension;
				text += "  " + file.Size.FormatFileSize();
				TextRenderer.DrawText(g, text, font, new Point(x, y), textColor, TextFormatFlags.Left);
				break;
			}
			case DriveItem drive:
			{
				var text = drive.DriveName;
				if (!string.IsNullOrEmpty(drive.Label))
					text += "  " + drive.Label;
				TextRenderer.DrawText(g, text, font, new Point(x, y), textColor, TextFormatFlags.Left);
				x += 180;
				var percent = drive.Total > 0 ? (float)drive.Available / drive.Total : 0f;
				var barRect = new Rectangle(x, y + font.Height / 4, 80, font.Height / 2);
				using (var barBg = new SolidBrush(theme.Border))
					g.FillRectangle(barBg, barRect);
				using (var barFg = new SolidBrush(theme.BackActive))
					g.FillRectangle(barFg, new Rectangle(barRect.Left, barRect.Top, (int)(barRect.Width * percent), barRect.Height));
				x += 90;
				TextRenderer.DrawText(g, drive.Available.FormatFileSize() + " 남음", font, new Point(x, y), textColor, TextFormatFlags.Left);
				break;
			}
			case UnknownItem unk:
				TextRenderer.DrawText(g, unk.UnknownDesc, font, new Point(x, y), textColor, TextFormatFlags.Left);
				break;
		}
	}

	private int _anchorIndex = -1; // Shift+선택의 기준 인덱스

	private static bool IsSelectable(ListViewItem item) => item switch
	{
		FileItem { IsDirectory: true, FileName: ".." } or DriveItem => false,
		_ => true
	};

	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);

		if (e.KeyCode is Keys.Space or Keys.Insert)
		{
			if (FocusedItem != null && IsSelectable(FocusedItem))
			{
				FocusedItem.Selected = !FocusedItem.Selected;
				_anchorIndex = FocusedItem.Index;
			}
			e.Handled = true;
		}
		else if (e is { Shift: true, KeyCode: Keys.Up or Keys.Down })
		{
			if (FocusedItem != null)
			{
				if (_anchorIndex == -1)
					_anchorIndex = FocusedItem.Index;
				int newIndex = FocusedItem.Index + (e.KeyCode == Keys.Up ? -1 : 1);
				if (newIndex >= 0 && newIndex < Items.Count && IsSelectable(Items[newIndex]))
				{
					Items[newIndex].Focused = true;
					Items[newIndex].EnsureVisible();
					SelectRange(_anchorIndex, newIndex);
				}
			}
			e.Handled = true;
		}
		else if (e is { Shift: false, KeyCode: Keys.Up or Keys.Down })
		{
			if (FocusedItem != null)
				_anchorIndex = FocusedItem.Index;
		}
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		Focus();

		var hit = HitTest(e.Location);
		if (hit.Item != null && IsSelectable(hit.Item))
		{
			var idx = hit.Item.Index;
			if (e.Button == MouseButtons.Left)
			{
				if (ModifierKeys.HasFlag(Keys.Shift))
				{
					if (_anchorIndex == -1)
						_anchorIndex = idx;
					SelectRange(_anchorIndex, idx);
				}
				else if (ModifierKeys.HasFlag(Keys.Control))
				{
					hit.Item.Selected = !hit.Item.Selected;
					_anchorIndex = idx;
				}
				else
				{
					_anchorIndex = idx;
				}
				FocusedItem = hit.Item;
			}
		}

		base.OnMouseDown(e);
	}

	private void SelectRange(int from, int to)
	{
		if (from > to)
			(from, to) = (to, from);
		for (var i = 0; i < Items.Count; i++)
			Items[i].Selected = (i >= from && i <= to && IsSelectable(Items[i]));
	}

	public class FileSelectionItem : ListViewItem
	{
		public Image? Icon { get; protected set; }
	}

	public class FileItem : FileSelectionItem
	{
		public FileSystemInfo Info { get; }

		public string FullName { get; }
		public string FileName { get; }
		public string Extension { get; }
		public long Size { get; }
		public DateTime Creation { get; }
		public FileAttributes Attributes { get; }
		public bool IsDirectory { get; }

		public FileItem(FileInfo fileInfo)
		{
			Info = fileInfo;
			FullName = fileInfo.FullName;
			FileName = fileInfo.Name;
			Extension = fileInfo.Extension.TrimStart('.');
			Size = fileInfo.Length;
			Creation = fileInfo.CreationTime;
			Attributes = fileInfo.Attributes;
			IsDirectory = false;
			Icon = IconCache.Instance.GetIcon(FullName, Extension, IsDirectory);
		}

		public FileItem(DirectoryInfo directoryInfo)
		{
			Info = directoryInfo;
			FullName = directoryInfo.FullName;
			FileName = directoryInfo.Name;
			Extension = string.Empty; // 디렉토리는 확장자가 없음
			Size = 0; // 디렉토리는 크기가 없음
			Creation = directoryInfo.CreationTime;
			Attributes = directoryInfo.Attributes;
			IsDirectory = true;
			Icon = IconCache.Instance.GetIcon(FullName, string.Empty, true);
		}
	}

	public class DriveItem : FileSelectionItem
	{
		public DriveInfo Info { get; }

		public string DriveName { get; }
		public string Label { get; }
		public long Total { get; }
		public long Available { get; }

		public DriveItem(DriveInfo driveInfo)
		{
			Info = driveInfo;
			DriveName = driveInfo.Name.TrimEnd('\\');
			Label = driveInfo.VolumeLabel;
			Total = driveInfo.TotalSize;
			Available = driveInfo.AvailableFreeSpace;
			Icon = IconCache.Instance.GetIcon(DriveName, string.Empty, false, true);
		}
	}

	public class UnknownItem : FileSelectionItem
	{
		public string UnknownDesc { get; }

		public UnknownItem(string unknownDesc)
		{
			UnknownDesc = unknownDesc;
			Icon = Resources.unknownfile16; // 기본 아이콘 설정
		}
	}
}
