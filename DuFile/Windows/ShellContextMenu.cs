using System.Runtime.InteropServices;
using System.Text;

namespace DuFile.Windows;

internal class ShellContextMenu
{
	// Win32/COM 관련 선언
#pragma warning disable SYSLIB1054
	[DllImport("user32.dll", SetLastError = true)]
    private static extern int TrackPopupMenu(IntPtr hMenu, uint uFlags, int x, int y, int nReserved, IntPtr hwnd, IntPtr lptpm);

	[DllImport("user32.dll")]
	public static extern IntPtr CreatePopupMenu();

	[DllImport("user32.dll")]
    private static extern bool DestroyMenu(IntPtr hMenu);

    [DllImport("shell32.dll")]
    private static extern int SHParseDisplayName([MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr pbc, out IntPtr ppidl, uint sfgaoIn, out uint psfgaoOut);

    [DllImport("shell32.dll")]
    private static extern int SHGetDesktopFolder(out IShellFolder ppshf);
#pragma warning restore SYSLIB1054

#pragma warning disable SYSLIB1096
	[ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("000214E6-0000-0000-C000-000000000046")]
    private interface IShellFolder
    {
        void ParseDisplayName(IntPtr hwnd, IntPtr pbc, [MarshalAs(UnmanagedType.LPWStr)] string pszDisplayName, ref uint pchEaten, out IntPtr ppidl, ref uint pdwAttributes);
        void EnumObjects(IntPtr hwnd, int grfFlags, out IntPtr ppenumIdList);
        void BindToObject(IntPtr pidl, IntPtr pbc, ref Guid riid, out IntPtr ppv);
        void BindToStorage(IntPtr pidl, IntPtr pbc, ref Guid riid, out IntPtr ppv);
        void CompareIDs(int lParam, IntPtr pidl1, IntPtr pidl2);
        void CreateViewObject(IntPtr hwndOwner, ref Guid riid, out IntPtr ppv);
        void GetAttributesOf(int cidl, IntPtr apidl, ref uint rgfInOut);
        void GetUIObjectOf(IntPtr hwndOwner, int cidl, IntPtr apidl, ref Guid riid, IntPtr rgfReserved, out IntPtr ppv);
        void GetDisplayNameOf(IntPtr pidl, uint uFlags, out IntPtr pName);
        void SetNameOf(IntPtr hwnd, IntPtr pidl, [MarshalAs(UnmanagedType.LPWStr)] string pszName, uint uFlags, out IntPtr ppidlOut);
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("000214E4-0000-0000-C000-000000000046")]
    private interface IContextMenu
    {
        void QueryContextMenu(IntPtr hMenu, uint indexMenu, uint idCmdFirst, uint idCmdLast, uint uFlags);
        void InvokeCommand(ref CmInvokeCommandInfoEx pici);
        void GetCommandString(UIntPtr idCmd, uint uFlags, IntPtr pReserved, [MarshalAs(UnmanagedType.LPStr)] StringBuilder pszName, uint cchMax);
    }
#pragma warning restore SYSLIB1096

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct CmInvokeCommandInfoEx
    {
        public int cbSize;
        public int fMask;
        public IntPtr hwnd;
        public IntPtr lpVerb;
        public IntPtr lpParameters;
        public IntPtr lpDirectory;
        public int nShow;
        public int dwHotKey;
        public IntPtr hIcon;
        public IntPtr lpTitle;
        public IntPtr lpVerbW;
        public IntPtr lpParametersW;
        public IntPtr lpDirectoryW;
        public IntPtr lpTitleW;
        public int ptInvoke_x;
        public int ptInvoke_y;
    }

    private const uint CMF_NORMAL = 0x00000000;
    //private const uint GCS_VERBW = 0x00000004;      // 명령의 Unicode(와이드) 버전 문자열을 요청할 때 사용
	private const int SW_SHOWNORMAL = 1;
    private const uint TPM_RETURNCMD = 0x0100;

    public static void Show(IWin32Window owner, Point screenPos, IList<string> filePaths)
    {
        if (filePaths.Count == 0)
            return;

		// 1. 데스크탑 폴더 가져오기
		var hr = SHGetDesktopFolder(out var desktopFolder);
		if (hr != 0)
			return; // 또는 예외 처리

		// 2. 파일 경로를 PIDL로 변환
		var pidls = new List<IntPtr>();
        foreach (var file in filePaths)
        {
            hr = SHParseDisplayName(file, IntPtr.Zero, out var pidl, 0, out _);
            if (hr == 0 && pidl != IntPtr.Zero)
	            pidls.Add(pidl);
		}
        if (pidls.Count == 0)
            return;

		// 3. IContextMenu 인터페이스를 가져오기 위해 PIDL 배열을 사용
		var apidl = Marshal.AllocCoTaskMem(IntPtr.Size * pidls.Count);
        for (var i = 0; i < pidls.Count; i++)
            Marshal.WriteIntPtr(apidl, i * IntPtr.Size, pidls[i]);
        var iidIContextMenu = typeof(IContextMenu).GUID;
        desktopFolder.GetUIObjectOf(IntPtr.Zero, pidls.Count, apidl, ref iidIContextMenu, IntPtr.Zero, out var contextMenuPtr);
        Marshal.FreeCoTaskMem(apidl);
        if (contextMenuPtr == IntPtr.Zero)
            return;
        var contextMenu = (IContextMenu)Marshal.GetObjectForIUnknown(contextMenuPtr);

		// 4. 팝업 메뉴 생성
		var hMenu = CreatePopupMenu();
        contextMenu.QueryContextMenu(hMenu, 0, 1, 0x7FFF, CMF_NORMAL);

		// 5. TrackPopupMenu를 사용하여 메뉴 표시
		var cmd = TrackPopupMenu(hMenu, TPM_RETURNCMD, screenPos.X, screenPos.Y, 0, owner.Handle, IntPtr.Zero);
        if (cmd > 0)
        {
            var invoke = new CmInvokeCommandInfoEx
            {
                cbSize = Marshal.SizeOf<CmInvokeCommandInfoEx>(),
                fMask = 0,
                hwnd = owner.Handle,
                lpVerb = cmd - 1,
                nShow = SW_SHOWNORMAL
            };
            contextMenu.InvokeCommand(ref invoke);
        }
        DestroyMenu(hMenu);
        Marshal.ReleaseComObject(contextMenu);
        foreach (var pidl in pidls)
            Marshal.FreeCoTaskMem(pidl);
    }
}
