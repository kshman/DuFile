namespace DuFile.Command;

internal class Definition
{
	public const string None = "#None";
	public const string Rename = "#Rename";
	public const string View = "#View";
	public const string Edit = "#Edit";
	public const string Copy = "#Copy";
	public const string Move = "#Move";
	public const string NewDirectory = "#NewDirectory";
	public const string Delete = "#Delete";
	public const string Console = "#Console";
	public const string Help = "#Help";
	public const string Properties = "#Properties";
	public const string Open= "#Open";
	public const string OpenWith = "#OpenWith";
	public const string Menu = "#Menu";
	public const string Trash = "#Trash";
	public const string AdvancedRename = "#AdvancedRename";
	public const string Refresh = "#Refresh";
	public const string SelectAll = "#SelectAll";
	public const string SelectInvert = "#SelectInvert";
	public const string SelectNone = "#SelectNone";	
	public const string SortByName = "#SortByName";
	public const string SortByExtension = "#SortByExtension";
	public const string SortBySize = "#SortBySize";
	public const string SortByTime = "#SortByTime";
	public const string SortByAttribute = "#SortByAttribute";
	public const string SystemDocument = "#SystemDocument";
	public const string SystemPicture = "#SystemPicture";
	public const string SystemVideo = "#SystemVideo";
	public const string SystemAudio = "#SystemAudio";
	public const string SystemDownload = "#SystemDownload";

	private static readonly Dictionary<string, string> _cmdMap = new()
	{
		{ None, "없음" },
		{ Rename, "이름 바꾸기" },
		{ View, "보기" },
		{ Edit, "편집" },
		{ Copy, "복사" },
		{ Move, "이동" },
		{ NewDirectory, "새 폴더" },
		{ Delete, "삭제" },
		{ Console, "콘솔" },
		{ Help, "도움말" },
		{ Properties, "속성" },
		{ Open, "열기" },
		{ OpenWith, "다른 프로그램으로 열기" },
		{ Menu, "메뉴" },
		{ Trash, "휴지통" },
		{ AdvancedRename, "고급 이름 바꾸기" },
		{ Refresh, "새로 고침" },
		{ SelectAll, "전체 선택" },
		{ SelectInvert, "선택 반전" },
		{ SelectNone, "선택 해제" },
		{ SortByName, "이름순 정렬" },
		{ SortByExtension, "확장자순 정렬" },
		{ SortBySize, "크기순 정렬" },
		{ SortByTime, "시간순 정렬" },
		{ SortByAttribute, "속성순 정렬" },
		{ SystemDocument, "문서 폴더" },
		{ SystemPicture, "사진 폴더" },
		{ SystemVideo, "동영상 폴더" },
		{ SystemAudio, "음악 폴더" },
		{ SystemDownload, "다운로드 폴더" },
	};

	public static string FriendlyName(string command)
	{
		if (_cmdMap.TryGetValue(command, out var friendlyName))
			return friendlyName;
		return command;
	}
}
