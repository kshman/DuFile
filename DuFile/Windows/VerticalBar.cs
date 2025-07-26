namespace DuFile.Windows;

/// <summary>
/// VerticalBar 컨트롤은 파일 패널 사이에 위치하는 세로 방향의 커맨드 버튼 바입니다.
/// 동기화, 복사, 선택, 정렬, 퀵 위치 이동 등 다양한 명령 버튼을 제공합니다.
/// </summary>
public sealed class VerticalBar : Control
{
	private record ButtonDef(int BottomMargin, string Text, string Command, string ToolTip);

	private readonly ButtonDef[] _buttonDefs =
	[
		new(0, "→", "#SyncLeftToRight", "오른쪽 폴더를 같게"),
		new(20, "←", "#SyncRightToLeft", "왼쪽 폴더를 같기"),
		new(0, "▶", "#CopyLeftToRight", "오른쪽으로 복사"),
		new(20, "◀", "#CopyRightToLeft", "왼쪽으로 복사"),
		new(5, "▣", "#SelectAll", "모두 선택"),
		new(20, "□", "#SelectNone", "선택 해제"),
		new(20, "≈", "#Sort", "파일 이름 정렬"),
		new(5, "1", "#QuickLocation1", ""),
		new(5, "2", "#QuickLocation2", ""),
		new(5, "3", "#QuickLocation3", ""),
		new(5, "4", "#QuickLocation4", ""),
		new(5, "5", "#QuickLocation5", "")
	];

	private readonly ToolTip _toolTip = new();
#nullable disable
	private Rectangle[] _buttonRects;
#nullable restore
	private int? _hoverIndex;
	private int? _pressedIndex;
	private int _lastToolTipIndex = -1;

	/// <summary>
	/// Gets the static width value.
	/// </summary>
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int StaticWidth => 20;

	/// <summary>
	/// VerticalBar에서 버튼이 클릭될 때 발생하는 이벤트입니다.
	/// </summary>
	public event EventHandler<VerticalBarButtonClickEventArgs>? ButtonClick;

	/// <summary>
	/// VerticalBar의 인스턴스를 초기화합니다.
	/// </summary>
	public VerticalBar()
	{
		SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.Selectable, true);

		var theme = Settings.Instance.Theme;

		DoubleBuffered = true;
		Width = StaticWidth;
		BackColor = theme.Background;
		ForeColor = theme.Foreground;

		RecalcButtonRects();
	}

	/// <summary>
	/// 버튼들의 위치와 크기를 다시 계산합니다.
	/// </summary>
	private void RecalcButtonRects()
	{
		const int height = 18;
		var y = 0;

		_buttonRects = new Rectangle[_buttonDefs.Length];
		for (var i = 0; i < _buttonDefs.Length; i++)
		{
			_buttonRects[i] = new Rectangle(0, y, StaticWidth, height);
			y += height + _buttonDefs[i].BottomMargin;
		}

		//Height = y;
	}

	/// <inheritdoc />
	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);

		var settings = Settings.Instance;
		var theme = settings.Theme;
		var font = new Font(settings.UiFontFamily, 8F, FontStyle.Bold, GraphicsUnit.Point);

		for (var i = 0; i < _buttonDefs.Length; i++)
		{
			var rect = _buttonRects[i];
			var def = _buttonDefs[i];

			var fill =
				_pressedIndex == i ? theme.BackActive :
				_hoverIndex == i ? theme.BackHover :
				theme.Background;

			using (var brush = new SolidBrush(fill))
				e.Graphics.FillRectangle(brush, rect);
			//using (var pen = new Pen(theme.Border))
			//	e.Graphics.DrawRectangle(pen, rect);

			TextRenderer.DrawText(
				e.Graphics, def.Text, font, rect, theme.Foreground,
				TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine
			);
		}
	}

	/// <inheritdoc />
	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);

		var prevHover = _hoverIndex;
		_hoverIndex = null;
		for (var i = 0; i < _buttonRects.Length; i++)
		{
			if (!_buttonRects[i].Contains(e.Location))
				continue;

			_hoverIndex = i;
			if (_lastToolTipIndex != i)
			{
				var tip = _buttonDefs[i].ToolTip;
				if (!string.IsNullOrEmpty(tip))
					_toolTip.Show(tip, this, _buttonRects[i].Right + 2, _buttonRects[i].Top + 2, 2000);
				else
					_toolTip.Hide(this);
				_lastToolTipIndex = i;
			}

			break;
		}

		if (_hoverIndex == null)
		{
			_toolTip.Hide(this);
			_lastToolTipIndex = -1;
		}

		if (_hoverIndex != prevHover)
			Invalidate();
	}

	/// <inheritdoc />
	protected override void OnMouseLeave(EventArgs e)
	{
		base.OnMouseLeave(e);

		_hoverIndex = null;
		_toolTip.Hide(this);
		Invalidate();
	}

	/// <inheritdoc />
	protected override void OnMouseDown(MouseEventArgs e)
	{
		base.OnMouseDown(e);
		Focus();

		_pressedIndex = null;
		for (var i = 0; i < _buttonRects.Length; i++)
		{
			if (!_buttonRects[i].Contains(e.Location))
				continue;

			_pressedIndex = i;
			Invalidate();
			break;
		}
	}

	/// <inheritdoc />
	protected override void OnMouseUp(MouseEventArgs e)
	{
		base.OnMouseUp(e);
		
		if (_pressedIndex != null && _buttonRects[_pressedIndex.Value].Contains(e.Location))
		{
			var def = _buttonDefs[_pressedIndex.Value];
			ButtonClick?.Invoke(this, new VerticalBarButtonClickEventArgs(def.Command, e.Location, e.Button, e.Clicks));
		}

		_pressedIndex = null;
		Invalidate();
	}

	/// <inheritdoc />
	protected override void OnResize(EventArgs e)
	{
		base.OnResize(e);
		RecalcButtonRects();
		Invalidate();
	}
}

/// <summary>
/// VerticalBar의 버튼 클릭 이벤트 인수입니다.
/// </summary>
public class VerticalBarButtonClickEventArgs(string command, Point location, MouseButtons button, int clicks) : EventArgs
{
	/// <summary>
	/// 클릭된 버튼의 명령 문자열입니다.
	/// </summary>
	public string Command { get; } = command;

	/// <summary>
	/// 클릭된 위치입니다.
	/// </summary>
	public Point Location { get; } = location;

	/// <summary>
	/// 마우스 버튼 종류입니다.
	/// </summary>
	public MouseButtons Button { get; } = button;

	/// <summary>
	/// 클릭 횟수입니다.
	/// </summary>
	public int Clicks { get; } = clicks;
}
