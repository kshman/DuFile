using Microsoft.Data.Sqlite;
using System.Globalization;

namespace DuFile;

internal class Settings
{
	private static Settings? _instance;
	private readonly string _connectionString;
	private readonly Dictionary<string, string> _cache = [];

	public Theme Theme { get; } = new();

	public static Settings Instance => _instance ??= new Settings();

	private Settings()
	{
		if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
		{
			_connectionString = string.Empty;
			return;
		}

		var appPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ksh");
		if (!Directory.Exists(appPath))
			Directory.CreateDirectory(appPath);
		var configPath = Path.Combine(appPath, "DuFile.config");
		_connectionString = $"Data Source={configPath}";

		InitializeDatabase();
	}

	private void InitializeDatabase()
	{
		using var conn = new SqliteConnection(_connectionString);
		conn.Open();

		var cmd = conn.CreateCommand();
		cmd.CommandText = @"
			CREATE TABLE IF NOT EXISTS Settings (
				id INTEGER PRIMARY KEY AUTOINCREMENT,
                key TEXT UNIQUE NOT NULL,
                value TEXT NOT NULL
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
		cmd.CommandText = "SELECT key, value FROM Settings";

		using var rdr = cmd.ExecuteReader();
		while (rdr.Read())
			_cache[rdr.GetString(0)] = rdr.GetString(1);
	}

	public void Save()
	{
		if (string.IsNullOrEmpty(_connectionString))
			return;

		using var conn = new SqliteConnection(_connectionString);
		conn.Open();

		using var transaction = conn.BeginTransaction();

		var cmd = conn.CreateCommand();
		cmd.CommandText = "INSERT OR REPLACE INTO Settings (key, value) VALUES (@key, @value)";

		foreach (var kvp in _cache)
		{
			cmd.Parameters.Clear();
			cmd.Parameters.AddWithValue("@key", kvp.Key);
			cmd.Parameters.AddWithValue("@value", kvp.Value);
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

	public Size WindowSize
	{
		get => new(GetInt("WindowWidth", 1200), GetInt("WindowHeight", 800));
		set { SetInt("WindowWidth", value.Width); SetInt("WindowHeight", value.Height); }
	}

	public Point WindowLocation
	{
		get => new(GetInt("WindowX", 100), GetInt("WindowY", 100));
		set { SetInt("WindowX", value.X); SetInt("WindowY", value.Y); }
	}

	public bool WindowMaximized
	{
		get => GetBool("WindowMaximized");
		set => SetBool("WindowMaximized", value);
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

	public string UiFontFamily
	{
		get => GetString("UiFontFamily", "Microsoft Sans Serif");
		set => SetString("UiFontFamily", value);
	}

	public float UiFontSize
	{
		get => GetFloat("UiFontSize", 10f);
		set => SetFloat("UiFontSize", value);
	}

	public string FileFontFamily
	{
		get => GetString("FileFontFamily", "Microsoft Sans Serif");
		set => SetString("FileFontFamily", value);
	}

	public float FileFontSize
	{
		get => GetFloat("FileFontSize", 11f);
		set => SetFloat("FileFontSize", value);
	}

	public int SortOrder
	{
		get => GetInt("SortOrder"); // 0: 정렬 안함, 1: 이름, 2: 확장자, 3: 크기, 4: 시간, 5: 속성, 6: 색상
		set => SetInt("SortOrder", value);
	}

	public bool SortDescending
	{
		get => GetBool("SortDescending");
		set => SetBool("SortDescending", value);
	}

	public bool ShowHiddenFiles
	{
		get => GetBool("ShowHiddenFiles");
		set => SetBool("ShowHiddenFiles", value);
	}

	public string GetFuncKeyAction(int functionKey)
	{
		return GetString($"ShortcutF{functionKey}", GetDefaultFuncKeyAction(functionKey));
	}

	public void SetFuncKeyAction(int functionKey, string action)
	{
		SetString($"ShortcutF{functionKey}", action);
	}

	private static string GetDefaultFuncKeyAction(int functionKey) => functionKey switch
	{
		1 => "#Help",
		2 => "#Rename",
		3 => "#View",
		4 => "#Edit",
		5 => "#Copy",
		6 => "#Move",
		7 => "#NewFolder",
		8 => "#Delete",
		9 => "#Settings",
		_ => string.Empty
	};

	public int ActivePanel
	{
		get => GetInt("ActivePanel", 1); // 1: left, 2: right
		set => SetInt("ActivePanel", value);
	}

	public int LeftActiveTab
	{
		get => GetInt("Panel1Active");
		set => SetInt("Panel1Active", value);
	}

	public int RightActiveTab
	{
		get => GetInt("Panel2Active");
		set => SetInt("Panel2Active", value);
	}

	public void SetLeftTabs(IEnumerable<string> tabs) =>
		SetString("Panel1Tabs", string.Join("|", tabs));

	public void SetRightTabs(IEnumerable<string> tabs) =>
		SetString("Panel2Tabs", string.Join("|", tabs));

	public IEnumerable<string> GetLeftTabs()
	{
		var tabs = GetString("Panel1Tabs", string.Empty);
		return string.IsNullOrEmpty(tabs) ? [] : tabs.Split('|', StringSplitOptions.RemoveEmptyEntries);
	}

	public IEnumerable<string> GetRightTabs()
	{
		var tabs = GetString("Panel2Tabs", string.Empty);
		return string.IsNullOrEmpty(tabs) ? [] : tabs.Split('|', StringSplitOptions.RemoveEmptyEntries);
	}
}
