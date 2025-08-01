﻿namespace DuFile.Dowa;

// 보조키
[Flags]
internal enum ModifierKey
{
	None = 0,
	Shift = 1,
	Control = 2,
	Alt = 4,
}

internal enum WatcherCommand
{
	Created,
	Deleted,
	Changed,
	Renamed,
}

internal enum OverwriteBy
{
	None,
	Skip,
	Always,
	Newer,
	Rename,
}