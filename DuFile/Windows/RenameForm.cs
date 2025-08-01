namespace DuFile.Windows;

/// <summary>
/// 여러 파일의 이름을 순차적으로 변경할 수 있는 폼입니다.
/// </summary>
internal class RenameForm : Form
{
#nullable disable
    private Label promptLabel;
    private Label currentLabel;
    private Label orderLabel;
    private TextBox nameTextBox;
    private TextBox extTextBox;
    private Label dotLabel;
    private Button okButton;
    private Button cancelButton;
#nullable restore

    private readonly List<string> _files;
    private int _currentIndex;

    /// <summary>
    /// 파일 이름 변경 작업을 수행하는 폼을 생성합니다.
    /// </summary>
    /// <param name="files">이름을 변경할 파일의 전체 경로 목록입니다.</param>
    public RenameForm(IEnumerable<string> files)
    {
        InitializeComponent();

        _files = files.ToList();
        UpdateFileInfo();
	}

    /// <summary>
	/// 디자이너용 생성자입니다.
	/// </summary>
	public RenameForm()
    {
	    InitializeComponent();

	    _files = [];
    }

	/// <summary>
	/// 폼의 컨트롤을 초기화합니다.
	/// </summary>
	private void InitializeComponent()
	{
		promptLabel = new Label();
		currentLabel = new Label();
		orderLabel = new Label();
		nameTextBox = new TextBox();
		extTextBox = new TextBox();
		dotLabel = new Label();
		okButton = new Button();
		cancelButton = new Button();
		SuspendLayout();
		// 
		// promptLabel
		// 
		promptLabel.AutoSize = true;
		promptLabel.Location = new Point(12, 9);
		promptLabel.Name = "promptLabel";
		promptLabel.Size = new Size(294, 15);
		promptLabel.TabIndex = 0;
		promptLabel.Text = "새로운 파일 이름을 입력하세요. (/\\:*?\"<>| 사용불가)";
		// 
		// currentLabel
		// 
		currentLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		currentLabel.ForeColor = Color.FromArgb(0, 122, 204);
		currentLabel.Location = new Point(12, 44);
		currentLabel.Name = "currentLabel";
		currentLabel.Size = new Size(433, 25);
		currentLabel.TabIndex = 1;
		currentLabel.Text = "현재 파일 이름";
		currentLabel.TextAlign = ContentAlignment.MiddleLeft;
		// 
		// orderLabel
		// 
		orderLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		orderLabel.ForeColor = SystemColors.ControlText;
		orderLabel.Location = new Point(372, 44);
		orderLabel.Name = "orderLabel";
		orderLabel.Size = new Size(200, 25);
		orderLabel.TabIndex = 2;
		orderLabel.Text = "(0 / 0)";
		orderLabel.TextAlign = ContentAlignment.MiddleRight;
		// 
		// nameTextBox
		// 
		nameTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		nameTextBox.Location = new Point(12, 71);
		nameTextBox.Name = "nameTextBox";
		nameTextBox.Size = new Size(453, 23);
		nameTextBox.TabIndex = 0;
		// 
		// extTextBox
		// 
		extTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		extTextBox.Location = new Point(492, 72);
		extTextBox.Name = "extTextBox";
		extTextBox.Size = new Size(80, 23);
		extTextBox.TabIndex = 1;
		// 
		// dotLabel
		// 
		dotLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		dotLabel.Location = new Point(471, 71);
		dotLabel.Name = "dotLabel";
		dotLabel.Size = new Size(15, 23);
		dotLabel.TabIndex = 3;
		dotLabel.Text = ".";
		dotLabel.TextAlign = ContentAlignment.MiddleCenter;
		// 
		// okButton
		// 
		okButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
		okButton.Location = new Point(366, 119);
		okButton.Name = "okButton";
		okButton.Size = new Size(100, 30);
		okButton.TabIndex = 2;
		okButton.Text = "확인";
		okButton.Click += OkButton_Click;
		// 
		// cancelButton
		// 
		cancelButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
		cancelButton.DialogResult = DialogResult.Cancel;
		cancelButton.Location = new Point(472, 119);
		cancelButton.Name = "cancelButton";
		cancelButton.Size = new Size(100, 30);
		cancelButton.TabIndex = 3;
		cancelButton.Text = "취소";
		cancelButton.Click += cancelButton_Click;
		// 
		// RenameForm
		// 
		AcceptButton = okButton;
		CancelButton = cancelButton;
		ClientSize = new Size(584, 161);
		Controls.Add(promptLabel);
		Controls.Add(currentLabel);
		Controls.Add(orderLabel);
		Controls.Add(nameTextBox);
		Controls.Add(dotLabel);
		Controls.Add(extTextBox);
		Controls.Add(okButton);
		Controls.Add(cancelButton);
		FormBorderStyle = FormBorderStyle.FixedDialog;
		MaximizeBox = false;
		MinimizeBox = false;
		Name = "RenameForm";
		ShowIcon = false;
		ShowInTaskbar = false;
		StartPosition = FormStartPosition.CenterParent;
		Text = "이름 바꾸기";
		ResumeLayout(false);
		PerformLayout();
	}

	// 취소 버튼 클릭 시 폼을 닫습니다.
	private void cancelButton_Click(object? sender, EventArgs e) =>
        Close();

    // 현재 파일 정보 및 입력값을 갱신합니다.
    private void UpdateFileInfo()
    {
        if (_currentIndex >= _files.Count)
            return;
        var filePath = _files[_currentIndex];
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var ext = Path.GetExtension(filePath).TrimStart('.');
        currentLabel.Text = fileName + (string.IsNullOrEmpty(ext) ? "" : $".{ext}");
        orderLabel.Text = $"({_currentIndex + 1} / {_files.Count})";
        nameTextBox.Text = fileName;
        extTextBox.Text = ext;
        nameTextBox.Focus();
        nameTextBox.SelectAll();
    }

    // 확인 버튼 클릭 시 파일 이름 변경 시도
    private void OkButton_Click(object? sender, EventArgs e)
    {
        while (true)
        {
            var name = nameTextBox.Text.Trim();
            var ext = extTextBox.Text.Trim();
            if (string.IsNullOrEmpty(name) || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
                (!string.IsNullOrEmpty(ext) && ext.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0))
            {
                MessageBox.Show("파일 이름 또는 확장자에 사용할 수 없는 문자가 포함되어 있습니다.", "이름 바꾸기", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                nameTextBox.Focus();
                return;
            }

            var oldPath = _files[_currentIndex];
            var dir = Path.GetDirectoryName(oldPath);
            var newFileName = string.IsNullOrEmpty(ext) ? name : $"{name}.{ext}";
            var newPath = Path.Combine(dir ?? string.Empty, newFileName);

            try
            {
				if (File.Exists(oldPath)) 
					File.Move(oldPath, newPath);
				else if (Directory.Exists(oldPath))
					Directory.Move(oldPath, newPath);
			}
            catch
            {
                var result = MessageBox.Show($"파일 이름 변경에 실패했습니다.", "이름 바꾸기", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                if (result == DialogResult.Retry)
                {
                    // 재입력
                    nameTextBox.Focus();
                    return;
                }
            }
            // 성공 시 다음 파일로 진행
            break;
        }
        _currentIndex++;
        if (_currentIndex < _files.Count)
            UpdateFileInfo();
        else
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }

    /// <summary>
    /// 폼을 모달로 실행합니다.
    /// </summary>
    /// <returns>다이얼로그 결과입니다.</returns>
    public DialogResult RunDialog(Form parent) => ShowDialog(parent);
}
