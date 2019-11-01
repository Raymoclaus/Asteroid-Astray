using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TextMeshProUtilities
{
	public static class TmpTeleType
	{
		private static List<TextMeshCoroutine> coroutines = new List<TextMeshCoroutine>();

		public static void Type(MonoBehaviour mono, TextMeshProUGUI textMesh,
			WaitForSeconds timeBetweenStrokes, Action onType, Action onFinishTyping)
		{
			if (IsTyping(textMesh)) return;
			Coroutine coro = mono.StartCoroutine(Typing(textMesh, timeBetweenStrokes, onType, onFinishTyping));
			coroutines.Add(new TextMeshCoroutine(textMesh, coro, mono));
		}

		public static void Type(MonoBehaviour mono, TextMeshProUGUI textMesh,
			WaitForSecondsRealtime timeBetweenStrokes, Action onType, Action onFinishTyping)
		{
			if (IsTyping(textMesh)) return;
			Coroutine coro = mono.StartCoroutine(Typing(textMesh, timeBetweenStrokes, onType, onFinishTyping));
			coroutines.Add(new TextMeshCoroutine(textMesh, coro, mono));
		}

		public static void RevealAllCharacters(TextMeshProUGUI textMesh)
		{
			textMesh.maxVisibleCharacters = textMesh.textInfo.characterCount;
			StopTyping(textMesh);
		}

		public static bool IsTyping(TextMeshProUGUI textMesh)
		{
			for (int i = 0; i < coroutines.Count; i++)
			{
				if (coroutines[i].textMesh == textMesh) return true;
			}
			return false;
		}

		private static IEnumerator Typing(TextMeshProUGUI textMesh,
			WaitForSeconds timeBetweenStrokes, Action onType, Action onFinishTyping)
		{
			textMesh.ForceMeshUpdate();
			textMesh.enableWordWrapping = true;

			int totalVisibleCharacters = textMesh.textInfo.characterCount;
			int counter = 0;
			int visibleCount = 0;

			while (visibleCount < totalVisibleCharacters)
			{
				visibleCount = counter % (totalVisibleCharacters + 1);
				textMesh.maxVisibleCharacters = visibleCount;
				counter += 1;
				onType?.Invoke();
				yield return timeBetweenStrokes;
			}

			RevealAllCharacters(textMesh);
			onFinishTyping?.Invoke();
		}

		private static IEnumerator Typing(TextMeshProUGUI textMesh,
			WaitForSecondsRealtime timeBetweenStrokes, Action onType, Action onFinishTyping)
		{
			textMesh.ForceMeshUpdate();
			textMesh.enableWordWrapping = true;

			int totalVisibleCharacters = textMesh.textInfo.characterCount;
			int counter = 0;
			int visibleCount = 0;

			while (visibleCount < totalVisibleCharacters)
			{
				visibleCount = counter % (totalVisibleCharacters + 1);
				textMesh.maxVisibleCharacters = visibleCount;
				counter += 1;
				onType?.Invoke();
				yield return timeBetweenStrokes;
			}

			RevealAllCharacters(textMesh);
			onFinishTyping?.Invoke();
			onFinishTyping = null;
		}

		private static void StopTyping(TextMeshProUGUI textMesh)
		{
			for (int i = 0; i < coroutines.Count; i++)
			{
				if (coroutines[i].textMesh == textMesh)
				{
					coroutines[i].mono.StopCoroutine(coroutines[i].coroutine);
					coroutines.RemoveAt(i);
					return;
				}
			}
		}

		private struct TextMeshCoroutine
		{
			public TextMeshProUGUI textMesh;
			public Coroutine coroutine;
			public MonoBehaviour mono;

			public TextMeshCoroutine(TextMeshProUGUI textMesh, Coroutine coroutine, MonoBehaviour mono)
			{
				this.textMesh = textMesh;
				this.coroutine = coroutine;
				this.mono = mono;
			}
		}
	}
}