namespace DuFile.Windows;

/// <summary>
/// 폴더, 파일, 드라이브 정보를 한 줄로 이미지처럼 표시하는 커스텀 라벨 컨트롤입니다.
/// </summary>
public sealed class PathLabel : ThemeControl
{
	// 폴더 정보
	private FolderDef _folder;
	// 드라이브 정보
	private DriveDef _drive;
	// 활성화 상태
	private bool _isActive;

	/// <summary>
	/// 고정된 높이 값을 반환합니다.
	/// </summary>
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int StaticHeight => 20;

	/// <summary>
	/// 현재 컨트롤이 활성 상태인지 가져오거나 설정합니다.
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
	/// 폴더 라벨이 클릭될 때 발생하는 이벤트입니다.
	/// </summary>
	/// <remarks>
	/// 사용자가 UI에서 폴더 라벨을 클릭할 때마다 발생합니다. 이 이벤트를 구독하면 폴더 이동, 정보 표시 등 원하는 동작을 구현할 수 있습니다.
	/// </remarks>
	[Category("PathLabel")]
	public event EventHandler<PathLabelPropertyClickEventArgs>? PropertyClick;

	/// <summary>
	/// PathLabel의 새 인스턴스를 초기화합니다. (기본 스타일 및 테마 적용)
	/// </summary>
	/// <remarks>
	/// 이 생성자는 더블 버퍼링, 사용자 지정 페인팅 등 컨트롤 스타일을 설정하여 렌더링 성능을 높이고, 테마 설정을 적용합니다.
	/// </remarks>
	public PathLabel()
	{
		SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.Selectable, true);
		TabStop = true;

		Height = StaticHeight;
	}

	/// <inheritdoc />
	protected override void OnCreateControl()
	{
		base.OnCreateControl();

		if (IsReallyDesignMode)
		{
			// 디자인 모드에서 기본 값 설정
			SetFolderInfo(string.Empty, 10, 20, 1234567);
			SetDriveInfo(null);
		}
	}

	/// <summary>
	/// 왼쪽 폴더/파일 정보를 갱신합니다.
	/// </summary>
	/// <param name="fullName">폴더 이름</param>
	/// <param name="dirCount">폴더 개수</param>
	/// <param name="fileCount">파일 개수</param>
	/// <param name="totalSize">전체 크기(바이트)</param>
	public void SetFolderInfo(string fullName, int dirCount, int fileCount, long totalSize)
	{
		_folder.FullName = fullName;
		_folder.Count = dirCount;
		_folder.FileCount = fileCount;
		_folder.TotalSize = totalSize;
		Invalidate();
	}

	/// <summary>
	/// 오른쪽 드라이브 정보를 갱신합니다.
	/// </summary>
	/// <param name="drive">드라이브 정보가 담긴 <see cref="DriveInfo"/> 객체</param>
	public void SetDriveInfo(DriveInfo? drive)
	{
		if (drive == null)
		{
			_drive.FullName = string.Empty;
			_drive.Name = "??";
			_drive.Label = "알 수 없음";
			_drive.Available = 0;
		}
		else
		{
			_drive.FullName = drive.Name;
			_drive.Name = drive.Name.TrimEnd('\\');
			_drive.Label = drive.VolumeLabel;
			_drive.Available = drive.AvailableFreeSpace;
		}

		_drive.Selected = string.Empty;
		Invalidate();
	}

	/// <summary>
	/// 선택된 항목 개수와 크기로 선택 정보 표시를 갱신합니다.
	/// </summary>
	/// <param name="count">선택된 항목 개수(0 이상)</param>
	/// <param name="size">선택된 항목의 전체 크기(바이트, 천 단위 구분 포함)</param>
	public void SetSelectionInfo(int count, long size)
	{
		_drive.Selected = $"{count}개 ({size:N0} 바이트)";
		Invalidate();
	}

	// 문자열을 그리는 헬퍼 메서드
	private float DrawString(Graphics g, string text, Brush brush, float x, float y)
	{
		var size = g.MeasureString(text, Font);
		g.DrawString(text, Font, brush, x, y);
		return size.Width;
	}

	// 문자열들의 너비를 측정하는 헬퍼 메서드
	private float MeasureStrings(Graphics g, params string[] texts) =>
		texts.Sum(text => g.MeasureString(text, Font).Width);

	/// <inheritdoc />
	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);

		var theme = Settings.Instance.Theme;
		var g = e.Graphics;
		g.Clear(IsActive ? theme.BackWindowActive : theme.BackContent);

		const string LeftFolder = "폴더, ";
		const string LeftFile = "파일 ";
		const string RightAvailable = "남음";

		using var highlightBrush = new SolidBrush(theme.Accelerator);
		using var textBrush = new SolidBrush(theme.Foreground);
		var y = (Height - Font.Height) / 2;
		float x = 0;

		// 왼쪽
		x += DrawString(g, $"{_folder.Count:N0}", highlightBrush, x, y);
		x += DrawString(g, LeftFolder, textBrush, x, y);
		x += DrawString(g, $"{_folder.FileCount:N0}", highlightBrush, x, y);
		x += DrawString(g, LeftFile, textBrush, x, y);
		x += DrawString(g, $"({_folder.TotalSize.FormatFileSize()})", textBrush, x, y);
		_folder.Rect = new Rectangle(0, 0, (int)x, Height);

		// 오른쪽
		if (!string.IsNullOrEmpty(_drive.Selected))
		{
			// 선택 정보가 있으면 선택 정보 표시
			x = Width - MeasureStrings(g, _drive.Selected);
			DrawString(g, _drive.Selected, highlightBrush, x, y);
			_drive.Rect = new Rectangle((int)x, 0, Width - (int)x, Height);
		}
		else
		{
			// 선택 정보가 없으면 드라이브 정보 표시
			var name = $"{_drive.Label} ({_drive.Name}) ";
			var available = _drive.Available.FormatFileSize();
			var start = Width - MeasureStrings(g, name, available, RightAvailable);
			x = start;
			x += DrawString(g, name, textBrush, x, y);
			x += DrawString(g, available, highlightBrush, x, y);
			DrawString(g, RightAvailable, textBrush, x, y);
			_drive.Rect = new Rectangle((int)start, 0, Width - (int)start, Height);
		}
	}

	/// <inheritdoc />
	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);

		switch (_folder.Rect.Contains(e.Location))
		{
			case true when !_folder.Hover:
				Cursor = Cursors.Hand;
				_folder.Hover = true;
				break;
			case false when _folder.Hover:
				_folder.Hover = false;
				Cursor = Cursors.Default;
				break;
		}

		if (string.IsNullOrEmpty(_drive.Selected))
		{
			switch (_drive.Rect.Contains(e.Location))
			{
				case true when !_drive.Hover:
					Cursor = Cursors.Hand;
					_drive.Hover = true;
					break;
				case false when _drive.Hover:
					_drive.Hover = false;
					Cursor = Cursors.Default;
					break;
			}
		}
	}

	/// <inheritdoc />
	protected override void OnMouseLeave(EventArgs e)
	{
		base.OnMouseLeave(e);
		_folder.Hover = false;
		_drive.Hover = false;
		Cursor = Cursors.Default;
	}

	/// <inheritdoc />
	protected override void OnMouseDown(MouseEventArgs e)
	{
		base.OnMouseDown(e);
		Focus();

		if (e.Button != MouseButtons.Left || PropertyClick == null)
			return;

		var path =
			_folder.Rect.Contains(e.Location) ? _folder.FullName :
			_drive.Rect.Contains(e.Location) && string.IsNullOrEmpty(_drive.Selected) ? _drive.FullName :
			null;
		if (!string.IsNullOrEmpty(path))
			PropertyClick.Invoke(this, new PathLabelPropertyClickEventArgs(this, e.Location, path));
	}

	// 폴더 정보 구조체
	private struct FolderDef
	{
		public string FullName { get; set; }
		public int Count { get; set; }
		public int FileCount { get; set; }
		public long TotalSize { get; set; }
		public Rectangle Rect { get; set; }
		public bool Hover { get; set; }
	}

	// 드라이브 정보 구조체
	private struct DriveDef
	{
		public string FullName { get; set; }
		public string Name { get; set; }
		public string Label { get; set; }
		public long Available { get; set; }
		public string Selected { get; set; }
		public Rectangle Rect { get; set; }
		public bool Hover { get; set; }
	}
}

/// <summary>
/// PathLabel 클릭 이벤트 인수 클래스입니다.
/// </summary>
public class PathLabelPropertyClickEventArgs : EventArgs
{
	/// <summary>
	/// 클릭된 위치(컨트롤 내 상대 좌표)입니다.
	/// </summary>
	public Point Location { get; }

	/// <summary>
	/// 클릭된 위치의 스크린 좌표입니다.
	/// </summary>
	public Point ScreenLocation { get; }

	/// <summary>
	/// 클릭한 경로(폴더 또는 드라이브)입니다.
	/// </summary>
	public string Path { get; }

	/// <summary>
	/// PathLabelPropertyClickEventArgs를 생성합니다.
	/// </summary>
	/// <param name="control">클릭된 PathLabel 컨트롤</param>
	/// <param name="location">클릭 위치(컨트롤 내 좌표)</param>
	/// <param name="path">클릭한 경로</param>
	public PathLabelPropertyClickEventArgs(PathLabel control, Point location, string path)
	{
		Location = location;
		ScreenLocation = control.PointToScreen(location);
		Path = path;
	}
}
