namespace DuFile;

internal struct Theme
{
	public Color Foreground = Color.FromArgb(241, 241, 241);
	public Color Background = Color.FromArgb(37, 37, 37);
	public Color BackHover = Color.FromArgb(0, 122, 204);
	public Color BackActive = Color.FromArgb(0, 150, 136);
	public Color BackSelection = Color.FromArgb(63, 63, 70);
	public Color BackContent = Color.FromArgb(20, 20, 20);
	public Color Border = Color.FromArgb(63, 63, 70);

	public Color Accelerator = Color.FromArgb(255, 128, 64);
	public Color Drive = Color.FromArgb(0x80, 0x80, 0xFF);
	public Color Folder = Color.FromArgb(0xea, 0x22, 0x22);
	public Color Hidden = Color.FromArgb(255, 0, 0);
	public Color ReadOnly = Color.FromArgb(255, 255, 0);

	internal readonly Dictionary<string, Color> ColorExtension = [];

	public Theme()
	{
		// Initialize any additional properties or settings if needed
	}

	// 확장자 색깔 얻기
	public Color GetColorExtension(string ext) =>
		ColorExtension.GetValueOrDefault(ext.ToUpperInvariant(), Foreground);
}
