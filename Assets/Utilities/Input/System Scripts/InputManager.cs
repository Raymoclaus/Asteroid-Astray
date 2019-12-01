using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InputHandlerSystem
{
	public static class InputManager
	{
		//current input mode
		private static InputMode mode;
		//used to query the control scheme of a keyboard
		private static KeyboardInputHandler keyLayout = new KeyboardInputHandler();
		//used to query the control scheme of a PS4 controller
		private static Ps4InputHandler ps4Layout = new Ps4InputHandler();
		//store all inputHandlers in a list
		private static List<CustomInputHandler> inputHandlers
			= new List<CustomInputHandler>()
		{
			keyLayout, ps4Layout
		};

		private static InputContext[] contexts;
		private static InputContext[] Contexts
			=> contexts != null
				? contexts
				: (contexts = Resources.LoadAll<InputContext>("Input Contexts"));

		//keep track of current context
		private static InputContext currentContext;
		public static InputContext CurrentContext
		{
			get => currentContext;
			set
			{
				currentContext = value;
				Debug.Log($"Input context set to {currentContext.contextName}");
				OnContextChanged?.Invoke();
			}
		}

		public static event Action OnInputModeChanged, OnContextChanged;

		private static void CheckForModeUpdate()
		{
			InputMode prevMode = mode;

			//check current mode if input detected
			CustomInputHandler currentHandler = GetHandler();
			if (currentHandler?.ProcessInputs(CurrentContext) ?? false) return;

			//if no input detected in current, check other input methods for input
			for (int i = 0; i < inputHandlers.Count; i++)
			{
				if (inputHandlers[i] == currentHandler) continue;

				if (inputHandlers[i].ProcessInputs(CurrentContext))
				{
					mode = inputHandlers[i].GetInputMode();
					break;
				}
			}

			if (prevMode != mode)
			{
				Debug.LogWarning($"Input mode set to: {mode}");
				OnInputModeChanged?.Invoke();
			}
		}

		//returns the input status of a given command. 0f usually means no input.
		public static float GetInput(string key)
		{
			CheckForModeUpdate();
			return GetHandler()?.GetInput(key, CurrentContext) ?? 0f;
		}

		public static bool GetInputDown(string key)
		{
			CheckForModeUpdate();
			return GetHandler()?.GetInputDown(key, CurrentContext) ?? false;
		}

		public static bool GetInputUp(string key)
		{
			CheckForModeUpdate();
			return GetHandler()?.GetInputUp(key, CurrentContext) ?? false;
		}

		//returns whether the current context contains the given action
		private static bool IsValidKey(string key)
			=> CurrentContext?.IsValidAction(key) ?? false;

		//returns the input combination for a given action to occur
		public static ActionCombination GetBinding(string key)
		{
			CheckForModeUpdate();
			return GetHandler()?.GetBinding(key, CurrentContext);
		}

		//returns the angle in degrees that the shuttle should turn to
		public static float GetLookAngle(Vector2 refLocation)
		{
			CheckForModeUpdate();
			return GetHandler()?.GetLookAngle(refLocation) ?? 0f;
		}

		//returns the current input mode
		public static InputMode GetMode() => mode;

		private static CustomInputHandler GetHandler()
		{
			for (int i = 0; i < inputHandlers.Count; i++)
			{
				if (GetMode() == inputHandlers[i].GetInputMode()) return inputHandlers[i];
			}
			return null;
		}

		public static void SetContext(string contextName)
		{
			if (CurrentContext != null && CurrentContext.contextName == contextName) return;
			
			if (Contexts.Length == 0) return;

			for (int i = 0; i < Contexts.Length; i++)
			{
				if (string.Compare(Contexts[i].contextName.ToLower(), contextName.ToLower()) == 0)
				{
					CurrentContext = Contexts[i];
					return;
				}
			}
			return;
		}

		public static List<string> GetCurrentActions() => GetActions(CurrentContext);

		public static List<string> GetActions(InputContext context) => context?.actions;
	}
}
