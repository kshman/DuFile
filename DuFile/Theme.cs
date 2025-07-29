// ReSharper disable MissingXmlDoc
namespace DuFile;

/// <summary>
/// 테마를 정의하는 클래스입니다.
/// </summary>
public class Theme
{
	public Color Background { get; set; } = Color.FromArgb(37, 37, 37);
	public Color BackHover { get; set; } = Color.FromArgb(0, 122, 204);
	public Color BackActive { get; set; } = Color.FromArgb(0, 150, 136);
	public Color BackSelection { get; set; } = Color.FromArgb(63, 63, 70);
	public Color BackContent { get; set; } = Color.FromArgb(20, 20, 20);

	public Color Foreground { get; set; } = Color.FromArgb(241, 241, 241);
	public Color Focus { get; set; } = Color.FromArgb(20, 20, 20);
	public Color Accelerator { get; set; } = Color.FromArgb(255, 128, 64);
	public Color File { get; set; } = Color.FromArgb(192, 192, 192);
	public Color Drive { get; set; } = Color.FromArgb(0x80, 0x80, 0xFF);
	public Color Folder { get; set; } = Color.FromArgb(0xea, 0x22, 0x22);
	public Color Hidden { get; set; } = Color.FromArgb(255, 0, 0);
	public Color ReadOnly { get; set; } = Color.FromArgb(255, 255, 0);
	public Color DebugLine { get; set; } = Color.FromArgb(255, 255, 0);
	public Color Border { get; set; } = Color.FromArgb(63, 63, 70);

	public string UiFontFamily { get; set; } = "맑은 고딕";
	public float UiFontSize { get; set; } = 10.0f;

	public string ContentFontFamily { get; set; } = "맑은 고딕";
	public float ContentFontSize { get; set; } = 10.0f;

	internal readonly Dictionary<string, Color> ColorExtension = [];
	internal readonly Dictionary<string, Color> ColorSize = [];

	public Theme()
	{
		// Initialize any additional properties or settings if needed
	}

	// 확장자 색깔 얻기
	public Color GetColorExtension(string ext) =>
		ColorExtension.GetValueOrDefault(ext.ToUpperInvariant(), Foreground);

	// 크기 색깔 얻기
	public Color GetColorSize(string suffix) =>
		ColorSize.GetValueOrDefault(suffix, File);
}

/// <summary>
/// Provides a method to update the current theme of the application.
/// </summary>
public interface IThemeUpate
{
	void UpdateTheme(Theme theme);
}
