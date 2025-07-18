namespace DuFile.Dowa;

internal static class Alter
{
	private static readonly string[] SizeSuffixes = ["B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"];

	public static string FormatFileSize(this long bytes)
	{
		double len = bytes;
		var order = 0;
		while (len>=1024 && order < SizeSuffixes.Length - 1)
		{
			order++;
			len /= 1024;
		}
		return $"{len:0.##} {SizeSuffixes[order]}";
	}

	// 색상 혼합(알파 블렌딩)
	public static Color BlendColor(Color c1, Color c2, float factor)
	{
		return Color.FromArgb(
			(int)(c1.R + (c2.R - c1.R) * factor),
			(int)(c1.G + (c2.G - c1.G) * factor),
			(int)(c1.B + (c2.B - c1.B) * factor)
		);
	}
}
