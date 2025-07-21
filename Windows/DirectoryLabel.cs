namespace DuFile.Windows;

/// <summary>
/// 폴더/파일/드라이브 정보를 한 줄에 이미지처럼 표시하는 커스텀 라벨 컨트롤입니다.
/// </summary>
public sealed class DirectoryLabel : Control
{
	// 폴더 개수
	private int _dirCount;
	// 파일 개수
	private int _fileCount;
	// 전체 크기 문자열
	private string _totalSize = string.Empty;
	// 드라이브 레이블
	private string _drvLabel = string.Empty;
	// 드라이브 이름
	private string _drvName = string.Empty;
	// 드라이브 남은 용량 문자열
	private string _drvAvailable = string.Empty;
	// 활성화 상태
	private bool _isActive;

	// 왼쪽 텍스트 영역
	private Rectangle _leftRect = Rectangle.Empty;
	// 오른쪽 텍스트 영역
	private Rectangle _rightRect = Rectangle.Empty;

	// 왼쪽 마우스 오버 상태
	private bool _leftHover;
	// 오른쪽 마우스 오버 상태
	private bool _rightHover;

	/// <summary>
	/// 디렉토리 라벨이 클릭될 때 발생합니다.
	/// </summary>
	/// <remarks>
	/// 사용자가 UI에서 디렉토리 라벨을 클릭할 때마다 발생합니다. 구독자는 이 이벤트를 처리하여 디렉토리로 이동하거나 추가 정보를 표시하는 등의 작업을 수행할 수 있습니다.
	/// </remarks>
	public event EventHandler<DirectoryLabelClickedEventArgs>? DirectoryLabelClicked;

	/// <summary>
	/// 고정 높이 값을 가져옵니다.
	/// </summary>
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int StaticHeight => 20;

	/// <summary>
	/// 현재 컨트롤이 활성 상태인지 여부를 가져오거나 설정합니다.
	/// </summary>
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool IsActive
	{
		get => _isActive;
		set
		{
			_isActive = value;
			Invalidate();
		}
	}

	// 디자인 모드 확인
	private bool IsReallyDesignMode => LicenseManager.UsageMode == LicenseUsageMode.Designtime || (Site?.DesignMode ?? false);

	/// <summary>
	/// DirectoryLabel 클래스의 새 인스턴스를 초기화합니다. (기본 스타일 및 테마 적용)
	/// </summary>
	/// <remarks>
	/// 이 생성자는 최적화된 더블 버퍼링과 사용자 지정 페인팅을 위한 컨트롤 스타일을 설정하여 렌더링 성능을 향상시킵니다. 또한 애플리케이션 설정에 지정된 배경색, 전경색, 폰트 등 현재 테마 설정을 적용합니다.
	/// </remarks>
	public DirectoryLabel()
	{
		SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.Selectable, true);
		TabStop = true;

		var settings = Settings.Instance;
		Font = new Font(settings.UiFontFamily, 8.25f, FontStyle.Regular, GraphicsUnit.Point);
		Height = StaticHeight;
	}

	/// <inheritdoc />
	protected override void OnCreateControl()
	{
		base.OnCreateControl();

		if (IsReallyDesignMode)
		{
			// 디자인 모드에서 기본 값 설정
			SetDirectoryInfo(10, 20, 1234567);
			SetDriveInfo("C:", "디자인모드", 76543212345);
		}
	}

	/// <summary>
	/// 왼쪽 폴더/파일 정보를 갱신합니다.
	/// </summary>
	/// <param name="dirCount">폴더 개수</param>
	/// <param name="fileCount">파일 개수</param>
	/// <param name="totalSize">전체 크기(바이트)</param>
	public void SetDirectoryInfo(int dirCount, int fileCount, long totalSize)
	{
		_dirCount = dirCount;
		_fileCount = fileCount;
		_totalSize = totalSize.FormatFileSize();
		Invalidate();
	}

	/// <summary>
	/// 오른쪽 드라이브 정보를 갱신합니다.
	/// </summary>
	/// <param name="drvLabel">드라이브 레이블</param>
	/// <param name="drvName">드라이브 이름</param>
	/// <param name="drvAvailable">남은 용량(바이트)</param>
	public void SetDriveInfo(string drvLabel, string drvName, long drvAvailable)
	{
		_drvLabel = drvLabel;
		_drvName = drvName;
		_drvAvailable = drvAvailable.FormatFileSize();
		Invalidate();
	}

	/// <inheritdoc />
	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);

		var theme = Settings.Instance.Theme;
		var g = e.Graphics;
		g.Clear(IsActive ? theme.BackHover : theme.BackContent);

		const string leftDirectory = " 디렉토리, ";
		const string leftFile = " 파일 ";
		const string rightAvailable = " 남음";

		var leftDirCount = $"{_dirCount}";
		var leftFileCount = $"{_fileCount}";
		var lefttotalSize = $"({_totalSize})";

		var rightDrvName = $"{_drvLabel} ({_drvName}) ";
		var rightDrvAvailable = $"{_drvAvailable}";

		// 왼쪽 텍스트 측정 및 그리기
		float x = 0;
		var y = (Height - Font.Height) / 2;
		using var highlightBrush = new SolidBrush(theme.Accelerator);
		using var textBrush = new SolidBrush(theme.Foreground);
		using var boldFont = new Font(Font, FontStyle.Bold);

		// [dirCount]
		var sizeDirCount = g.MeasureString(leftDirCount, Font);
		g.DrawString(leftDirCount, Font, highlightBrush, x, y);
		x += sizeDirCount.Width;
		// " 디렉토리, "
		var sizeDirectory = g.MeasureString(leftDirectory, Font);
		g.DrawString(leftDirectory, Font, textBrush, x, y);
		x += sizeDirectory.Width;
		// [fileCount]
		var sizeFileCount = g.MeasureString(leftFileCount, Font);
		g.DrawString(leftFileCount, Font, highlightBrush, x, y);
		x += sizeFileCount.Width;
		// " 파일 "
		var sizeFile = g.MeasureString(leftFile, Font);
		g.DrawString(leftFile, Font, textBrush, x, y);
		x += sizeFile.Width;
		// ([totalSize])
		var sizeTotal = g.MeasureString(lefttotalSize, boldFont);
		g.DrawString(lefttotalSize, boldFont, textBrush, x, y);
		x += sizeTotal.Width;
		// 왼쪽 텍스트 전체 영역 저장
		_leftRect = new Rectangle(0, 0, (int)x, Height);

		// 오른쪽 텍스트 측정 및 그리기
		var sizeDrvName = g.MeasureString(rightDrvName, Font);
		var sizeDrvAvailable = g.MeasureString(rightDrvAvailable, boldFont);
		var sizeAvailable = g.MeasureString(rightAvailable, Font);
		var rx = Width - (sizeDrvName.Width + sizeDrvAvailable.Width + sizeAvailable.Width);
		var rightStart = rx;
		// [drvLabel] ([drvName])
		g.DrawString(rightDrvName, Font, textBrush, rx, y);
		rx += sizeDrvName.Width;
		// [drvAvailable] (파란색, Bold)
		g.DrawString(rightDrvAvailable, boldFont, highlightBrush, rx, y);
		rx += sizeDrvAvailable.Width;
		// " 남음"
		g.DrawString(rightAvailable, Font, textBrush, rx, y);
		// 오른쪽 텍스트 전체 영역 저장
		_rightRect = new Rectangle((int)rightStart, 0, Width - (int)rightStart, Height);
	}

	/// <inheritdoc />
	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		var leftNow = _leftRect.Contains(e.Location);
		var rightNow = _rightRect.Contains(e.Location);

		switch (leftNow)
		{
			case true when !_leftHover:
				Cursor = Cursors.Hand;
				_leftHover = true;
				break;
			case false when _leftHover:
				_leftHover = false;
				Cursor = Cursors.Default;
				break;
		}

		switch (rightNow)
		{
			case true when !_rightHover:
				Cursor = Cursors.Hand;
				_rightHover = true;
				break;
			case false when _rightHover:
				_rightHover = false;
				Cursor = Cursors.Default;
				break;
		}
	}

	/// <inheritdoc />
	protected override void OnMouseLeave(EventArgs e)
	{
		base.OnMouseLeave(e);
		_leftHover = false;
		_rightHover = false;
		Cursor = Cursors.Default;
	}

	/// <inheritdoc />
	protected override void OnMouseDown(MouseEventArgs e)
	{
		base.OnMouseDown(e);
		Focus();
		
		if (e.Button != MouseButtons.Left)
			return;

		if (_leftRect.Contains(e.Location))
			DirectoryLabelClicked?.Invoke(this, new DirectoryLabelClickedEventArgs(e.Location, DirectoryLabelClickedArea.Left));
		else if (_rightRect.Contains(e.Location))
			DirectoryLabelClicked?.Invoke(this, new DirectoryLabelClickedEventArgs(e.Location, DirectoryLabelClickedArea.Right));
	}
}

/// <summary>
/// DirectoryLabel에서 클릭된 영역 정보를 제공합니다.
/// </summary>
public enum DirectoryLabelClickedArea
{
	/// <summary>왼쪽 영역을 나타냅니다.</summary>
	Left,
	/// <summary>오른쪽 영역을 나타냅니다.</summary>
	Right
}

/// <summary>
/// DirectoryLabel 클릭 이벤트 인수.
/// </summary>
public class DirectoryLabelClickedEventArgs(Point location, DirectoryLabelClickedArea area) : EventArgs
{
	/// <summary>
	/// 클릭된 위치를 가져옵니다.
	/// </summary>
	public Point Location { get; } = location;

	/// <summary>
	/// 클릭된 영역(왼쪽/오른쪽)을 가져옵니다.
	/// </summary>
	public DirectoryLabelClickedArea Area { get; } = area;
}
