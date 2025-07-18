namespace DuFile.Cuscon;

/// <summary>
/// 기능 키(F2~F9 등) 버튼을 한 줄로 배열하는 커스텀 컨트롤입니다.
/// 각 버튼은 커맨드 또는 외부 실행 파일과 연결할 수 있습니다.
/// </summary>
public sealed class FuncBar : Control
{
	/// <summary>
	/// 각 기능 버튼의 정의를 나타냅니다.
	/// </summary>
	private record FuncDef(string Key, string Command, string External);

	// F2~F9 키 및 연결 명령 정의
	private readonly FuncDef[] _funcDefs =
	[
		new("F2", "#Rename", string.Empty),
		new("F3", "#View", string.Empty),
		new("F4", "#Edit", string.Empty),
		new("F5", "#Copy", string.Empty),
		new("F6", "#Move", string.Empty),
		new("F7", "#NewDirectory", string.Empty),
		new("F8", "#Trash", string.Empty),
		new("F9", "#Console", string.Empty)
	];

	/// <summary>
	/// 버튼 높이(픽셀)
	/// </summary>
	[Category("FuncBar")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	public int ButtonHeight { get; set; } = 25;

	// 마우스 오버/클릭 상태 관리
	private int? _hoverIndex;
	private int? _pressedIndex;

	/// <summary>
	/// 버튼 클릭 시 발생하는 이벤트입니다.
	/// </summary>
	public event EventHandler<FuncBarButtonClickedEventArgs>? ButtonClicked;

	/// <inheritdoc/>
	public FuncBar()
	{
		SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);

		// 테마 색상 및 폰트는 Settings.Instance에서 가져옵니다.
		var settings = Settings.Instance;
		var theme = settings.Theme;

		Dock = DockStyle.Bottom;
		Height = ButtonHeight;
		BackColor = theme.Border;
		ForeColor = theme.Foreground;

		DoubleBuffered = true;
	}

	/// <inheritdoc/>
	protected override void OnResize(EventArgs e)
	{
		base.OnResize(e);
		// 컨트롤 크기 변경 시 버튼 높이 재설정
		Height = ButtonHeight;
		Invalidate();
	}

	/// <inheritdoc/>
	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);

		var settings = Settings.Instance;
		var theme = settings.Theme;
		using var font = new Font(settings.UiFontFamily, settings.UiFontSize, FontStyle.Bold, GraphicsUnit.Point);

		var buttonCount = _funcDefs.Length;
		var buttonWidth = Width / buttonCount;
		var extraWidth = Width / buttonCount;
		var height = ButtonHeight;

		for (var i = 0; i < buttonCount; i++)
		{
			var w = buttonWidth + (i == buttonCount - 1 ? extraWidth : 0);
			var rect = new Rectangle(i * buttonWidth, 0, w, height);
			var def = _funcDefs[i];

			var fill =
				_pressedIndex == i ? theme.Accent :
				_hoverIndex == i ? theme.Hover :
				theme.Border;

			using (var brush = new SolidBrush(fill))
				e.Graphics.FillRectangle(brush, rect);

			using (var pen = new Pen(theme.Border, 1))
				e.Graphics.DrawRectangle(pen, rect);

			var accel = $" {def.Key}";
			TextRenderer.DrawText(e.Graphics, accel, font, rect, theme.Accelerator,
				TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
			var accelSize = TextRenderer.MeasureText(accel, font);

			var text = !string.IsNullOrEmpty(def.Command)
				? Command.Definition.FriendlyName(def.Command)
				: Path.GetFileName(def.External);
			var textRect = new Rectangle(rect.Left + accelSize.Width, rect.Top, rect.Width - accelSize.Width, rect.Height);
			TextRenderer.DrawText(e.Graphics, text, font, textRect, theme.Foreground,
				TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
		}
	}

	/// <inheritdoc/>
	protected override void OnMouseLeave(EventArgs e)
	{
		base.OnMouseLeave(e);
		_hoverIndex = null;
		Invalidate();
	}

	/// <inheritdoc/>
	protected override void OnMouseDown(MouseEventArgs e)
	{
		base.OnMouseDown(e);
		var index = GetButtonIndexAtPoint(e.Location);
		_pressedIndex = index;
		Invalidate();
	}

	/// <inheritdoc/>
	protected override void OnMouseUp(MouseEventArgs e)
	{
		base.OnMouseUp(e);

		var index = GetButtonIndexAtPoint(e.Location);
		if (_pressedIndex != null && index == _pressedIndex)
		{
			var def = _funcDefs[_pressedIndex.Value];
			ButtonClicked?.Invoke(this, new FuncBarButtonClickedEventArgs(_pressedIndex.Value, def.Command, def.External));
		}
		_pressedIndex = null;
		Invalidate();
	}

	/// <inheritdoc/>
	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);

		var prevHover = _hoverIndex;
		_hoverIndex = GetButtonIndexAtPoint(e.Location);

		if (_hoverIndex != prevHover)
			Invalidate();
	}

	/// <summary>
	/// 지정한 좌표에 해당하는 버튼 인덱스를 반환합니다.
	/// </summary>
	private int? GetButtonIndexAtPoint(Point p)
	{
		var buttonCount = _funcDefs.Length;
		var buttonWidth = Width / buttonCount;
		var height = ButtonHeight;

		for (var i = 0; i < buttonCount; i++)
		{
			var rect = new Rectangle(i * buttonWidth, 0, buttonWidth, height);
			if (rect.Contains(p))
				return i;
		}
		return null;
	}

	/// <summary>
	/// 버튼의 커맨드를 변경합니다.
	/// </summary>
	/// <param name="index">버튼 인덱스(0~7)</param>
	/// <param name="command">새 커맨드 문자열</param>
	public void SetFuncCommand(int index, string command)
	{
		if (index < 0 || index >= _funcDefs.Length)
			throw new ArgumentOutOfRangeException(nameof(index), "Index must be between 0 and 7.");

		_funcDefs[index] = new FuncDef(_funcDefs[index].Key, command, string.Empty);
		Invalidate();
	}

	/// <summary>
	/// 버튼에 외부 실행 파일을 등록합니다.
	/// </summary>
	/// <param name="index">버튼 인덱스(0~7)</param>
	/// <param name="external">실행 파일 경로</param>
	public void SetFuncExternal(int index, string external)
	{
		if (index < 0 || index >= _funcDefs.Length)
			throw new ArgumentOutOfRangeException(nameof(index), "Index must be between 0 and 7.");
		if (string.IsNullOrEmpty(external))
			throw new ArgumentException("External command cannot be null or empty.", nameof(external));

		_funcDefs[index] = new FuncDef(_funcDefs[index].Key, string.Empty, external);
		Invalidate();
	}
}

/// <summary>
/// FuncBar 버튼 클릭 이벤트 데이터
/// </summary>
public class FuncBarButtonClickedEventArgs(int index, string command, string external) : EventArgs
{
	/// <summary>버튼 인덱스</summary>
	public int Index { get; } = index;

	/// <summary>커맨드 문자열(없으면 "")</summary>
	public string Command { get; } = command;

	/// <summary>외부 실행 문자열(없으면 "")</summary>
	public string External { get; } = external;
}
