namespace DuFile.Windows;

/// <summary>
/// 기능 키(F2~F9 등) 버튼을 한 줄로 배열하는 커스텀 컨트롤입니다.
/// 각 버튼은 커맨드 또는 외부 실행 파일과 연결할 수 있습니다.
/// </summary>
public sealed class FuncBar : Control
{
	private const int MaxFuncCount = 8;
	private static readonly int[] FuncNumbers = [2, 3, 4, 5, 6, 7, 8, 9];

	private int? _hoverIndex;
	private int? _pressedIndex;
	private ModifierKey _prevModifier;
	private ModifierKey _modifier;

	/// <summary>
	/// 버튼 높이(픽셀)
	/// </summary>
	[Category("FuncBar")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	public int ButtonHeight { get; set; } = 25;

	/// <summary>
	/// 버튼 클릭 시 발생하는 이벤트입니다.
	/// </summary>
	public event EventHandler<FuncBarButtonClickEventArgs>? ButtonClick;

	/// <inheritdoc/>
	public FuncBar()
	{
		Debugs.Assert(MaxFuncCount == FuncNumbers.Length);

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

		var buttonWidth = Width / MaxFuncCount;
		var extraWidth = Width / MaxFuncCount;
		var height = ButtonHeight;

		for (var i = 0; i < MaxFuncCount; i++)
		{
			var w = buttonWidth + (i == MaxFuncCount - 1 ? extraWidth : 0);
			var rect = new Rectangle(i * buttonWidth, 0, w, height);
			var num = FuncNumbers[i];

			var fill =
				_pressedIndex == i ? theme.BackActive :
				_hoverIndex == i ? theme.BackHover :
				theme.Border;

			using (var brush = new SolidBrush(fill))
				e.Graphics.FillRectangle(brush, rect);

			using (var pen = new Pen(theme.Border, 1))
				e.Graphics.DrawRectangle(pen, rect);

			var accel = $" F{num}";
			TextRenderer.DrawText(e.Graphics, accel, font, rect, theme.Accelerator,
				TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
			var accelSize = TextRenderer.MeasureText(accel, font);

			var cmd = settings.GetFuncKeyCommand(num, _modifier);
			if (string.IsNullOrEmpty(cmd))
				continue;

			var text = cmd[0] == '#' ? Commands.ToFriendlyName(cmd) : Path.GetFileName(cmd);
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
			var num = FuncNumbers[_pressedIndex.Value];
			var cmd = Settings.Instance.GetFuncKeyCommand(num, _modifier);
			ButtonClick?.Invoke(this, new FuncBarButtonClickEventArgs(_pressedIndex.Value, num, cmd));
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
		var buttonWidth = Width / MaxFuncCount;
		var height = ButtonHeight;

		for (var i = 0; i < MaxFuncCount; i++)
		{
			var rect = new Rectangle(i * buttonWidth, 0, buttonWidth, height);
			if (rect.Contains(p))
				return i;
		}
		return null;
	}

	/// <summary>
	/// Updates the current modifier key state based on the specified key data.
	/// </summary>
	/// <remarks>This method sets the internal modifier key state to reflect the current state of the Control,
	/// Shift, and Alt keys. If the modifier state changes, the method triggers a refresh of the associated
	/// component.</remarks>
	/// <param name="keyData">A bitwise combination of <see cref="Keys"/> values representing the current state of the keyboard modifier keys.</param>
	public void SetModifier(Keys keyData)
	{
		_prevModifier = _modifier;
		_modifier = ModifierKey.None;
		if ((keyData & Keys.Control) == Keys.Control)
			_modifier |= ModifierKey.Control;
		if ((keyData & Keys.Shift) == Keys.Shift)
			_modifier |= ModifierKey.Shift;
		if ((keyData & Keys.Alt) == Keys.Alt)
			_modifier |= ModifierKey.Alt;
		if (_modifier != _prevModifier)
			Invalidate();
	}

	/// <summary>
	/// Retrieves the command associated with the specified key code.
	/// </summary>
	/// <remarks>This method calculates the command based on the provided key code and the current modifier
	/// state.</remarks>
	/// <param name="keyCode">The key code representing a function key. Must be a value between <see cref="Keys.F2"/> and the maximum function
	/// key supported.</param>
	/// <returns>The command string associated with the specified key code. Returns <see cref="Commands.None"/> if the key code is
	/// not within the valid range.</returns>
	public string GetCommand(Keys keyCode)
	{
		var val = (int)keyCode - (int)Keys.F2;
		if (val is < 0 or >= MaxFuncCount)
			return Commands.None;
		var num = FuncNumbers[val];
		return Settings.Instance.GetFuncKeyCommand(num, _modifier);
	}
}

/// <summary>
/// FuncBar 버튼 클릭 이벤트 데이터
/// </summary>
public class FuncBarButtonClickEventArgs(int index, int funcKey, string command) : EventArgs
{
	/// <summary>버튼 인덱스</summary>
	public int Index { get; } = index;

	/// <summary>기능 키 번호 (F2~F9에서 F빼고)</summary>
	public int FuncionKeyNumber { get; } = funcKey;

	/// <summary>커맨드 문자열(없으면 "")</summary>
	public string Command { get; } = command;
}
