using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace DialogueSystem
{
	[CreateAssetMenu(menuName = "Scriptable Objects/Dialogue/Basic Conversation")]
	public class ConversationEvent : ScriptableObject
	{
		[SerializeField] private TextAsset conversationFile;
		public CharacterProfile[] speakers;
		public DialogueTextEvent[] conversation;

		public virtual ConversationEventPosition GetNextConversation()
		{
			return null;
		}

		public virtual string[] GetLines()
		{
			if (conversation.Length == 0) return new string[] { DialogueTextEvent.DEFAULT_LINE };

			string[] lines = new string[conversation.Length];
			for (int i = 0; i < lines.Length; i++)
			{
				lines[i] = conversation[i].line;
			}
			return lines;
		}

		public void Load()
		{
			string convo = conversationFile.text;
			string[] lines = convo.Split('\n');
			DialogueTextEvent[] previousConversation = conversation;
			int fileLineCount = LineCountFromFile;
			if (conversation == null || conversation.Length != fileLineCount)
			{
				conversation = new DialogueTextEvent[fileLineCount];
				previousConversation = conversation;
			}

			int previousLength = previousConversation.Length;
			int lineCounter = 0;
			for (int i = 0; lineCounter < fileLineCount; i++)
			{
				string[] line = lines[i].Split('|');
				byte speaker = (previousLength <= lineCounter || previousConversation[lineCounter] == null)
					? (byte)0
					: previousConversation[lineCounter].speakerID;
				float delay = (previousLength <= lineCounter || previousConversation[lineCounter] == null)
					? 0f
					: previousConversation[lineCounter].delay;
				float revealSpeed = (previousLength <= lineCounter || previousConversation[lineCounter] == null)
					? 1f
					: previousConversation[lineCounter].characterRevealSpeedMultiplier;
				if (line.Length > 1 && byte.TryParse(line[0], out speaker))
				{
					conversation[lineCounter] = new DialogueTextEvent();
					conversation[lineCounter].speakerID = speaker;
					conversation[lineCounter].delay = delay;
					conversation[lineCounter].characterRevealSpeedMultiplier = revealSpeed;
					conversation[lineCounter].line = line[1];
					lineCounter++;
				}
				//check for comment
				else if (line.Length == 1 && line[0].Length > 0 && line[0][0] == '#')
				{

				}
				//check for invalid line
				else
				{
					conversation[lineCounter] = new DialogueTextEvent();
					conversation[lineCounter].line = DialogueTextEvent.DEFAULT_LINE;
					lineCounter++;
				}
			}
		}

		/// <summary>
		/// Returns number of lines in text file. Excludes blank and comment lines.
		/// </summary>
		private int LineCountFromFile
		{
			get
			{
				string[] lines = conversationFile.text.Split('\n');
				int counter = 0;
				for (int i = 0; i < lines.Length; i++)
				{
					string line = lines[i];
					if (line.Length == 0 || line[0] == '#') continue;
					counter++;
				}

				return counter;
			}
		}

#if UNITY_EDITOR
		public void Save()
		{
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < conversation.Length; i++)
			{
				sb.Append($"{conversation[i].speakerID}|{conversation[i].line}{(i == conversation.Length - 1 ? string.Empty : "\n")}");
			}
			File.WriteAllText(AssetDatabase.GetAssetPath(conversationFile), sb.ToString());
			EditorUtility.SetDirty(conversationFile);
		}
#endif
	}
}