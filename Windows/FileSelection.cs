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
		BackColor = theme.Content;
		ForeColor = theme.Foreground;
	}

	protected override void OnDrawColumnHeader(DrawListViewColumnHeaderEventArgs e)
	{
		using var b = new SolidBrush(Settings.Instance.Theme.Background);
		using var f = new SolidBrush(Settings.Instance.Theme.Foreground);
		e.Graphics.FillRectangle(b, e.Bounds);
		TextRenderer.DrawText(e.Graphics, e.Header.Text, Font, e.Bounds, Settings.Instance.Theme.Foreground, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
	}

	protected override void OnDrawItem(DrawListViewItemEventArgs e)
	{
		if (View == View.Details)
			return;

		DrawRowSmallIcon(e.Graphics, e.Item, e.Bounds, e.Item.Selected);
	}

	protected override void OnDrawSubItem(DrawListViewSubItemEventArgs e)
	{
		if (View != View.Details)
			return;

		if (e.ColumnIndex == 0)
			DrawRowDetails(e.Graphics, e.Item, e.Item.Bounds, e.Item.Selected);
	}

	private void DrawRowDetails(Graphics g, ListViewItem item, Rectangle bounds, bool selected)
	{
		var tag = item.Tag;
		var theme = Settings.Instance.Theme;

		if (selected)
		{
			using var selBrush = new SolidBrush(theme.Accent);
			g.FillRectangle(selBrush, bounds);
		}
		else
		{
			using var bgBrush = new SolidBrush(theme.Content);
			g.FillRectangle(bgBrush, bounds);
		}

		Image? icon = tag switch
		{
			ItemFile file => IconCache.Instance.GetIcon(file.Name + file.Extension),
			ItemDrive drive => IconCache.Instance.GetIcon(drive.Name, isDrive: true),
			_ => Resources.unknownfile16
		};

		if (icon != null)
		{
			g.DrawImage(icon, bounds.Left + 3, bounds.Top + (bounds.Height - 16) / 2, 16, 16);
		}

		var textColor = selected ? theme.Content : theme.Foreground;
		var font = Font;
		int x = bounds.Left + 24;
		int y = bounds.Top + (bounds.Height - font.Height) / 2;

		if (tag is ItemFile fileItem)
		{
			TextRenderer.DrawText(g, fileItem.Name, font, new Point(x, y), textColor, TextFormatFlags.Left);
			x += 180;
			TextRenderer.DrawText(g, fileItem.Extension, font, new Point(x, y), textColor, TextFormatFlags.Left);
			x += 60;
			TextRenderer.DrawText(g, fileItem.Size.FormatFileSize(), font, new Point(x + 70, y), textColor, TextFormatFlags.Right);
			x += 80;
			TextRenderer.DrawText(g, fileItem.Creation.ToString("yyyy-MM-dd"), font, new Point(x, y), textColor, TextFormatFlags.Left);
			x += 90;
			TextRenderer.DrawText(g, fileItem.Creation.ToString("HH:mm:ss"), font, new Point(x, y), textColor, TextFormatFlags.Left);
			x += 70;
			TextRenderer.DrawText(g, fileItem.Attribute.ToString(), font, new Point(x, y), textColor, TextFormatFlags.Left);
		}
		else if (tag is ItemDrive driveItem)
		{
			TextRenderer.DrawText(g, driveItem.Name, font, new Point(x, y), textColor, TextFormatFlags.Left);
			x += 100;
			TextRenderer.DrawText(g, driveItem.Label, font, new Point(x, y), textColor, TextFormatFlags.Left);
			x += 100;
			float percent = driveItem.Total > 0 ? (float)driveItem.Available / driveItem.Total : 0f;
			var barRect = new Rectangle(x, y + font.Height / 4, 80, font.Height / 2);
			using (var barBg = new SolidBrush(theme.Border))
				g.FillRectangle(barBg, barRect);
			using (var barFg = new SolidBrush(theme.Accent))
				g.FillRectangle(barFg, new Rectangle(barRect.Left, barRect.Top, (int)(barRect.Width * percent), barRect.Height));
			x += 90;
			TextRenderer.DrawText(g, driveItem.Available.FormatFileSize() + " 남음", font, new Point(x, y), textColor, TextFormatFlags.Right);
		}
		else if (tag is ItemUnknown unknownItem)
		{
			TextRenderer.DrawText(g, unknownItem.Name, font, new Point(x, y), textColor, TextFormatFlags.Left);
		}
	}

	private void DrawRowSmallIcon(Graphics g, ListViewItem item, Rectangle bounds, bool selected)
	{
		object? tag = item.Tag;
		var theme = Settings.Instance.Theme;

		if (selected)
		{
			using var selBrush = new SolidBrush(theme.Accent);
			g.FillRectangle(selBrush, bounds);
		}
		else
		{
			using var bgBrush = new SolidBrush(theme.Content);
			g.FillRectangle(bgBrush, bounds);
		}

		Image? icon = tag switch
		{
			ItemFile file => IconCache.Instance.GetIcon(file.Name + file.Extension),
			ItemDrive drive => IconCache.Instance.GetIcon(drive.Name, isDrive: true),
			_ => Resources.unknownfile16
		};

		if (icon != null)
		{
			g.DrawImage(icon, bounds.Left + 3, bounds.Top + (bounds.Height - 16) / 2, 16, 16);
		}

		var textColor = selected ? theme.Content : theme.Foreground;
		var font = Font;
		int x = bounds.Left + 24;
		int y = bounds.Top + (bounds.Height - font.Height) / 2;

		if (tag is ItemFile fileItem)
		{
			string text = fileItem.Name;
			if (!string.IsNullOrEmpty(fileItem.Extension))
				text += "." + fileItem.Extension;
			text += "  " + fileItem.Size.FormatFileSize();
			TextRenderer.DrawText(g, text, font, new Point(x, y), textColor, TextFormatFlags.Left);
		}
		else if (tag is ItemDrive driveItem)
		{
			string text = driveItem.Name;
			if (!string.IsNullOrEmpty(driveItem.Label))
				text += "  " + driveItem.Label;
			TextRenderer.DrawText(g, text, font, new Point(x, y), textColor, TextFormatFlags.Left);
			x += 180;
			float percent = driveItem.Total > 0 ? (float)driveItem.Available / driveItem.Total : 0f;
			var barRect = new Rectangle(x, y + font.Height / 4, 80, font.Height / 2);
			using (var barBg = new SolidBrush(theme.Border))
				g.FillRectangle(barBg, barRect);
			using (var barFg = new SolidBrush(theme.Accent))
				g.FillRectangle(barFg, new Rectangle(barRect.Left, barRect.Top, (int)(barRect.Width * percent), barRect.Height));
			x += 90;
			TextRenderer.DrawText(g, driveItem.Available.FormatFileSize() + " 남음", font, new Point(x, y), textColor, TextFormatFlags.Left);
		}
		else if (tag is ItemUnknown unknownItem)
		{
			TextRenderer.DrawText(g, unknownItem.Name, font, new Point(x, y), textColor, TextFormatFlags.Left);
		}
	}

	// 데이터 모델 예시
	public class ItemFile
	{
		public string Name = string.Empty;
		public string Extension = string.Empty;
		public long Size;
		public DateTime Creation;
		public int Attribute;
	}

	public class ItemDrive
	{
		public string Name = string.Empty;
		public string Label = string.Empty;
		public long Total;
		public long Available;
	}

	public class ItemUnknown
	{
		public string Name = string.Empty;
	}
}
