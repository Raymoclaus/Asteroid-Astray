using System;
using System.Collections.Generic;
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

		//keep track of current context
		private static InputContext _currentContext;

		public static event Action OnInputModeChanged, OnContextChanged;

		private static void CheckForModeUpdate()
		{
			InputMode prevMode = mode;

			//check current mode if input detected
			CustomInputHandler currentHandler = GetHandler();
			if (currentHandler?.ProcessInputs(GetCurrentContext()) ?? false) return;

			//if no input detected in current, check other input methods for input
			for (int i = 0; i < inputHandlers.Count; i++)
			{
				if (inputHandlers[i] == currentHandler) continue;

				if (inputHandlers[i].ProcessInputs(GetCurrentContext()))
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
		public static float GetInput(GameAction action)
		{
			if (action == null) return 0f;
			CheckForModeUpdate();
			return GetHandler()?.GetInput(action.ActionName, GetCurrentContext()) ?? 0f;
		}

		public static bool GetInputDown(GameAction action)
		{
			if (action == null) return false;
			CheckForModeUpdate();
			return GetHandler()?.GetInputDown(action.ActionName, GetCurrentContext()) ?? false;
		}

		public static bool GetInputUp(GameAction action)
		{
			if (action == null) return false;
			CheckForModeUpdate();
			return GetHandler()?.GetInputUp(action.ActionName, GetCurrentContext()) ?? false;
		}

		//returns whether the current context contains the given action
		private static bool IsValidKey(string key)
			=> GetCurrentContext()?.IsValidAction(key) ?? false;

		//returns the input combination for a given action to occur
		public static ActionCombination GetBinding(string key)
		{
			CheckForModeUpdate();
			return GetHandler()?.GetBinding(key, GetCurrentContext());
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

		public static InputContext GetCurrentContext()
		{
			if (_currentContext != null) return _currentContext;
			UnityEngine.Object.FindObjectOfType<SceneContextInitialiser>()?.SetContext();
			return _currentContext;
		}

		public static void SetCurrentContext(InputContext context)
		{
			if (context == null || _currentContext == context) return;
			_currentContext = context;
			Debug.Log($"Input context set to {_currentContext.contextName}");
			OnContextChanged?.Invoke();
		}

		public static List<string> GetCurrentActions() => GetActions(GetCurrentContext());

		public static List<string> GetActions(InputContext context) => context?.Actions;

		public static bool CurrentContextContainsAction(GameAction action) =>
			action.IntendedContext == GetCurrentContext();
	}
}
