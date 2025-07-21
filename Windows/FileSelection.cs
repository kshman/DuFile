// ReSharper disable MissingXmlDoc

namespace DuFile.Windows;

/// <summary>
/// 파일/드라이브/알수없음 항목을 다양한 모드로 커스텀 그리기하는 리스트뷰 컨트롤입니다.
/// </summary>
public sealed class FileSelection : ListView
{
	public FileSelection()
	{
		OwnerDraw = true;
		FullRowSelect = true;
		View = View.Details;
		HideSelection = false;
		DoubleBuffered = true;

		// 기본 컬럼 구성 (자세히 보기 모드)
		Columns.Add("", 22); // 아이콘
		Columns.Add("파일이름", 180);
		Columns.Add("확장자", 60);
		Columns.Add("파일크기", 80, HorizontalAlignment.Right);
		Columns.Add("만든날짜", 90);
		Columns.Add("만든시간", 70);
		Columns.Add("속성", 60);

		var settings = Settings.Instance;
		var theme = settings.Theme;

		Font = new Font(settings.FileFontFamily, settings.FileFontSize, FontStyle.Regular, GraphicsUnit.Point);
		BackColor = theme.BackContent;
		ForeColor = theme.Foreground;
	}

	public void AddFileItem(FileInfo fileinfo)
	{
		var item = new FilItem(fileinfo);
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
			case FilItem file:
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
				TextRenderer.DrawText(g, file.Attribute.ToString(), font, new Point(x, y), textColor, TextFormatFlags.Left);
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
			case FilItem file:
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

	public class FileSelectionItem : ListViewItem
	{
		public Image? Icon { get; protected set; }
	}

	public class FilItem : FileSelectionItem
	{
		public FileInfo FileInfo { get; }

		public string FullName => FileInfo.FullName;
		public string FileName => FileInfo.Name;
		public string Extension => FileInfo.Extension.TrimStart('.');
		public long Size => FileInfo.Length;
		public DateTime Creation => FileInfo.CreationTime;
		public FileAttributes Attribute => FileInfo.Attributes;
		public bool IsDirectory => FileInfo.Attributes.HasFlag(FileAttributes.Directory);

		public FilItem(FileInfo fileInfo)
		{
			FileInfo = fileInfo;
			Icon = IconCache.Instance.GetIcon(FullName, Extension, IsDirectory);
		}
	}

	public class DriveItem : FileSelectionItem
	{
		public DriveInfo DriveInfo { get; }

		public string DriveName => DriveInfo.Name;
		public string Label => DriveInfo.VolumeLabel;
		public long Total => DriveInfo.TotalSize;
		public long Available => DriveInfo.AvailableFreeSpace;

		public DriveItem(DriveInfo driveInfo)
		{
			DriveInfo = driveInfo;
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
