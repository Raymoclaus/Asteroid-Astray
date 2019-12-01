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
		public EntityProfile[] speakers;
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
			if (conversation == null)
			{
				conversation = new DialogueTextEvent[lines.Length];
				previousConversation = conversation;
			}

			int previousLength = previousConversation.Length;

			for (int i = 0; i < lines.Length; i++)
			{
				string[] line = lines[i].Split('|');
				byte speaker = previousLength <= i ? (byte)0 : previousConversation[i].speakerID;
				float delay = previousLength <= i ? 0f : previousConversation[i].delay;
				float revealSpeed = previousLength <= i ? 1f : previousConversation[i].characterRevealSpeedMultiplier;
				conversation[i] = new DialogueTextEvent();
				if (line.Length > 1 && byte.TryParse(line[0], out speaker))
				{
					conversation[i].speakerID = speaker;
					conversation[i].delay = delay;
					conversation[i].characterRevealSpeedMultiplier = revealSpeed;
					conversation[i].line = line[1];
				}
				else
				{
					conversation[i].line = DialogueTextEvent.DEFAULT_LINE;
				}
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