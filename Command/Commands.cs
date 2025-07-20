namespace DuFile.Command;

internal class Commands
{
	public const string None = "#없어요";
	public const string Exit = "#끝내기";
	public const string Rename = "#이름바꾸기";
	public const string View = "#보기";
	public const string Edit = "#편집";
	public const string Copy = "#복사";
	public const string Move = "#이동";
	public const string NewDirectory = "#새디렉토리";
	public const string NewEmptyFile = "#새빈파일";
	public const string NewShortcutDesktop = "#새바로가기";
	public const string Delete = "#지우기";
	public const string Console = "#콘솔";
	public const string Help = "#도움말";
	public const string Properties = "#속성";
	public const string ChangeProperties = "#속성바꾸기";
	public const string Open= "#열기";
	public const string OpenWith = "#인수로열기";
	public const string OpenAll = "#모두열기";
	public const string Menu = "#메뉴";
	public const string Trash = "#휴지통으로";
	public const string AdvancedRename = "#확장이름바꾸기";
	public const string Refresh = "#새로고침";
	public const string CalcSelectedDirectory = "#선택한디렉토리계산";
	public const string DetemineExtension = "#확장자쓰임새확인";
	public const string ChecksumCrc = "#CRC체크섬";
	public const string ChecksumMd5 = "#MD5체크섬";
	public const string ChecksumSha1 = "#Sha1체크섬";
	public const string SelectAll = "#모두선택";
	public const string SelectInvert = "#선택반전";
	public const string SelectNone = "#선택해제";	
	public const string SortByName = "#이름으로정렬";
	public const string SortByExtension = "#확장자로정렬";
	public const string SortBySize = "#크기로정렬";
	public const string SortByTime = "#일시로정렬";
	public const string SortByAttribute = "#속성으로정렬";
	public const string UserDocument = "#사용자문서";
	public const string UserPicture = "#사용자그림";
	public const string UserVideo = "#사용자비디오";
	public const string UserAudio = "#사용자오디오";
	public const string UserDownload = "#사용자다운로드";

	private static readonly Dictionary<string, string> FriendlyMap = new()
	{
		{ None, "" },
		{ Exit, "끝내기" },
		{ Rename, "이름 바꾸기" },
		{ View, "보기" },
		{ Edit, "편집" },
		{ Copy, "복사" },
		{ Move, "이동" },
		{ NewDirectory, "새 폴더" },
		{ NewEmptyFile, "새 빈 파일"},
		{ NewShortcutDesktop, "바탕화면 바로가기 만들기" },
		{ Delete, "삭제" },
		{ Console, "콘솔" },
		{ Help, "도움말" },
		{ Properties, "속성" },
		{ ChangeProperties, "속성 변경" },
		{ Open, "열기" },
		{ OpenWith, "인수와 함께 열기" },
		{ OpenAll, "모두 열기" },
		{ Menu, "메뉴" },
		{ Trash, "휴지통으로" },
		{ AdvancedRename, "고급 이름 바꾸기" },
		{ Refresh, "새로 고침" },
		{ CalcSelectedDirectory, "선택한 디렉토리 크기 계산"},
		{ DetemineExtension, "확장자가 뭔지 검색" },
		{ ChecksumCrc, "CRC 체크섬" },
		{ ChecksumMd5, "MD5 체크섬" },
		{ ChecksumSha1, "SHA1 체크섬" },
		{ SelectAll, "전체 선택" },
		{ SelectInvert, "선택 반전" },
		{ SelectNone, "선택 해제" },
		{ SortByName, "이름으로 정렬" },
		{ SortByExtension, "확장자로 정렬" },
		{ SortBySize, "크기로 정렬" },
		{ SortByTime, "날짜/시간으로 정렬" },
		{ SortByAttribute, "속성으로 정렬" },
		{ UserDocument, "문서 폴더" },
		{ UserPicture, "사진 폴더" },
		{ UserVideo, "동영상 폴더" },
		{ UserAudio, "음악 폴더" },
		{ UserDownload, "다운로드 폴더" },
	};

	public static string FriendlyName(string command) =>
		FriendlyMap.GetValueOrDefault(command, command);
}
