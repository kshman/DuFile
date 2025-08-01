// ReSharper disable MissingXmlDoc
namespace DuFile.Windows;

public partial class MainForm
{
	private readonly Dictionary<string, ToolStripMenuItem> _menuItems = [];
	private Dictionary<string, Action> _menuActions = [];

	private long _countExecute;
	private long _countShellContextMenu;
	private long _countSheelProperties;

	// 메뉴 정의
	private static readonly MenuDef[] MainMenus =
	[
		new()
		{
			Text = "파일(&F)",
			SubMenus =
			[
				new MenuDef { Text = "열기(&O)", Command = Commands.Open },
				new MenuDef { Text = "인수와 함께 열기(&W)", Command = Commands.OpenWith, Shortcut = "Ctrl+Enter" },
				new MenuDef(),
				new MenuDef { Text = "휴지통으로(&T)", Command = Commands.Trash, Shortcut = "Delete" },
				new MenuDef { Text = "바로 지우기(&D)", Command = Commands.Delete, Shortcut = "Shift+Delete" },
				new MenuDef(),
				new MenuDef { Text = "이름 바꾸기(&R)", Command = Commands.Rename },
				new MenuDef(),
				new MenuDef { Text = "속성(&A)", Command = Commands.Properties, Shortcut = "Alt+Enter" },
				new MenuDef(),
				new MenuDef { Text = "끝내기(&X)", Command = Commands.Exit, Shortcut = "Alt+X" },
			]
		},
		new()
		{
			Text = "폴더(&R)",
			SubMenus =
			[
				new MenuDef { Text = "즐겨찾기(&F)", Command = Commands.None, Shortcut = "F11" },
				new MenuDef(),
				new MenuDef { Text = "새 폴더 만들기(&N)", Command = Commands.NewFolder },
				new MenuDef(),
				new MenuDef { Text = "상위 폴더로(&X)", Command = Commands.NavParentFolder },
				new MenuDef { Text = "최상위 폴더로(&Z)", Command = Commands.NavRootFolder },
			]
		},
		new()
		{
			Text = "편집(&E)",
			SubMenus = [
				new MenuDef { Text = "복사(&C)", Command = Commands.ClipboardCopy, Shortcut = "Ctrl+C" },
				new MenuDef { Text = "붙여넣기(&V)", Command = Commands.ClipboardPaste, Shortcut = "Ctrl+V" },
				new MenuDef { Text = "잘라내기(&X)", Command = Commands.ClipboardCut, Shortcut = "Ctrl+X" },
				new MenuDef(),
				new MenuDef { Text = "전체 경로 이름 복사(&L)", Command = Commands.None, Shortcut = "Ctrl+Alt+F" },
				new MenuDef { Text = "파일 이름만 복사(&A)", Command = Commands.None, Shortcut = "Ctrl+Alt+A" },
				new MenuDef { Text = "경로 이름만 복사(&R)", Command = Commands.None, Shortcut = "Ctrl+Alt+R" },
				new MenuDef(),
				new MenuDef { Text = "선택/해제(&B)", Command = Commands.None },
				new MenuDef { Text = "전체 선택(&U)", Command = Commands.None, Shortcut = "Alt+U" },
				new MenuDef { Text = "선택 해제(&N)", Command = Commands.None, Shortcut = "Ctrl+Alt+U" },
				new MenuDef { Text = "선택 반전(&I)", Command = Commands.None, Shortcut = "Ctrl+Alt+I" },
				new MenuDef { Text = "고급 선택(&G)", Command = Commands.None, Shortcut = "Alt+D" },
				new MenuDef { Text = "고급 해제(&W)", Command = Commands.None, Shortcut = "Ctrl+Alt+D" },
				new MenuDef(),
				new MenuDef { Text = "같은 확장자 모두 선택(&E)", Command = Commands.None, Shortcut = "Ctrl+Alt+E" },
				new MenuDef { Text = "같은 이름 모두 선택(&N)", Command = Commands.None, Shortcut = "Ctrl+Alt+N" },
			]
		},
		new()
		{
			Text = "보기(&V)",
			SubMenus =
			[
				new MenuDef
				{
					Text = "정렬(&A)",
					SubMenus =
					[
						new MenuDef { Text = "이름으로(&N)", Command = Commands.SortByName, Shortcut = "Ctrl+Shift+1" },
						new MenuDef { Text = "확장자로(&E)", Command = Commands.SortByExtension, Shortcut = "Ctrl+Shift+2" },
						new MenuDef { Text = "크기로(&S)", Command = Commands.SortBySize, Shortcut = "Ctrl+Shift+3" },
						new MenuDef { Text = "날짜/시간으로(&T)", Command = Commands.SortByDateTime, Shortcut = "Ctrl+Shift+4" },
						new MenuDef { Text = "속성으로(&R)", Command = Commands.SortByAttribute, Shortcut = "Ctrl+Shift+5" },
						new MenuDef(),
						new MenuDef { Text = "내림차순(&D)", Command = Commands.SortDesc, Shortcut = "Ctrl+Shift+D" },
					]
				},
				new MenuDef(),
				new MenuDef { Text = "숨김 파일 보기(&Z)", Command = Commands.ShowHidden, Shortcut = "Alt+Z" },
				new MenuDef(),
				new MenuDef { Text = "새 탭(&T)", Command = Commands.NewTab, Shortcut = "Ctrl+T" },
				new MenuDef { Text = "탭 목록(&Y)", Command = Commands.TabList },
			]
		},
		new()
		{
			Text = "도구(&T)",
		},
		new()
		{
			Text = "도움말(&H)",
			SubMenus =
			[
				new MenuDef { Text = "버전 정보", Command = Commands.None, Shortcut = null }
			]
		}
	];

	// 메뉴 초기화
	private void IntiializeMenu()
	{
		// 메뉴 액션 등록 (먼저 등록 안하면 아이템 등록할 때 못찾는다
		_menuActions = new Dictionary<string, Action>
		{
			{ Commands.None, MenuNone },
			// 파일
			{ Commands.Exit, MenuExit },
			{ Commands.Open, MenuOpen },
			{ Commands.OpenWith, MenuOpenWith },
			{ Commands.Trash, MenuTrash },
			{ Commands.Delete, MenuDelete },
			{ Commands.Rename, MenuRename },
			{ Commands.Properties, MenuProperties },
			// 폴더 
			{ Commands.NewFolder, MenuNewFolder },
			{ Commands.NavParentFolder, MenuNavParentFolder },
			{ Commands.NavRootFolder, MenuNavRootFolder },
			// 편집
			{ Commands.ClipboardCopy, MenuClipboardCopy },
			{ Commands.ClipboardCut, MenuClipboardCut },
			// 보기
			{ Commands.SortByName, MenuSortByName },
			{ Commands.SortByExtension, MenuSortByExtension },
			{ Commands.SortBySize, MenuSortBySize },
			{ Commands.SortByDateTime, MenuSortByDateTime },
			{ Commands.SortByAttribute, MenuSortByAttribute },
			{ Commands.SortDesc, MenuSortDesc },
			{ Commands.ShowHidden, MenuShowHidden },
			{ Commands.NewTab, MenuNewTab },
			{ Commands.TabList, MenuTabList },
			{ Commands.NextTab, MenuNextTab },
			{ Commands.PreviousTab, MenuPreviousTab },
			{ Commands.SwitchPanel, MenuSwitchPanel },
			// 펑션바
			{ Commands.Copy, MenuCopy },
			{ Commands.Move, MenuMove },
			// 계속 추가합시다.
		};

		// 메뉴 아이템 등록
		var keyConverter = new KeysConverter();
		AddMenuItems(keyConverter, menuStrip.Items, MainMenus);

		CheckMenuItem();
	}

	// 메뉴에 항목 넣기
	private void AddMenuItems(KeysConverter keyConverter, ToolStripItemCollection items, MenuDef[] menus)
	{
		foreach (var menu in menus)
		{
			if (string.IsNullOrEmpty(menu.Text))
			{
				items.Add(new ToolStripSeparator());
				continue;
			}

			var menuItem = new ToolStripMenuItem(menu.Text);
			if (menu.Shortcut != null)
				menuItem.ShortcutKeys = Alter.ParseKeyString(keyConverter, menu.Shortcut);
			if (menu.Disable)
				menuItem.Enabled = false;

			if (!string.IsNullOrEmpty(menu.Command))
			{
				_menuItems[menu.Command] = menuItem;

				if (_menuActions.TryGetValue(menu.Command, out var action))
					menuItem.Click += (_, _) => action.Invoke();
			}

			if (menu.SubMenus is { Length: > 0 })
				AddMenuItems(keyConverter, menuItem.DropDownItems, menu.SubMenus);
			items.Add(menuItem);
		}
	}

	// 메뉴 체크
	private void SetCheckMenuItem(string command, bool check)
	{
		if (_menuItems.TryGetValue(command, out var item))
			item.Checked = check;
	}

	// 체크 아이템 설정
	private void CheckMenuItem()
	{
		var settings = Settings.Instance;
		SetCheckMenuItem(Commands.ShowHidden, settings.ShowHidden);
		SetCheckMenuItem(Commands.SortDesc, settings.SortDescending);
		SetCheckMenuItem(Commands.SortByName, settings.SortOrder == 0);
		SetCheckMenuItem(Commands.SortByExtension, settings.SortOrder == 1);
		SetCheckMenuItem(Commands.SortBySize, settings.SortOrder == 2);
		SetCheckMenuItem(Commands.SortByDateTime, settings.SortOrder == 3);
		SetCheckMenuItem(Commands.SortByAttribute, settings.SortOrder == 4);
	}

	// 명령 호출
	private void ExecuteCommand(string command)
	{
		if (_menuActions.TryGetValue(command, out var action))
			action();
	}

	// 파일 실행
	public void ExcuteProcess(string fileName, string? arguments = null)
	{
		if (string.IsNullOrEmpty(fileName))
			return;

		var info = new FileInfo(fileName);
		if (!info.Exists)
			return;

		try
		{
			var (shell, args, directory) = arguments is null
				? (true, string.Empty, string.Empty)
				: (false, arguments, info.DirectoryName ?? string.Empty);
			var process = new System.Diagnostics.Process
			{
				StartInfo = new System.Diagnostics.ProcessStartInfo
				{
					FileName = info.FullName,
					Arguments = args,
					UseShellExecute = shell,
					ErrorDialog = true,
					WorkingDirectory = directory,
				}
			};
			process.Start();
			_countExecute++;
		}
		catch (Exception ex)
		{
			MessageBox.Show($"파일({info.Name})을 열 수 없어요!", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
			Debugs.WriteLine($"파일 열기 오류: {ex.Message}");
		}
	}

	// 쉘 메뉴 열기
	public void ExcuteShowContextMenu(IWin32Window owner, Point screenPos, IList<string> files)
	{
		try
		{
			ShellContext.ShowMenu(owner, screenPos, files);
			_countShellContextMenu++;
		}
		catch (Exception ex)
		{
			MessageBox.Show("컨텍스트 메뉴를 열 수 없어요!", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
			Debugs.WriteLine($"컨텍스트 메뉴 열기 오류: {ex.Message}");
		}
	}

	// 쉘 속성 열기
	public void ExcuteShowProperties(IWin32Window owner, string file)
	{
		try
		{
			ShellContext.ShowProperties(owner, file);
			_countSheelProperties++;
		}
		catch (Exception ex)
		{
			MessageBox.Show($"파일({file}) 속성을 열 수 없어요!", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
			Debugs.WriteLine($"속성 열기 오류: {ex.Message}");
		}
	}

	// 패널 전환
	public void SwitchPanel()
	{
		var index = _activePanel.PanelIndex;
		switch (index)
		{
			case 1:
				rightPanel.Focus();
				_activePanel = rightPanel;
				break;
			case 2:
				leftPanel.Focus();
				_activePanel = leftPanel;
				break;
		}
	}

	// 없구먼
	private void MenuNone()
	{
		MessageBox.Show("이 기능은 아직 구현되지 않았어요!", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
		Debugs.WriteLine($"실행 횟수: {_countExecute}");
		Debugs.WriteLine($"쉘 컨텍스트 메뉴 호출 횟수: {_countShellContextMenu}");
		Debugs.WriteLine($"쉘 속성 호출 횟수: {_countSheelProperties}");
	}

	// 끝내기
	private void MenuExit()
	{
		Close();
	}

	// 열기
	private void MenuOpen()
	{
		var name = _activePanel.GetFocusedItem();
		if (string.IsNullOrEmpty(name))
			return;
		ExcuteProcess(name);
	}

	// 인수와 함께 열기
	private void MenuOpenWith()
	{
		var name = _activePanel.GetFocusedItem();
		if (string.IsNullOrEmpty(name))
			return;

		using var dlg = new LineInputForm("인수와 함께 실행", "실행에 필요한 인수를 입력해주세요.");
		if (dlg.RunDialog() != DialogResult.OK)
			return;

		ExcuteProcess(name, dlg.InputText);
	}

	private void MenuTrash()
	{
		var files = _activePanel.GetSelectedItems();
		if (files.Count == 0)
			return;

		using var mesg = new MesgBoxForm("휴지통으로", $"선택한 {files.Count}개의 항목을 휴지통으로 보낼까요?", files);
		mesg.UtilText = "삭제(&D)";
		mesg.DisplayIcon = MessageBoxIcon.Question;

		var ret = mesg.RunDialog();
		if (ret == DialogResult.Cancel)
			return;

		_activePanel.Watching = false;
		using var dlg = new DeleteForm("파일 삭제", files);
		dlg.TrashMode = ret != DialogResult.Yes; // Yes면 바로 삭제, 아니면 휴지통으로 이동
		dlg.RunDialog();

		_activePanel.Navigate(FilePanelNavigation.Current);
	}

	private void MenuDelete()
	{
		var files = _activePanel.GetSelectedItems();
		if (files.Count == 0)
			return;

		using var mesg = new MesgBoxForm("파일 삭제", $"선택한 {files.Count}개의 항목을 바로 삭제할까요?", files);
		mesg.UtilText = "휴지통으로(&T)";
		mesg.DisplayIcon = MessageBoxIcon.Warning;

		var ret = mesg.RunDialog();
		if (ret == DialogResult.Cancel)
			return;

		_activePanel.Watching = false;
		using var dlg = new DeleteForm("파일 삭제", files);
		dlg.TrashMode = ret == DialogResult.Yes; // Yes면 휴지통으로 이동, 아니면 바로 삭제
		dlg.RunDialog();

		_activePanel.Navigate(FilePanelNavigation.Current);
	}

	private void MenuRename()
	{
		var files = _activePanel.GetSelectedItems();
		if (files.Count == 0)
			return;

		_activePanel.Watching = false;
		using var dlg = new RenameForm(files);
		dlg.RunDialog();

		_activePanel.Navigate(FilePanelNavigation.Current);
	}

	private void MenuProperties()
	{
		var name = _activePanel.GetFocusedItem();
		if (string.IsNullOrEmpty(name))
			return;
		ExcuteShowProperties(this, name);
	}

	private void MenuNewFolder()
	{
		var dir = _activePanel.CurrentDirectory;
		if (string.IsNullOrEmpty(dir))
			return;

		using var dlg = new LineInputForm("새 폴더 만들기", "새 폴더의 이름을 입력해주세요.");
		if (dlg.RunDialog() != DialogResult.OK)
			return;

		var name = dlg.InputText.Trim();
		if (string.IsNullOrEmpty(name) || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
		{
			MessageBox.Show("폴더 이름에 사용할 수 없는 문자가 포함되어 있습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			return;
		}

		var fullName = Path.Combine(dir, name);
		try
		{
			_activePanel.Watching = false;
			Directory.CreateDirectory(fullName);
			_activePanel.Navigate(FilePanelNavigation.Current);
			_activePanel.EnsureFocus(fullName);
		}
		catch
		{
			MessageBox.Show($"폴더를 만들 수 없어요!", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}

	private void MenuNavParentFolder() => _activePanel.Navigate(FilePanelNavigation.Parent);
	private void MenuNavRootFolder() => _activePanel.Navigate(FilePanelNavigation.Root);

	private void SortBySort(int sortOrder)
	{
		var settings = Settings.Instance;
		settings.SortOrder = sortOrder;
		leftPanel.Sort();
		rightPanel.Sort();
		CheckMenuItem();
	}

	private void MenuSortByName() => SortBySort(0);
	private void MenuSortByExtension() => SortBySort(1);
	private void MenuSortBySize() => SortBySort(2);
	private void MenuSortByDateTime() => SortBySort(3);
	private void MenuSortByAttribute() => SortBySort(4);

	private void MenuSortDesc()
	{
		var settings = Settings.Instance;
		settings.SortDescending = !settings.SortDescending;
		leftPanel.Sort();
		rightPanel.Sort();
		CheckMenuItem();
	}

	private void MenuShowHidden()
	{
		var settings = Settings.Instance;
		settings.ShowHidden = !settings.ShowHidden;
		leftPanel.Navigate(FilePanelNavigation.Current);
		rightPanel.Navigate(FilePanelNavigation.Current);
		CheckMenuItem();
	}

	private void MenuNewTab() => _activePanel.AddTab();
	private void MenuTabList() => _activePanel.ShowTabListMenu();
	private void MenuNextTab() => _activePanel.NextTab();
	private void MenuPreviousTab() => _activePanel.PreviousTab();
	private void MenuSwitchPanel() => SwitchPanel();

	private bool CheckPanelDirectory()
	{
		var leftDir = leftPanel.CurrentDirectory;
		var rightDir = rightPanel.CurrentDirectory;
		if (leftDir == rightDir)
		{
			MessageBox.Show("왼쪽과 오른쪽이 동일한 폴더를 가리키고 있어요. 다른 폴더를 선택해주세요.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			return false;
		}

		return true;
	}

	private void MenuCopy()
	{
		if (!CheckPanelDirectory())
			return;

		var files = _activePanel.GetSelectedItems();
		if (files.Count == 0)
			return;

		using var mesg = new MesgBoxForm("파일 복사", $"선택한 {files.Count}개의 항목을 복사할까요?", files);
		mesg.DisplayIcon = MessageBoxIcon.Question;
		var ret = mesg.RunDialog();
		if (ret == DialogResult.Cancel)
			return;

		leftPanel.Watching = false;
		rightPanel.Watching = false;

		var other = OtherPanel;
		using var dlg = new CopyForm("파일 복사", files, other.CurrentDirectory);
		dlg.MoveMode = false;
		dlg.RunDialog();

		leftPanel.Navigate(FilePanelNavigation.Current);
		rightPanel.Navigate(FilePanelNavigation.Current);
	}

	private void MenuMove()
	{
		if (!CheckPanelDirectory())
			return;

		var files = _activePanel.GetSelectedItems();
		if (files.Count == 0)
			return;

		using var mesg = new MesgBoxForm("파일 이동", $"선택한 {files.Count}개의 항목을 이동할까요?", files);
		mesg.DisplayIcon = MessageBoxIcon.Question;
		var ret = mesg.RunDialog();
		if (ret == DialogResult.Cancel)
			return;

		leftPanel.Watching = false;
		rightPanel.Watching = false;

		var other = OtherPanel;
		using var dlg = new CopyForm("파일 이동", files, other.CurrentDirectory);
		dlg.MoveMode = true;
		dlg.RunDialog();

		leftPanel.Navigate(FilePanelNavigation.Current);
		rightPanel.Navigate(FilePanelNavigation.Current);
	}

	private static void SetClipboardFiles(IEnumerable<string> files, bool cut)
	{
		var data = new DataObject();
		// 파일 목록을 CF_HDROP 포맷으로 추가
		data.SetData(DataFormats.FileDrop, files.ToArray());

		// Preferred DropEffect 설정 (복사: 5, 잘라내기: 2)
		var effect = cut ? 2 : 5;
		var bytes = BitConverter.GetBytes(effect);
		var ms = new MemoryStream();
		ms.Write(bytes, 0, bytes.Length);
		data.SetData("Preferred DropEffect", ms);

		Clipboard.SetDataObject(data, true);
	}

	private void MenuClipboardCopy()
	{
		var files = _activePanel.GetSelectedItems();
		if (files.Count == 0)
			return;
		SetClipboardFiles(files, false);
	}

	private void MenuClipboardCut()
	{
		var files = _activePanel.GetSelectedItems();
		if (files.Count == 0)
			return;
		SetClipboardFiles(files, true);
	}
}
