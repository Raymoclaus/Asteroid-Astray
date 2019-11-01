using System.Text.RegularExpressions;

public static class StringExtensions
{
	public static int CountExcludingRichTextTags(this string source)
		=> source.RemoveRichTextTags().Length;

	public static string RemoveRichTextTags(this string source)
		=> Regex.Replace(source, "<.*?>", string.Empty);
}
