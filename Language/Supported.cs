using System.Collections.Generic;

namespace Ei_Dimension.Language
{
	public static class Supported
	{
		public static List<(string, string)> Languages { get; } = new List<(string, string)> { ("English", "en-US"),
	  ("中文", "zh-CN") };//, ("Русский", "ru-Ru"), ("עִברִית", "he-IL") };
	}
}
