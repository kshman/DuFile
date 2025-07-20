global using System;
global using System.Windows.Forms;
global using System.Drawing;
global using System.ComponentModel;
global using System.IO;
global using System.Linq;
global using DuFile.Command;
global using DuFile.Dowa;
global using DebugOut = System.Diagnostics.Debug;
global using Resources = DuFile.Properties.Resources;

namespace DuFile;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        Settings.Instance.Load();
        // To customize application configuration such as set high DPI settings or default font,
		// see https://aka.ms/applicationconfiguration.
		ApplicationConfiguration.Initialize();
		Application.Run(new Windows.MainForm());
        Settings.Instance.Save();
	}
}
