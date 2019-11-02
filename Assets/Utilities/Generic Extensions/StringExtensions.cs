using System.Text.RegularExpressions;

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
}
