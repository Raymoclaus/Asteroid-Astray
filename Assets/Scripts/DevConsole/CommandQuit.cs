using UnityEngine;

namespace Console
{
	public class CommandQuit : ConsoleCommand
	{
		public override string Name { get; protected set; }
		public override string[] CommandAliases { get; protected set; }
		public override string Description { get; protected set; }
		public override string Usage { get; protected set; }

		public CommandQuit()
		{
			Name = "Quit";
			CommandAliases = new string[] { "quit", "exit", "close", "yeet" };
			Description = "Quits the game.";
			Usage = $"\"{CommandAliases[0]}\" Use with no arguments.";

			AddCommandToConsole();
		}

		public override void Run(string[] args)
		{
			DeveloperConsole.Log("Quitting...");
			if (Application.isEditor)
			{
#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false;
#endif
			}
			else
			{
				Application.Quit();
			}
		}

		public override bool ArgsAreValid(string[] args)
		{
			if (args.Length > 0)
			{
				DeveloperConsole.Log("Too many arguments");
				return false;
			}
			return true;
		}
	}
}
