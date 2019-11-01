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
		private static InputContext currentContext;
		public static InputContext CurrentContext
		{
			get
			{
				if (mostRecentContextController == null)
				{
					currentContext = null;
				}
				if (currentContext != null) return currentContext;
				ContextController[] contextCtrls = Object.FindObjectsOfType<ContextController>();
				if (contextCtrls.Length != 0)
				{
					ContextController contextCtrl = contextCtrls[0];
					for (int i = 1; i < contextCtrls.Length; i++)
					{
						if (contextCtrls[i].priority > contextCtrl.priority)
						{
							contextCtrl = contextCtrls[i];
						}
					}
					mostRecentContextController = contextCtrl;
					SetContext(contextCtrl.contextName);
					return currentContext;
				}
				InputContext[] anyContext = Resources.LoadAll<InputContext>("");
				if (anyContext.Length != 0)
				{
					SetContext(anyContext[0].contextName);
				}
				return currentContext;
			}
		}
		private static ContextController mostRecentContextController;

		public delegate void InputModeChangedEventHandler();
		public static event InputModeChangedEventHandler InputModeChanged;

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
				InputModeChanged?.Invoke();
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
			if (currentContext != null && currentContext.contextName == contextName) return;

			InputContext[] contexts = Resources.LoadAll<InputContext>("");
			if (contexts.Length == 0) return;

			for (int i = 0; i < contexts.Length; i++)
			{
				if (string.Compare(contexts[i].contextName.ToLower(), contextName.ToLower()) == 0)
				{
					Debug.Log($"Input context set to {contextName}");
					currentContext = contexts[i];
					return;
				}
			}
			return;
		}

		public static List<string> GetCurrentActions() => GetActions(CurrentContext);

		public static List<string> GetActions(InputContext context) => context?.actions;
	}
}
