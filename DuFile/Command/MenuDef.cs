namespace DuFile.Command;

internal struct MenuDef
{
	public string? Text { get; set; }
	public string Command { get; set; }
	public string? Shortcut { get; set; }
	public string? Icon { get; set; }
	public bool Disable { get; set; }
	public string? Variable { get; set; }
	public MenuDef[]? SubMenus { get; set; }
}
