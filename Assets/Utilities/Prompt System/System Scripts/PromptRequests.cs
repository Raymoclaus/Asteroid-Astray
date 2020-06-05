﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace PromptSystem
{
	public static class PromptRequests
	{
		public static event Action<PromptRequestData> OnPromptSendRequestReceived;
		public static event Action<PromptRequestData> OnPromptRemovalRequestReceived;
		public static event Action<PromptRequestData> OnLatestPromptSelected;
		public static event Action<PromptRequestData, string> OnPromptUpdated;
		private static List<PromptRequestData> promptRequests
			= new List<PromptRequestData>();

		public static void PromptSendRequest(string key, string promptText)
		{
			int index = FindMatchingRequestIndex(key);
			if (index >= 0 && index < promptRequests.Count)
			{
				PromptRequestData matchingRequest = FindMatchingRequest(index);
				string matchingKey = matchingRequest.key;
				PromptRequestData updatedRequest = new PromptRequestData(
					matchingKey, promptText);
				promptRequests[index] = updatedRequest;
				OnPromptUpdated?.Invoke(updatedRequest, promptText);
				return;
			}

			PromptRequestData request = new PromptRequestData(key, promptText);
			if (request.IsInvalid) return;

			promptRequests.Add(request);
			OnPromptSendRequestReceived?.Invoke(request);
			OnLatestPromptSelected?.Invoke(request);
		}

		public static void PromptRemovalRequest(string key)
		{
			PromptRequestData request = FindMatchingRequest(key);
			if (request.IsInvalid) return;

			promptRequests.Remove(request);
			OnPromptRemovalRequestReceived?.Invoke(request);

			if (promptRequests.Count > 0)
			{
				PromptRequestData lastPromptRequest = promptRequests.Last();
				OnLatestPromptSelected?.Invoke(request);
			}
			else
			{
				OnLatestPromptSelected?.Invoke(PromptRequestData.Invalid);
			}
		}

		private static int FindMatchingRequestIndex(string key)
		{
			for (int i = 0; i < promptRequests.Count; i++)
			{
				if (promptRequests[i].key == key) return i;
			}
			return -1;
		}

		private static PromptRequestData FindMatchingRequest(string key)
		{
			int index = FindMatchingRequestIndex(key);
			return FindMatchingRequest(index);
		}

		private static PromptRequestData FindMatchingRequest(int index)
		{
			if (index < 0 || index >= promptRequests.Count) return PromptRequestData.Invalid;
			return promptRequests[index];
		}
	}

}