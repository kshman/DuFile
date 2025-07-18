namespace DuFile.Cuscon;

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
#nullable restore

	private string _currentDirectory = string.Empty;
	private bool _isActivePanel = false;
	private List<string> _history = [];

	public event EventHandler? PanelActivated;

	public FilePanel()
	{
		InitializeComponent();
	}

	private void InitializeComponent()
	{
		var settings = Settings.Instance;
		var theme = settings.Theme;

		tabStrip = new TabStrip();
		fileInfoLabel = new Label();
		workPanel = new Panel();
		workPanel.SuspendLayout();
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
		fileInfoLabel.Dock = DockStyle.Bottom;
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
		workPanel.Controls.Add(fileInfoLabel);
		workPanel.Dock = DockStyle.Fill;
		workPanel.Location = new Point(0, 23);
		workPanel.Name = "workPanel";
		workPanel.Size = new Size(400, 327);
		workPanel.TabIndex = 2;
		// 
		// FilePanel
		// 
		Controls.Add(workPanel);
		Controls.Add(tabStrip);
		Name = "FilePanel";
		Size = new Size(400, 350);
		BackColor = theme.Background;
		ForeColor = theme.Foreground;
		workPanel.ResumeLayout(false);
		ResumeLayout(false);

	}

	public List<string> GetHistory() => _history;

	public void NavigateTo(string path)
	{

	}
}
