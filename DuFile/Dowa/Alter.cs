using System.Runtime.InteropServices;
using System.Text;

namespace DuFile.Dowa;

internal static class Alter
{
	private static readonly string[] SizeSuffixes = ["", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"];

	// 파일 크기 숫자 부분만 변환
	private static string SizeFormatNumber(double size) => size switch
	{
		>= 100 => Math.Round(size).ToString("0"),
		>= 10 => Math.Round(size, 1).ToString("0.0"),
		_ => Math.Round(size, 2).ToString("0.00")
	};

	// 크기를 사람이 읽을 수 있는 형식으로 변환
	public static string FormatFileSize(this long bytes)
	{
		if (bytes < 1024)
			return $"{bytes}";
		double len = bytes;
		var order = 0;
		while (len >= 1024 && order < SizeSuffixes.Length - 1)
		{
			order++;
			len /= 1024;
		}
		return $"{SizeFormatNumber(len)} {SizeSuffixes[order]}";
	}

	// 파일 리스트를 위한 파일 크기 포맷
	public static string FormatFileSize(long bytes, out string suffix)
	{
		if (bytes < 1024)
		{
			suffix = string.Empty;
			return $"{bytes}";
		}
		double len = bytes;
		var order = 0;
		while (len >= 1024 && order < SizeSuffixes.Length - 1)
		{
			order++;
			len /= 1024;
		}
		suffix = SizeSuffixes[order];
		return SizeFormatNumber(len);
	}

	// 파일 속성을 사람이 읽을 수 있는 형식으로 변환
	public static string FormatString(this FileAttributes attributes)
	{
		var sb = new StringBuilder();
		sb.Append(attributes.HasFlag(FileAttributes.Archive) ? 'A' : '_');
		sb.Append(attributes.HasFlag(FileAttributes.Hidden) ? 'H' : '_');
		sb.Append(attributes.HasFlag(FileAttributes.ReadOnly) ? 'R' : '_');
		sb.Append(attributes.HasFlag(FileAttributes.System) ? 'S' : '_');
		return sb.ToString();
	}

	// 날짜를 오늘 기준 30일 내 상대 문구로 변환
	public static string FormatRelativeDate(this DateTime date)
	{
		var now = DateTime.Now.Date;
		var diff = now - date.Date;
		return diff.Days switch
		{
			< 1 => "오늘",
			< 2 => "어제",
			< 3 => "그저께",
			< 31 => $"{(int)diff.TotalDays}일 전",
			_ => date.ToString("yyyy-MM-dd")
		};
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

	// 키 입력 문자열을 키 조합으로 변환
	public static Keys ParseKeyString(KeysConverter converter, string input)
	{
		try
		{
			var keys = (Keys)(converter.ConvertFromString(input) ?? Keys.None);
			return keys;
		}
		catch
		{
			Debugs.WriteLine($"잘못된 키 변환: {input}");
			return Keys.None;
		}
	}

	// 문자열 분리기
	public static string[] SplitWithSeparator(this string input, char separator = ',')
	{
		// var tagList = tags.Split('|').Where(s => !string.IsNullOrEmpty(s))
		return string.IsNullOrWhiteSpace(input) ? 
			[] : 
			input.Split(separator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
	}

	// 자연스러운 파일명 비교 (Windows 탐색기와 동일)
	public static int CompareNatualFilename(string? left, string? right)
	{
		if (ReferenceEquals(left, right)) return 0;
		if (left is null) return -1;
		if (right is null) return 1;

		// Win32 API로 비교 (LOCALE_NAME_USER_DEFAULT, 대소문자 무시, 숫자 자연 정렬)
		var result = CompareStringEx(
			"", // LOCALE_NAME_USER_DEFAULT
			NORM_IGNORECASE | SORT_DIGITSASNUMBERS,
			left, left.Length,
			right, right.Length,
			IntPtr.Zero, IntPtr.Zero, 0);

		// CompareStringEx는 1(less), 2(equal), 3(greater) 반환
		return result switch
		{
			1 => -1,
			2 => 0,
			3 => 1,
			_ => string.Compare(left, right, StringComparison.OrdinalIgnoreCase)
		};
	}

	// 휴지통으로 이동 (SHFileOperation 사용)
	public static bool MoveToRecycleBin(string path)
	{
		var fs = new SHFILEOPSTRUCT
		{
			wFunc = FO_DELETE,
			pFrom = path + "\0\0",
			fFlags = FOF_ALLOWUNDO | FOF_NOCONFIRMATION | FOF_SILENT,
		};
		return SHFileOperation(ref fs) == 0;
	}

	// Win32 P/Invoke
#pragma warning disable SYSLIB1054
	[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
	private static extern int CompareStringEx(
		string lpLocaleName, uint dwCmpFlags,
		string lpString1, int cchCount1,
		string lpString2, int cchCount2,
		IntPtr lpVersionInformation, IntPtr lpReserved, int lParam);

	[DllImport("shell32.dll", CharSet = CharSet.Unicode)]
	private static extern int SHFileOperation(ref SHFILEOPSTRUCT fileOp);
#pragma warning restore SYSLIB1054

	private const uint NORM_IGNORECASE = 0x00000001;
	private const uint SORT_DIGITSASNUMBERS = 0x00000008;

	private const int FO_DELETE = 3;
	private const int FOF_ALLOWUNDO = 0x40;
	private const int FOF_NOCONFIRMATION = 0x10;
	private const int FOF_SILENT = 0x4;

	// ReSharper disable once InconsistentNaming
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct SHFILEOPSTRUCT
	{
		public IntPtr hwnd;
		public int wFunc;
		public string pFrom;
		public string pTo;
		public int fFlags;
		public bool fAnyOperationsAborted;
		public IntPtr hNameMappings;
		public string lpszProgressTitle;
	}
}
