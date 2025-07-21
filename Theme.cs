namespace DuFile;

internal struct Theme
{
	public Color Foreground = Color.FromArgb(241, 241, 241);
	public Color Background = Color.FromArgb(37, 37, 37);
	public Color BackHover = Color.FromArgb(0, 122, 204);
	public Color BackActive = Color.FromArgb(0, 150, 136);
	public Color BackSelection = Color.FromArgb(63, 63, 70);
	public Color BackContent = Color.FromArgb(20, 20, 20);
	public Color Border = Color.FromArgb(63, 63, 70);
	public Color Accelerator = Color.FromArgb(255, 128, 64);

	public Theme()
	{
		// Initialize any additional properties or settings if needed
	}

	public readonly void Apply(Control.ControlCollection controls, Font? font)
	{
		font ??= new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
		ApplyChild(controls, font);
	}

	private readonly void ApplyChild(Control.ControlCollection controls, Font font)
	{
		foreach (Control control in controls)
		{
			if (control is MenuStrip or ToolStrip)
			{
				control.BackColor = Background;
				control.ForeColor = Foreground;
				control.Font = font;
			}
			else if (control is Button button)
			{
				button.FlatStyle = FlatStyle.Flat;
				button.FlatAppearance.BorderColor = Border;
				button.FlatAppearance.MouseOverBackColor = BackActive;
			}
			else if (control is TextBox textBox)
			{
				textBox.BackColor = Background;
				textBox.ForeColor = Foreground;
				textBox.BorderStyle = BorderStyle.FixedSingle;
			}
			else if (control is Label label)
			{
				label.BackColor = Color.Transparent; // Labels typically don't need a background
			}
			else
			{
				control.BackColor = Background;
				control.ForeColor = Foreground;
			}

			if (control.HasChildren)
				ApplyChild(control.Controls, font);
		}
	}
}
