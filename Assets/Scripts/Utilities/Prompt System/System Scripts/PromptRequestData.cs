namespace PromptSystem
{
	public struct PromptRequestData
	{
		public string key, promptText;

		public static PromptRequestData Invalid => new PromptRequestData(null, null);

		public PromptRequestData(string key, string promptText)
		{
			this.key = key;
			this.promptText = promptText;
		}

		public bool IsInvalid
			=> string.IsNullOrEmpty(key) || string.IsNullOrEmpty(promptText);
	}
}