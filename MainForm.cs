
namespace DuFile;

public partial class MainForm : Form
{
	public MainForm()
	{
		InitializeComponent();
		ApplyTheme();
		ApplySettings();
	}

	private void ApplyTheme()
	{
		var theme = Settings.Instance.Theme;
		BackColor = theme.Background;
		ForeColor = theme.Foreground;
		theme.Apply(Controls, null);
	}

	private void ApplySettings()
	{
		var settings = Settings.Instance;

		if (settings.WindowMaximized)
			WindowState = FormWindowState.Maximized;
		else
		{
			Size = settings.WindowSize;
			Location = settings.WindowLocation;
		}
	}

	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
		UpdateLayout();
		// ≈« ∫π±∏
	}

	protected override void OnFormClosing(FormClosingEventArgs e)
	{
		// ≈« ¿˙¿Â
		base.OnFormClosing(e);
	}

	protected override void OnResize(EventArgs e)
	{
		base.OnResize(e);
		UpdateLayout();
	}

	private void MainForm_KeyDown(object sender, KeyEventArgs e)
	{

	}

	private void UpdateLayout()
	{
		var menuHeight = menuStrip.Height;
		var topOffset = menuHeight + (toolStrip.Visible ? toolStrip.Height : 0);
		var bottomOffset = funcBar.Visible ? funcBar.Height : 0;
		var availableHeight = ClientSize.Height - topOffset - bottomOffset;
		var availableWidth = ClientSize.Width;
		//var verticalBarWidth = verticalBar.Visible ? verticalBar.Width : 0;
		var verticalBarWidth = verticalBar.Visible ? 20 : 0;
		var panelWidth = (availableWidth - verticalBarWidth) / 2;

		leftPanel.Location = new Point(0, topOffset);
		leftPanel.Size = new Size(panelWidth, availableHeight);

		verticalBar.Location = new Point(panelWidth, topOffset);
		verticalBar.Size = new Size(verticalBarWidth, availableHeight);

		rightPanel.Location = new Point(panelWidth + verticalBarWidth, topOffset);
		rightPanel.Size = new Size(panelWidth, availableHeight);
	}
}
