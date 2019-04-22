using UnityEngine;
using System.Collections.Generic;

namespace Console
{
	public class CommandList : ConsoleCommand
	{
		public override string Name { get; protected set; }
		public override string[] CommandAliases { get; protected set; }
		public override string Description { get; protected set; }
		public override string Usage { get; protected set; }

		public CommandList()
		{
			Name = "List";
			CommandAliases = new string[DeveloperConsole.HELP_COMMANDS.Count + 1];
			CommandAliases[0] = "list";
			for (int i = 1; i < CommandAliases.Length; i++)
			{
				CommandAliases[i] = DeveloperConsole.HELP_COMMANDS[i - 1];
			}
			Description = "Lists all available commands usable in the developer console.";
			Usage = $"\"{CommandAliases[0]}\" Use with no arguments.";

			AddCommandToConsole();
		}

		public override void Run(string[] args)
		{
			DeveloperConsole.Log("Command List:");
			DeveloperConsole.Log("=============");
			List<ConsoleCommand> commands = DeveloperConsole.GetCommandNames();
			for (int i = 0; i < commands.Count; i++)
			{
				DeveloperConsole.Log($"Name: {commands[i].Name}");
				DeveloperConsole.Log($"Description: {commands[i].Description}");
				DeveloperConsole.Log(
					$"Aliases: {commands[i].GetAliasesString()}{(i < commands.Count - 1 ? "\n" : string.Empty)}");
			}
			DeveloperConsole.Log("=============");
			DeveloperConsole.Log("Tip: Use \"<commandName> ?\" for an explanation on how to use the specified command.");
			DeveloperConsole.Log($"Example: \"{CommandAliases[0]} ?\"");
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
