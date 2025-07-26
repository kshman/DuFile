// ReSharper disable MissingXmlDoc

namespace DuFile.Windows;

/// <summary>
/// Represents the main form of the application, providing the primary user interface and menu structure.
/// </summary>
/// <remarks>The <see cref="MainForm"/> class initializes and manages the main menu system of the application,
/// offering various commands and shortcuts for file operations, directory navigation, editing, viewing options, tools,
/// and help. The menu structure is defined using a hierarchical array of <see cref="MenuDef"/> objects, which specify
/// the text, command, shortcut, and submenus for each menu item.</remarks>
public partial class MainForm : Form
{
	public MainForm()
	{
		InitializeComponent();
		IntiializeMenu();
		ApplyTheme();
		ApplySettings();

		leftPanel.MainForm = this;
		rightPanel.MainForm = this;
	}

	private void ApplyTheme()
	{
		var theme = Settings.Instance.Theme;
		BackColor = theme.Background;
		ForeColor = theme.Foreground;
	}

	private void ApplySettings()
	{
		var settings = Settings.Instance;

		if (settings.WindowMaximized)
			WindowState = FormWindowState.Maximized;
		else
		{
			var loc = settings.WindowLocation;
			if (loc.X < 0 || loc.Y < 0)
				StartPosition = FormStartPosition.CenterScreen;
			else
			{
				StartPosition = FormStartPosition.Manual;
				Location = settings.WindowLocation;
				Size = settings.WindowSize;
			}
		}
	}

	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
		UpdateLayout();
	}

	protected override void OnFormClosing(FormClosingEventArgs e)
	{
		base.OnFormClosing(e);

		var settings = Settings.Instance;

		if (settings.ConfirmOnExit)
		{
			var result = MessageBox.Show("정말로 종료하시겠습니까?", "종료 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (result == DialogResult.No)
			{
				e.Cancel = true;
				return;
			}
		}

		if (WindowState == FormWindowState.Maximized)
			settings.WindowMaximized = true;
		else
		{
			settings.WindowMaximized = false;
			settings.WindowLocation = Location;
			settings.WindowSize = Size;
		}
	}

	protected override void OnResize(EventArgs e)
	{
		base.OnResize(e);
		UpdateLayout();
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);
		funcBar.SetModifier(e.Modifiers);

		// ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
		switch (e.KeyCode)
		{
			case Keys.F1:
				ExecuteCommand(Commands.Help);
				break;

			case Keys.F2:
			case Keys.F3:
			case Keys.F4:
			case Keys.F5:
			case Keys.F6:
			case Keys.F7:
			case Keys.F8:
			case Keys.F9:
				ExecuteCommand(funcBar.GetCommand(e.KeyCode));
				break;
		}
	}

	protected override void OnKeyUp(KeyEventArgs e)
	{
		base.OnKeyUp(e);
		funcBar.SetModifier(e.Modifiers);
	}

	private void UpdateLayout()
	{
		var menuHeight = menuStrip.Height;
		var topOffset = menuHeight + (toolStrip.Visible ? toolStrip.Height : 0);
		var bottomOffset = funcBar.Visible ? funcBar.Height : 0;
		var availableHeight = ClientSize.Height - topOffset - bottomOffset;
		var availableWidth = ClientSize.Width;
		var verticalBarWidth = verticalBar.Visible ? verticalBar.StaticWidth : 0;
		var panelWidth = (availableWidth - verticalBarWidth) / 2;

		leftPanel.Location = new Point(0, topOffset);
		leftPanel.Size = new Size(panelWidth, availableHeight);

		verticalBar.Location = new Point(panelWidth, topOffset);
		verticalBar.Size = new Size(verticalBarWidth, availableHeight);

		rightPanel.Location = new Point(panelWidth + verticalBarWidth, topOffset);
		rightPanel.Size = new Size(panelWidth, availableHeight);
	}

	private void rightPanel_Load(object sender, EventArgs e)
	{

	}

	private void leftPanel_Load(object sender, EventArgs e)
	{

	}

	private void FuncBarButtonClick(object sender, FuncBarButtonClickEventArgs e)
	{
		ExecuteCommand(e.Command);
	}
}
