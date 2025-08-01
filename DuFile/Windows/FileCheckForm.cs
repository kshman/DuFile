// ReSharper disable MissingXmlDoc
namespace DuFile.Windows;

internal sealed class FileCheckForm : Form
{
#nullable disable
	private Label promptLabel;
	private Label fileNameLabel;
	private Panel sourcePanel;
	private Panel destPanel;
	private PictureBox sourceIconBox;
	private PictureBox destIconBox;
	private Label sourceInfoLabel;
	private Label destInfoLabel;
	private Button newerButton;
	private Button skipButton;
	private Button alwaysButton;
	private Button renameButton;
	private Button cancelButton;
	private CheckBox allCheckBox;
	private Label nameLabel;
#nullable restore

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public string SourceFile { get; set; } = string.Empty;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public string DestinationFile { get; set; } = string.Empty;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool OverwriteAlways
	{
		get => alwaysButton.Enabled;
		set => alwaysButton.Enabled = value;
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool OverwriteNewer
	{
		get => newerButton.Enabled;
		set => newerButton.Enabled = value;
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool ApplyToAll => allCheckBox.Checked;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public string? NewFileName { get; set; }

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public OverwriteBy OverwriteResult { get; set; } = OverwriteBy.None;

	public FileCheckForm(string title, string prompt, string name)
	{
		InitializeComponent();
		Text = title;
		promptLabel.Text = prompt;
		nameLabel.Text = name;
	}

	private void InitializeComponent()
	{
		promptLabel = new Label();
		fileNameLabel = new Label();
		sourcePanel = new Panel();
		sourceIconBox = new PictureBox();
		sourceInfoLabel = new Label();
		destPanel = new Panel();
		destIconBox = new PictureBox();
		destInfoLabel = new Label();
		newerButton = new Button();
		skipButton = new Button();
		alwaysButton = new Button();
		renameButton = new Button();
		cancelButton = new Button();
		allCheckBox = new CheckBox();
		nameLabel = new Label();
		sourcePanel.SuspendLayout();
		((ISupportInitialize)sourceIconBox).BeginInit();
		destPanel.SuspendLayout();
		((ISupportInitialize)destIconBox).BeginInit();
		SuspendLayout();
		// 
		// promptLabel
		// 
		promptLabel.AutoSize = true;
		promptLabel.Location = new Point(20, 20);
		promptLabel.Name = "promptLabel";
		promptLabel.Size = new Size(55, 15);
		promptLabel.TabIndex = 0;
		promptLabel.Text = "프롬프트";
		// 
		// fileNameLabel
		// 
		fileNameLabel.AutoSize = true;
		fileNameLabel.Location = new Point(20, 50);
		fileNameLabel.Name = "fileNameLabel";
		fileNameLabel.Size = new Size(0, 15);
		fileNameLabel.TabIndex = 1;
		// 
		// sourcePanel
		// 
		sourcePanel.BorderStyle = BorderStyle.FixedSingle;
		sourcePanel.Controls.Add(sourceIconBox);
		sourcePanel.Controls.Add(sourceInfoLabel);
		sourcePanel.Location = new Point(20, 80);
		sourcePanel.Name = "sourcePanel";
		sourcePanel.Size = new Size(400, 70);
		sourcePanel.TabIndex = 2;
		// 
		// sourceIconBox
		// 
		sourceIconBox.Location = new Point(10, 10);
		sourceIconBox.Name = "sourceIconBox";
		sourceIconBox.Size = new Size(48, 48);
		sourceIconBox.SizeMode = PictureBoxSizeMode.CenterImage;
		sourceIconBox.TabIndex = 0;
		sourceIconBox.TabStop = false;
		// 
		// sourceInfoLabel
		// 
		sourceInfoLabel.Location = new Point(70, 10);
		sourceInfoLabel.Name = "sourceInfoLabel";
		sourceInfoLabel.Size = new Size(320, 48);
		sourceInfoLabel.TabIndex = 0;
		// 
		// destPanel
		// 
		destPanel.BorderStyle = BorderStyle.FixedSingle;
		destPanel.Controls.Add(destIconBox);
		destPanel.Controls.Add(destInfoLabel);
		destPanel.Location = new Point(20, 160);
		destPanel.Name = "destPanel";
		destPanel.Size = new Size(400, 70);
		destPanel.TabIndex = 3;
		// 
		// destIconBox
		// 
		destIconBox.Location = new Point(10, 10);
		destIconBox.Name = "destIconBox";
		destIconBox.Size = new Size(48, 48);
		destIconBox.SizeMode = PictureBoxSizeMode.CenterImage;
		destIconBox.TabIndex = 0;
		destIconBox.TabStop = false;
		// 
		// destInfoLabel
		// 
		destInfoLabel.Location = new Point(70, 10);
		destInfoLabel.Name = "destInfoLabel";
		destInfoLabel.Size = new Size(320, 48);
		destInfoLabel.TabIndex = 0;
		// 
		// newerButton
		// 
		newerButton.Location = new Point(440, 153);
		newerButton.Name = "newerButton";
		newerButton.Size = new Size(120, 30);
		newerButton.TabIndex = 7;
		newerButton.Text = "새파일 덮어쓰기(&E)";
		newerButton.Visible = false;
		newerButton.Click += NewerButton_Click;
		// 
		// skipButton
		// 
		skipButton.Location = new Point(440, 117);
		skipButton.Name = "skipButton";
		skipButton.Size = new Size(120, 30);
		skipButton.TabIndex = 6;
		skipButton.Text = "건너뛰기(&S)";
		skipButton.Click += SkipButton_Click;
		// 
		// alwaysButton
		// 
		alwaysButton.Location = new Point(440, 80);
		alwaysButton.Name = "alwaysButton";
		alwaysButton.Size = new Size(120, 30);
		alwaysButton.TabIndex = 5;
		alwaysButton.Text = "덮어쓰기(&W)";
		alwaysButton.Click += AlwayButton_Click;
		// 
		// renameButton
		// 
		renameButton.Location = new Point(440, 189);
		renameButton.Name = "renameButton";
		renameButton.Size = new Size(120, 30);
		renameButton.TabIndex = 8;
		renameButton.Text = "이름바꾸기(&R)";
		renameButton.Click += RenameButton_Click;
		// 
		// cancelButton
		// 
		cancelButton.Location = new Point(440, 240);
		cancelButton.Name = "cancelButton";
		cancelButton.Size = new Size(120, 30);
		cancelButton.TabIndex = 9;
		cancelButton.Text = "취소(C)";
		cancelButton.Click += CancelButton_Click;
		// 
		// allCheckBox
		// 
		allCheckBox.Location = new Point(20, 240);
		allCheckBox.Name = "allCheckBox";
		allCheckBox.Size = new Size(200, 20);
		allCheckBox.TabIndex = 4;
		allCheckBox.Text = "이후 모두 동일하게 처리(&A)";
		// 
		// nameLabel
		// 
		nameLabel.AutoSize = true;
		nameLabel.ForeColor = Color.FromArgb(0, 122, 204);
		nameLabel.Location = new Point(20, 50);
		nameLabel.Name = "nameLabel";
		nameLabel.Size = new Size(31, 15);
		nameLabel.TabIndex = 10;
		nameLabel.Text = "이름";
		// 
		// FileCheckForm
		// 
		ClientSize = new Size(584, 280);
		Controls.Add(nameLabel);
		Controls.Add(promptLabel);
		Controls.Add(fileNameLabel);
		Controls.Add(sourcePanel);
		Controls.Add(destPanel);
		Controls.Add(allCheckBox);
		Controls.Add(newerButton);
		Controls.Add(skipButton);
		Controls.Add(alwaysButton);
		Controls.Add(renameButton);
		Controls.Add(cancelButton);
		FormBorderStyle = FormBorderStyle.FixedDialog;
		MaximizeBox = false;
		MinimizeBox = false;
		Name = "FileCheckForm";
		ShowIcon = false;
		ShowInTaskbar = false;
		StartPosition = FormStartPosition.CenterParent;
		sourcePanel.ResumeLayout(false);
		((ISupportInitialize)sourceIconBox).EndInit();
		destPanel.ResumeLayout(false);
		((ISupportInitialize)destIconBox).EndInit();
		ResumeLayout(false);
		PerformLayout();
	}

	protected override void OnShown(EventArgs e)
	{
		base.OnShown(e);

		UpdateSourceInfo();
		UpdateDestInfo();
	}

	private void UpdateSourceInfo()
	{
		if (File.Exists(SourceFile))
		{
			var fi = new FileInfo(SourceFile);
			fileNameLabel.Text = fi.Name;
			sourceIconBox.Image = IconCache.Instance.GetLargeIcon(SourceFile);
			sourceInfoLabel.Text = $"{fi.DirectoryName}\n{fi.Length:N0} 바이트 ({fi.LastWriteTime})";
		}
		else if (Directory.Exists(SourceFile))
		{
			var di = new DirectoryInfo(SourceFile);
			fileNameLabel.Text = di.Name;
			sourceIconBox.Image = IconCache.Instance.GetLargeIcon(SourceFile, true);
			sourceInfoLabel.Text = $"{di.FullName}\n({di.LastWriteTime})";
		}
		else
		{
			fileNameLabel.Text = SourceFile;
			sourceIconBox.Image = null;
			sourceInfoLabel.Text = string.Empty;
		}
	}

	private void UpdateDestInfo()
	{
		if (File.Exists(DestinationFile))
		{
			var fi = new FileInfo(DestinationFile);
			destPanel.Visible = true;
			destIconBox.Image = IconCache.Instance.GetLargeIcon(DestinationFile);
			destInfoLabel.Text = $"{fi.DirectoryName}\n{fi.Length:N0} 바이트 ({fi.LastWriteTime})";
		}
		else if (Directory.Exists(DestinationFile))
		{
			var di = new DirectoryInfo(DestinationFile);
			destPanel.Visible = true;
			destIconBox.Image = IconCache.Instance.GetLargeIcon(DestinationFile, true);
			destInfoLabel.Text = $"{di.FullName}\n({di.LastWriteTime})";
		}
		else
		{
			destPanel.Visible = false;
			destIconBox.Image = null;
			destInfoLabel.Text = string.Empty;
		}
	}

	private void AlwayButton_Click(object? sender, EventArgs e)
	{
		OverwriteResult = OverwriteBy.Always;
		DialogResult = DialogResult.OK;
		Close();
	}

	private void SkipButton_Click(object? sender, EventArgs e)
	{
		OverwriteResult = OverwriteBy.Skip;
		DialogResult = DialogResult.OK;
		Close();
	}

	private void NewerButton_Click(object? sender, EventArgs e)
	{
		OverwriteResult = OverwriteBy.Newer;
		DialogResult = DialogResult.OK;
		Close();
	}

	private void CancelButton_Click(object? sender, EventArgs e)
	{
		DialogResult = DialogResult.Cancel;
		Close();
	}

	private void RenameButton_Click(object? sender, EventArgs e)
	{
		var fi = new FileInfo(SourceFile);
		using var dlg = new LineInputForm("이름 바꾸기", "새 파일 이름을 입력하세요.", fi.Name);
		if (dlg.RunDialog() == DialogResult.OK)
		{
			NewFileName = dlg.InputText;
			OverwriteResult = OverwriteBy.Rename;
			DialogResult = DialogResult.OK;
			Close();
		}
	}

	public OverwriteBy RunDialog() =>
		ShowDialog() == DialogResult.OK ? OverwriteResult : OverwriteBy.None;
}
