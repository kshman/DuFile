namespace DuFile.Windows;

/// <summary>
/// 원하는 색상과 두께로 외곽선을 그릴 수 있는 커스텀 패널 컨트롤입니다.
/// </summary>
public class EkePanel : Panel
{
	/// <summary>
	/// 외곽선 색상을 가져오거나 설정합니다.
	/// </summary>
	[Category("EkePanel")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	public Color BorderColor { get; set; } = Color.Gray;

	/// <summary>
	/// 외곽선 두께(픽셀)를 가져오거나 설정합니다.
	/// </summary>
	[Category("EkePanel")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	public int BorderThickness { get; set; } = 1;

	/// <summary>
	/// EkePanel의 인스턴스를 초기화합니다.
	/// </summary>
	public EkePanel()
	{
		SetStyle(ControlStyles.UserPaint, true);
	}

	/// <inheritdoc/>
	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		using var borderPen = new Pen(BorderColor, BorderThickness);
		e.Graphics.DrawRectangle(borderPen, 0, 0, Width - BorderThickness, Height - BorderThickness);
	}
}
