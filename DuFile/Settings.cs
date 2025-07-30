using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.Sqlite;
using System.Globalization;

namespace DuFile;

internal class Settings
{
	private static Settings? _instance;
	private string _connectionString = string.Empty;

	private readonly Dictionary<string, string> _cache = [];
	private readonly Dictionary<string, string> _historyDrive = [];
	private readonly List<string> _historyConsole = [];

	public Theme Theme { get; } = new();

	public static Settings Instance => _instance ??= new Settings();

	private static string InitialStartFolder => 
		Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
	private static string LocalApplicationDataFolder =>
		Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

	private Settings()
	{
		// 여기서 초기화를 했더니 디자이너가 맘대로 설정 파일을 갖고 논다
	}

	public void Initialize(bool initializeDatabase)
	{
		var appPath = Path.Combine(LocalApplicationDataFolder, "ksh");
		if (!Directory.Exists(appPath))
			Directory.CreateDirectory(appPath);
		var configPath = Path.Combine(appPath, "DuFile.conf");
		_connectionString = $"Data Source={configPath}";

		if (initializeDatabase)
			InitializeDatabase();
	}

	private void InitializeDatabase()
	{
		using var conn = new SqliteConnection(_connectionString);
		conn.Open();

		var cmd = conn.CreateCommand();

		// 설정
		cmd.CommandText = @"
			CREATE TABLE IF NOT EXISTS Settings (
                key TEXT UNIQUE PRIMARY KEY NOT NULL,
                value TEXT NOT NULL
            )";
		cmd.ExecuteNonQuery();

		// 확장자 색상
		cmd.CommandText = @"
			CREATE TABLE IF NOT EXISTS ColorExtension (
				ext TEXT UNIQUE PRIMARY KEY NOT NULL,
				color TEXT NOT NULL
			)";
		cmd.ExecuteNonQuery();

		// 크기 색상
		cmd.CommandText = @"
			CREATE TABLE IF NOT EXISTS ColorSize (
				size TEXT UNIQUE PRIMARY KEY NOT NULL,
				color TEXT NOT NULL
			)";
		cmd.ExecuteNonQuery();

		// 드라이브 이력
		cmd.CommandText = @"
			CREATE TABLE IF NOT EXISTS HistoryDrive (
				drive TEXT UNIQUE PRIMARY KEY NOT NULL,
				path TEXT NOT NULL
			)";
		cmd.ExecuteNonQuery();

		// 콘솔 실행 이력
		cmd.CommandText = @"
			CREATE TABLE IF NOT EXISTS HistoryConsole (
				command TEXT UNIQUE PRIMARY KEY NOT NULL
			)";
		cmd.ExecuteNonQuery();
	}

	public void Load()
	{
		if (string.IsNullOrEmpty(_connectionString))
			return;

		using var conn = new SqliteConnection(_connectionString);
		conn.Open();

		var cmd = conn.CreateCommand();

		// 설정
		cmd.CommandText = "SELECT key, value FROM Settings";
		using (var rdr = cmd.ExecuteReader())
		{
			while (rdr.Read())
				_cache[rdr.GetString(0)] = rdr.GetString(1);
		}

		// 확장자 색상
		cmd.CommandText = "SELECT ext, color FROM ColorExtension";
		using (var colorRdr = cmd.ExecuteReader())
		{
			while (colorRdr.Read())
			{
				var ext = colorRdr.GetString(0);
				if (uint.TryParse(colorRdr.GetString(1), NumberStyles.HexNumber, null, out var result))
					Theme.ColorExtension[ext] = Color.FromArgb(unchecked((int)(result | 0xFF000000)));
			}
		}

		// 크기 색상
		cmd.CommandText = "SELECT size, color FROM ColorSize";
		using (var sizeRdr = cmd.ExecuteReader())
		{
			while (sizeRdr.Read())
			{
				var size = sizeRdr.GetString(0);
				if (uint.TryParse(sizeRdr.GetString(1), NumberStyles.HexNumber, null, out var result))
					Theme.ColorSize[size] = Color.FromArgb(unchecked((int)(result | 0xFF000000)));
			}
		}

		// 드라이브 이력
		cmd.CommandText = "SELECT drive, path FROM HistoryDrive";
		using (var driveRdr = cmd.ExecuteReader())
		{
			while (driveRdr.Read())
			{
				var drive = driveRdr.GetString(0);
				var path = driveRdr.GetString(1);
				if (!string.IsNullOrEmpty(drive) && !string.IsNullOrEmpty(path))
					_historyDrive[drive] = path;
			}
		}

		// 콘솔 이력
		cmd.CommandText = "SELECT command FROM HistoryConsole";
		using (var consoleRdr = cmd.ExecuteReader())
		{
			while (consoleRdr.Read())
			{
				var command = consoleRdr.GetString(0);
				if (!string.IsNullOrEmpty(command))
					_historyConsole.Add(command);
			}
		}
	}

	public void Save()
	{
		if (string.IsNullOrEmpty(_connectionString))
			return;

		using var conn = new SqliteConnection(_connectionString);
		conn.Open();

		using var transaction = conn.BeginTransaction();
		var cmd = conn.CreateCommand();

		// 설정
		cmd.CommandText = "INSERT OR REPLACE INTO Settings (key, value) VALUES (@key, @value)";
		foreach (var kvp in _cache)
		{
			cmd.Parameters.Clear();
			cmd.Parameters.AddWithValue("@key", kvp.Key);
			cmd.Parameters.AddWithValue("@value", kvp.Value);
			cmd.ExecuteNonQuery();
		}

		// 드라이브 이력
		cmd.CommandText = "INSERT OR REPLACE INTO HistoryDrive (drive, path) VALUES (@drive, @path)";
		foreach (var kvp in _historyDrive)
		{
			cmd.Parameters.Clear();
			cmd.Parameters.AddWithValue("@drive", kvp.Key);
			cmd.Parameters.AddWithValue("@path", kvp.Value);
			cmd.ExecuteNonQuery();
		}

		// 콘솔 이력
		cmd.CommandText = "INSERT OR REPLACE INTO HistoryConsole (command) VALUES (@command)";
		foreach (var command in _historyConsole)
		{
			cmd.Parameters.Clear();
			cmd.Parameters.AddWithValue("@command", command);
			cmd.ExecuteNonQuery();
		}

		transaction.Commit();
	}

	/// <summary>
	/// 문자열 설정값 반환
	/// </summary>
	public string GetString(string key, string defaultValue = "") =>
		_cache.GetValueOrDefault(key, defaultValue);

	/// <summary>
	/// 문자열 설정값 저장
	/// </summary>
	public void SetString(string key, string value) =>
		_cache[key] = value;

	/// <summary>
	/// 정수형 설정값 반환
	/// </summary>
	public int GetInt(string key, int defaultValue = 0) =>
		_cache.TryGetValue(key, out var value) && int.TryParse(value, out var result) ? result : defaultValue;

	/// <summary>
	/// 정수형 설정값 저장
	/// </summary>
	public void SetInt(string key, int value) =>
		_cache[key] = value.ToString();

	/// <summary>
	/// bool 설정값 반환
	/// </summary>
	public bool GetBool(string key, bool defaultValue = false) =>
		_cache.TryGetValue(key, out var value) && bool.TryParse(value, out var result) ? result : defaultValue;

	/// <summary>
	/// bool 설정값 저장
	/// </summary>
	public void SetBool(string key, bool value) =>
		_cache[key] = value.ToString();

	public float GetFloat(string key, float defaultValue = 0f)
	{
		if (_cache.TryGetValue(key, out var value) && float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
			return result;
		return defaultValue;
	}

	public void SetFloat(string key, float value) =>
		_cache[key] = value.ToString(CultureInfo.InvariantCulture);

	/// <summary>
	/// Sets the specified configuration setting to the given value.
	/// </summary>
	/// <param name="key">The key identifying the configuration setting to be updated. Cannot be null or empty.</param>
	/// <param name="value">The value to assign to the configuration setting. Cannot be null.</param>
	public void SetSetting(string key, string value) =>
		SetString(key, value);

	public string StartFolder
	{
		get => GetString("StartFolder", InitialStartFolder);
		set => SetString("StartFolder", value);
	}

	public Size WindowSize
	{
		get => new(GetInt("WindowWidth", 1200), GetInt("WindowHeight", 800));
		set { SetInt("WindowWidth", value.Width); SetInt("WindowHeight", value.Height); }
	}

	public Point WindowLocation
	{
		get => new(GetInt("WindowX", -1), GetInt("WindowY", -1));
		set { SetInt("WindowX", value.X); SetInt("WindowY", value.Y); }
	}

	public bool WindowMaximized
	{
		get => GetBool("WindowMaximized");
		set => SetBool("WindowMaximized", value);
	}

	public bool ConfirmOnExit
	{
		get => GetBool("ConfirmOnExit");
		set => SetBool("ConfirmOnExit", value);
	}

	public bool ShowMenubar
	{
		get => GetBool("ShowMenubar", true);
		set => SetBool("ShowMenubar", value);
	}

	public bool ShowToolbar
	{
		get => GetBool("ShowToolbar");
		set => SetBool("ShowToolbar", value);
	}

	public bool ShowFuncBar
	{
		get => GetBool("ShowFuncBar", true);
		set => SetBool("ShowFuncBar", value);
	}

	public bool ShowGridLines
	{
		get => GetBool("ShowGridLines");
		set => SetBool("ShowGridLines", value);
	}

	public bool CharCase
	{
		get => GetBool("CharCase");
		set => SetBool("CharCase", value);
	}

	public int SortOrder
	{
		get => GetInt("SortOrder"); // 0: 이름, 1: 확장자, 2: 크기, 3: 시간, 4: 속성, 5: 색상
		set => SetInt("SortOrder", value);
	}

	public bool SortDescending
	{
		get => GetBool("SortDescending");
		set => SetBool("SortDescending", value);
	}

	public bool ShowHidden
	{
		get => GetBool("ShowHidden");
		set => SetBool("ShowHidden", value);
	}

	public string GetFuncKeyCommand(int functionKey, ModifierKey modifier)
	{
		return GetString($"Shortcut{(int)modifier}F{functionKey}", GetDefaultFuncKeyCommand(functionKey, modifier));
	}

	public void SetFuncKeyCommand(int functionKey, ModifierKey modifier, string command)
	{
		SetString($"Shortcut{(int)modifier}F{functionKey}", command);
	}

	[SuppressMessage("ReSharper", "RedundantSwitchExpressionArms")]
	private static string GetDefaultFuncKeyCommand(int functionKey, ModifierKey modifier) => modifier switch
	{
		ModifierKey.Control => functionKey switch
		{
			1 => Commands.Help,
			2 => Commands.AdvancedRename,
			3 => Commands.None,
			4 => Commands.None,
			5 => Commands.None,
			6 => Commands.None,
			7 => Commands.None,
			8 => Commands.None,
			9 => Commands.None,
			_ => Commands.None
		},
		ModifierKey.Shift => functionKey switch
		{
			1 => Commands.Help,
			2 => Commands.UserDocument,
			3 => Commands.UserDownload,
			4 => Commands.UserPicture,
			5 => Commands.UserAudio,
			6 => Commands.UserVideo,
			7 => Commands.None,
			8 => Commands.Delete,
			9 => Commands.None,
			_ => Commands.None
		},
		_ => functionKey switch
		{
			1 => Commands.Help,
			2 => Commands.Rename,
			3 => Commands.View,
			4 => Commands.Edit,
			5 => Commands.Copy,
			6 => Commands.Move,
			7 => Commands.NewFolder,
			8 => Commands.Trash,
			9 => Commands.Console,
			_ => Commands.None
		}
	};

	public int ActivePanel
	{
		get => GetInt("ActivePanel", 1); // 1: left, 2: right
		set => SetInt("ActivePanel", value);
	}

	public string GetDriveHistory(string drive) =>
		_historyDrive.GetValueOrDefault(drive, drive);

	public void SetDriveHistory(string drive, string path)
	{
		if (string.IsNullOrEmpty(drive) || string.IsNullOrEmpty(path))
			return;
		_historyDrive[drive] = path;
	}
}
