﻿using System.Text;

namespace DuFile.Dowa;

internal static class Alter
{
	private static readonly string[] SizeSuffixes = ["B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"];

	// 크기를 사람이 읽을 수 있는 형식으로 변환
	public static string FormatFileSize(this long bytes)
	{
		double len = bytes;
		var order = 0;
		while (len >= 1024 && order < SizeSuffixes.Length - 1)
		{
			order++;
			len /= 1024;
		}
		return $"{len:0.##} {SizeSuffixes[order]}";
	}

	// 파일 속성을 사람이 읽을 수 있는 형식으로 변환
	public static string FormatString(this FileAttributes attributes)
	{
		var sb = new StringBuilder();
		sb.Append(attributes.HasFlag(FileAttributes.Archive) ? 'A' : '_');
		sb.Append(attributes.HasFlag(FileAttributes.Hidden) ? 'H' : '_');
		sb.Append(attributes.HasFlag(FileAttributes.ReadOnly) ? 'R' : '_');
		sb.Append(attributes.HasFlag(FileAttributes.System) ? 'S' : '_');
		return sb.ToString();
	}

	// 날짜를 오늘 기준 30일 내 상대 문구로 변환
	public static string FormatRelativeDate(this DateTime date)
	{
		var now = DateTime.Now;
		var diff = now - date;
		return diff.TotalDays switch
		{
			< 1 => "오늘",
			< 2 => "어제",
			< 3 => "그저께",
			< 30 => $"{(int)diff.TotalDays}일 전",
			_ => date.ToString("yyyy-MM-dd")
		};
	}

	// 색상 혼합(알파 블렌딩)
	public static Color BlendColor(Color c1, Color c2, float factor)
	{
		return Color.FromArgb(
			(int)(c1.R + (c2.R - c1.R) * factor),
			(int)(c1.G + (c2.G - c1.G) * factor),
			(int)(c1.B + (c2.B - c1.B) * factor)
		);
	}

	// 키 입력 문자열을 키 조합으로 변환
	public static Keys ParseKeyString(KeysConverter converter, string input)
	{
		try
		{
			var keys = (Keys)(converter.ConvertFromString(input) ?? Keys.None);
			return keys;
		}
		catch
		{
			Debugs.WriteLine($"잘못된 키 변환: {input}");
			return Keys.None;
		}
	}

	// 문자열 분리기
	public static string[] SplitWithSeparator(this string input, char separator = ',')
	{
		// var tagList = tags.Split('|').Where(s => !string.IsNullOrEmpty(s))
		return string.IsNullOrWhiteSpace(input) ? 
			[] : 
			input.Split(separator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
	}
}
