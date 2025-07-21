// ReSharper disable MissingXmlDoc
namespace DuFile.Windows;

/// <summary>
/// Represents a user control that displays and manages a file panel with tabbed navigation.
/// </summary>
/// <remarks>The <see cref="FilePanel"/> control provides a tabbed interface for navigating directories and files.
/// It includes features such as displaying directory information, managing tabs, and handling user interactions through
/// context menus. The control is designed to be integrated into a larger application where file management is
/// required.</remarks>
public class FilePanel : UserControl
{
#nullable disable
	private TabStrip tabStrip;
	private Label fileInfoLabel;
	private EkePanel workPanel;
	private Panel dirPanel;
	private BreadcrumbPath breadcrumbPath;
	private TextBox pathTextBox;
	private DirectoryLabel drvDirLabel;
	private Button historyButton;
	private Button refreshButton;
	private Button editDirButton;
	private Panel infoPanel;
	private FileSelection fileSelection;
#nullable restore

	private string _currentDirectory = string.Empty;
	private bool _isActivePanel;
	private bool _isEditPathMode;
	private bool _tabLoaded;
	private List<string> _history = [];

	[Category("FilePanel")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	public int PanelIndex { get; set; } = 0;

	[Category("FilePanel")]
	public event EventHandler<FilePanelActiveEventArgs>? PanelActivated;

	// 디자인 모드 확인
	private bool IsReallyDesignMode => LicenseManager.UsageMode == LicenseUsageMode.Designtime || (Site?.DesignMode ?? false);

	public FilePanel()
	{
		SetStyle(ControlStyles.Selectable, true);
		TabStop = true;

		InitializeComponent();
		ApplyTheme();
	}

	private void InitializeComponent()
	{
		tabStrip = new TabStrip();
		fileInfoLabel = new Label();
		workPanel = new EkePanel();
		infoPanel = new Panel();
		drvDirLabel = new DirectoryLabel();
		dirPanel = new Panel();
		historyButton = new Button();
		refreshButton = new Button();
		editDirButton = new Button();
		breadcrumbPath = new BreadcrumbPath();
		pathTextBox = new TextBox();
		fileSelection = new FileSelection();
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
		tabStrip.CloseClicked += TabStripCloseClicked;
		tabStrip.Clicked += tabStrip_Clicked;
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
		fileInfoLabel.Click += fileInfoLabel_Click;
		// 
		// workPanel
		// 
		workPanel.BorderColor = Color.Gray;
		workPanel.BorderStyle = BorderStyle.FixedSingle;
		workPanel.BorderThickness = 1;
		workPanel.Controls.Add(fileSelection);
		workPanel.Controls.Add(infoPanel);
		workPanel.Controls.Add(dirPanel);
		workPanel.Controls.Add(fileInfoLabel);
		workPanel.Dock = DockStyle.Fill;
		workPanel.Location = new Point(0, 23);
		workPanel.Name = "workPanel";
		workPanel.Size = new Size(400, 327);
		workPanel.TabIndex = 2;
		// 
		// infoPanel
		// 
		infoPanel.BackColor = Color.FromArgb(20, 20, 20);
		infoPanel.Controls.Add(drvDirLabel);
		infoPanel.Dock = DockStyle.Top;
		infoPanel.Location = new Point(0, 20);
		infoPanel.Name = "infoPanel";
		infoPanel.Size = new Size(398, 20);
		infoPanel.TabIndex = 3;
		// 
		// drvDirLabel
		// 
		drvDirLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		drvDirLabel.Font = new Font("Microsoft Sans Serif", 8.25F);
		drvDirLabel.Location = new Point(3, 0);
		drvDirLabel.Name = "drvDirLabel";
		drvDirLabel.Size = new Size(392, 20);
		drvDirLabel.TabIndex = 1;
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
		breadcrumbPath.BreadcrumbPathClicked += breadcrumbPath_BreadcrumbPathClicked;
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
		// fileSelection
		// 
		fileSelection.BackColor = Color.FromArgb(20, 20, 20);
		fileSelection.Dock = DockStyle.Fill;
		fileSelection.Font = new Font("Microsoft Sans Serif", 10F);
		fileSelection.ForeColor = Color.FromArgb(241, 241, 241);
		fileSelection.FullRowSelect = true;
		fileSelection.Location = new Point(0, 40);
		fileSelection.Name = "fileSelection";
		fileSelection.OwnerDraw = true;
		fileSelection.Size = new Size(398, 265);
		fileSelection.TabIndex = 4;
		fileSelection.UseCompatibleStateImageBehavior = false;
		fileSelection.View = View.Details;
		fileSelection.MouseDown += fileSelection_MouseDown;
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

	private void ApplyTheme()
	{
		var theme = Settings.Instance.Theme;

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

	/// <inheritdoc />
	protected override void Dispose(bool disposing)
	{
		if (disposing)
			SaveTabs();

		base.Dispose(disposing);
	}

	/// <inheritdoc />
	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
		LoadTabs();
	}

	/// <inheritdoc />
	protected override void OnEnter(EventArgs e)
	{
		base.OnEnter(e);
		SetActivePanel(true);
	}

	/// <inheritdoc />
	protected override void OnLeave(EventArgs e)
	{
		base.OnLeave(e);
		if (_isEditPathMode)
			LeavePathEditMode();
		SetActivePanel(false);
	}

	private void TabStripCloseClicked(object? sender, TabStripCloseClickedEventArgs e)
	{
		tabStrip.RemoveTabAt(e.Index);
	}

	private void tabStrip_SelectedIndexChanged(object? sender, TabStripIndexChangedEventArgs e)
	{
		if (tabStrip.SelectedTab?.Value is string path)
		{
			if (!NavigateTo(path))
			{
				// 경로가 이상하다. 기본 경로로 돌아간다
				// 이것도 잘못되면.. 모르겠다
				// TODO: 경로가 잘못되면 탭을 닫아야 하는데, 안했다.
				NavigateTo(Settings.Instance.StartDirectory);
			}
		}
	}

	private void tabStrip_Clicked(object? sender, TabStripClickedEventArgs e)
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

	private void pathTextBox_LostFocus(object? sender, EventArgs e)
	{
		LeavePathEditMode();
	}

	private void editDirButton_Click(object? sender, EventArgs e)
	{
		if (_isEditPathMode)
			LeavePathEditMode();
		else
			EnterPathEditMode();
	}

	private void refreshButton_Click(object? sender, EventArgs e) =>
		NavigateTo(breadcrumbPath.Path);

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

	private void breadcrumbPath_BreadcrumbPathClicked(object? sender, BreadcrumbPathClickedEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
			NavigateTo(e.Path);
	}

	private void fileInfoLabel_Click(object? sender, EventArgs e)
	{

	}

	private void fileSelection_MouseDown(object? sender, MouseEventArgs e)
	{
		SetActivePanel(true);
	}

	public bool NavigateTo(string directory)
	{
		if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
			return false;

		var settings = Settings.Instance;
		var showHidden = settings.ShowHiddenFiles;
		var info = new DirectoryInfo(directory);

		_currentDirectory = directory;
		breadcrumbPath.Path = directory;

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

		fileSelection.BeginUpdate();
		fileSelection.ClearItems();

		DriveInfo? drive = null;
		try
		{
			// 루트 디렉토리가 아니면 ".." 항목을 추가
			if (info.Parent is { Exists: true })
				fileSelection.AddDirectoryItem(info.Parent);

			// 디렉토리 정보 갱신
			foreach (var d in info.GetDirectories())
			{
				if (!showHidden && (d.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
					continue;

				fileSelection.AddDirectoryItem(d);
				dirCount++;
			}

			// 파일 정보 갱신
			foreach (var f in info.GetFiles())
			{
				if (!showHidden && (f.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
					continue;

				fileSelection.AddFileItem(f);
				fileCount++;
				totalSize += f.Length;
			}

			// 드라이브 정보 갱신
			foreach (var v in DriveInfo.GetDrives())
			{
				if (!v.IsReady)
					continue;

				fileSelection.AddDriveItem(v);

				if (drive == null && v.Name.Equals(info.Root.Name, StringComparison.OrdinalIgnoreCase))
					drive = v;
			}
		}
		catch
		{
			// 아니 왜...?
		}

		fileSelection.EndUpdate();

		// 디렉토리 정보
		drvDirLabel.SetDirectoryInfo(dirCount, fileCount, totalSize);

		// 드라이브 정보
		drive ??= DriveInfo.GetDrives().FirstOrDefault(d => d.Name.Equals(info.Root.Name, StringComparison.OrdinalIgnoreCase));
		drvDirLabel.SetDriveInfo(drive);

		return true;
	}

	public void SetActivePanel(bool isActive)
	{
		// TODO: 먼저 이벤트를 보내고 그담에 상태를 바꿔야 할까?
		if (_isActivePanel)
			return;

		_isActivePanel = isActive;
		workPanel.BorderColor = isActive ? Settings.Instance.Theme.BackHover : Settings.Instance.Theme.Border;
		PanelActivated?.Invoke(this, new FilePanelActiveEventArgs(this, isActive));
	}

	// 경로 편집 모드 들어감
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

	// 결로 편집 모드 나감
	private void LeavePathEditMode()
	{
		_isEditPathMode = false;
		pathTextBox.Visible = false;
		breadcrumbPath.Visible = true;
		editDirButton.BackColor = Settings.Instance.Theme.Background;
	}

	// 새탭 추가
	public void AddTab(string? directory, bool force = false)
	{
		if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
			directory = Directory.Exists(_currentDirectory) ? _currentDirectory : Settings.Instance.StartDirectory;

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

	// 다음 탭으로 이동
	public void NextTab()
	{
		if (tabStrip.SelectedIndex < tabStrip.Count - 1)
			tabStrip.SelectedIndex++;
		else
			tabStrip.SelectedIndex = 0;
	}

	// 이전 탭으로 이동
	public void PreviousTab()
	{
		if (tabStrip.SelectedIndex > 0)
			tabStrip.SelectedIndex--;
		else
			tabStrip.SelectedIndex = tabStrip.Count - 1;
	}

	// 탭 목록 읽기. 만들 탭이 없으면 만든다.
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
			var directories = tabs.SplitWithSeparator('|');
			if (directories.Length > 0)
			{
				foreach (var directory in directories)
				{
					var d = new DirectoryInfo(directory);
					if (!d.Exists)
						continue; // 유효하지 않은 경로는 무시
					tabStrip.AddTab(d.Name, directory);
				}

				if (tabStrip.Count > 0)
				{
					if (activeIndex < tabStrip.Count)
						tabStrip.SelectedIndex = activeIndex;
				}
			}
		}
	}

	// 탭 목록 저장. Dispose에서 호출된다.
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
}

//
public class FilePanelActiveEventArgs(FilePanel panel, bool isActive) : EventArgs
{
	public FilePanel Panel { get; } = panel;
	public bool IsActive { get; } = isActive;
}
