global using System;
global using System.Windows.Forms;
global using System.Drawing;
global using System.ComponentModel;
global using System.IO;
global using System.Linq;
global using DuFile.Dowa;

namespace DuFile;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        Settings.Instance.Load();
		Application.Run(new MainForm());
        Settings.Instance.Save();
	}
}
