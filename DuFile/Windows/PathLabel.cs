namespace DuFile.Windows;

/// <summary>
/// 폴더/파일/드라이브 정보를 한 줄에 이미지처럼 표시하는 커스텀 라벨 컨트롤입니다.
/// </summary>
public sealed class PathLabel : Control
{
	// 폴더 개수
	private int _folderCount;
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
	// 선택 상태
	private string _selected = string.Empty;
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
	/// 폴더 라벨이 클릭될 때 발생합니다.
	/// </summary>
	/// <remarks>
	/// 사용자가 UI에서 폴더 라벨을 클릭할 때마다 발생합니다. 구독자는 이 이벤트를 처리하여 폴더로 이동하거나 추가 정보를 표시하는 등의 작업을 수행할 수 있습니다.
	/// </remarks>
	[Category("PathLabel")]
	public event EventHandler<PathLabelClickedEventArgs>? Clicked;

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
	/// PathLabel 클래스의 새 인스턴스를 초기화합니다. (기본 스타일 및 테마 적용)
	/// </summary>
	/// <remarks>
	/// 이 생성자는 최적화된 더블 버퍼링과 사용자 지정 페인팅을 위한 컨트롤 스타일을 설정하여 렌더링 성능을 향상시킵니다. 또한 애플리케이션 설정에 지정된 배경색, 전경색, 폰트 등 현재 테마 설정을 적용합니다.
	/// </remarks>
	public PathLabel()
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
			SetFolderInfo(10, 20, 1234567);
			SetDriveInfo(null);
		}
	}

	/// <summary>
	/// 왼쪽 폴더/파일 정보를 갱신합니다.
	/// </summary>
	/// <param name="dirCount">폴더 개수</param>
	/// <param name="fileCount">파일 개수</param>
	/// <param name="totalSize">전체 크기(바이트)</param>
	public void SetFolderInfo(int dirCount, int fileCount, long totalSize)
	{
		_folderCount = dirCount;
		_fileCount = fileCount;
		_totalSize = totalSize.FormatFileSize();
		Invalidate();
	}

	/// <summary>
	/// 오른쪽 드라이브 정보를 갱신합니다. <see cref="DriveInfo"/> 
	/// </summary>
	/// <param name="drive">드라이브 정보가 담긴 <see cref="DriveInfo"/> 오브젝트</param>
	public void SetDriveInfo(DriveInfo? drive)
	{
		if (drive == null)
		{
			_drvLabel = "알 수 없음";
			_drvName = "??";
			_drvAvailable = "0 B";
		}
		else
		{
			_drvLabel = drive.VolumeLabel;
			_drvName = drive.Name.TrimEnd('\\');
			_drvAvailable = drive.AvailableFreeSpace.FormatFileSize();
		}

		_selected = string.Empty;
		Invalidate();
	}

	/// <summary>
	/// Updates the selected information display with the specified count and size.
	/// </summary>
	/// <remarks>This method updates the internal state to reflect the current selection and triggers a redraw of
	/// the display.</remarks>
	/// <param name="count">The number of selected items. Must be non-negative.</param>
	/// <param name="size">The total size of the selected items, formatted as a number with thousands separators.</param>
	public void SetSelectedInfo(int count, long size)
	{
		_selected = $"선택: {count}개 ({size:N0} 바이트)";
		Invalidate();
	}

	/// <inheritdoc />
	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);

		var theme = Settings.Instance.Theme;
		var g = e.Graphics;
		g.Clear(IsActive ? theme.BackHover : theme.BackContent);

		const string leftFolder = " 폴더, ";
		const string leftFile = " 파일 ";
		const string rightAvailable = " 남음";

		var leftDirCount = $"{_folderCount}";
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
		// " 폴더, "
		var sizeFolder = g.MeasureString(leftFolder, Font);
		g.DrawString(leftFolder, Font, textBrush, x, y);
		x += sizeFolder.Width;
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
		if (!string.IsNullOrEmpty(_selected))
		{
			// 선택이 있으면 선택 정보 표시
			var sizeSelected = g.MeasureString(_selected, Font);
			var rx = Width - sizeSelected.Width;
			// "선택: [count]개 ([size])"
			g.DrawString(_selected, Font, highlightBrush, rx, y);
			// 오른쪽 텍스트 전체 영역 저장
			_rightRect = new Rectangle((int)rx, 0, Width - (int)rx, Height);
		}
		else
		{
			// 아니면 드라이브 정보 표시
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
	}

	/// <inheritdoc />
	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);

		switch (_leftRect.Contains(e.Location))
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

		if (string.IsNullOrEmpty(_selected))
		{
			switch (_rightRect.Contains(e.Location))
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

		if (e.Button != MouseButtons.Left || Clicked == null)
			return;

		if (_leftRect.Contains(e.Location))
			Clicked.Invoke(this, 
				new PathLabelClickedEventArgs(e.Location, PointToScreen(e.Location), PathLabelArea.Folder));
		else if (_rightRect.Contains(e.Location) && string.IsNullOrEmpty(_selected))
			Clicked.Invoke(this, 
				new PathLabelClickedEventArgs(e.Location, PointToScreen(e.Location), PathLabelArea.Drive));
	}
}

/// <summary>
/// PathLabel에서 클릭된 영역 정보를 제공합니다.
/// </summary>
public enum PathLabelArea
{
	/// <summary>왼쪽 폴더 영역을 나타냅니다.</summary>
	Folder,
	/// <summary>오른쪽 드라이브 영역을 나타냅니다.</summary>
	Drive
}

/// <summary>
/// PathLabel 클릭 이벤트 인수.
/// </summary>
public class PathLabelClickedEventArgs(Point location, Point scrLocation, PathLabelArea area) : EventArgs
{
	/// <summary>
	/// 클릭된 위치를 가져옵니다.
	/// </summary>
	public Point Location { get; } = location;

	/// <summary>
	/// Gets the screen coordinates of the current object.
	/// </summary>
	public Point ScreenLocation { get; } = scrLocation;

	/// <summary>
	/// 클릭된 영역(왼쪽/오른쪽)을 가져옵니다.
	/// </summary>
	public PathLabelArea Area { get; } = area;
}
