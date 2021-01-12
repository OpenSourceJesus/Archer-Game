using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Extensions
{
	public static class StringExtensions
	{
		public static string SubstringStartEnd (this string str, int startIndex, int endIndex)
		{
			return str.Substring(startIndex, endIndex - startIndex);
		}
		
		public static string RemoveEach (this string str, string remove)
		{
			return str.Replace(remove, "");
		}

		public static string StartAt (this string str, string startAt)
		{
			return str.Substring(str.IndexOf(startAt));
		}
		
		public static string StartAfter (this string str, string startAfter)
		{
			return str.Substring(str.IndexOf(startAfter) + startAfter.Length);
		}

		public static int IndexOf_StartAfter (this string str, string find, int startIndex)
		{
			return str.Substring(startIndex).IndexOf(find) + startIndex;
		}

		public static int IndexOf_StartAfter (this string str, string find, string startAfter)
		{
			return str.StartAfter(startAfter).IndexOf(find) + str.IndexOf(startAfter) + startAfter.Length;
		}

		public static string RemoveStartEnd (this string str, int startIndex, int endIndex)
		{
			return str.Remove(startIndex, endIndex - startIndex);
		}

		public static string RemoveBetween (this string str, string startString, string endString)
		{
			string output = str;
			int indexOfStartString = str.IndexOf(startString);
			if (indexOfStartString != -1)
			{
				string startOfStr = str.Substring(0, indexOfStartString);
				str = str.Substring(indexOfStartString + startString.Length);
				output = startOfStr + str.RemoveStartEnd(0, str.IndexOf(endString) + endString.Length);
			}
			return output;
		}
	}
}