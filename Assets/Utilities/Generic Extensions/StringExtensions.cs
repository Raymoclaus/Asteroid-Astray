using System;
using System.Text.RegularExpressions;
using UnityEngine;

public static class StringExtensions
{
	public static int CountExcludingRichTextTags(this string source)
		=> source.RemoveRichTextTags().Length;

	public static string RemoveRichTextTags(this string source)
		=> Regex.Replace(source, "<.*?>", string.Empty);

	public static int StringOccurrenceCount(this string source, string pattern)
	{
		// Loop through all instances of the string 'text'.
		int count = 0;
		int i = 0;
		while ((i = source.IndexOf(pattern, i)) != -1)
		{
			i += pattern.Length;
			count++;
		}
		return count;
	}

	/// <summary>
	/// Parses a string to a Vector3. Expected input example: (-9.7, -42.3, 0.0)
	/// </summary>
	/// <param name="s"></param>
	/// <param name="vector"></param>
	/// <returns>vector becomes Vector3.zero if parsing fails.</returns>
	public static bool TryParseToVector3(this string s, out Vector3 vector)
	{
		// Remove the parentheses and spaces
		s = s.Replace("(", string.Empty).Replace(")", string.Empty).Replace(" ", string.Empty);

		// split the items
		string[] parts = s.Split(',');

		// parse strings
		try
		{
			vector = Vector3.zero;
			vector.x = float.Parse(parts[0]);
			vector.y = float.Parse(parts[1]);
			vector.z = float.Parse(parts[2]);
			return true;
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			vector = Vector3.zero;
			return false;
		}
	}
}
