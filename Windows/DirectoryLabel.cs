namespace DuFile.Windows;

/// <summary>
/// 폴더/파일/드라이브 정보를 한 줄에 이미지처럼 표시하는 커스텀 라벨 컨트롤입니다.
/// </summary>
public sealed class DirectoryLabel : Control
{
	private int _dirCount;
	private int _fileCount;
	private string _totalSize = string.Empty;
	private string _drvLabel = string.Empty;
	private string _drvName = string.Empty;
	private string _drvAvailable = string.Empty;
	private bool _isActive;

	private Rectangle _leftRect = Rectangle.Empty;
	private Rectangle _rightRect = Rectangle.Empty;

	private bool _leftHover;
	private bool _rightHover;

	/// <summary>
	/// Occurs when a directory label is clicked.
	/// </summary>
	/// <remarks>This event is triggered whenever a user clicks on a directory label in the UI.  Subscribers can
	/// handle this event to perform actions such as navigating to the  directory or displaying additional
	/// information.</remarks>
	public event EventHandler<DirectoryLabelClickedEventArgs>? DirectoryLabelClicked;

	/// <summary>
	/// Gets the static height value.
	/// </summary>
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int StaticHeight => 20;

	/// <summary>
	/// Gets or sets a value indicating whether the component is currently active.
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

	/// <summary>
	/// Initializes a new instance of the <see cref="DirectoryLabel"/> class with default styling and theme settings.
	/// </summary>
	/// <remarks>This constructor sets the control styles for optimized double buffering and custom painting to
	/// enhance rendering performance. It also applies the current theme settings for background color, foreground color,
	/// and font, as specified in the application settings.</remarks>
	public DirectoryLabel()
	{
		SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);

		var settings = Settings.Instance;
		var theme = settings.Theme;
		Font = new Font(settings.UiFontFamily, 8.25f, FontStyle.Regular, GraphicsUnit.Point);
		Height = StaticHeight;

		if (DesignMode)
		{
			// 디자인 모드에서 기본 값 설정
			SetDirectoryInfo(10, 20, 1234567);
			SetDriveInfo("C:", "Local Disk", 7654321);
		}
	}

	/// <summary>
	/// 왼쪽 폴더/파일 정보를 갱신합니다.
	/// </summary>
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
		g.Clear(IsActive ? theme.Hover : theme.Content);

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
		var sizeTotal = g.MeasureString(lefttotalSize, new Font(Font, FontStyle.Bold));
		g.DrawString(lefttotalSize, new Font(Font, FontStyle.Bold), textBrush, x, y);
		x += sizeTotal.Width;
		// 왼쪽 텍스트 전체 영역 저장
		_leftRect = new Rectangle(0, 0, (int)x, Height);

		// 오른쪽 텍스트 측정 및 그리기
		var sizeDrvName = g.MeasureString(rightDrvName, Font);
		var sizeDrvAvailable = g.MeasureString(rightDrvAvailable, new Font(Font, FontStyle.Bold));
		var sizeAvailable = g.MeasureString(rightAvailable, Font);
		var rx = Width - (sizeDrvName.Width + sizeDrvAvailable.Width + sizeAvailable.Width);
		var rightStart = rx;
		// [drvLabel] ([drvName])
		g.DrawString(rightDrvName, Font, textBrush, rx, y);
		rx += sizeDrvName.Width;
		// [drvAvailable] (파란색, Bold)
		g.DrawString(rightDrvAvailable, new Font(Font, FontStyle.Bold), highlightBrush, rx, y);
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
		if (e.Button != MouseButtons.Left)
			return;

		if (_leftRect.Contains(e.Location))
		{
			DirectoryLabelClicked?.Invoke(this, new DirectoryLabelClickedEventArgs(e.Location, DirectoryLabelClickedArea.Left));
		}
		else if (_rightRect.Contains(e.Location))
		{
			DirectoryLabelClicked?.Invoke(this, new DirectoryLabelClickedEventArgs(e.Location, DirectoryLabelClickedArea.Right));
		}
	}
}

/// <summary>
/// DirectoryLabel에서 클릭된 영역 정보를 제공합니다.
/// </summary>
public enum DirectoryLabelClickedArea
{
	/// <summary>
	/// Represents the left direction in a set of directional options.
	/// </summary>
	/// <remarks>This enumeration value can be used to specify a leftward direction in navigation or layout
	/// contexts.</remarks>
	Left,
	/// <summary>
	/// Represents a direction or alignment to the right.
	/// </summary>
	/// <remarks>This enumeration value can be used to specify rightward alignment or direction in various contexts,
	/// such as text alignment, layout positioning, or navigation.</remarks>
	Right
}

/// <summary>
/// DirectoryLabel 클릭 이벤트 인수.
/// </summary>
public class DirectoryLabelClickedEventArgs(Point location, DirectoryLabelClickedArea area) : EventArgs
{
	/// <summary>
	/// Gets the coordinates of the current location.
	/// </summary>
	public Point Location { get; } = location;

	/// <summary>
	/// Gets the area of the directory label that was clicked.
	/// </summary>
	public DirectoryLabelClickedArea Area { get; } = area;
}
