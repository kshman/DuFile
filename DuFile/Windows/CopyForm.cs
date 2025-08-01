// ReSharper disable MissingXmlDoc
namespace DuFile.Windows;

internal sealed class CopyForm : Form
{
#nullable disable
	private Label nameLabel; // 현재 파일명
	private Label indexLabel; // 전체 파일 수
	private Label sizeLabel; // 누적/전체 크기
	private ProgressBar progressBar; // 진행도
	private Button cancelButton; // 취소 버튼
#nullable restore

	private readonly List<string> _files; // 복사/이동할 파일 목록
	private readonly string _dest;

	private readonly CancellationTokenSource _cts = new();
	private string _mode = string.Empty;
	private long _totalCount;
	private long _totalSize;
	private long _passCount;
	private long _passSize;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool MoveMode { get; set; }

	public CopyForm(string title, IEnumerable<string> files, string dest)
	{
		InitializeComponent();
		Text = title;

		_files = files.ToList();
		_dest = dest;
	}

	private void InitializeComponent()
	{
		nameLabel = new Label();
		indexLabel = new Label();
		progressBar = new ProgressBar();
		sizeLabel = new Label();
		cancelButton = new Button();
		SuspendLayout();
		// 
		// nameLabel
		// 
		nameLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		nameLabel.Location = new Point(12, 20);
		nameLabel.Name = "nameLabel";
		nameLabel.Size = new Size(560, 20);
		nameLabel.TabIndex = 0;
		nameLabel.Text = "파일 이름";
		// 
		// indexLabel
		// 
		indexLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		indexLabel.Location = new Point(12, 80);
		indexLabel.Name = "indexLabel";
		indexLabel.Size = new Size(244, 20);
		indexLabel.TabIndex = 2;
		indexLabel.Text = "전체: 0";
		// 
		// progressBar
		// 
		progressBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		progressBar.Location = new Point(12, 54);
		progressBar.Maximum = 1000;
		progressBar.Name = "progressBar";
		progressBar.Size = new Size(560, 23);
		progressBar.TabIndex = 3;
		// 
		// sizeLabel
		// 
		sizeLabel.Location = new Point(275, 80);
		sizeLabel.Name = "sizeLabel";
		sizeLabel.Size = new Size(297, 20);
		sizeLabel.TabIndex = 4;
		sizeLabel.Text = "0 / 0";
		sizeLabel.TextAlign = ContentAlignment.TopRight;
		// 
		// cancelButton
		// 
		cancelButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
		cancelButton.DialogResult = DialogResult.Cancel;
		cancelButton.Location = new Point(482, 105);
		cancelButton.Name = "cancelButton";
		cancelButton.Size = new Size(90, 30);
		cancelButton.TabIndex = 5;
		cancelButton.Text = "취소";
		cancelButton.Click += CancelButton_Click;
		// 
		// CopyForm
		// 
		CancelButton = cancelButton;
		ClientSize = new Size(584, 147);
		Controls.Add(nameLabel);
		Controls.Add(indexLabel);
		Controls.Add(progressBar);
		Controls.Add(sizeLabel);
		Controls.Add(cancelButton);
		FormBorderStyle = FormBorderStyle.FixedDialog;
		MaximizeBox = false;
		MinimizeBox = false;
		Name = "CopyForm";
		ShowIcon = false;
		ShowInTaskbar = false;
		StartPosition = FormStartPosition.CenterParent;
		ResumeLayout(false);
	}

	protected override void OnShown(EventArgs e)
	{
		base.OnShown(e);
		_mode = MoveMode ? "이동" : "복사";
		Task.Run(() => WorkAsync(_cts.Token));
	}

	private async Task InvokeUpdateInfo(string name)
	{
		await InvokeAsync(() =>
		{
			nameLabel.Text = name;
			indexLabel.Text = $"{_passCount} / {_totalCount}";
			sizeLabel.Text = $"{_passSize:N0} / {_totalSize:N0}";
			progressBar.Value = _totalSize > 0 ? (int)(_passSize * 1000 / _totalSize) : 0;
			progressBar.Refresh();
		});
	}

	private async Task<DialogResult> InvokeError(string mesg, params string[] args)
	{
		var dlg = new MesgBoxForm("오류", mesg, args);
		dlg.SetButtonText("무시", "취소", "재시도");
		dlg.DisplayIcon = MessageBoxIcon.Error;
		var result = DialogResult.None;
		await InvokeAsync(() => result = dlg.RunDialog());
		return result;
	}

	private async Task WorkFilesAsync(string dest,
		FileInfo[] files, DirectoryInfo[] dirs, CancellationToken token)
	{
		if (token.IsCancellationRequested)
			return;

		_totalCount += files.Length + dirs.Length;
		_totalSize += files.Sum(f => f.Length);

		// 파일 먼저
		foreach (var file in files)
		{
			if (token.IsCancellationRequested)
				return;
			var destFile = Path.Combine(dest, file.Name);

			_passCount++;
			_passSize += file.Length;
			await InvokeUpdateInfo(file.Name);

			if (Path.Exists(destFile))
			{
				var check = new FileCheckForm(
					"같은 파일이 이미 있어요",
					"같은 이름의 파일이 이미 있습니다. 어떻게 할까요?",
					Path.GetFileName(destFile));
				check.SourceFile = file.FullName;
				check.DestinationFile = destFile;
				var res = check.RunDialog();
				switch (res)
				{
					case OverwriteBy.None:
						await _cts.CancelAsync();
						return;
					case OverwriteBy.Skip:
						continue;
					case OverwriteBy.Always:
						break;
					case OverwriteBy.Newer:
					{
						var diff = file.LastWriteTime - File.GetLastWriteTime(destFile);
						if (diff < TimeSpan.Zero)
							continue; // 대상이 더 최신이면 건너뛰기
						break;
					}
					case OverwriteBy.Rename:
						// 이름 바꾸기, 바꾼 이름도 있으면 걍 덮어쓴다
						if (!string.IsNullOrEmpty(check.NewFileName))
							destFile = check.NewFileName;
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				// 한번 더 검사하고 있으면 휴지통으로
				if (Path.Exists(destFile))
					Alter.MoveToRecycleBin(destFile);
			}

			var done = false;
			while (!done)
			{
				try
				{
					if (MoveMode)
						File.Move(file.FullName, destFile, overwrite: true);
					else
					{
						await using var srcStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
						await using var destStream = new FileStream(destFile, FileMode.Create, FileAccess.Write, FileShare.None);
						await srcStream.CopyToAsync(destStream, 81920, token);

						File.SetAttributes(destFile, file.Attributes);
						File.SetCreationTime(destFile, file.CreationTime);
						File.SetLastWriteTime(destFile, file.LastWriteTime);
						File.SetLastAccessTime(destFile, file.LastAccessTime);
					}
				}
				catch
				{
					var result = await InvokeError($"파일 {_mode} 중 오류가 났어요", file.Name);
					switch (result)
					{
						case DialogResult.Cancel:
							await _cts.CancelAsync();
							return;
						case DialogResult.Retry:
							continue;
					}
					break;
				}

				done = true;
			}
		}

		// 디렉토리
		foreach (var dir in dirs)
		{
			if (token.IsCancellationRequested)
				return;
			var destDir = Path.Combine(dest, dir.Name);

			_passCount++;
			await InvokeUpdateInfo(dir.Name);

			if (Path.Exists(destDir))
			{
				var check = new FileCheckForm(
					"같은 폴더가 이미 있어요",
					"같은 이름의 폴더가 이미 있습니다. 어떻게 할까요?",
					Path.GetFileName(destDir));
				check.SourceFile = dir.FullName;
				check.DestinationFile = destDir;
				var res = check.RunDialog();
				switch (res)
				{
					case OverwriteBy.None:
						await _cts.CancelAsync();
						return;
					case OverwriteBy.Skip:
						continue;
					case OverwriteBy.Always:
						break;
					case OverwriteBy.Newer:
					{
						var diff = dir.LastWriteTime - File.GetLastWriteTime(destDir);
						if (diff < TimeSpan.Zero)
							continue; // 대상이 더 최신이면 건너뛰기
						break;
					}
					case OverwriteBy.Rename:
						// 이름 바꾸기, 바꾼 이름도 있으면 걍 덮어쓴다
						if (!string.IsNullOrEmpty(check.NewFileName))
							destDir = check.NewFileName;
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				// 한번 더 검사하고 있으면 휴지통으로
				if (Path.Exists(destDir))
					Alter.MoveToRecycleBin(destDir);
			}

			var done = false;
			while (!done)
			{
				try
				{
					Directory.CreateDirectory(destDir);
				}
				catch
				{
					var result = await InvokeError($"디렉토리 {_mode} 중 오류가 났어요", dir.Name);
					switch (result)
					{
						case DialogResult.Cancel:
							await _cts.CancelAsync();
							return;
						case DialogResult.Retry:
							continue;
					}
					break;
				}

				await WorkFilesAsync(destDir, dir.GetFiles(), dir.GetDirectories(), token);

				if (MoveMode)
				{
					try
					{
						Directory.Delete(dir.FullName, true);
					}
					catch
					{
						var result = await InvokeError($"디렉토리 {_mode} 후 삭제 중 오류가 났어요", dir.Name);
						switch (result)
						{
							case DialogResult.Cancel:
								await _cts.CancelAsync();
								return;
							case DialogResult.Retry:
								continue;
						}
					}
				}

				done = true;
			}
		}
	}

	private async Task WorkAsync(CancellationToken token)
	{
		List<FileInfo> files = [];
		List<DirectoryInfo> dirs = [];
		foreach (var file in _files)
		{
			if (token.IsCancellationRequested)
				return;

			if (Directory.Exists(file))
				dirs.Add(new DirectoryInfo(file));
			else if (File.Exists(file))
				files.Add(new FileInfo(file));
		}

		await WorkFilesAsync(_dest, files.ToArray(), dirs.ToArray(), token);
		await InvokeAsync(Close, CancellationToken.None);
	}

	private void CancelButton_Click(object? sender, EventArgs e)
	{
		cancelButton.Enabled = false;
		_cts.Cancel();
	}

	public void RunDialog() => ShowDialog();
}
