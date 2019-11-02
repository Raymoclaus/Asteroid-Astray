using GenericExtensions;
using System.Collections.Generic;

namespace InputHandlerSystem
{
	public static class StringExtensions
	{
		public static int CountAfterActionTagFormatting(this string source, bool excludeRichTextTags)
		{
			int count = 0;
			List<string> actions = InputManager.GetCurrentActions();
			for (int i = 0; i < actions?.Count; i++)
			{
				string action = actions[i];
				int actionLength = action.Length;
				int bindingCount = InputManager.GetBinding(action).ValidCombinationCount;
				string v1 = $"[{action}]";
				string v2 = $"[:{action}]";
				string v3 = $"[{action}:]";

				int v1Count = source.StringOccurrenceCount(v1);
				count += v1Count * actionLength + v1Count * bindingCount;
				int v2Count = source.StringOccurrenceCount(v2);
				count += v2Count * actionLength;
				int v3Count = source.StringOccurrenceCount(v3);
				count += v3Count * bindingCount;
			}

			for (int i = 0; i < source.Length; i++)
			{
				char c = source[i];
				if (c == '[')
				{
					int end = source.IndexOf(']', i);
					if (end != -1)
					{
						i = end + 1;
					}
				}

				if (excludeRichTextTags && c == '<')
				{
					int end = source.IndexOf('>', i);
					if (end != -1)
					{
						i = end + 1;
					}
				}

				if (i < source.Length)
				{
					count++;
				}
			}

			return count;
		}
	}
}
