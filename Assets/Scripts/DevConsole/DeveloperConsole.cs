using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Console
{
	public abstract class ConsoleCommand
	{
		public abstract string Name { get; protected set; }
		public abstract string[] CommandAliases { get; protected set; }
		public abstract string Description { get; protected set; }
		public abstract string Usage { get; protected set; }

		public void AddCommandToConsole()
		{
			DeveloperConsole.AddCommandsToConsole(Name, this);
		}

		public abstract void Run(string[] args);

		public virtual bool ArgsAreValid(string[] args) => true;

		public string GetAliasesString()
		{
			string aliases = string.Empty;
			for (int i = 0; i < CommandAliases.Length; i++)
			{
				aliases += $"{CommandAliases[i]}{(i < CommandAliases.Length - 1 ? " | " : string.Empty)}";
			}
			return aliases;
		}
	}

	public class DeveloperConsole : MonoBehaviour
	{
		private static DeveloperConsole instance;
		private static DeveloperConsole Instance
		{
			get { return instance ?? (instance = FindObjectOfType<DeveloperConsole>()); }
		}
		public static bool IsActive
		{
			get { return Instance?.consoleCanvas.gameObject.activeInHierarchy ?? false; }
		}
		private static Dictionary<string, ConsoleCommand> Commands { get; set; }
			= new Dictionary<string, ConsoleCommand>();

		private delegate void ConsoleLogEventHandler(string log);
		private static event ConsoleLogEventHandler OnConsoleLog;
		public static void Log(string s) => OnConsoleLog?.Invoke(s);

		public static readonly List<string> HELP_COMMANDS = new List<string> { "help", "?", "plsexplain" };

		[Header("UI Components")]
		public Canvas consoleCanvas;
		public Text consoleText;
		public Text inputText;
		public InputField consoleInput;

		private void Start()
		{
			if (Instance != this) return;

			consoleCanvas.gameObject.SetActive(false);
			for (int i = 0; i < HELP_COMMANDS.Count; i++) HELP_COMMANDS[i] = HELP_COMMANDS[i].ToLower();
			CreateCommands();
		}

		private void CreateCommands()
		{
			Type baseType = typeof(ConsoleCommand);
			Assembly.GetAssembly(baseType).GetTypes()
				.Where(t => t != baseType && baseType.IsAssignableFrom(t)).ToList()
				.ForEach(t => Activator.CreateInstance(t));
		}

		private void OnEnable() => OnConsoleLog += AddMessageToConsole;

		private void OnDisable() => OnConsoleLog -= AddMessageToConsole;

		public static void AddCommandsToConsole(string name, ConsoleCommand command)
		{
			if (!Commands.ContainsKey(name))
			{
				Commands.Add(name.ToLower(), command);
			}
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.BackQuote))
			{
				bool currentlyActive = consoleCanvas.gameObject.activeInHierarchy;
				consoleCanvas.gameObject.SetActive(!currentlyActive);
				Pause.InstantPause(!currentlyActive);
				if (!currentlyActive)
				{
					consoleInput.ActivateInputField();
					consoleInput.Select();
				}
			}

			if (consoleCanvas.gameObject.activeInHierarchy)
			{
				if (Input.GetKeyDown(KeyCode.Return))
				{
					if (inputText.text != string.Empty)
					{
						Log($">>{inputText.text}");
						ParseInput(inputText.text);
						consoleInput.ActivateInputField();
						consoleInput.Select();
						consoleInput.text = string.Empty;
					}
				}
			}
		}

		public void AddMessageToConsole(string msg) => consoleText.text += $"\n{msg}";

		private void ParseInput(string inputArgs)
		{
			//get args array
			string[] input = inputArgs.Split(null);

			//ignore invalid input
			ConsoleCommand command = (input.Length == 0 || input == null)
				? null : FindCommandByAlias(input[0].ToLower());
			if (command == null)
			{
				Log("Command not recognised.");
				return;
			}

			//create array of args excluding initial command call
			string[] args = GetArgs(input);

			//execute
			ExecuteCommand(command, args);
		}

		private ConsoleCommand FindCommandByAlias(string alias)
		{
			foreach (KeyValuePair<string , ConsoleCommand> command in Commands)
			{
				ConsoleCommand cc = command.Value;
				for (int i = 0; i < cc.CommandAliases.Length; i++)
				{
					if (cc.CommandAliases[i] == alias) return cc;
				}
			}
			return null;
		}

		private void ExecuteCommand(ConsoleCommand command, string[] args)
		{
			//check for help option
			if (args.Length > 0 && HELP_COMMANDS.Contains(args[0]))
			{
				Log($"Description: {command.Description}");
				Log($"Usage: {command.Usage}");
				Log($"Aliases: {command.GetAliasesString()}");
				return;
			}

			//run command if args are valid, else log help message
			if (command.ArgsAreValid(args))
			{
				command.Run(args);
			}
			else
			{
				Log(command.Usage);
			}
		}

		private string[] GetArgs(string[] input)
		{
			string[] args = new string[input.Length - 1];
			for (int i = 0; i < args.Length; i++)
			{
				args[i] = input[i + 1].ToLower();
			}
			return args;
		}

		public static List<ConsoleCommand> GetCommandNames()
		{
			List<ConsoleCommand> commandNames = new List<ConsoleCommand>();
			foreach (KeyValuePair<string, ConsoleCommand> command in Commands)
			{
				commandNames.Add(command.Value);
			}
			return commandNames;
		}
	}
}

