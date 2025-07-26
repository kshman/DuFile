using System.Runtime.InteropServices;
using System.Text;
// ReSharper disable InconsistentNaming

namespace DuFile.Windows;

internal static class ShellContext
{
	public static void ShowMenu(IWin32Window owner, Point screenPos, IList<string> files)
	{
		if (files.Count == 0)
			return;

		if (SHGetDesktopFolder(out var desktopFolder) != 0)
			return;

		IShellFolder? parentFolder = null;
		IContextMenu? contextMenu = null;
		var pidlParent = nint.Zero;
		var pidlItems = new List<nint>();
		uint chEaten = 0, dwAttributes = 0;

		try
		{
			var isDrive = false;
			var dirName= files[0];
			if (dirName.EndsWith(@":\"))
			{
				// 드라이브
				if (files.Count > 1)
					return; // 드라이브는 하나만 선택 가능
				isDrive = true;
				dirName = @"::{20D04FE0-3AEA-1069-A2D8-08002B30309D}";
			}
			else
			{
				// 일반 폴더 또는 파일
				dirName = Path.GetDirectoryName(dirName);
				if (string.IsNullOrEmpty(dirName))
					return;
			}

			desktopFolder.ParseDisplayName(nint.Zero, nint.Zero, dirName, ref chEaten, out pidlParent, ref dwAttributes);
			if (pidlParent == nint.Zero)
				return;

			var iidIShellFolder = typeof(IShellFolder).GUID;
			desktopFolder.BindToObject(pidlParent, nint.Zero, ref iidIShellFolder, out parentFolder);

			if (isDrive)
			{
				parentFolder.ParseDisplayName(nint.Zero, nint.Zero, files[0], ref chEaten, out var pidl, ref dwAttributes);
				if (pidl != nint.Zero)
					pidlItems.Add(pidl);
			}
			else
			{
				foreach (var file in files)
				{
					var fileName = Path.GetFileName(file);
					parentFolder.ParseDisplayName(nint.Zero, nint.Zero, fileName, ref chEaten, out var pidl, ref dwAttributes);
					if (pidl != nint.Zero)
						pidlItems.Add(pidl);
				}
			}
			if (pidlItems.Count == 0)
				return;

			var apidls = pidlItems.ToArray();
			var iidIContextMenu = typeof(IContextMenu).GUID;
			parentFolder.GetUIObjectOf(nint.Zero, pidlItems.Count, apidls, ref iidIContextMenu, nint.Zero, out contextMenu);

			var hMenu = CreatePopupMenu();
			if (hMenu != nint.Zero)
			{
				contextMenu.QueryContextMenu(hMenu, 0, 1, 0x7FFF, CMF_NORMAL);
				var cmd = TrackPopupMenuEx(hMenu, TPM_RETURNCMD | TPM_RIGHTBUTTON, screenPos.X, screenPos.Y, owner.Handle, nint.Zero);
				if (cmd > 0)
				{
					var cmi = new CMINVOKECOMMANDINFOEX
					{
						cbSize = Marshal.SizeOf<CMINVOKECOMMANDINFOEX>(),
						fMask = 0,
						hwnd = owner.Handle,
						lpVerb = cmd - 1,
						nShow = SW_SHOWNORMAL
					};
					contextMenu.InvokeCommand(ref cmi);
				}
				DestroyMenu(hMenu);
			}
		}
		finally
		{
			foreach (var pidl in pidlItems)
				Marshal.FreeCoTaskMem(pidl);
			if (pidlParent != nint.Zero) Marshal.FreeCoTaskMem(pidlParent);
			if (contextMenu != null) Marshal.ReleaseComObject(contextMenu);
			if (parentFolder != null) Marshal.ReleaseComObject(parentFolder);
			Marshal.ReleaseComObject(desktopFolder);
		}
	}

	public static void ShowProperties(IWin32Window owner, string fileName)
	{
		if (fileName.EndsWith(@":\"))
		{
			ShowDriveProperties(owner, fileName);
			return;
		}

		var dir = Path.GetDirectoryName(fileName);
		var name = Path.GetFileName(fileName);
		if (string.IsNullOrEmpty(dir) || string.IsNullOrEmpty(name))
			return;

		if (SHGetDesktopFolder(out var desktopFolder) != 0)
			return;

		IShellFolder? parentFolder = null;
		var pidlParent = nint.Zero;
		var pidlItem = nint.Zero;
		uint chEaten = 0, dwAttributes = 0;

		try
		{
			desktopFolder.ParseDisplayName(nint.Zero, nint.Zero, dir, ref chEaten, out pidlParent, ref dwAttributes);
			if (pidlParent == nint.Zero)
				return;

			var iidIShellFolder = typeof(IShellFolder).GUID;
			desktopFolder.BindToObject(pidlParent, nint.Zero, ref iidIShellFolder, out parentFolder);

			parentFolder.ParseDisplayName(nint.Zero, nint.Zero, name, ref chEaten, out pidlItem, ref dwAttributes);
			if (pidlItem == nint.Zero)
				return;

			var sei = new SHELLEXECUTEINFO
			{
				cbSize = Marshal.SizeOf<SHELLEXECUTEINFO>(),
				fMask = SEE_MASK_INVOKEIDLIST,
				hwnd = owner.Handle,
				lpVerb = "properties",
				lpFile = null,
				lpParameters = null,
				lpDirectory = null,
				nShow = SW_SHOWNORMAL,
				lpIDList = pidlItem
			};
			ShellExecuteEx(ref sei);
		}
		finally
		{
			if (pidlItem != nint.Zero) Marshal.FreeCoTaskMem(pidlItem);
			if (pidlParent != nint.Zero) Marshal.FreeCoTaskMem(pidlParent);
			if (parentFolder != null) Marshal.ReleaseComObject(parentFolder);
			Marshal.ReleaseComObject(desktopFolder);
		}
	}

	private static void ShowDriveProperties(IWin32Window owner, string driveName)
	{
		if (SHGetDesktopFolder(out var desktopFolder) != 0)
			return;

		var pidlDrive = nint.Zero;
		uint chEaten = 0, dwAttributes = 0;

		try
		{
			desktopFolder.ParseDisplayName(nint.Zero, nint.Zero, driveName, ref chEaten, out pidlDrive, ref dwAttributes);
			if (pidlDrive == nint.Zero)
				return;

			var sei = new SHELLEXECUTEINFO
			{
				cbSize = Marshal.SizeOf<SHELLEXECUTEINFO>(),
				fMask = SEE_MASK_INVOKEIDLIST,
				hwnd = owner.Handle,
				lpVerb = "properties",
				lpFile = null,
				lpParameters = null,
				lpDirectory = null,
				nShow = SW_SHOWNORMAL,
				lpIDList = pidlDrive
			};
			ShellExecuteEx(ref sei);
		}
		finally
		{
			if (pidlDrive != nint.Zero) Marshal.FreeCoTaskMem(pidlDrive);
			Marshal.ReleaseComObject(desktopFolder);
		}
	}

	// Win32/COM 관련 선언
#pragma warning disable SYSLIB1054
	[DllImport("shell32.dll")]
	private static extern int SHGetDesktopFolder(out IShellFolder ppshf);

	[DllImport("user32.dll")]
	public static extern nint CreatePopupMenu();

	[DllImport("user32.dll")]
	private static extern bool DestroyMenu(nint hMenu);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern int TrackPopupMenuEx(nint hmenu, uint flags, int x, int y, nint hwnd, nint lptpm);

	[DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern bool ShellExecuteEx(ref SHELLEXECUTEINFO lpExecInfo);
#pragma warning restore SYSLIB1054

#pragma warning disable SYSLIB1096
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("000214E6-0000-0000-C000-000000000046")]
	private interface IShellFolder
	{
		void ParseDisplayName(nint hwnd, nint pbc, [MarshalAs(UnmanagedType.LPWStr)] string pszDisplayName, ref uint pchEaten, out nint ppidl, ref uint pdwAttributes);
		void EnumObjects(); // not used
		void BindToObject(nint pidl, nint pbc, [In] ref Guid riid, out IShellFolder ppv);
		void BindToStorage(); // not used
		void CompareIDs(); // not used
		void CreateViewObject(); // not used
		void GetAttributesOf(); // not used
		void GetUIObjectOf(nint hwndOwner, int cidl, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] nint[] apidl, [In] ref Guid riid, nint rgfReserved, out IContextMenu ppv);
		void GetDisplayNameOf(IntPtr pidl, uint uFlags, out IntPtr pName);
		void SetNameOf(IntPtr hwnd, IntPtr pidl, [MarshalAs(UnmanagedType.LPWStr)] string pszName, uint uFlags, out IntPtr ppidlOut);
	}

	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("000214E4-0000-0000-C000-000000000046")]
	private interface IContextMenu
	{
		void QueryContextMenu(nint hMenu, uint indexMenu, uint idCmdFirst, uint idCmdLast, uint uFlags);
		void InvokeCommand(ref CMINVOKECOMMANDINFOEX pici);
		void GetCommandString(UIntPtr idCmd, uint uFlags, IntPtr pReserved, [MarshalAs(UnmanagedType.LPStr)] StringBuilder pszName, uint cchMax);
	}
#pragma warning restore SYSLIB1096

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct CMINVOKECOMMANDINFOEX
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

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct SHELLEXECUTEINFO
	{
		public int cbSize;
		public uint fMask;
		public IntPtr hwnd;
		[MarshalAs(UnmanagedType.LPWStr)] public string? lpVerb;
		[MarshalAs(UnmanagedType.LPWStr)] public string? lpFile;
		[MarshalAs(UnmanagedType.LPWStr)] public string? lpParameters;
		[MarshalAs(UnmanagedType.LPWStr)] public string? lpDirectory;
		public int nShow;
		public IntPtr hInstApp;
		public IntPtr lpIDList;
		[MarshalAs(UnmanagedType.LPWStr)] public string? lpClass;
		public IntPtr hkeyClass;
		public uint dwHotKey;
		public IntPtr hIcon;
		public IntPtr hProcess;
	}

	private const int SW_SHOWNORMAL = 1;
	private const int SEE_MASK_INVOKEIDLIST = 0x0000000C;
	private const uint CMF_NORMAL = 0x00000000;
	private const uint TPM_RETURNCMD = 0x0100;
	private const uint TPM_RIGHTBUTTON = 0x0002;
}
