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
				new MenuDef { Text = "탭 목록(&Y)", Command = Commands.None },
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
			// 보기
			{ Commands.SortByName, MenuSortByName },
			{ Commands.SortByExtension, MenuSortByExtension }, 
			{ Commands.SortBySize, MenuSortBySize },
			{ Commands.SortByDateTime, MenuSortByDateTime },
			{ Commands.SortByAttribute, MenuSortByAttribute },
			{ Commands.SortDesc, MenuSortDesc },
			{ Commands.ShowHidden, MenuShowHidden },
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

		_activePanel.NavigateAgain();
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

		_activePanel.NavigateAgain();
	}

	private void MenuRename()
	{
		var files = _activePanel.GetSelectedItems();
		if (files.Count == 0)
			return;

		_activePanel.Watching = false;
		using var dlg = new RenameForm(files);
		dlg.RunDialog();

		_activePanel.NavigateAgain();
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
			_activePanel.NavigateAgain();
			_activePanel.EnsureFocus(fullName);
		}
		catch
		{
			MessageBox.Show($"폴더를 만들 수 없어요!", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}

	private void MenuNavParentFolder() => _activePanel.NavigateParent();
	private void MenuNavRootFolder() => _activePanel.NavigateRoot();

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
		leftPanel.NavigateAgain();
		rightPanel.NavigateAgain();
		CheckMenuItem();
	}
}
