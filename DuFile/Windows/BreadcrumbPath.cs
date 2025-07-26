namespace DuFile.Windows;

/// <summary>
/// 디렉터리 경로를 BreadCrumb 형식으로 보여주는 컨트롤입니다.
/// </summary>
public class BreadcrumbPath : Control
{
	private readonly List<string> _parts = [];
	private readonly List<Rectangle> _partRects = [];
	private Rectangle? _ellipsisRect;
	private int _hoverIndex = -1;
	private bool _hoverEllipsis;

	/// <summary>
	/// BreadCrumb의 전체 경로 문자열 (예: "C:\Users\kshman\Documents")
	/// </summary>
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public string Path
	{
		get => string.Join(System.IO.Path.DirectorySeparatorChar.ToString(), _parts);
		set => SetPath(value);
	}

	/// <summary>
	/// BreadCrumb의 경로 파트(폴더 이름) 배열
	/// </summary>
	public string[] Parts => _parts.ToArray();

	/// <summary>
	/// Gets the static height value.
	/// </summary>
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int StaticHeight => 20;

	/// <summary>
	/// BreadCrumb에서 폴더 클릭 시 발생하는 이벤트입니다.
	/// </summary>
	public event EventHandler<BreadcrumbPathClickEventArgs>? PathClick;

	// 디자인 모드 확인
	bool IsReallyDesignMode => LicenseManager.UsageMode == LicenseUsageMode.Designtime || (Site?.DesignMode ?? false);

	/// <summary>
	/// Initializes a new instance of the <see cref="BreadcrumbPath"/> class with default settings.
	/// </summary>
	/// <remarks>This constructor sets up the control with optimized painting styles and initializes the default
	/// height and path. The control is not focusable by default.</remarks>
	public BreadcrumbPath()
	{
		SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.Selectable, true);
		TabStop = true;
		Height = StaticHeight;
		Path = "";
	}

	/// <inheritdoc />
	protected override void OnCreateControl()
	{
		base.OnCreateControl();

		if (IsReallyDesignMode)
		{
			// 디자인 모드에서 기본 경로 설정
			Path = Settings.Instance.StartFolder;
		}
	}

	/// <summary>
	/// 경로를 파싱하여 내부적으로 분리합니다.
	/// </summary>
	private void SetPath(string path)
	{
		_parts.Clear();
		if (string.IsNullOrEmpty(path))
			return;

		var p = path.Replace('/', '\\').Trim();

		if (p.StartsWith(@"\\"))
		{
			// UNC 경로 (예: \\server\share\folder)
			_parts.Add(@"\\");
			p = p[2..];
			var arr = p.Split(['\\'], StringSplitOptions.RemoveEmptyEntries);
			_parts.AddRange(arr);
		}
		else if (p.Length >= 2 && char.IsLetter(p[0]) && p[1] == ':')
		{
			// 드라이브 문자 (예: C:\, D:\)
			_parts.Add(p[..2]);
			p = p[2..].TrimStart('\\');
			if (!string.IsNullOrEmpty(p))
				_parts.AddRange(p.Split(['\\'], StringSplitOptions.RemoveEmptyEntries));
		}
		else
		{
			// 기타 상대 경로 등
			_parts.AddRange(p.Split(['\\'], StringSplitOptions.RemoveEmptyEntries));
		}

		Invalidate();
	}

	/// <inheritdoc/>
	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);

		e.Graphics.Clear(BackColor);
		if (_parts.Count == 0)
			return;

		var settings = Settings.Instance;
		var theme = settings.Theme;

		var font = new Font(settings.UiFontFamily, 8.25f, FontStyle.Regular);
		const int partMargin = 0;
		const int sepWidth = 20;
		const int sepMargin = 0;
		var h = Height;

		// 각 파트별 크기 측정
		var partSizes = _parts.Select(p => TextRenderer.MeasureText(p, font).Width + partMargin * 2).ToList();

		// 보여줄 첫 파트 결정 (오른쪽은 반드시 다 보이게, 앞부분은 잘릴 수 있음)
		var showStart = 0;
		var total = 0;
		for (var i = _parts.Count - 1; i >= 0; i--)
		{
			var partW = partSizes[i];
			if (i < _parts.Count - 1)
				partW += sepWidth;
			total += partW;
			if (total > Width)
			{
				showStart = i + 1;
				break;
			}
		}
		if (showStart >= _parts.Count)
			showStart = _parts.Count - 1;

		// 앞부분이 잘릴 때 "..." 표시 필요
		var showEllipsis = showStart > 0;
		var ellipsisWidth = 0;
		if (showEllipsis)
			ellipsisWidth = sepWidth; // ... 표시 영역도 sepWidth와 맞춤

		// x 위치: 항상 왼쪽 정렬, 앞부분은 잘려도 됨
		_partRects.Clear();
		_ellipsisRect = null;
		var x = 0;

		// "..." 표시
		if (showEllipsis)
		{
			_ellipsisRect = new Rectangle(x, 0, ellipsisWidth, h);
			DrawEllipsis(e.Graphics, _ellipsisRect.Value, font, _hoverEllipsis ? theme.BackContent : theme.Background, theme.Foreground);
			x += ellipsisWidth;
		}

		// 실제 Path 파트들
		for (var i = showStart; i < _parts.Count; i++)
		{
			var partW = partSizes[i];
			var rect = new Rectangle(x, 0, partW, h);
			_partRects.Add(rect);

			var fill = (i == _hoverIndex) ? theme.BackContent : theme.Background;
			using (var brush = new SolidBrush(fill))
				e.Graphics.FillRectangle(brush, rect);

			TextRenderer.DrawText(e.Graphics, _parts[i], font, rect, theme.Foreground, 
				TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);

			// 분리자
			if (i < _parts.Count - 1)
			{
				var sepX = rect.Right;
				var sepRect = new Rectangle(sepX + sepMargin, sepMargin, sepWidth - sepMargin * 2, h - sepMargin * 2);
				DrawArrow(e.Graphics, sepRect, theme.Foreground);
				x += partW + sepWidth;
			}
			else
			{
				x += partW;
			}
		}
	}

	/// <summary>
	/// "..."을 그립니다.
	/// </summary>
	private static void DrawEllipsis(Graphics g, Rectangle rect, Font font, Color back, Color fore)
	{
		using (var brush = new SolidBrush(back))
			g.FillRectangle(brush, rect);

		TextRenderer.DrawText(g, "...", font, rect, fore, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
	}

	/// <summary>
	/// 삼각형(>) 화살표를 그립니다.
	/// </summary>
	private static void DrawArrow(Graphics g, Rectangle rect, Color color)
	{
		var midY = rect.Top + rect.Height / 2;
		var arrowPad = Math.Max(2, rect.Width / 4);
		var left = rect.Left + arrowPad;
		var right = rect.Right - arrowPad;
		var top = rect.Top + arrowPad;
		var bottom = rect.Bottom - arrowPad;

		// 꼭짓점 3개 (좌상, 좌하, 우중앙)
		var pts = new[]
		{
			new Point(left, top),
			new Point(left, bottom),
			new Point(right, midY)
		};
		using var brush = new SolidBrush(color);
		g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
		g.FillPolygon(brush, pts);
		g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
	}

	/// <inheritdoc/>
	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);

		var prevHoverEllipsis = _hoverEllipsis;
		var prevHoverIndex = _hoverIndex;

		_hoverEllipsis = false;
		_hoverIndex = -1;

		if (_ellipsisRect != null && _ellipsisRect.Value.Contains(e.Location))
			_hoverEllipsis = true;
		else
		{
			var idx = GetPartIndexAt(e.Location);
			if (idx >= 0)
				_hoverIndex = idx;
		}

		if (prevHoverEllipsis != _hoverEllipsis || prevHoverIndex != _hoverIndex)
			Invalidate();
	}

	/// <inheritdoc/>
	protected override void OnMouseLeave(EventArgs e)
	{
		base.OnMouseLeave(e);
		if (_hoverIndex != -1 || _hoverEllipsis)
		{
			_hoverIndex = -1;
			_hoverEllipsis = false;
			Invalidate();
		}
	}

	/// <inheritdoc/>
	protected override void OnMouseDown(MouseEventArgs e)
	{
		base.OnMouseDown(e);
		Focus();

		// ... 클릭
		if (_hoverEllipsis && _ellipsisRect != null)
		{
			// ...을 클릭하면 0번째(최상위) 폴더 클릭으로 간주하거나, 필요에 따라 이벤트를 확장할 수 있음
			PathClick?.Invoke(this, new BreadcrumbPathClickEventArgs(GetDirectoryPath(0), e.Location, e.Button));
			return;
		}

		var idx = GetPartIndexAt(e.Location);
		if (idx < 0)
			return;

		PathClick?.Invoke(this, new BreadcrumbPathClickEventArgs(GetDirectoryPath(idx), e.Location, e.Button));
	}

	/// <summary>
	/// 마우스 위치에 해당하는 파트 인덱스를 반환
	/// </summary>
	private int GetPartIndexAt(Point pt)
	{
		for (var i = 0; i < _partRects.Count; i++)
		{
			if (_partRects[i].Contains(pt))
				return _parts.Count - _partRects.Count + i;
		}
		return -1;
	}

	/// <summary>
	/// 해당 인덱스까지의 경로 문자열 반환
	/// </summary>
	private string GetDirectoryPath(int index)
	{
		if (index < 0 || index >= _parts.Count)
			return string.Empty;
		return string.Join(System.IO.Path.DirectorySeparatorChar.ToString(), _parts.Take(index + 1));
	}

	/// <inheritdoc/>
	protected override void OnFontChanged(EventArgs e)
	{
		base.OnFontChanged(e);
		Invalidate();
	}
}

/// <summary>
/// Breadcrumb 클릭 이벤트 데이터
/// </summary>
public class BreadcrumbPathClickEventArgs(string path, Point location, MouseButtons button) : EventArgs
{
	/// <summary>해당 폴더 경로</summary>
	public string Path { get; } = path;

	/// <summary>
	/// 클릭된 위치입니다.
	/// </summary>
	public Point Location { get; } = location;

	/// <summary>
	/// 마우스 버튼 종류입니다.
	/// </summary>
	public MouseButtons Button { get; } = button;
}
