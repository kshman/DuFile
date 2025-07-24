using System.Reflection;
// ReSharper disable MissingXmlDoc

namespace DuFile.Windows;

public partial class MainForm
{
	private ToolStripMenuItem? _menuAlignOrder;         // 정렬 메뉴의 오름차순/내림차순 토글을 위한 변수
	private ToolStripMenuItem? _menuOrderByName;        // 정렬 기준이 이름
	private ToolStripMenuItem? _menuOrderByExt;         // 정렬 기준이 확장자
	private ToolStripMenuItem? _menuOrderBySize;        // 정렬 기준이 크기	
	private ToolStripMenuItem? _menuOrderByDateTime;    // 정렬 기준이 날짜/시간
	private ToolStripMenuItem? _menuOrderByAttribute;   // 정렬 기준이 속성
	private ToolStripMenuItem? _menuViewHidden;         // 숨김 파일 보기 토글을 위한 변수

	// 메뉴 정의
	private static readonly MenuDef[] MainMenus =
	[
		new()
		{
			Text = "파일(&F)",
			SubMenus =
			[
				new MenuDef { Text = "열기(&O)", Command = Commands.Open },
				new MenuDef { Text = "인수와 함께 열기(&W)", Command = Commands.OpenWith },
				new MenuDef { Text = "선택한 모든 파일 열기(&L)", Command = Commands.OpenAll },
				new MenuDef(),
				new MenuDef { Text = "휴지통으로(&T)", Command = Commands.Trash, Shortcut = "Delete" },
				new MenuDef { Text = "바로 지우기(&D)", Command = Commands.Delete, Shortcut = "Shift+Delete" },
				new MenuDef(),
				new MenuDef { Text = "이름 바꾸기(&R)", Command = Commands.Rename, Shortcut = "Ctrl+R" },
				new MenuDef { Text = "고급 이름 바꾸기(&E)", Command = Commands.AdvancedRename, Shortcut = "Ctrl+Shift+R" },
				new MenuDef(),
				new MenuDef { Text = "속성(&A)", Command = Commands.Properties, Shortcut = "Alt+Enter" },
				new MenuDef { Text = "속성 바꾸기(&Z)", Command = Commands.ChangeProperties, Shortcut = "Ctrl+Z" },
				new MenuDef(),
				new MenuDef { Text = "새 빈 파일(&N)", Command = Commands.NewEmptyFile, Shortcut = "Ctrl+N" },
				new MenuDef { Text = "바탕화면 바로가기 만들기(&B)", Command = Commands.NewShortcutDesktop, Shortcut = "Ctrl+B" },
				new MenuDef(),
				new MenuDef { Text = "선택한 폴더 크기 계산(&L)", Command = Commands.CalcSelectedFolder, Shortcut = "Ctrl+Shift+Q" },
				new MenuDef { Text = "확장자가 뭔지 검색(&Q)", Command = Commands.DetemineExtension, Shortcut = "Alt+Q" },
				new MenuDef
				{
					Text="체크섬(&H)",
					SubMenus =
					[
						new MenuDef { Text = "CRC(&1)", Command = Commands.ChecksumCrc, Shortcut = null },
						new MenuDef { Text = "MD5(&2)", Command = Commands.ChecksumMd5, Shortcut = null },
						new MenuDef { Text = "SHA1(&3)", Command = Commands.ChecksumSha1, Shortcut = null },
					]
				},
				new MenuDef(),
				new MenuDef { Text = "끝내기(&X)", Command = Commands.Exit, Shortcut = "Alt+X" },
			]
		},
		new()
		{
			Text = "폴더(&R)",
			SubMenus =
			[
				new MenuDef { Text = "폴더 선택(&T)", Command = Commands.None, Disable = true },
				new MenuDef { Text = "즐겨찾기(&F)", Command = Commands.None, Shortcut = "F11" },
				new MenuDef(),
				new MenuDef { Text = "새 폴더 만들기(&N)", Command = Commands.None, Shortcut = "Alt+K" },
				new MenuDef(),
				new MenuDef { Text = "뒤로(&B)", Command = Commands.None, Shortcut = "Ctrl+Left" },
				new MenuDef { Text = "앞으로(&F)", Command = Commands.None, Shortcut = "Ctrl+Right" },
				new MenuDef { Text = "상위 폴더로(&X)", Command = Commands.None },
				new MenuDef { Text = "최상위 폴더로(&Z)", Command = Commands.None },
				new MenuDef(),
				new MenuDef {
					Text = "작업 폴더 설정(&W)",
					SubMenus =
					[
						new MenuDef { Text = "작업 폴더 &1", Command = Commands.None, Shortcut = "Alt+1" },
						new MenuDef { Text = "작업 폴더 &2", Command = Commands.None, Shortcut = "Alt+2" },
						new MenuDef { Text = "작업 폴더 &3", Command = Commands.None, Shortcut = "Alt+3" },
						new MenuDef { Text = "작업 폴더 &4", Command = Commands.None, Shortcut = "Alt+4" },
						new MenuDef { Text = "작업 폴더 &5", Command = Commands.None, Shortcut = "Alt+5" },
					]
				},
				new MenuDef {
					Text = "작업 폴더로 가기(&G)",
					SubMenus =
					[
						new MenuDef { Text = "작업 폴더 &1", Command = Commands.None, Shortcut = "Ctrl+1" },
						new MenuDef { Text = "작업 폴더 &2", Command = Commands.None, Shortcut = "Ctrl+2" },
						new MenuDef { Text = "작업 폴더 &3", Command = Commands.None, Shortcut = "Ctrl+3" },
						new MenuDef { Text = "작업 폴더 &4", Command = Commands.None, Shortcut = "Ctrl+4" },
						new MenuDef { Text = "작업 폴더 &5", Command = Commands.None, Shortcut = "Ctrl+5" },
					]
				},
			]
		},
		new()
		{
			Text = "편집(&E)",
			SubMenus = [
				new MenuDef { Text = "복사(&C)", Command = Commands.None, Shortcut = "Ctrl+C" },
				new MenuDef { Text = "붙여넣기(&V)", Command = Commands.None, Shortcut = "Ctrl+V" },
				new MenuDef { Text = "잘라내기(&X)", Command = Commands.None, Shortcut = "Ctrl+X" },
				new MenuDef(),
				new MenuDef { Text = "전체 경로 이름 복사(&L)", Command = Commands.None, Shortcut = "Ctrl+Alt+F" },
				new MenuDef { Text = "파일 이름만 복사(&A)", Command = Commands.None, Shortcut = "Ctrl+Alt+A" },
				new MenuDef { Text = "경로 이름만 복사(&R)", Command = Commands.None, Shortcut = "Ctrl+Alt+R" },
				new MenuDef(),
				new MenuDef { Text = "선택/해제(&B)", Command = Commands.None },
				new MenuDef { Text = "전체 선택(&U)", Command = Commands.None, Shortcut = "Ctrl+U" },
				new MenuDef { Text = "선택 해제(&N)", Command = Commands.None, Shortcut = "Ctrl+Shift+U" },
				new MenuDef { Text = "선택 반전(&I)", Command = Commands.None, Shortcut = "Ctrl+Shift+I" },
				new MenuDef { Text = "고급 선택(&G)", Command = Commands.None, Shortcut = "Ctrl+D" },
				new MenuDef { Text = "고급 해제(&W)", Command = Commands.None, Shortcut = "Ctrl+Shift+D" },
				new MenuDef(),
				new MenuDef { Text = "같은 확장자 모두 선택(&E)", Command = Commands.None, Shortcut = "Ctrl+Alt+E" },
				new MenuDef { Text = "같은 이름 모두 선택(&N)", Command = Commands.None, Shortcut = "Ctrl+Alt+N" },
			]
		},
		new()
		{
			Text = "보기(&V)",
			SubMenus = [
				new MenuDef { Text = "도구 모음(&B)", Command = Commands.None },
				new MenuDef { Text = "기능키 표시줄(&C)", Command = Commands.None },
				new MenuDef(),
				new MenuDef { Text = "폴더/드라이브 정보 표시줄(&D)", Command = Commands.None },
				new MenuDef { Text = "파일 정보 표시줄(&F)", Command = Commands.None },
				new MenuDef(),
				new MenuDef { Text = "목록 보기 방식(&S)", Command = Commands.None },
				new MenuDef { Text = "열 보기 방식(&L)", Command = Commands.None },
				new MenuDef(),
				new MenuDef {
					Text = "정렬(&A)",
					SubMenus = [
						new MenuDef { Text = "이름으로(&N)", Command = Commands.None, Shortcut = "Ctrl+Shift+N", Variable = "_menuOrderByName" },
						new MenuDef { Text = "확장자로(&E)", Command = Commands.None, Shortcut = "Ctrl+Shift+E", Variable = "_menuOrderByExt" },
						new MenuDef { Text = "크기로(&S)", Command = Commands.None, Shortcut = "Ctrl+Shift+S", Variable = "_menuOrderBySize" },
						new MenuDef { Text = "날짜/시간으로(&T)", Command = Commands.None, Shortcut = "Ctrl+Shift+T", Variable = "_menuOrderByDateTime" },
						new MenuDef { Text = "속성으로(&R)", Command = Commands.None, Shortcut = "Ctrl+Shift+R", Variable = "_menuOrderByAttribute" },
						new MenuDef(),
						new MenuDef { Text = "내림차순(&D)", Command = Commands.None, Shortcut = "Ctrl+Shift+D", Variable = "_menuAlignOrder" },
					]
				},
				new MenuDef { Text = "골라보기(&F)", Command = Commands.None },
				new MenuDef(),
				new MenuDef { Text = "숨김 파일 보기(&Z)", Command = Commands.None, Shortcut = "Alt+Z", Variable = "_menuViewHidden"},
				new MenuDef(),
				new MenuDef { Text = "새 탭(&T)", Command = Commands.None, Shortcut = "Ctrl+T" },
				new MenuDef { Text = "탭 목록(&Y)", Command = Commands.None },
			]
		},
		new()
		{
			Text = "도구(&T)",
			SubMenus = [
				new MenuDef { Text = "설정(&S)", Command = Commands.None, Shortcut = "Ctrl+F12" },
			]
		},
		new()
		{
			Text = "도움말(&H)",
			SubMenus =
			[
				new MenuDef { Text = "도움말", Command = Commands.Help, Shortcut = "F1" },
				new MenuDef { Text = "버전 정보", Command = Commands.None, Shortcut = null }
			]
		}
	];

	// 메뉴 액션
	private Dictionary<string, Action> _menuActions = [];

	// 메뉴 초기화
	private void IntiializeMenu()
	{
		var keyConverter = new KeysConverter();
		AddMenuItems(keyConverter, menuStrip.Items, MainMenus);

		_menuActions = new Dictionary<string, Action>
		{
			{ Commands.None, () => { } },
			{ Commands.Exit, MenuFileExit },
			// 여기에 다른 명령어와 해당 작업을 추가할 수 있습니다.
		};
	}

	// 메뉴에 항목 넣기
	private void AddMenuItems(KeysConverter keyConverter, ToolStripItemCollection items, MenuDef[] menus)
	{
		foreach (var menu in menus)
		{
			if (menu.Text == null)
			{
				items.Add(new ToolStripSeparator());
				continue;
			}

			var menuItem = new ToolStripMenuItem(menu.Text);
			menuItem.Click += MenuItem_Clicked;
			if (menu.Shortcut != null)
				menuItem.ShortcutKeys = Alter.ParseKeyString(keyConverter, menu.Shortcut);
			if (menu.Disable)
				menuItem.Enabled = false;
			menuItem.Tag = menu.Command;
			if (menu.Variable != null)
			{
				// menu.Variable에 해당하는 MainForm의 필드 정보를 가져옴
				var f = typeof(MainForm).GetField(menu.Variable, BindingFlags.Instance | BindingFlags.NonPublic);
				if (f != null && f.FieldType == typeof(ToolStripMenuItem))
					f.SetValue(this, menuItem);
			}
			if (menu.SubMenus is { Length: > 0 })
				AddMenuItems(keyConverter, menuItem.DropDownItems, menu.SubMenus);
			items.Add(menuItem);
		}
	}

	// 메뉴 항목 클릭 이벤트 핸들러
	private void MenuItem_Clicked(object? sender, EventArgs args)
	{
		if (sender is ToolStripMenuItem { Tag: string command })
			ExecuteCommand(command);
	}

	/// <summary>
	/// Executes the specified command by invoking the associated action.
	/// </summary>
	/// <remarks>If the command is not found in the menu actions dictionary, an error message is displayed to the
	/// user.</remarks>
	/// <param name="command">The command to execute. This should be a key that exists in the menu actions dictionary.</param>
	public void ExecuteCommand(string command)
	{
		if (_menuActions.TryGetValue(command, out var action))
			action();
		else
			MessageBox.Show($"'{command}' 기능은 아직 구현되지 않았습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
	}

	// 파일 실행
	public void ExcuteProcess(string filename)
	{
		if (string.IsNullOrEmpty(filename))
			return;
		try
		{
			var process = new System.Diagnostics.Process
			{
				StartInfo = new System.Diagnostics.ProcessStartInfo
				{
					FileName = filename,
					UseShellExecute = true,
					ErrorDialog = true
				}
			};
			process.Start();
		}
		catch (Exception ex)
		{
			MessageBox.Show($"파일({filename})을 열 수 없어요!", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
			Debugs.WriteLine($"파일 열기 오류: {ex.Message}");
		}
	}

	// 쉘 메뉴 열기
	public void ExcuteShellContextMenu(IWin32Window owner, Point screenPos, IList<string> files)
	{
		ShellContextMenu.Show(owner, screenPos, files);
	}

	// 끝내기
	private void MenuFileExit()
	{
		Close();
	}
}
