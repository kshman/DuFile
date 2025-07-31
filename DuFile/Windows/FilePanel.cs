namespace DuFile.Windows;

/// <summary>
/// 탭 UI를 통해 디렉터리와 파일을 탐색하고 관리하는 사용자 컨트롤입니다.
/// </summary>
/// <remarks>
/// <see cref="FilePanel"/> 컨트롤은 디렉터리 및 파일 탐색을 위한 탭 인터페이스를 제공합니다.
/// 디렉터리 정보 표시, 탭 관리, 사용자 상호작용(컨텍스트 메뉴 등) 기능을 포함하며,
/// 파일 관리가 필요한 애플리케이션에 통합되어 사용됩니다.
/// </remarks>
public class FilePanel : UserControl, IThemeUpate
{
#nullable disable
	private TabStrip tabStrip; // 탭 UI 컨트롤
	private Label fileInfoLabel; // 파일 정보 표시 라벨
	private Panel workPanel; // 작업 영역 패널
	private Panel dirPanel; // 디렉터리 패널
	private BreadcrumbPath breadcrumbPath; // 경로 표시 컨트롤
	private TextBox pathTextBox; // 경로 입력 텍스트박스
	private PathLabel pathLabel; // 경로 정보 라벨
	private Button historyButton; // 히스토리 버튼
	private Button refreshButton; // 새로고침 버튼
	private Button editDirButton; // 경로 편집 버튼
	private Panel infoPanel; // 정보 표시 패널
	private FileList fileList; // 파일 리스트 컨트롤
#nullable restore

	private string _current = string.Empty; // 현재 디렉터리 경로
	private bool _isActive; // 활성화 상태
	private bool _isEditPathMode; // 경로 편집 모드 여부
	private bool _tabLoaded; // 탭 로드 여부
	private List<string> _history = []; // 디렉터리 이동 이력

	/// <summary>
	/// 파일 패널의 인덱스입니다.
	/// </summary>
	[Category("FilePanel")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	public int PanelIndex { get; set; } = 0;

	/// <summary>
	/// 패널 활성화 이벤트입니다.
	/// </summary>
	[Category("FilePanel")]
	public event EventHandler<FilePanelActiveEventArgs>? PanelActivated;

	/// <summary>
	/// 메인 폼 참조입니다.
	/// </summary>
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public MainForm? MainForm { get; set; }

	// 디자인 모드 여부 확인
	private bool IsReallyDesignMode => LicenseManager.UsageMode == LicenseUsageMode.Designtime || (Site?.DesignMode ?? false);

	/// <summary>
	/// 파일 패널을 초기화합니다.
	/// </summary>
	public FilePanel()
	{
		SetStyle(ControlStyles.Selectable, true);
		TabStop = true;

		InitializeComponent();
	}

	// 컨트롤 구성 요소 초기화
	private void InitializeComponent()
	{
		tabStrip = new TabStrip();
		fileInfoLabel = new Label();
		workPanel = new Panel();
		fileList = new FileList();
		infoPanel = new Panel();
		pathLabel = new PathLabel();
		dirPanel = new Panel();
		historyButton = new Button();
		refreshButton = new Button();
		editDirButton = new Button();
		breadcrumbPath = new BreadcrumbPath();
		pathTextBox = new TextBox();
		workPanel.SuspendLayout();
		infoPanel.SuspendLayout();
		dirPanel.SuspendLayout();
		SuspendLayout();
		// 
		// tabStrip
		// 
		tabStrip.ActiveTabHighlightHeight = 2;
		tabStrip.CloseButtonSize = 14;
		tabStrip.Dock = DockStyle.Top;
		tabStrip.IconSize = 16;
		tabStrip.Location = new Point(0, 0);
		tabStrip.MaxTabWidth = 180;
		tabStrip.MinTabWidth = 60;
		tabStrip.Name = "tabStrip";
		tabStrip.ScrollButtonWidth = 22;
		tabStrip.Size = new Size(400, 23);
		tabStrip.TabIndex = 0;
		tabStrip.Text = "tabStrip";
		tabStrip.SelectedIndexChanged += tabStrip_SelectedIndexChanged;
		tabStrip.CloseClick += tabStrip_CloseClick;
		tabStrip.ElementClick += tabStrip_ElementClick;
		// 
		// fileInfoLabel
		// 
		fileInfoLabel.BackColor = Color.FromArgb(63, 63, 70);
		fileInfoLabel.Dock = DockStyle.Bottom;
		fileInfoLabel.ForeColor = Color.FromArgb(241, 241, 241);
		fileInfoLabel.Location = new Point(0, 305);
		fileInfoLabel.Name = "fileInfoLabel";
		fileInfoLabel.Size = new Size(398, 20);
		fileInfoLabel.TabIndex = 1;
		fileInfoLabel.Text = "(파일 정보)";
		fileInfoLabel.TextAlign = ContentAlignment.MiddleLeft;
		// 
		// workPanel
		// 
		workPanel.BorderStyle = BorderStyle.FixedSingle;
		workPanel.Controls.Add(fileList);
		workPanel.Controls.Add(infoPanel);
		workPanel.Controls.Add(dirPanel);
		workPanel.Controls.Add(fileInfoLabel);
		workPanel.Dock = DockStyle.Fill;
		workPanel.Location = new Point(0, 23);
		workPanel.Name = "workPanel";
		workPanel.Size = new Size(400, 327);
		workPanel.TabIndex = 2;
		// 
		// fileList
		// 
		fileList.BackColor = Color.FromArgb(20, 20, 20);
		fileList.Dock = DockStyle.Fill;
		fileList.ForeColor = Color.FromArgb(241, 241, 241);
		fileList.Location = new Point(0, 40);
		fileList.Name = "fileList";
		fileList.Size = new Size(398, 265);
		fileList.TabIndex = 4;
		fileList.Text = "fileList1";
		fileList.FocusedIndexChanged += fileList_FocusedIndexChanged;
		fileList.SelectionChanged += fileList_SelectionChanged;
		fileList.ItemActivate += fileList_ItemActivate;
		fileList.ItemClicked += fileList_ItemClicked;
		fileList.MouseDown += fileList_MouseDown;
		// 
		// infoPanel
		// 
		infoPanel.BackColor = Color.FromArgb(20, 20, 20);
		infoPanel.Controls.Add(pathLabel);
		infoPanel.Dock = DockStyle.Top;
		infoPanel.Location = new Point(0, 20);
		infoPanel.Name = "infoPanel";
		infoPanel.Size = new Size(398, 20);
		infoPanel.TabIndex = 3;
		// 
		// pathLabel
		// 
		pathLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		pathLabel.Location = new Point(3, 0);
		pathLabel.Name = "pathLabel";
		pathLabel.Size = new Size(392, 20);
		pathLabel.TabIndex = 1;
		pathLabel.PropertyClick += pathLabel_PropertyClick;
		// 
		// dirPanel
		// 
		dirPanel.BackColor = Color.FromArgb(63, 63, 70);
		dirPanel.Controls.Add(historyButton);
		dirPanel.Controls.Add(refreshButton);
		dirPanel.Controls.Add(editDirButton);
		dirPanel.Controls.Add(breadcrumbPath);
		dirPanel.Controls.Add(pathTextBox);
		dirPanel.Dock = DockStyle.Top;
		dirPanel.Location = new Point(0, 0);
		dirPanel.Name = "dirPanel";
		dirPanel.Size = new Size(398, 20);
		dirPanel.TabIndex = 2;
		// 
		// historyButton
		// 
		historyButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		historyButton.FlatAppearance.BorderSize = 0;
		historyButton.FlatStyle = FlatStyle.Flat;
		historyButton.Image = Resources.history16;
		historyButton.Location = new Point(375, 0);
		historyButton.Margin = new Padding(0);
		historyButton.Name = "historyButton";
		historyButton.Size = new Size(20, 20);
		historyButton.TabIndex = 3;
		historyButton.UseVisualStyleBackColor = true;
		historyButton.Click += historyButton_Click;
		// 
		// refreshButton
		// 
		refreshButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		refreshButton.FlatAppearance.BorderSize = 0;
		refreshButton.FlatStyle = FlatStyle.Flat;
		refreshButton.Image = Resources.refresh16;
		refreshButton.Location = new Point(354, 0);
		refreshButton.Margin = new Padding(0);
		refreshButton.Name = "refreshButton";
		refreshButton.Size = new Size(20, 20);
		refreshButton.TabIndex = 2;
		refreshButton.UseVisualStyleBackColor = true;
		refreshButton.Click += refreshButton_Click;
		// 
		// editDirButton
		// 
		editDirButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		editDirButton.FlatAppearance.BorderSize = 0;
		editDirButton.FlatStyle = FlatStyle.Flat;
		editDirButton.Image = Resources.pen16;
		editDirButton.Location = new Point(333, 0);
		editDirButton.Margin = new Padding(0);
		editDirButton.Name = "editDirButton";
		editDirButton.Size = new Size(20, 20);
		editDirButton.TabIndex = 1;
		editDirButton.UseVisualStyleBackColor = true;
		editDirButton.Click += editDirButton_Click;
		// 
		// breadcrumbPath
		// 
		breadcrumbPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		breadcrumbPath.Location = new Point(0, 0);
		breadcrumbPath.Name = "breadcrumbPath";
		breadcrumbPath.Size = new Size(330, 20);
		breadcrumbPath.TabIndex = 0;
		breadcrumbPath.TabStop = false;
		breadcrumbPath.Text = "breadcrumbPath";
		breadcrumbPath.PathClick += breadcrumbPath_PathClick;
		// 
		// pathTextBox
		// 
		pathTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		pathTextBox.Location = new Point(0, 0);
		pathTextBox.Name = "pathTextBox";
		pathTextBox.Size = new Size(330, 23);
		pathTextBox.TabIndex = 0;
		pathTextBox.TabStop = false;
		pathTextBox.Visible = false;
		pathTextBox.KeyDown += pathTextBox_KeyDown;
		pathTextBox.LostFocus += pathTextBox_LostFocus;
		// 
		// FilePanel
		// 
		BackColor = Color.FromArgb(37, 37, 37);
		Controls.Add(workPanel);
		Controls.Add(tabStrip);
		ForeColor = Color.FromArgb(241, 241, 241);
		Name = "FilePanel";
		Size = new Size(400, 350);
		workPanel.ResumeLayout(false);
		infoPanel.ResumeLayout(false);
		dirPanel.ResumeLayout(false);
		dirPanel.PerformLayout();
		ResumeLayout(false);

	}

	/// <summary>
	/// 테마를 적용합니다.
	/// </summary>
	public void UpdateTheme(Theme theme)
	{
		BackColor = theme.Background;
		ForeColor = theme.Foreground;

		fileInfoLabel.BackColor = theme.Background;
		fileInfoLabel.ForeColor = theme.Foreground;

		dirPanel.BackColor = theme.Background;
		dirPanel.ForeColor = theme.Foreground;

		infoPanel.BackColor = theme.BackContent;
		infoPanel.ForeColor = theme.Foreground;

		editDirButton.ForeColor = theme.Foreground;
		editDirButton.BackColor = theme.Background;
		editDirButton.FlatAppearance.MouseOverBackColor = theme.BackHover;
		editDirButton.FlatAppearance.MouseDownBackColor = theme.BackActive;

		refreshButton.ForeColor = theme.Foreground;
		refreshButton.BackColor = theme.Background;
		refreshButton.FlatAppearance.MouseOverBackColor = theme.BackHover;
		refreshButton.FlatAppearance.MouseDownBackColor = theme.BackActive;

		historyButton.ForeColor = theme.Foreground;
		historyButton.BackColor = theme.Background;
		historyButton.FlatAppearance.MouseOverBackColor = theme.BackHover;
		historyButton.FlatAppearance.MouseDownBackColor = theme.BackActive;
	}

	/// <inheritdoc/>
	protected override void Dispose(bool disposing)
	{
		if (disposing)
			SaveTabs();

		base.Dispose(disposing);
	}

	/// <inheritdoc/>
	protected override void OnCreateControl()
	{
		base.OnCreateControl();
		UpdateTheme(Settings.Instance.Theme);
	}

	/// <inheritdoc/>
	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
		LoadTabs();
	}

	/// <inheritdoc/>
	protected override void OnEnter(EventArgs e)
	{
		base.OnEnter(e);
		SetActivePanel(true);
	}

	/// <inheritdoc/>
	protected override void OnLeave(EventArgs e)
	{
		base.OnLeave(e);
		if (_isEditPathMode)
			LeavePathEditMode();
		SetActivePanel(false);
	}

	// 탭 닫기 이벤트 핸들러
	private void tabStrip_CloseClick(object? sender, TabStripCloseClickEventArgs e)
	{
		tabStrip.RemoveTabAt(e.Index);
	}

	// 탭 선택 변경 이벤트 핸들러
	private void tabStrip_SelectedIndexChanged(object? sender, TabStripIndexChangedEventArgs e)
	{
		if (tabStrip.SelectedTab?.Value is string path)
		{
			if (!NavigateTo(path))
			{
				if (tabStrip.Count > 1)
					tabStrip.RemoveTabAt(e.Index);
				else
					NavigateTo(Settings.Instance.StartFolder);
			}
		}
	}

	// 탭 요소 클릭 이벤트 핸들러
	private void tabStrip_ElementClick(object? sender, TabStripElementClickEventArgs e)
	{
		if (e.Element is TabStripElement.Tab or TabStripElement.None)
		{
			if (e.Button == MouseButtons.Right)
			{
				// 탭 메뉴 처리
				var menu = new ContextMenuStrip();

				if (e.Element == TabStripElement.Tab)
				{
					Debugs.Assert(e.Index >= 0);

					// 선택 탭 닫기
					var closeItem = new ToolStripMenuItem("닫기", null, (_, _) =>
					{
						tabStrip.RemoveTabAt(e.Index);
					});
					menu.Items.Add(closeItem);

					// 다른 탭 닫기
					if (tabStrip.Count > 1)
					{
						var closeOthersItem = new ToolStripMenuItem("다른 탭 닫기", null, (_, _) =>
						{
							tabStrip.RemoveOtherTabs(e.Index);
						});
						menu.Items.Add(closeOthersItem);
					}

					// 가로 줄 추가
					menu.Items.Add(new ToolStripSeparator());
				}

				// 새 탭 추가
				var newTabItem = new ToolStripMenuItem("새 탭 추가", null, (_, _) =>
				{
					AddTab(null, true);
				});
				menu.Items.Add(newTabItem);

				menu.Show(tabStrip, e.Location);
				e.Handled = true; // 메뉴가 처리했으므로 기본 동작은 하지 않음
			}
			else if (e is { Button: MouseButtons.Middle, Element: TabStripElement.Tab })
			{
				// 가운데 클릭으로 탭 닫기
				tabStrip.RemoveTabAt(e.Index);
				e.Handled = true;
			}
		}
	}

	// 경로 텍스트박스 키 입력 이벤트 핸들러
	private void pathTextBox_KeyDown(object? sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Enter)
		{
			var path = pathTextBox.Text.Trim();
			NavigateTo(path);
			LeavePathEditMode();
			e.Handled = true;
		}
		else if (e.KeyCode == Keys.Escape)
		{
			LeavePathEditMode();
			e.Handled = true;
		}
	}

	// 경로 텍스트박스 포커스 해제 이벤트 핸들러
	private void pathTextBox_LostFocus(object? sender, EventArgs e)
	{
		LeavePathEditMode();
	}

	// 경로 편집 버튼 클릭 이벤트 핸들러
	private void editDirButton_Click(object? sender, EventArgs e)
	{
		if (_isEditPathMode)
			LeavePathEditMode();
		else
			EnterPathEditMode();
	}

	// 새로고침 버튼 클릭 이벤트 핸들러
	private void refreshButton_Click(object? sender, EventArgs e) =>
		NavigateTo(breadcrumbPath.Path);

	// 히스토리 버튼 클릭 이벤트 핸들러
	private void historyButton_Click(object? sender, EventArgs e)
	{
		if (_history.Count == 0)
			return;

		var menu = new ContextMenuStrip();

		var i = 0;
		foreach (var path in _history.Take(9))
		{
			i++;
			var item = new ToolStripMenuItem(path)
			{
				Tag = $"(&{i}) {path}",
			};
			item.Click += (_, _) => NavigateTo(item.Tag.ToString()!);
			menu.Items.Add(item);
		}

		menu.Show(historyButton, new Point(0, historyButton.Height));
	}

	// 경로 클릭 이벤트 핸들러
	private void breadcrumbPath_PathClick(object? sender, BreadcrumbPathClickEventArgs e)
	{
		switch (e.Button)
		{
			case MouseButtons.Left when !string.IsNullOrEmpty(e.Path):
				NavigateTo(e.Path);
				break;
			case MouseButtons.Right:
			{
				// 메뉴
				var menu = new ContextMenuStrip();
				ToolStripMenuItem item;

				if (!string.IsNullOrEmpty(e.Path))
				{
					item = new ToolStripMenuItem(e.Path, null);
					item.Enabled = false;
					menu.Items.Add(item);

					menu.Items.Add(new ToolStripSeparator());
				}

				item = new ToolStripMenuItem("전체 주소 복사", null, (_, _) =>
				{
					Clipboard.SetText(breadcrumbPath.Path);
				});
				menu.Items.Add(item);

				if (!string.IsNullOrEmpty(e.Path))
				{
					item = new ToolStripMenuItem("주소 복사", null, (_, _) =>
					{
						Clipboard.SetText(e.Path);
					});
					menu.Items.Add(item);
				}

				item = new ToolStripMenuItem("주소 편집", null, (_, _) =>
				{
					EnterPathEditMode();
				});
				menu.Items.Add(item);

				if (!string.IsNullOrEmpty(e.Path))
				{
					menu.Items.Add(new ToolStripSeparator());

					item = new ToolStripMenuItem("새 탭에서 열기", null, (_, _) =>
					{
						AddTab(e.Path, true);
					});
					menu.Items.Add(item);
				}

				menu.Show(breadcrumbPath, e.Location);
				break;
			}
		}
	}

	// 경로 라벨 속성 클릭 이벤트 핸들러
	private void pathLabel_PropertyClick(object? sender, PathLabelPropertyClickEventArgs e)
	{
		MainForm?.ExcuteShowProperties((Control)sender!, e.Path);
	}

	// 파일 리스트 마우스 다운 이벤트 핸들러
	private void fileList_MouseDown(object? sender, MouseEventArgs e)
	{
		SetActivePanel(true);
	}

	// 파일 리스트 포커스 변경 이벤트 핸들러
	private void fileList_FocusedIndexChanged(object? sender, EventArgs e)
	{
		var item = fileList.GetItem(fileList.FocusedIndex);
		fileInfoLabel.Text = item switch
		{
			null => "(선택한 파일이 없어요)",
			FileListFileItem fi => $"{fi.Size:N0} | {fi.LastWrite} | {fi.Attributes.FormatString()} | {fi.FileName}",
			FileListFolderItem di => $"폴더 | {di.LastWrite} | {di.Attributes.FormatString()} | {di.DirName}",
			FileListDriveItem vi => vi.Type switch
			{
				DriveType.Fixed or DriveType.Ram =>
					$"{vi.VolumeLabel} ({vi.Letter}) | {vi.Format} 디스크 | {vi.Available.FormatFileSize()} 남음 / {vi.Total.FormatFileSize()}",
				DriveType.Removable =>
					$"{vi.VolumeLabel} ({vi.Letter} ) | {vi.Format} 이동식 디스크 | {vi.Available.FormatFileSize()} 남음 / {vi.Total.FormatFileSize()}",
				DriveType.Network =>
					$"{vi.VolumeLabel} ({vi.Letter}) | 네트워크 드라이브 | {vi.Available.FormatFileSize()} 남음 / {vi.Total.FormatFileSize()}",
				DriveType.CDRom => $"{vi.VolumeLabel} ({vi.Letter}) | 광 디스크 | {vi.Total.FormatFileSize()}",
				_ => $"{vi.VolumeLabel} ({vi.Letter}) | 알 수 없는 드라이브"
			},
			_ => fileInfoLabel.Text
		};
	}

	// 파일 리스트 항목 활성화 이벤트 핸들러
	private void fileList_ItemActivate(object? sender, EventArgs e)
	{
		var item = fileList.GetItem(fileList.FocusedIndex);
		switch (item)
		{
			case null:
				break;
			case FileListFileItem fi:
				MainForm?.ExcuteProcess(fi.FullName);
				break;
			case FileListFolderItem di:
				NavigateTo(di.FullName, fileList.FullName);
				break;
			case FileListDriveItem vi:
				var path = Settings.Instance.GetDriveHistory(vi.DriveName);
				NavigateTo(path);
				break;
		}
	}

	// 파일 리스트 항목 클릭 이벤트 핸들러
	private void fileList_ItemClicked(object? sender, FileListClickEventArgs e)
	{
		if (e.Button == MouseButtons.Right)
		{
			// 오른쪽 눌리면 쉘 컨텍스트 메뉴를 띄운다
			var l = fileList.GetSelectedOrFocused();
			if (l.Count > 0)
			{
				var scrPos = fileList.PointToScreen(e.Location);
				MainForm?.ExcuteShowContextMenu(fileList, scrPos, l);
			}
		}
	}

	// 파일 리스트 선택 변경 이벤트 핸들러
	private void fileList_SelectionChanged(object? sender, EventArgs e)
	{
		var count = fileList.GetSelectedCount();
		if (count == 0)
		{
			// 선택이 없으면 드라이브 정보
			var di = new DirectoryInfo(_current);
			pathLabel.SetDriveInfo(di.Exists ? new DriveInfo(di.Root.FullName) : null);
		}
		else
		{
			// 아니면 선택 정보
			pathLabel.SetSelectionInfo(count, fileList.GetSelectedSize());
		}
	}

	/// <summary>
	/// 지정한 디렉터리로 이동합니다.
	/// </summary>
	/// <param name="directory">이동할 디렉터리 경로</param>
	/// <param name="selection">선택할 항목 이름</param>
	/// <returns>이동 성공 여부</returns>
	public bool NavigateTo(string directory, string? selection = null)
	{
		if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
			return false;

		var settings = Settings.Instance;
		var showHidden = settings.ShowHidden;

		_current = directory;
		breadcrumbPath.Path = directory;

		var info = new DirectoryInfo(directory);
		settings.SetDriveHistory(info.Name, directory);

		// 이력 갱신
		_history.Remove(directory); // 중복 제거
		_history.Add(directory);
		if (_history.Count > 20)
			_history.RemoveAt(0); // 최대 20개까지만 유지

		// 탭 갱신
		tabStrip.SetTabTextAt(tabStrip.SelectedIndex, info.Name, directory);

		// 리스트 갱신
		//... 인데 리스트가 없으니 일단 다른것부터
		var dirCount = 0;
		var fileCount = 0;
		var totalSize = 0L;

		fileList.FullName = info.FullName;
		fileList.BeginUpdate();
		fileList.ClearItems();

		try
		{
			// 루트 디렉토리가 아니면 ".." 항목을 추가
			if (info.Parent is { Exists: true })
				fileList.AddParentFolder(info.Parent);

			// 폴더 정보 갱신
			foreach (var d in info.GetDirectories())
			{
				if (!showHidden && (d.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
					continue;

				fileList.AddFolder(d);
				dirCount++;
			}

			// 파일 정보 갱신
			foreach (var f in info.GetFiles())
			{
				if (!showHidden && (f.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
					continue;

				fileList.AddFile(f);
				fileCount++;
				totalSize += f.Length;
			}

			// 드라이브 정보 갱신
			foreach (var v in DriveInfo.GetDrives())
			{
				if (!v.IsReady)
					continue;

				fileList.AddDrive(v);
			}
		}
		catch
		{
			// 아니 왜...?
		}

		fileList.EndUpdate();
		fileList.EnsureFocusByName(selection);

		pathLabel.SetFolderInfo(info.FullName, dirCount, fileCount, totalSize);
		pathLabel.SetDriveInfo(new DriveInfo(info.Root.FullName));

		return true;
	}

	/// <summary>
	/// 패널의 활성화 상태를 설정합니다.
	/// </summary>
	/// <param name="isActive">활성화 여부</param>
	public void SetActivePanel(bool isActive)
	{
		if (_isActive == isActive)
			return;

		if (isActive)
			ActiveControl = fileList;

		_isActive = isActive;
		fileList.IsActive = isActive;
		pathLabel.IsActive = isActive;
		tabStrip.IsActive = isActive;

		PanelActivated?.Invoke(this, new FilePanelActiveEventArgs(this, isActive));
	}

	// 경로 편집 모드로 진입
	private void EnterPathEditMode()
	{
		_isEditPathMode = true;
		breadcrumbPath.Visible = false;
		pathTextBox.Text = breadcrumbPath.Path;
		pathTextBox.Visible = true;
		pathTextBox.Focus();
		pathTextBox.SelectAll();
		editDirButton.BackColor = Settings.Instance.Theme.BackActive;
	}

	// 경로 편집 모드 종료
	private void LeavePathEditMode()
	{
		_isEditPathMode = false;
		pathTextBox.Visible = false;
		breadcrumbPath.Visible = true;
		editDirButton.BackColor = Settings.Instance.Theme.Background;
	}

	/// <summary>
	/// 새 탭을 추가합니다.
	/// </summary>
	/// <param name="directory">탭에 추가할 디렉터리 경로</param>
	/// <param name="force">강제로 추가 여부</param>
	public void AddTab(string? directory, bool force = false)
	{
		if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
		{
			var info = new DirectoryInfo(_current);
			directory = info is { Exists: true } ? info.FullName : Settings.Instance.StartFolder;
		}

		var index = tabStrip.GetTabIndexByValue(directory);
		if (!force && index >= 0)
		{
			tabStrip.SelectedIndex = index;
			return;
		}

		var di = new DirectoryInfo(directory);
		index = tabStrip.AddTab(di.Name, di.FullName);
		if (index >= 0)
			tabStrip.SelectedIndex = index;
	}

	/// <summary>
	/// 다음 탭으로 이동합니다.
	/// </summary>
	public void NextTab()
	{
		if (tabStrip.SelectedIndex < tabStrip.Count - 1)
			tabStrip.SelectedIndex++;
		else
			tabStrip.SelectedIndex = 0;
	}

	/// <summary>
	/// 이전 탭으로 이동합니다.
	/// </summary>
	public void PreviousTab()
	{
		if (tabStrip.SelectedIndex > 0)
			tabStrip.SelectedIndex--;
		else
			tabStrip.SelectedIndex = tabStrip.Count - 1;
	}

	// 탭 목록을 읽어옵니다. 한 번만 호출 가능합니다.
	private void LoadTabs()
	{
		if (_tabLoaded)
			throw new InvalidOperationException("탭 목록은 한 번만 읽을 수 있습니다.");

		_tabLoaded = true;

		if (IsReallyDesignMode)
		{
			// 디자인 모드에서는 탭을 처리하면 안된다
			return;
		}

		var settings = Settings.Instance;
		var prefix = $"Panel{PanelIndex}";

		var isOk = true;
		var activeIndex = settings.GetInt($"{prefix}Active", -1);
		if (activeIndex < 0)
			isOk = false;
		var tabs = settings.GetString($"{prefix}Tabs", string.Empty);
		if (string.IsNullOrEmpty(tabs))
			isOk = false;
		var history = settings.GetString($"{prefix}History", string.Empty);

		// 일단 히스토리부터
		if (!string.IsNullOrEmpty(history))
		{
			_history = history.SplitWithSeparator('|').ToList();
			if (_history.Count > 20)
				_history = _history.Take(20).ToList(); // 최대 20개까지만 유지
		}

		// 읽을 탭이 있으면 처리
		if (isOk)
		{
			var folders = tabs.SplitWithSeparator('|');
			if (folders.Length > 0)
			{
				foreach (var folder in folders)
				{
					var d = new DirectoryInfo(folder);
					if (!d.Exists)
						continue; // 유효하지 않은 경로는 무시
					tabStrip.AddTab(d.Name, folder);
				}

				if (tabStrip.Count > 0)
				{
					if (activeIndex < tabStrip.Count)
						tabStrip.SelectedIndex = activeIndex;
				}
			}
		}

		// 액티브
		if (settings.ActivePanel == PanelIndex)
			SetActivePanel(true);
	}

	// 탭 목록을 저장합니다. Dispose에서 호출됩니다.
	private void SaveTabs()
	{
		if (IsReallyDesignMode)
		{
			// 디자인 모드에서는 탭을 처리하면 안된다
			return;
		}

		var settings = Settings.Instance;
		var prefix = $"Panel{PanelIndex}";
		var tags = tabStrip.GetValueList();

		settings.SetInt($"{prefix}Active", tabStrip.SelectedIndex);
		settings.SetString($"{prefix}Tabs", tags.Count > 0 ? string.Join("|", tags) : string.Empty);
		settings.SetString($"{prefix}History", _history.Count > 0 ? string.Join("|", _history) : string.Empty);
	}

	/// <summary>
	/// 현재 디렉터리 경로를 반환합니다.
	/// </summary>
	public string CurrentDirectory => _current;

	/// <summary>
	/// 파일 리스트를 설정에 따라 정렬합니다.
	/// </summary>
	public void Sort()
	{
		fileList.Sort();
		fileList.Invalidate();
	}

	/// <summary>
	/// 선택된 항목의 개수를 반환합니다.
	/// </summary>
	public int GetSelectedCount() =>
		fileList.GetSelectedCount();

	/// <summary>
	/// 선택된 파일의 총 크기를 반환합니다.
	/// </summary>
	public long GetSelectedSize() =>
		fileList.GetSelectedSize();

	/// <summary>
	/// 선택된 파일 목록을 반환합니다.
	/// </summary>
	public List<string> GetSelectedItems() =>
		fileList.GetSelectedOrFocused();

	/// <summary>
	/// 현재 포커스된 항목의 전체 경로를 반환합니다.
	/// </summary>
	public string? GetFocusedItem()
	{
		var index = fileList.FocusedIndex;
		var item = fileList.GetItem(index);
		return item?.FullName;
	}
}

/// <summary>
/// 파일 패널의 활성화 이벤트 인자 클래스입니다.
/// </summary>
/// <param name="panel">이벤트가 발생한 파일 패널</param>
/// <param name="isActive">활성화 여부</param>
public class FilePanelActiveEventArgs(FilePanel panel, bool isActive) : EventArgs
{
	/// <summary>
	/// 이벤트가 발생한 파일 패널입니다.
	/// </summary>
	public FilePanel Panel { get; } = panel;
	/// <summary>
	/// 패널의 활성화 여부입니다.
	/// </summary>
	public bool IsActive { get; } = isActive;
}
