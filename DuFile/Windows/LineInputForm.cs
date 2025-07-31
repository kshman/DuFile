// ReSharper disable MissingXmlDoc
namespace DuFile.Windows;

public sealed class LineInputForm : Form
{
#nullable disable
	private Label promptLabel;
	private TextBox inputTextBox;
	private Button okButton;
	private Button cancelButton;
#nullable restore

	public string InputText => inputTextBox.Text;

	public LineInputForm()
	{
		InitializeComponent();
	}

	public LineInputForm(string title, string prompt, string defaultValue = "")
		: this()
	{
		Text = title;
		promptLabel.Text = prompt;
		inputTextBox.Text = defaultValue;
	}

	private void InitializeComponent()
	{
		promptLabel = new Label();
		inputTextBox = new TextBox();
		okButton = new Button();
		cancelButton = new Button();
		SuspendLayout();
		// 
		// promptLabel
		// 
		promptLabel.AutoSize = true;
		promptLabel.Location = new Point(12, 9);
		promptLabel.Name = "promptLabel";
		promptLabel.Size = new Size(63, 15);
		promptLabel.TabIndex = 0;
		promptLabel.Text = "한 줄 입력";
		// 
		// inputTextBox
		// 
		inputTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		inputTextBox.Location = new Point(15, 35);
		inputTextBox.Name = "inputTextBox";
		inputTextBox.Size = new Size(407, 23);
		inputTextBox.TabIndex = 1;
		// 
		// okButton
		// 
		okButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
		okButton.DialogResult = DialogResult.OK;
		okButton.Location = new Point(266, 74);
		okButton.Name = "okButton";
		okButton.Size = new Size(75, 25);
		okButton.TabIndex = 2;
		okButton.Text = "확인";
		okButton.UseVisualStyleBackColor = true;
		// 
		// cancelButton
		// 
		cancelButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
		cancelButton.DialogResult = DialogResult.Cancel;
		cancelButton.Location = new Point(347, 74);
		cancelButton.Name = "cancelButton";
		cancelButton.Size = new Size(75, 25);
		cancelButton.TabIndex = 3;
		cancelButton.Text = "취소";
		cancelButton.UseVisualStyleBackColor = true;
		// 
		// LineInputForm
		// 
		AcceptButton = okButton;
		CancelButton = cancelButton;
		ClientSize = new Size(434, 111);
		Controls.Add(promptLabel);
		Controls.Add(inputTextBox);
		Controls.Add(okButton);
		Controls.Add(cancelButton);
		FormBorderStyle = FormBorderStyle.FixedDialog;
		MaximizeBox = false;
		MinimizeBox = false;
		Name = "LineInputForm";
		ShowIcon = false;
		ShowInTaskbar = false;
		StartPosition = FormStartPosition.CenterParent;
		ResumeLayout(false);
		PerformLayout();
	}

	// 다이얼로그 띄우기
	public DialogResult RunDialog() => ShowDialog();
}
