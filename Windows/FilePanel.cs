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
	private Panel workPanel;
	private Panel dirPanel;
	private BreadcrumbPath breadcrumbPath;
	private Label drvInfoLabel;
	private Label dirInfoLabel;
	private Button historyButton;
	private Button refreshButton;
	private Button editDirButton;
	private Panel infoPanel;
#nullable restore

	private string _currentDirectory = string.Empty;
	private bool _isActivePanel = false;
	private List<string> _history = [];

	public event EventHandler? PanelActivated;

	public FilePanel()
	{
		InitializeComponent();
		ApplyTheme();

		breadcrumbPath.Path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
	}

	private void InitializeComponent()
	{
		tabStrip = new TabStrip();
		fileInfoLabel = new Label();
		workPanel = new Panel();
		dirPanel = new Panel();
		drvInfoLabel = new Label();
		dirInfoLabel = new Label();
		breadcrumbPath = new BreadcrumbPath();
		editDirButton = new Button();
		refreshButton = new Button();
		historyButton = new Button();
		infoPanel = new Panel();
		workPanel.SuspendLayout();
		dirPanel.SuspendLayout();
		infoPanel.SuspendLayout();
		SuspendLayout();
		// 
		// tabStrip
		// 
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
		workPanel.Controls.Add(infoPanel);
		workPanel.Controls.Add(dirPanel);
		workPanel.Controls.Add(fileInfoLabel);
		workPanel.Dock = DockStyle.Fill;
		workPanel.Location = new Point(0, 23);
		workPanel.Name = "workPanel";
		workPanel.Size = new Size(400, 327);
		workPanel.TabIndex = 2;
		// 
		// dirPanel
		// 
		dirPanel.BackColor = Color.FromArgb(63, 63, 70);
		dirPanel.Controls.Add(historyButton);
		dirPanel.Controls.Add(refreshButton);
		dirPanel.Controls.Add(editDirButton);
		dirPanel.Controls.Add(breadcrumbPath);
		dirPanel.Dock = DockStyle.Top;
		dirPanel.Location = new Point(0, 0);
		dirPanel.Name = "dirPanel";
		dirPanel.Size = new Size(398, 20);
		dirPanel.TabIndex = 2;
		// 
		// drvInfoLabel
		// 
		drvInfoLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		drvInfoLabel.Location = new Point(199, 0);
		drvInfoLabel.Name = "drvInfoLabel";
		drvInfoLabel.Size = new Size(196, 20);
		drvInfoLabel.TabIndex = 2;
		drvInfoLabel.Text = "label2";
		drvInfoLabel.TextAlign = ContentAlignment.MiddleRight;
		// 
		// dirInfoLabel
		// 
		dirInfoLabel.Location = new Point(3, 0);
		dirInfoLabel.Name = "dirInfoLabel";
		dirInfoLabel.Size = new Size(177, 20);
		dirInfoLabel.TabIndex = 1;
		dirInfoLabel.Text = "label1";
		dirInfoLabel.TextAlign = ContentAlignment.MiddleLeft;
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
		// 
		// editDirButton
		// 
		editDirButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		editDirButton.FlatAppearance.BorderSize = 0;
		editDirButton.FlatStyle = FlatStyle.Flat;
		editDirButton.Image = Properties.Resources.pen16;
		editDirButton.Location = new Point(333, 0);
		editDirButton.Margin = new Padding(0);
		editDirButton.Name = "editDirButton";
		editDirButton.Size = new Size(20, 20);
		editDirButton.TabIndex = 1;
		editDirButton.UseVisualStyleBackColor = true;
		// 
		// refreshButton
		// 
		refreshButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		refreshButton.FlatAppearance.BorderSize = 0;
		refreshButton.FlatStyle = FlatStyle.Flat;
		refreshButton.Image = Properties.Resources.refresh16;
		refreshButton.Location = new Point(354, 0);
		refreshButton.Margin = new Padding(0);
		refreshButton.Name = "refreshButton";
		refreshButton.Size = new Size(20, 20);
		refreshButton.TabIndex = 2;
		refreshButton.UseVisualStyleBackColor = true;
		// 
		// historyButton
		// 
		historyButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		historyButton.FlatAppearance.BorderSize = 0;
		historyButton.FlatStyle = FlatStyle.Flat;
		historyButton.Image = Properties.Resources.history16;
		historyButton.Location = new Point(375, 0);
		historyButton.Margin = new Padding(0);
		historyButton.Name = "historyButton";
		historyButton.Size = new Size(20, 20);
		historyButton.TabIndex = 3;
		historyButton.UseVisualStyleBackColor = true;
		// 
		// infoPanel
		// 
		infoPanel.BackColor = Color.FromArgb(20, 20, 20);
		infoPanel.Controls.Add(dirInfoLabel);
		infoPanel.Controls.Add(drvInfoLabel);
		infoPanel.Dock = DockStyle.Top;
		infoPanel.Location = new Point(0, 20);
		infoPanel.Name = "infoPanel";
		infoPanel.Size = new Size(398, 20);
		infoPanel.TabIndex = 3;
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
		dirPanel.ResumeLayout(false);
		infoPanel.ResumeLayout(false);
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

		infoPanel.BackColor = theme.Content;
		infoPanel.ForeColor = theme.Foreground;

		editDirButton.ForeColor = theme.Foreground;
		editDirButton.BackColor = theme.Background;
		editDirButton.FlatAppearance.MouseOverBackColor = theme.Hover;
		editDirButton.FlatAppearance.MouseDownBackColor = theme.Accent;

		refreshButton.ForeColor = theme.Foreground;
		refreshButton.BackColor = theme.Background;
		refreshButton.FlatAppearance.MouseOverBackColor = theme.Hover;
		refreshButton.FlatAppearance.MouseDownBackColor = theme.Accent;

		historyButton.ForeColor = theme.Foreground;
		historyButton.BackColor = theme.Background;
		historyButton.FlatAppearance.MouseOverBackColor = theme.Hover;
		historyButton.FlatAppearance.MouseDownBackColor = theme.Accent;
	}

	public List<string> GetHistory() => _history;

	public void NavigateTo(string path)
	{

	}
}
