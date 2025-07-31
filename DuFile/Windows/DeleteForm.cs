using System.Runtime.InteropServices;

namespace DuFile.Windows;

/// <summary>
/// 파일을 실제로 삭제하거나 휴지통으로 이동하는 폼입니다.
/// </summary>
public sealed class DeleteForm : Form
{
#nullable disable
	private Label promptLabel; // 현재 삭제 중인 파일 표시
	private Button cancelButton; // 취소 버튼
#nullable restore

	private readonly IEnumerable<string> _files; // 삭제할 파일 목록
	private readonly CancellationTokenSource _cts = new(); // 삭제 취소용

	/// <summary>
	/// 휴지통으로 이동할지 여부 (true: 휴지통, false: 실제 삭제)
	/// </summary>
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool TrashMode { get; set; }

	/// <summary>
	/// 테스트 모드 (true면 실제 삭제/이동하지 않음)
	/// </summary>
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool TestMode { get; set; }

	/// <summary>
	/// 삭제가 취소되었는지 여부
	/// </summary>
	public bool IsCanceled { get; private set; }

	/// <summary>
	/// 삭제 폼 생성자
	/// </summary>
	/// <param name="title">폼 타이틀</param>
	/// <param name="files">삭제할 파일 목록</param>
	public DeleteForm(string title, IEnumerable<string> files)
	{
		InitializeComponent();

		Text = title;
		_files = files;
	}

	private void InitializeComponent()
	{
		promptLabel = new Label();
		cancelButton = new Button();
		SuspendLayout();
		// 
		// promptLabel
		// 
		promptLabel.AutoSize = true;
		promptLabel.Location = new Point(12, 20);
		promptLabel.Name = "promptLabel";
		promptLabel.Size = new Size(56, 15);
		promptLabel.TabIndex = 0;
		promptLabel.Text = "삭제 중...";
		// 
		// cancelButton
		// 
		cancelButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
		cancelButton.DialogResult = DialogResult.Cancel;
		cancelButton.Location = new Point(318, 58);
		cancelButton.Name = "cancelButton";
		cancelButton.Size = new Size(90, 30);
		cancelButton.TabIndex = 1;
		cancelButton.Text = "취소";
		cancelButton.Click += CancelButton_Click;
		// 
		// DeleteForm
		// 
		CancelButton = cancelButton;
		ClientSize = new Size(420, 100);
		Controls.Add(promptLabel);
		Controls.Add(cancelButton);
		FormBorderStyle = FormBorderStyle.FixedDialog;
		MaximizeBox = false;
		MinimizeBox = false;
		Name = "DeleteForm";
		ShowIcon = false;
		ShowInTaskbar = false;
		StartPosition = FormStartPosition.CenterParent;
		ResumeLayout(false);
		PerformLayout();
	}

	/// <inheritdocs />
	protected override void OnShown(EventArgs e)
	{
		base.OnShown(e);
		// 삭제 작업 시작
		Task.Run(() => DeleteFilesAsync(_cts.Token));
	}

	private static bool QueryFailAction(string mesg, string filename)
	{
		var result = MessageBox.Show(
			$"{mesg} 실패: {filename}\n계속 진행하시겠습니까?", mesg,
			MessageBoxButtons.YesNo,
			MessageBoxIcon.Error);
		return result == DialogResult.Yes;
	}

	private async Task DeleteFilesAsync(CancellationToken token)
	{
		if (TrashMode)
		{
			await DeleteToRecycleBinAsync(_files, token);
		}
		else
		{
			await DeleteFilesAndDirectoriesAsync(_files, token);
		}

		await InvokeAsync(Close, token);
	}

	private async Task DeleteToRecycleBinAsync(IEnumerable<string> items, CancellationToken token)
	{
		foreach (var item in items)
		{
			if (token.IsCancellationRequested)
				break;

			Invoke(() => promptLabel.Text = $"삭제 중: {item}");

			if (!TestMode)
			{
				var success = MoveToRecycleBin(item);
				if (!success)
				{
					var shouldContinue = QueryFailAction("휴지통으로 이동", item);
					if (!shouldContinue)
						break;
				}
			}

			await Task.Delay(20, token);
		}
	}

	private async Task DeleteFilesAndDirectoriesAsync(IEnumerable<string> items, CancellationToken token)
	{
		var filesToDelete = new List<string>();
		var dirsToDelete = new List<string>();

		foreach (var item in items)
		{
			if (token.IsCancellationRequested)
				break;

			if (File.Exists(item))
			{
				filesToDelete.Add(item);
			}
			else if (Directory.Exists(item))
			{
				await CollectDirectoryItemsAsync(item, filesToDelete, dirsToDelete, token);
				dirsToDelete.Add(item);
			}
		}

		// 파일 삭제
		foreach (var file in filesToDelete)
		{
			if (token.IsCancellationRequested)
				break;

			Invoke(() => promptLabel.Text = $"파일 삭제: {file}");

			if (!TestMode)
			{
				try
				{
					File.Delete(file);
				}
				catch
				{
					var shouldContinue = QueryFailAction("파일 삭제", file);
					if (!shouldContinue)
						break;
				}
			}

			await Task.Delay(20, token);
		}

		// 디렉터리 삭제 (하위부터)
		foreach (var dir in dirsToDelete.AsEnumerable().Reverse())
		{
			if (token.IsCancellationRequested)
				break;

			Invoke(() => promptLabel.Text = $"폴더 삭제: {dir}");

			if (!TestMode)
			{
				try
				{
					Directory.Delete(dir, false);
				}
				catch
				{
					var shouldContinue = QueryFailAction("폴더 삭제", dir);
					if (!shouldContinue)
						break;
				}
			}

			await Task.Delay(20, token);
		}
	}

	private async Task CollectDirectoryItemsAsync(string dir, List<string> files, List<string> dirs, CancellationToken token)
	{
		if (token.IsCancellationRequested)
			return;

		Invoke(() => promptLabel.Text = $"폴더 확인중: {dir}");

		try
		{
			foreach (var file in Directory.GetFiles(dir))
			{
				if (token.IsCancellationRequested)
					return;
				files.Add(file);
			}
			foreach (var subdir in Directory.GetDirectories(dir))
			{
				if (token.IsCancellationRequested)
					return;
				await CollectDirectoryItemsAsync(subdir, files, dirs, token);
				dirs.Add(subdir);
			}
		}
		catch
		{
			// 수집 실패 시 무시
		}

		await Task.Delay(10, token); // 수집 중에도 취소 체크 및 UI 갱신
	}

	private void CancelButton_Click(object? sender, EventArgs e)
	{
		IsCanceled = true;
		cancelButton.Enabled = false;
		_cts.Cancel();
	}

	// 휴지통으로 이동 (SHFileOperation 사용)
	private static bool MoveToRecycleBin(string path)
	{
		var fs = new SHFILEOPSTRUCT
		{
			wFunc = FO_DELETE,
			pFrom = path + "\0\0",
			fFlags = FOF_ALLOWUNDO | FOF_NOCONFIRMATION | FOF_SILENT,
		};
		return SHFileOperation(ref fs) == 0;
	}

	private const int FO_DELETE = 3;
	private const int FOF_ALLOWUNDO = 0x40;
	private const int FOF_NOCONFIRMATION = 0x10;
	private const int FOF_SILENT = 0x4;

	// ReSharper disable once InconsistentNaming
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct SHFILEOPSTRUCT
	{
		public IntPtr hwnd;
		public int wFunc;
		public string pFrom;
		public string pTo;
		public int fFlags;
		public bool fAnyOperationsAborted;
		public IntPtr hNameMappings;
		public string lpszProgressTitle;
	}

#pragma warning disable SYSLIB1054
	[DllImport("shell32.dll", CharSet = CharSet.Unicode)]
	private static extern int SHFileOperation(ref SHFILEOPSTRUCT fileOp);
#pragma warning restore SYSLIB1054

	/// <summary>
	/// 다이얼로그를 실행합니다.
	/// </summary>
	public void RunDialog() => ShowDialog();
}
