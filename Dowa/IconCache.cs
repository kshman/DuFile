using System.Runtime.InteropServices;
// ReSharper disable InconsistentNaming

namespace DuFile.Dowa;

internal class IconCache
{
	private static IconCache? _instance;
	private readonly Dictionary<string, Bitmap?> _cache = [];

	public static IconCache Instance => _instance ??= new IconCache();

	private IconCache()
	{
		// Private constructor to enforce singleton pattern
	}

	public void Clear()
	{
		_cache.Clear();
	}

	public Bitmap? GetIcon(string path, bool isDirectory = false, bool isDrive = false)
	{
		string key;
		if (isDrive)
			key = $"drive#{path}";
		else if (isDirectory)
			key = "directory";
		else if (Path.HasExtension(path))
		{
			var ext = Path.GetExtension(path);
			key =ext is ".exe" or ".ico" ? path : ext;
		}
		else
		{
			// 파일인데 확장가가 없네
			key = "file";
		}

		if (_cache.TryGetValue(key, out var cachedIcon))
			return cachedIcon;

		var bmp = ExtractIcon(path, isDirectory);
		_cache.Add(key, bmp);
		return bmp;
	}

	// Win32 API 선언
#pragma warning disable SYSLIB1054
	[DllImport("shell32.dll", CharSet = CharSet.Unicode)]
	private static extern IntPtr SHGetFileInfo(string pszPath, int dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);
#pragma warning restore SYSLIB1054

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	private struct SHFILEINFO
	{
		public IntPtr hIcon;
		public int iIcon;
		public uint dwAttributes;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
		public string szDisplayName;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
		public string szTypeName;
	}

	// SHGetFileInfo에서 사용하는 플래그 상수
	private const uint SHGFI_ICON = 0x000000100;
	private const uint SHGFI_SMALLICON = 0x000000001;
	private const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;

	private static Bitmap? ExtractIcon(string path, bool isDirectory)
	{
		try
		{
			var shinfo = new SHFILEINFO();
			var attr = 0;
			var flags = SHGFI_ICON | SHGFI_SMALLICON;

			if (isDirectory)
			{
				// 디렉토리는 속성 사용
				attr = 0x00000010; // FILE_ATTRIBUTE_DIRECTORY
				flags |= SHGFI_USEFILEATTRIBUTES;
			}

			var ret = SHGetFileInfo(path, attr, ref shinfo, (uint)Marshal.SizeOf(shinfo), flags);
			return ret == IntPtr.Zero ? null : Bitmap.FromHicon(shinfo.hIcon);
		}
		catch
		{
			return null;
		}

		// 뭐랄까 확장자만으로 얻는 방법을 해보면 좋을 것이다.
		// 플래스에 SHGFI_USEFILEATTRIBUTES를 지정하고
		// 속성에 0x80(FILE_ATTRIBUTE_NORMAL)로 지정하면
		// 파일 아이콘을 얻을 수 있다.
	}
}
