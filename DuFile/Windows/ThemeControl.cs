namespace DuFile.Windows;

/// <summary>
/// 테마를 지원하는 컨트롤입니다.
/// </summary>
public abstract class ThemeControl : Control, IThemeUpate
{
	/// <summary>
	/// 디자인 모드 확인
	/// </summary>
	protected bool IsReallyDesignMode => 
		LicenseManager.UsageMode == LicenseUsageMode.Designtime || 
		(Site?.DesignMode ?? false);

	/// <inheritdoc />
	protected override void OnCreateControl()
	{
		base.OnCreateControl();
		OnUpdateTheme(Settings.Instance.Theme);
	}

	/// <summary>
	/// 테마를 업데이트합니다.
	/// </summary>
	/// <param name="theme">설정할 테마</param>
	/// <remarks>
	/// <see cref="OnCreateControl"/>와 다른 점은 <see cref="Control.Invalidate()"/>를 호출한다는 것입니다.
	/// </remarks>
	public void UpdateTheme(Theme theme)
	{
		OnUpdateTheme(theme);
		Invalidate();
	}

	/// <summary>
	/// 테마 색상을 적용합니다.
	/// </summary>
	/// <param name="theme">설정할 테마</param>
	/// <remarks>
	/// <see cref="Control.Font"/>는 <see cref="Theme.UiFontFamily"/>와 <see cref="Theme.UiFontSize"/>,<br/>
	/// <see cref="Control.BackColor"/>는 <see cref="Theme.Background"/>,<br/>
	/// <see cref="Control.ForeColor"/>는 <see cref="Theme.Foreground"/><br/>
	/// 이 값들을 기본으로 설정하므로, 기본값 대신 설정할 때는 부모 메소드를 호출하지 마세요.
	/// </remarks>
	protected virtual void OnUpdateTheme(Theme theme)
	{
		Font = new Font(theme.UiFontFamily, theme.UiFontSize, FontStyle.Regular, GraphicsUnit.Point);
		BackColor = theme.Background;
		ForeColor = theme.Foreground;
	}
}
