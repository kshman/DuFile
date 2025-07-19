namespace DuFile
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

		#region Windows Form Designer generated code

		/// <summary>
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			menuStrip = new MenuStrip();
			fileMenuItem = new ToolStripMenuItem();
			openItem = new ToolStripMenuItem();
			openWithItem = new ToolStripMenuItem();
			openAlllItem = new ToolStripMenuItem();
			toolStripSeparator1 = new ToolStripSeparator();
			trashItem = new ToolStripMenuItem();
			removeItem = new ToolStripMenuItem();
			toolStripSeparator2 = new ToolStripSeparator();
			renameItem = new ToolStripMenuItem();
			advRenameItem = new ToolStripMenuItem();
			toolStripSeparator3 = new ToolStripSeparator();
			attrItem = new ToolStripMenuItem();
			setAttrItem = new ToolStripMenuItem();
			toolStripSeparator4 = new ToolStripSeparator();
			newEmptyItem = new ToolStripMenuItem();
			newShortcutItem = new ToolStripMenuItem();
			toolStripSeparator5 = new ToolStripSeparator();
			calcSizeItem = new ToolStripMenuItem();
			detemineExtensionItem = new ToolStripMenuItem();
			checksumItem = new ToolStripMenuItem();
			checksumCrcItem = new ToolStripMenuItem();
			checksumMd5Item = new ToolStripMenuItem();
			checksumSha1Item = new ToolStripMenuItem();
			toolStripSeparator6 = new ToolStripSeparator();
			exitItem = new ToolStripMenuItem();
			directoryMenuItem = new ToolStripMenuItem();
			dirSelectMenuItem = new ToolStripMenuItem();
			dirFavorityMenuItem = new ToolStripMenuItem();
			toolStripSeparator7 = new ToolStripSeparator();
			dirNewMenuItem = new ToolStripMenuItem();
			toolStripSeparator8 = new ToolStripSeparator();
			dirHistoryBackMenuItem = new ToolStripMenuItem();
			dirHistoryForwardMenuItem = new ToolStripMenuItem();
			dirParentMenuItem = new ToolStripMenuItem();
			dirTopMenuItem = new ToolStripMenuItem();
			toolStripSeparator9 = new ToolStripSeparator();
			dirSetCustomMenuItem = new ToolStripMenuItem();
			setCustom1MenuItem = new ToolStripMenuItem();
			setCustom2MenuItem = new ToolStripMenuItem();
			setCustom3MenuItem = new ToolStripMenuItem();
			setCustom4MenuItem = new ToolStripMenuItem();
			setCustom5MenuItem = new ToolStripMenuItem();
			dirGoCustomMenuItem = new ToolStripMenuItem();
			custom1MenuItem = new ToolStripMenuItem();
			custom2MenuItem = new ToolStripMenuItem();
			custom3MenuItem = new ToolStripMenuItem();
			custom4MenuItem = new ToolStripMenuItem();
			custom5MenuItem = new ToolStripMenuItem();
			editMenuItem = new ToolStripMenuItem();
			copyItem = new ToolStripMenuItem();
			pasteItem = new ToolStripMenuItem();
			cutItem = new ToolStripMenuItem();
			toolStripSeparator10 = new ToolStripSeparator();
			fullPathItem = new ToolStripMenuItem();
			filenameOnlyItem = new ToolStripMenuItem();
			directoryOnlyItem = new ToolStripMenuItem();
			toolStripSeparator11 = new ToolStripSeparator();
			selectCurrentItem = new ToolStripMenuItem();
			selectAllItem = new ToolStripMenuItem();
			selectNoneItem = new ToolStripMenuItem();
			selectInvertItem = new ToolStripMenuItem();
			advSelectItem = new ToolStripMenuItem();
			advUnselectItem = new ToolStripMenuItem();
			toolStripSeparator12 = new ToolStripSeparator();
			selectByExtItem = new ToolStripMenuItem();
			selectByNameItem = new ToolStripMenuItem();
			viewMenuItem = new ToolStripMenuItem();
			toggleToolbarItem = new ToolStripMenuItem();
			toggleFuncBarItem = new ToolStripMenuItem();
			toolStripSeparator13 = new ToolStripSeparator();
			toggleDirectoryInfoItem = new ToolStripMenuItem();
			toggleFileInfoItem = new ToolStripMenuItem();
			toolStripSeparator14 = new ToolStripSeparator();
			viewListViewTypeMenuItem = new ToolStripMenuItem();
			viewColumnTypeMenuItem = new ToolStripMenuItem();
			toolStripSeparator15 = new ToolStripSeparator();
			sortItem = new ToolStripMenuItem();
			sortNoneItem = new ToolStripMenuItem();
			toolStripSeparator18 = new ToolStripSeparator();
			sortNameItem = new ToolStripMenuItem();
			sortExtItem = new ToolStripMenuItem();
			sortSizeItem = new ToolStripMenuItem();
			sortTimeItem = new ToolStripMenuItem();
			sortAttrItem = new ToolStripMenuItem();
			toolStripSeparator19 = new ToolStripSeparator();
			toggleSortDescItem = new ToolStripMenuItem();
			viewSelectMenuItem = new ToolStripMenuItem();
			toolStripSeparator16 = new ToolStripSeparator();
			toggleHiddenItem = new ToolStripMenuItem();
			toolStripSeparator17 = new ToolStripSeparator();
			newTabItem = new ToolStripMenuItem();
			tabListItem = new ToolStripMenuItem();
			systemMenuItem = new ToolStripMenuItem();
			toolMenuItem = new ToolStripMenuItem();
			toolSettingMenuItem = new ToolStripMenuItem();
			helpMenuItem = new ToolStripMenuItem();
			aboutItem = new ToolStripMenuItem();
			toolStrip = new ToolStrip();
			leftPanel = new DuFile.Windows.FilePanel();
			verticalBar = new DuFile.Windows.VerticalBar();
			rightPanel = new DuFile.Windows.FilePanel();
			funcBar = new DuFile.Windows.FuncBar();
			menuStrip.SuspendLayout();
			SuspendLayout();
			// 
			// menuStrip
			// 
			menuStrip.Items.AddRange(new ToolStripItem[] { fileMenuItem, directoryMenuItem, editMenuItem, viewMenuItem, systemMenuItem, toolMenuItem, helpMenuItem });
			menuStrip.Location = new Point(0, 0);
			menuStrip.Name = "menuStrip";
			menuStrip.Size = new Size(800, 24);
			menuStrip.TabIndex = 0;
			menuStrip.Text = "menuStrip1";
			// 
			// fileMenuItem
			// 
			fileMenuItem.DropDownItems.AddRange(new ToolStripItem[] { openItem, openWithItem, openAlllItem, toolStripSeparator1, trashItem, removeItem, toolStripSeparator2, renameItem, advRenameItem, toolStripSeparator3, attrItem, setAttrItem, toolStripSeparator4, newEmptyItem, newShortcutItem, toolStripSeparator5, calcSizeItem, detemineExtensionItem, checksumItem, toolStripSeparator6, exitItem });
			fileMenuItem.Name = "fileMenuItem";
			fileMenuItem.Size = new Size(57, 20);
			fileMenuItem.Text = "파일(&F)";
			// 
			// openItem
			// 
			openItem.Name = "openItem";
			openItem.Size = new Size(298, 22);
			openItem.Text = "열기/실행(&O)";
			// 
			// openWithItem
			// 
			openWithItem.Name = "openWithItem";
			openWithItem.Size = new Size(298, 22);
			openWithItem.Text = "인수와 함께 열기(&W)";
			// 
			// openAlllItem
			// 
			openAlllItem.Name = "openAlllItem";
			openAlllItem.Size = new Size(298, 22);
			openAlllItem.Text = "선택한 모든 파일 열기(&L)";
			// 
			// toolStripSeparator1
			// 
			toolStripSeparator1.Name = "toolStripSeparator1";
			toolStripSeparator1.Size = new Size(295, 6);
			// 
			// trashItem
			// 
			trashItem.Name = "trashItem";
			trashItem.ShortcutKeys = Keys.Delete;
			trashItem.Size = new Size(298, 22);
			trashItem.Text = "휴지통으로 버리기&(D)";
			// 
			// removeItem
			// 
			removeItem.Name = "removeItem";
			removeItem.ShortcutKeys = Keys.Shift | Keys.Delete;
			removeItem.Size = new Size(298, 22);
			removeItem.Text = "바로 지우기(&S)";
			// 
			// toolStripSeparator2
			// 
			toolStripSeparator2.Name = "toolStripSeparator2";
			toolStripSeparator2.Size = new Size(295, 6);
			// 
			// renameItem
			// 
			renameItem.Name = "renameItem";
			renameItem.ShortcutKeys = Keys.Control | Keys.R;
			renameItem.Size = new Size(298, 22);
			renameItem.Text = "이름 바꾸기(&R)";
			// 
			// advRenameItem
			// 
			advRenameItem.Name = "advRenameItem";
			advRenameItem.ShortcutKeys = Keys.Control | Keys.Shift | Keys.R;
			advRenameItem.Size = new Size(298, 22);
			advRenameItem.Text = "고급 이름 바꾸기(&A)";
			// 
			// toolStripSeparator3
			// 
			toolStripSeparator3.Name = "toolStripSeparator3";
			toolStripSeparator3.Size = new Size(295, 6);
			// 
			// attrItem
			// 
			attrItem.Name = "attrItem";
			attrItem.Size = new Size(298, 22);
			attrItem.Text = "속성(&E)";
			// 
			// setAttrItem
			// 
			setAttrItem.Name = "setAttrItem";
			setAttrItem.ShortcutKeys = Keys.Control | Keys.Z;
			setAttrItem.Size = new Size(298, 22);
			setAttrItem.Text = "속성 바꾸기(&Z)";
			// 
			// toolStripSeparator4
			// 
			toolStripSeparator4.Name = "toolStripSeparator4";
			toolStripSeparator4.Size = new Size(295, 6);
			// 
			// newEmptyItem
			// 
			newEmptyItem.Name = "newEmptyItem";
			newEmptyItem.ShortcutKeys = Keys.Control | Keys.N;
			newEmptyItem.Size = new Size(298, 22);
			newEmptyItem.Text = "빈 파일 만들기(&B)";
			// 
			// newShortcutItem
			// 
			newShortcutItem.Name = "newShortcutItem";
			newShortcutItem.ShortcutKeys = Keys.Control | Keys.K;
			newShortcutItem.Size = new Size(298, 22);
			newShortcutItem.Text = "바탕화면에 단축 아이콘 만들기(&K)";
			// 
			// toolStripSeparator5
			// 
			toolStripSeparator5.Name = "toolStripSeparator5";
			toolStripSeparator5.Size = new Size(295, 6);
			// 
			// calcSizeItem
			// 
			calcSizeItem.Name = "calcSizeItem";
			calcSizeItem.ShortcutKeys = Keys.Control | Keys.Shift | Keys.Q;
			calcSizeItem.Size = new Size(298, 22);
			calcSizeItem.Text = "선택한 폴더 크기 계산(&L)";
			// 
			// detemineExtensionItem
			// 
			detemineExtensionItem.Name = "detemineExtensionItem";
			detemineExtensionItem.ShortcutKeys = Keys.Alt | Keys.Q;
			detemineExtensionItem.Size = new Size(298, 22);
			detemineExtensionItem.Text = "확장자가 뭔지 검색(Q)";
			// 
			// checksumItem
			// 
			checksumItem.DropDownItems.AddRange(new ToolStripItem[] { checksumCrcItem, checksumMd5Item, checksumSha1Item });
			checksumItem.Name = "checksumItem";
			checksumItem.Size = new Size(298, 22);
			checksumItem.Text = "체크섬(&H)";
			// 
			// checksumCrcItem
			// 
			checksumCrcItem.Name = "checksumCrcItem";
			checksumCrcItem.Size = new Size(120, 22);
			checksumCrcItem.Text = "CRC(&C)";
			// 
			// checksumMd5Item
			// 
			checksumMd5Item.Name = "checksumMd5Item";
			checksumMd5Item.Size = new Size(120, 22);
			checksumMd5Item.Text = "MD5(&M)";
			// 
			// checksumSha1Item
			// 
			checksumSha1Item.Name = "checksumSha1Item";
			checksumSha1Item.Size = new Size(120, 22);
			checksumSha1Item.Text = "SHA1(&S)";
			// 
			// toolStripSeparator6
			// 
			toolStripSeparator6.Name = "toolStripSeparator6";
			toolStripSeparator6.Size = new Size(295, 6);
			// 
			// exitItem
			// 
			exitItem.Name = "exitItem";
			exitItem.ShortcutKeys = Keys.Alt | Keys.X;
			exitItem.Size = new Size(298, 22);
			exitItem.Text = "끝내기(&X)";
			// 
			// directoryMenuItem
			// 
			directoryMenuItem.DropDownItems.AddRange(new ToolStripItem[] { dirSelectMenuItem, dirFavorityMenuItem, toolStripSeparator7, dirNewMenuItem, toolStripSeparator8, dirHistoryBackMenuItem, dirHistoryForwardMenuItem, dirParentMenuItem, dirTopMenuItem, toolStripSeparator9, dirSetCustomMenuItem, dirGoCustomMenuItem });
			directoryMenuItem.Name = "directoryMenuItem";
			directoryMenuItem.Size = new Size(84, 20);
			directoryMenuItem.Text = "디렉토리(&D)";
			// 
			// dirSelectMenuItem
			// 
			dirSelectMenuItem.Enabled = false;
			dirSelectMenuItem.Name = "dirSelectMenuItem";
			dirSelectMenuItem.Size = new Size(232, 22);
			dirSelectMenuItem.Text = "디렉토리 선택(&T)";
			// 
			// dirFavorityMenuItem
			// 
			dirFavorityMenuItem.Name = "dirFavorityMenuItem";
			dirFavorityMenuItem.ShortcutKeys = Keys.F11;
			dirFavorityMenuItem.Size = new Size(232, 22);
			dirFavorityMenuItem.Text = "즐겨찾기(&F)";
			// 
			// toolStripSeparator7
			// 
			toolStripSeparator7.Name = "toolStripSeparator7";
			toolStripSeparator7.Size = new Size(229, 6);
			// 
			// dirNewMenuItem
			// 
			dirNewMenuItem.Name = "dirNewMenuItem";
			dirNewMenuItem.ShortcutKeys = Keys.Alt | Keys.K;
			dirNewMenuItem.Size = new Size(232, 22);
			dirNewMenuItem.Text = "새 디렉토리 만들기(&N)";
			// 
			// toolStripSeparator8
			// 
			toolStripSeparator8.Name = "toolStripSeparator8";
			toolStripSeparator8.Size = new Size(229, 6);
			// 
			// dirHistoryBackMenuItem
			// 
			dirHistoryBackMenuItem.Name = "dirHistoryBackMenuItem";
			dirHistoryBackMenuItem.ShortcutKeys = Keys.Control | Keys.Left;
			dirHistoryBackMenuItem.Size = new Size(232, 22);
			dirHistoryBackMenuItem.Text = "뒤로(&B)";
			// 
			// dirHistoryForwardMenuItem
			// 
			dirHistoryForwardMenuItem.Name = "dirHistoryForwardMenuItem";
			dirHistoryForwardMenuItem.ShortcutKeys = Keys.Control | Keys.Right;
			dirHistoryForwardMenuItem.Size = new Size(232, 22);
			dirHistoryForwardMenuItem.Text = "앞으로(&F)";
			// 
			// dirParentMenuItem
			// 
			dirParentMenuItem.Name = "dirParentMenuItem";
			dirParentMenuItem.Size = new Size(232, 22);
			dirParentMenuItem.Text = "상위 디렉토리로(&X)";
			// 
			// dirTopMenuItem
			// 
			dirTopMenuItem.Name = "dirTopMenuItem";
			dirTopMenuItem.Size = new Size(232, 22);
			dirTopMenuItem.Text = "최상위 디렉토리로(&Z)";
			// 
			// toolStripSeparator9
			// 
			toolStripSeparator9.Name = "toolStripSeparator9";
			toolStripSeparator9.Size = new Size(229, 6);
			// 
			// dirSetCustomMenuItem
			// 
			dirSetCustomMenuItem.DropDownItems.AddRange(new ToolStripItem[] { setCustom1MenuItem, setCustom2MenuItem, setCustom3MenuItem, setCustom4MenuItem, setCustom5MenuItem });
			dirSetCustomMenuItem.Name = "dirSetCustomMenuItem";
			dirSetCustomMenuItem.Size = new Size(232, 22);
			dirSetCustomMenuItem.Text = "작업 디렉토리 설정(&W)";
			// 
			// setCustom1MenuItem
			// 
			setCustom1MenuItem.Name = "setCustom1MenuItem";
			setCustom1MenuItem.ShortcutKeys = Keys.Alt | Keys.D1;
			setCustom1MenuItem.Size = new Size(198, 22);
			setCustom1MenuItem.Text = "작업 디렉토리 &1";
			// 
			// setCustom2MenuItem
			// 
			setCustom2MenuItem.Name = "setCustom2MenuItem";
			setCustom2MenuItem.ShortcutKeys = Keys.Alt | Keys.D2;
			setCustom2MenuItem.Size = new Size(198, 22);
			setCustom2MenuItem.Text = "작업 디렉토리 &2";
			// 
			// setCustom3MenuItem
			// 
			setCustom3MenuItem.Name = "setCustom3MenuItem";
			setCustom3MenuItem.ShortcutKeys = Keys.Alt | Keys.D3;
			setCustom3MenuItem.Size = new Size(198, 22);
			setCustom3MenuItem.Text = "작업 디렉토리 &3";
			// 
			// setCustom4MenuItem
			// 
			setCustom4MenuItem.Name = "setCustom4MenuItem";
			setCustom4MenuItem.ShortcutKeys = Keys.Alt | Keys.D4;
			setCustom4MenuItem.Size = new Size(198, 22);
			setCustom4MenuItem.Text = "작업 디렉토리 &4";
			// 
			// setCustom5MenuItem
			// 
			setCustom5MenuItem.Name = "setCustom5MenuItem";
			setCustom5MenuItem.ShortcutKeys = Keys.Alt | Keys.D5;
			setCustom5MenuItem.Size = new Size(198, 22);
			setCustom5MenuItem.Text = "작업 디렉토리 &5";
			// 
			// dirGoCustomMenuItem
			// 
			dirGoCustomMenuItem.DropDownItems.AddRange(new ToolStripItem[] { custom1MenuItem, custom2MenuItem, custom3MenuItem, custom4MenuItem, custom5MenuItem });
			dirGoCustomMenuItem.Name = "dirGoCustomMenuItem";
			dirGoCustomMenuItem.Size = new Size(232, 22);
			dirGoCustomMenuItem.Text = "작업 디렉토리로 가기(&G)";
			// 
			// custom1MenuItem
			// 
			custom1MenuItem.Name = "custom1MenuItem";
			custom1MenuItem.ShortcutKeys = Keys.Control | Keys.D1;
			custom1MenuItem.Size = new Size(202, 22);
			custom1MenuItem.Text = "작업 디렉토리 &1";
			// 
			// custom2MenuItem
			// 
			custom2MenuItem.Name = "custom2MenuItem";
			custom2MenuItem.ShortcutKeys = Keys.Control | Keys.D2;
			custom2MenuItem.Size = new Size(202, 22);
			custom2MenuItem.Text = "작업 디렉토리 &2";
			// 
			// custom3MenuItem
			// 
			custom3MenuItem.Name = "custom3MenuItem";
			custom3MenuItem.ShortcutKeys = Keys.Control | Keys.D3;
			custom3MenuItem.Size = new Size(202, 22);
			custom3MenuItem.Text = "작업 디렉토리 &3";
			// 
			// custom4MenuItem
			// 
			custom4MenuItem.Name = "custom4MenuItem";
			custom4MenuItem.ShortcutKeys = Keys.Control | Keys.D4;
			custom4MenuItem.Size = new Size(202, 22);
			custom4MenuItem.Text = "작업 디렉토리 &4";
			// 
			// custom5MenuItem
			// 
			custom5MenuItem.Name = "custom5MenuItem";
			custom5MenuItem.ShortcutKeys = Keys.Control | Keys.D5;
			custom5MenuItem.Size = new Size(202, 22);
			custom5MenuItem.Text = "작업 디렉토리 &5";
			// 
			// editMenuItem
			// 
			editMenuItem.DropDownItems.AddRange(new ToolStripItem[] { copyItem, pasteItem, cutItem, toolStripSeparator10, fullPathItem, filenameOnlyItem, directoryOnlyItem, toolStripSeparator11, selectCurrentItem, selectAllItem, selectNoneItem, selectInvertItem, advSelectItem, advUnselectItem, toolStripSeparator12, selectByExtItem, selectByNameItem });
			editMenuItem.Name = "editMenuItem";
			editMenuItem.Size = new Size(57, 20);
			editMenuItem.Text = "편집(&E)";
			// 
			// copyItem
			// 
			copyItem.Name = "copyItem";
			copyItem.ShortcutKeys = Keys.Control | Keys.C;
			copyItem.Size = new Size(271, 22);
			copyItem.Text = "복사(&C)";
			// 
			// pasteItem
			// 
			pasteItem.Name = "pasteItem";
			pasteItem.ShortcutKeys = Keys.Control | Keys.V;
			pasteItem.Size = new Size(271, 22);
			pasteItem.Text = "붙여넣기(&V)";
			// 
			// cutItem
			// 
			cutItem.Name = "cutItem";
			cutItem.ShortcutKeys = Keys.Control | Keys.X;
			cutItem.Size = new Size(271, 22);
			cutItem.Text = "잘라내기(&X)";
			// 
			// toolStripSeparator10
			// 
			toolStripSeparator10.Name = "toolStripSeparator10";
			toolStripSeparator10.Size = new Size(268, 6);
			// 
			// fullPathItem
			// 
			fullPathItem.Name = "fullPathItem";
			fullPathItem.ShortcutKeys = Keys.Control | Keys.Alt | Keys.F;
			fullPathItem.Size = new Size(271, 22);
			fullPathItem.Text = "전체 경로 이름 복사(&L)";
			// 
			// filenameOnlyItem
			// 
			filenameOnlyItem.Name = "filenameOnlyItem";
			filenameOnlyItem.ShortcutKeys = Keys.Control | Keys.Alt | Keys.A;
			filenameOnlyItem.Size = new Size(271, 22);
			filenameOnlyItem.Text = "파일 이름만 복사(&A)";
			// 
			// directoryOnlyItem
			// 
			directoryOnlyItem.Name = "directoryOnlyItem";
			directoryOnlyItem.ShortcutKeys = Keys.Control | Keys.Alt | Keys.R;
			directoryOnlyItem.Size = new Size(271, 22);
			directoryOnlyItem.Text = "경로 이름만 복사(&R)";
			// 
			// toolStripSeparator11
			// 
			toolStripSeparator11.Name = "toolStripSeparator11";
			toolStripSeparator11.Size = new Size(268, 6);
			// 
			// selectCurrentItem
			// 
			selectCurrentItem.Name = "selectCurrentItem";
			selectCurrentItem.Size = new Size(271, 22);
			selectCurrentItem.Text = "선택/해제(&B)";
			// 
			// selectAllItem
			// 
			selectAllItem.Name = "selectAllItem";
			selectAllItem.ShortcutKeys = Keys.Control | Keys.U;
			selectAllItem.Size = new Size(271, 22);
			selectAllItem.Text = "전체 선택(&U)";
			// 
			// selectNoneItem
			// 
			selectNoneItem.Name = "selectNoneItem";
			selectNoneItem.ShortcutKeys = Keys.Control | Keys.Shift | Keys.U;
			selectNoneItem.Size = new Size(271, 22);
			selectNoneItem.Text = "선택 해제(&N)";
			// 
			// selectInvertItem
			// 
			selectInvertItem.Name = "selectInvertItem";
			selectInvertItem.ShortcutKeys = Keys.Control | Keys.Shift | Keys.I;
			selectInvertItem.Size = new Size(271, 22);
			selectInvertItem.Text = "선택 반전(&I)";
			// 
			// advSelectItem
			// 
			advSelectItem.Name = "advSelectItem";
			advSelectItem.ShortcutKeys = Keys.Control | Keys.D;
			advSelectItem.Size = new Size(271, 22);
			advSelectItem.Text = "고급 선택(&G)";
			// 
			// advUnselectItem
			// 
			advUnselectItem.Name = "advUnselectItem";
			advUnselectItem.ShortcutKeys = Keys.Control | Keys.Shift | Keys.D;
			advUnselectItem.Size = new Size(271, 22);
			advUnselectItem.Text = "고급 해제(&W)";
			// 
			// toolStripSeparator12
			// 
			toolStripSeparator12.Name = "toolStripSeparator12";
			toolStripSeparator12.Size = new Size(268, 6);
			// 
			// selectByExtItem
			// 
			selectByExtItem.Name = "selectByExtItem";
			selectByExtItem.ShortcutKeys = Keys.Control | Keys.Alt | Keys.E;
			selectByExtItem.Size = new Size(271, 22);
			selectByExtItem.Text = "같은 확장자 모두 선택(&E)";
			// 
			// selectByNameItem
			// 
			selectByNameItem.Name = "selectByNameItem";
			selectByNameItem.ShortcutKeys = Keys.Control | Keys.Alt | Keys.N;
			selectByNameItem.Size = new Size(271, 22);
			selectByNameItem.Text = "같은 이름 모두 선택(&N)";
			// 
			// viewMenuItem
			// 
			viewMenuItem.DropDownItems.AddRange(new ToolStripItem[] { toggleToolbarItem, toggleFuncBarItem, toolStripSeparator13, toggleDirectoryInfoItem, toggleFileInfoItem, toolStripSeparator14, viewListViewTypeMenuItem, viewColumnTypeMenuItem, toolStripSeparator15, sortItem, viewSelectMenuItem, toolStripSeparator16, toggleHiddenItem, toolStripSeparator17, newTabItem, tabListItem });
			viewMenuItem.Name = "viewMenuItem";
			viewMenuItem.Size = new Size(59, 20);
			viewMenuItem.Text = "보기(&V)";
			// 
			// toggleToolbarItem
			// 
			toggleToolbarItem.Name = "toggleToolbarItem";
			toggleToolbarItem.Size = new Size(260, 22);
			toggleToolbarItem.Text = "도구 모음(&B)";
			// 
			// toggleFuncBarItem
			// 
			toggleFuncBarItem.Name = "toggleFuncBarItem";
			toggleFuncBarItem.Size = new Size(260, 22);
			toggleFuncBarItem.Text = "기능키 표시줄(&C)";
			// 
			// toolStripSeparator13
			// 
			toolStripSeparator13.Name = "toolStripSeparator13";
			toolStripSeparator13.Size = new Size(257, 6);
			// 
			// toggleDirectoryInfoItem
			// 
			toggleDirectoryInfoItem.Name = "toggleDirectoryInfoItem";
			toggleDirectoryInfoItem.Size = new Size(260, 22);
			toggleDirectoryInfoItem.Text = "디렉토리/드라이브 정보 표시줄(&D)";
			// 
			// toggleFileInfoItem
			// 
			toggleFileInfoItem.Name = "toggleFileInfoItem";
			toggleFileInfoItem.Size = new Size(260, 22);
			toggleFileInfoItem.Text = "파일 정보 표시줄(&F)";
			// 
			// toolStripSeparator14
			// 
			toolStripSeparator14.Name = "toolStripSeparator14";
			toolStripSeparator14.Size = new Size(257, 6);
			// 
			// viewListViewTypeMenuItem
			// 
			viewListViewTypeMenuItem.Name = "viewListViewTypeMenuItem";
			viewListViewTypeMenuItem.Size = new Size(260, 22);
			viewListViewTypeMenuItem.Text = "목록 보기 방식(&S)";
			// 
			// viewColumnTypeMenuItem
			// 
			viewColumnTypeMenuItem.Name = "viewColumnTypeMenuItem";
			viewColumnTypeMenuItem.Size = new Size(260, 22);
			viewColumnTypeMenuItem.Text = "열 보기 방식(&L)";
			// 
			// toolStripSeparator15
			// 
			toolStripSeparator15.Name = "toolStripSeparator15";
			toolStripSeparator15.Size = new Size(257, 6);
			// 
			// sortItem
			// 
			sortItem.DropDownItems.AddRange(new ToolStripItem[] { sortNoneItem, toolStripSeparator18, sortNameItem, sortExtItem, sortSizeItem, sortTimeItem, sortAttrItem, toolStripSeparator19, toggleSortDescItem });
			sortItem.Name = "sortItem";
			sortItem.Size = new Size(260, 22);
			sortItem.Text = "정렬(&A)";
			// 
			// sortNoneItem
			// 
			sortNoneItem.Name = "sortNoneItem";
			sortNoneItem.ShortcutKeys = Keys.Control | Keys.Shift | Keys.O;
			sortNoneItem.Size = new Size(238, 22);
			sortNoneItem.Text = "정렬 안함(&O)";
			// 
			// toolStripSeparator18
			// 
			toolStripSeparator18.Name = "toolStripSeparator18";
			toolStripSeparator18.Size = new Size(235, 6);
			// 
			// sortNameItem
			// 
			sortNameItem.Name = "sortNameItem";
			sortNameItem.ShortcutKeys = Keys.Control | Keys.Shift | Keys.N;
			sortNameItem.Size = new Size(238, 22);
			sortNameItem.Text = "이름으로(&N)";
			// 
			// sortExtItem
			// 
			sortExtItem.Name = "sortExtItem";
			sortExtItem.ShortcutKeys = Keys.Control | Keys.Shift | Keys.E;
			sortExtItem.Size = new Size(238, 22);
			sortExtItem.Text = "확장자로(&E)";
			// 
			// sortSizeItem
			// 
			sortSizeItem.Name = "sortSizeItem";
			sortSizeItem.ShortcutKeys = Keys.Control | Keys.Shift | Keys.S;
			sortSizeItem.Size = new Size(238, 22);
			sortSizeItem.Text = "크기로(&S)";
			// 
			// sortTimeItem
			// 
			sortTimeItem.Name = "sortTimeItem";
			sortTimeItem.ShortcutKeys = Keys.Control | Keys.Shift | Keys.T;
			sortTimeItem.Size = new Size(238, 22);
			sortTimeItem.Text = "날짜/시간으로(&T)";
			// 
			// sortAttrItem
			// 
			sortAttrItem.Name = "sortAttrItem";
			sortAttrItem.ShortcutKeys = Keys.Control | Keys.Shift | Keys.R;
			sortAttrItem.Size = new Size(238, 22);
			sortAttrItem.Text = "속성으로(&R)";
			// 
			// toolStripSeparator19
			// 
			toolStripSeparator19.Name = "toolStripSeparator19";
			toolStripSeparator19.Size = new Size(235, 6);
			// 
			// toggleSortDescItem
			// 
			toggleSortDescItem.Name = "toggleSortDescItem";
			toggleSortDescItem.ShortcutKeys = Keys.Control | Keys.Shift | Keys.D;
			toggleSortDescItem.Size = new Size(238, 22);
			toggleSortDescItem.Text = "내림차순(&D)";
			// 
			// viewSelectMenuItem
			// 
			viewSelectMenuItem.Name = "viewSelectMenuItem";
			viewSelectMenuItem.Size = new Size(260, 22);
			viewSelectMenuItem.Text = "골라보기(&F)";
			// 
			// toolStripSeparator16
			// 
			toolStripSeparator16.Name = "toolStripSeparator16";
			toolStripSeparator16.Size = new Size(257, 6);
			// 
			// toggleHiddenItem
			// 
			toggleHiddenItem.Name = "toggleHiddenItem";
			toggleHiddenItem.ShortcutKeys = Keys.Alt | Keys.Z;
			toggleHiddenItem.Size = new Size(260, 22);
			toggleHiddenItem.Text = "숨김 파일 보기(&Z)";
			// 
			// toolStripSeparator17
			// 
			toolStripSeparator17.Name = "toolStripSeparator17";
			toolStripSeparator17.Size = new Size(257, 6);
			// 
			// newTabItem
			// 
			newTabItem.Name = "newTabItem";
			newTabItem.ShortcutKeys = Keys.Control | Keys.T;
			newTabItem.Size = new Size(260, 22);
			newTabItem.Text = "새 탭(&T)";
			// 
			// tabListItem
			// 
			tabListItem.Name = "tabListItem";
			tabListItem.Size = new Size(260, 22);
			tabListItem.Text = "탭 목록(&Y)";
			// 
			// systemMenuItem
			// 
			systemMenuItem.Name = "systemMenuItem";
			systemMenuItem.Size = new Size(70, 20);
			systemMenuItem.Text = "시스템(&S)";
			// 
			// toolMenuItem
			// 
			toolMenuItem.DropDownItems.AddRange(new ToolStripItem[] { toolSettingMenuItem });
			toolMenuItem.Name = "toolMenuItem";
			toolMenuItem.Size = new Size(57, 20);
			toolMenuItem.Text = "도구(&T)";
			// 
			// toolSettingMenuItem
			// 
			toolSettingMenuItem.Name = "toolSettingMenuItem";
			toolSettingMenuItem.ShortcutKeys = Keys.Control | Keys.F12;
			toolSettingMenuItem.Size = new Size(167, 22);
			toolSettingMenuItem.Text = "설정(&S)";
			// 
			// helpMenuItem
			// 
			helpMenuItem.DropDownItems.AddRange(new ToolStripItem[] { aboutItem });
			helpMenuItem.Name = "helpMenuItem";
			helpMenuItem.Size = new Size(72, 20);
			helpMenuItem.Text = "도움말(&H)";
			// 
			// aboutItem
			// 
			aboutItem.Name = "aboutItem";
			aboutItem.Size = new Size(166, 22);
			aboutItem.Text = "두파일은 뭘까(&A)";
			// 
			// toolStrip
			// 
			toolStrip.Location = new Point(0, 24);
			toolStrip.Name = "toolStrip";
			toolStrip.Size = new Size(800, 25);
			toolStrip.TabIndex = 1;
			toolStrip.Text = "toolStrip";
			toolStrip.Visible = false;
			// 
			// leftPanel
			// 
			leftPanel.BackColor = Color.FromArgb(37, 37, 37);
			leftPanel.ForeColor = Color.FromArgb(241, 241, 241);
			leftPanel.Location = new Point(0, 24);
			leftPanel.Name = "leftPanel";
			leftPanel.Size = new Size(378, 401);
			leftPanel.TabIndex = 2;
			// 
			// verticalBar
			// 
			verticalBar.BackColor = Color.FromArgb(37, 37, 37);
			verticalBar.ForeColor = Color.FromArgb(241, 241, 241);
			verticalBar.Location = new Point(384, 24);
			verticalBar.Name = "verticalBar";
			verticalBar.Size = new Size(20, 401);
			verticalBar.TabIndex = 3;
			// 
			// rightPanel
			// 
			rightPanel.BackColor = Color.FromArgb(37, 37, 37);
			rightPanel.ForeColor = Color.FromArgb(241, 241, 241);
			rightPanel.Location = new Point(420, 24);
			rightPanel.Name = "rightPanel";
			rightPanel.Size = new Size(380, 401);
			rightPanel.TabIndex = 4;
			// 
			// funcBar
			// 
			funcBar.BackColor = Color.FromArgb(63, 63, 70);
			funcBar.Dock = DockStyle.Bottom;
			funcBar.ForeColor = Color.FromArgb(241, 241, 241);
			funcBar.Location = new Point(0, 425);
			funcBar.Name = "funcBar";
			funcBar.Size = new Size(800, 25);
			funcBar.TabIndex = 5;
			// 
			// MainForm
			// 
			AllowDrop = true;
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(800, 450);
			Controls.Add(funcBar);
			Controls.Add(rightPanel);
			Controls.Add(verticalBar);
			Controls.Add(leftPanel);
			Controls.Add(toolStrip);
			Controls.Add(menuStrip);
			KeyPreview = true;
			MainMenuStrip = menuStrip;
			MinimumSize = new Size(800, 450);
			Name = "MainForm";
			Text = "두파일";
			KeyDown += MainForm_KeyDown;
			menuStrip.ResumeLayout(false);
			menuStrip.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private MenuStrip menuStrip;
		private ToolStripMenuItem fileMenuItem;
		private ToolStripMenuItem directoryMenuItem;
		private ToolStripMenuItem editMenuItem;
		private ToolStripMenuItem viewMenuItem;
		private ToolStripMenuItem systemMenuItem;
		private ToolStripMenuItem toolMenuItem;
		private ToolStripMenuItem helpMenuItem;
		private ToolStripMenuItem openItem;
		private ToolStripMenuItem openWithItem;
		private ToolStripMenuItem openAlllItem;
		private ToolStripSeparator toolStripSeparator1;
		private ToolStripMenuItem trashItem;
		private ToolStripMenuItem removeItem;
		private ToolStripSeparator toolStripSeparator2;
		private ToolStripMenuItem renameItem;
		private ToolStripMenuItem advRenameItem;
		private ToolStripSeparator toolStripSeparator3;
		private ToolStripMenuItem attrItem;
		private ToolStripMenuItem setAttrItem;
		private ToolStripSeparator toolStripSeparator4;
		private ToolStripMenuItem newEmptyItem;
		private ToolStripMenuItem newShortcutItem;
		private ToolStripSeparator toolStripSeparator5;
		private ToolStripMenuItem detemineExtensionItem;
		private ToolStripMenuItem checksumItem;
		private ToolStripMenuItem checksumCrcItem;
		private ToolStripMenuItem checksumMd5Item;
		private ToolStripMenuItem checksumSha1Item;
		private ToolStripSeparator toolStripSeparator6;
		private ToolStripMenuItem exitItem;
		private ToolStripMenuItem calcSizeItem;
		private ToolStripMenuItem dirSelectMenuItem;
		private ToolStripMenuItem dirFavorityMenuItem;
		private ToolStripSeparator toolStripSeparator7;
		private ToolStripMenuItem dirNewMenuItem;
		private ToolStripSeparator toolStripSeparator8;
		private ToolStripMenuItem dirHistoryBackMenuItem;
		private ToolStripMenuItem dirHistoryForwardMenuItem;
		private ToolStripMenuItem dirParentMenuItem;
		private ToolStripMenuItem dirTopMenuItem;
		private ToolStripSeparator toolStripSeparator9;
		private ToolStripMenuItem dirSetCustomMenuItem;
		private ToolStripMenuItem setCustom1MenuItem;
		private ToolStripMenuItem setCustom2MenuItem;
		private ToolStripMenuItem setCustom3MenuItem;
		private ToolStripMenuItem setCustom4MenuItem;
		private ToolStripMenuItem setCustom5MenuItem;
		private ToolStripMenuItem dirGoCustomMenuItem;
		private ToolStripMenuItem custom1MenuItem;
		private ToolStripMenuItem custom2MenuItem;
		private ToolStripMenuItem custom3MenuItem;
		private ToolStripMenuItem custom4MenuItem;
		private ToolStripMenuItem custom5MenuItem;
		private ToolStripMenuItem copyItem;
		private ToolStripMenuItem pasteItem;
		private ToolStripMenuItem cutItem;
		private ToolStripSeparator toolStripSeparator10;
		private ToolStripMenuItem fullPathItem;
		private ToolStripMenuItem filenameOnlyItem;
		private ToolStripMenuItem directoryOnlyItem;
		private ToolStripSeparator toolStripSeparator11;
		private ToolStripMenuItem selectCurrentItem;
		private ToolStripMenuItem selectAllItem;
		private ToolStripMenuItem selectNoneItem;
		private ToolStripMenuItem selectInvertItem;
		private ToolStripMenuItem advSelectItem;
		private ToolStripMenuItem advUnselectItem;
		private ToolStripSeparator toolStripSeparator12;
		private ToolStripMenuItem selectByExtItem;
		private ToolStripMenuItem selectByNameItem;
		private ToolStripMenuItem toggleToolbarItem;
		private ToolStripMenuItem toggleFuncBarItem;
		private ToolStripSeparator toolStripSeparator13;
		private ToolStripMenuItem toggleDirectoryInfoItem;
		private ToolStripMenuItem toggleFileInfoItem;
		private ToolStripSeparator toolStripSeparator14;
		private ToolStripMenuItem viewListViewTypeMenuItem;
		private ToolStripMenuItem viewColumnTypeMenuItem;
		private ToolStripSeparator toolStripSeparator15;
		private ToolStripMenuItem sortItem;
		private ToolStripMenuItem viewSelectMenuItem;
		private ToolStripSeparator toolStripSeparator16;
		private ToolStripMenuItem toggleHiddenItem;
		private ToolStripSeparator toolStripSeparator17;
		private ToolStripMenuItem newTabItem;
		private ToolStripMenuItem tabListItem;
		private ToolStripMenuItem sortNoneItem;
		private ToolStripSeparator toolStripSeparator18;
		private ToolStripMenuItem sortNameItem;
		private ToolStripMenuItem sortExtItem;
		private ToolStripMenuItem sortSizeItem;
		private ToolStripMenuItem sortTimeItem;
		private ToolStripMenuItem sortAttrItem;
		private ToolStripSeparator toolStripSeparator19;
		private ToolStripMenuItem toggleSortDescItem;
		private ToolStripMenuItem toolSettingMenuItem;
		private ToolStripMenuItem aboutItem;
		private ToolStrip toolStrip;
		private Windows.FilePanel leftPanel;
		private Windows.VerticalBar verticalBar;
		private Windows.FilePanel rightPanel;
		private Windows.FuncBar funcBar;
	}
}
