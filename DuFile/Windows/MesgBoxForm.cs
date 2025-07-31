// ReSharper disable MissingXmlDoc
namespace DuFile.Windows;

public sealed class MesgBoxForm : Form
{
#nullable disable
	private PictureBox iconBox;
	private Label promptLabel;
	private ListBox listItem;
	private Button okButton;
	private Button cancelButton;
	private Button utilButton;
#nullable restore

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public string OkText
	{
		get => okButton.Text;
		set => okButton.Text = value;
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public string CancelText
	{
		get => cancelButton.Text;
		set => cancelButton.Text = value;
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public string UtilText
	{
		get => utilButton.Text;
		set => utilButton.Text = value;
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public MessageBoxIcon DisplayIcon { get; set; } = MessageBoxIcon.None;

	public MesgBoxForm()
	{
		InitializeComponent();
	}

	public MesgBoxForm(string title, string prompt, IEnumerable<string>? items)
		: this()
	{
		Text = title;
		promptLabel.Text = prompt;
		if (items != null)
			AddItems(items);
	}

	private void InitializeComponent()
	{
		promptLabel = new Label();
		okButton = new Button();
		cancelButton = new Button();
		listItem = new ListBox();
		utilButton = new Button();
		iconBox = new PictureBox();
		((ISupportInitialize)iconBox).BeginInit();
		SuspendLayout();
		// 
		// promptLabel
		// 
		promptLabel.AutoSize = true;
		promptLabel.Location = new Point(68, 28);
		promptLabel.Name = "promptLabel";
		promptLabel.Size = new Size(63, 15);
		promptLabel.TabIndex = 0;
		promptLabel.Text = "한 줄 입력";
		// 
		// okButton
		// 
		okButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
		okButton.DialogResult = DialogResult.OK;
		okButton.Location = new Point(216, 298);
		okButton.Name = "okButton";
		okButton.Size = new Size(100, 30);
		okButton.TabIndex = 2;
		okButton.Text = "확인";
		okButton.UseVisualStyleBackColor = true;
		// 
		// cancelButton
		// 
		cancelButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
		cancelButton.DialogResult = DialogResult.Cancel;
		cancelButton.Location = new Point(322, 298);
		cancelButton.Name = "cancelButton";
		cancelButton.Size = new Size(100, 30);
		cancelButton.TabIndex = 3;
		cancelButton.Text = "취소";
		cancelButton.UseVisualStyleBackColor = true;
		// 
		// listItem
		// 
		listItem.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		listItem.FormattingEnabled = true;
		listItem.Location = new Point(12, 65);
		listItem.Name = "listItem";
		listItem.Size = new Size(410, 229);
		listItem.TabIndex = 4;
		// 
		// utilButton
		// 
		utilButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
		utilButton.DialogResult = DialogResult.Yes;
		utilButton.Location = new Point(12, 298);
		utilButton.Name = "utilButton";
		utilButton.Size = new Size(100, 30);
		utilButton.TabIndex = 5;
		utilButton.UseVisualStyleBackColor = true;
		// 
		// iconBox
		// 
		iconBox.Location = new Point(12, 12);
		iconBox.Name = "iconBox";
		iconBox.Size = new Size(50, 50);
		iconBox.TabIndex = 6;
		iconBox.TabStop = false;
		// 
		// MesgBoxForm
		// 
		AcceptButton = okButton;
		CancelButton = cancelButton;
		ClientSize = new Size(434, 340);
		Controls.Add(iconBox);
		Controls.Add(utilButton);
		Controls.Add(listItem);
		Controls.Add(promptLabel);
		Controls.Add(okButton);
		Controls.Add(cancelButton);
		FormBorderStyle = FormBorderStyle.FixedDialog;
		MaximizeBox = false;
		MinimizeBox = false;
		Name = "MesgBoxForm";
		ShowIcon = false;
		ShowInTaskbar = false;
		StartPosition = FormStartPosition.CenterParent;
		Text = "알려 드립니다";
		((ISupportInitialize)iconBox).EndInit();
		ResumeLayout(false);
		PerformLayout();
	}

	// 아이콘을 시스템 아이콘으로
	private Icon? ConvertIcon() => DisplayIcon switch
	{
		MessageBoxIcon.Error => SystemIcons.Error,
		MessageBoxIcon.Question => SystemIcons.Question,
		MessageBoxIcon.Warning => SystemIcons.Warning,
		MessageBoxIcon.Information => SystemIcons.Information,
		_ => null
	};

	// 리스트에 아이템 추가
	public void AddItem(string item)
	{
		if (!string.IsNullOrEmpty(item))
			listItem.Items.Add(item);
	}

	// 리스트에 다수 아이템 추가
	public void AddItems(IEnumerable<string> items)
	{
		foreach (var item in items)
		{
			if (!string.IsNullOrEmpty(item))
				listItem.Items.Add(item);
		}
	}

	public DialogResult RunDialog()
	{
		iconBox.Image = ConvertIcon()?.ToBitmap();

		if (okButton.Text.Length == 0)
		{
			okButton.Text = "확인";
		}

		if (cancelButton.Text.Length == 0)
		{
			cancelButton.Visible = false;
			cancelButton.Enabled = false;
		}

		if (utilButton.Text.Length == 0)
		{
			utilButton.Visible = false;
			utilButton.Enabled = false;
		}

		return ShowDialog();
	}
}
