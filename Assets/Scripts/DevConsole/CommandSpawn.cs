using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Console
{
	public class CommandSpawn : ConsoleCommand
	{
		public override string Name { get; protected set; }
		public override string[] CommandAliases { get; protected set; }
		public override string Description { get; protected set; }
		public override string Usage { get; protected set; }

		private const string LIST_ARG = "list";
		private List<string> entityNames = new List<string>();
		private string entityNameList;
		private List<string> validFirstArgs = new List<string> { LIST_ARG };

		private EntityPrefabDB entityPrefabs;
		private EntityPrefabDB EntityPrefabs
			{ get { return entityPrefabs ?? (entityPrefabs = Resources.Load<EntityPrefabDB>("EntityPrefabDB")); } }

		public CommandSpawn()
		{
			GetEntityNames();
			CreateEntityNameList(entityNames);
			validFirstArgs.AddRange(entityNames);

			Name = "Spawn";
			CommandAliases = new string[] { "spawn", "create" };
			Description = "Spawns an entity.";
			Usage = $"<example> \"{CommandAliases[0]} asteroid\" spawns 1 asteroid nearby.\n" +
				$"<example> \"{CommandAliases[0]} asteroid 5\" spawns 5 asteroids nearby.\n" +
				$"\"{CommandAliases[0]} {LIST_ARG}\" returns a list of entity names usable with this command.";

			AddCommandToConsole();
		}

		public override void Run(string[] args)
		{
			string firstArg = args[0];
			if (firstArg == LIST_ARG)
			{
				List();
			}

			if (entityNames.Contains(firstArg))
			{
				Spawn(args, firstArg);
			}
		}

		private void Spawn(string[] args, string firstArg)
		{
			if (Object.FindObjectOfType<EntityGenerator>() == null)
			{
				DeveloperConsole.Log("You cannot spawn entities right now.");
				return;
			}

			int amount = args.Length == 1 ? 1 : int.Parse(args[1]);

			SpawnableEntity se = EntityPrefabs.GetSpawnableEntity(firstArg);
			if (se == null)
			{
				DeveloperConsole.Log($"Entity type {firstArg} not found.");
				return;
			}

			DeveloperConsole.Log($"Spawning {amount} {firstArg}{(amount > 1 ? "s" : string.Empty)}.");
			for (int i = 0; i < amount; i++)
			{
				Entity newEntity = EntityGenerator.SpawnEntity(se);
				if (newEntity == null)
				{
					DeveloperConsole.Log($"{i + 1}: {firstArg} spawn attempt was unsuccessful.");
				}
				else
				{
					DeveloperConsole.Log($"{i + 1}: {firstArg} spawn attempt was successful.");
					Vector2 pos = Camera.main.transform.position;
					pos += new Vector2(Random.value - 0.5f, Random.value - 0.5f) * 5f;
					newEntity.transform.position = pos;
				}
			}
		}

		private void List() => DeveloperConsole.Log(entityNameList);

		public override bool ArgsAreValid(string[] args)
		{
			if (args.Length == 0)
			{
				DeveloperConsole.Log("Not enough arguments.");
				return false;
			}
			string firstArg = args[0];

			if (!validFirstArgs.Contains(firstArg))
			{
				DeveloperConsole.Log($"\"{firstArg}\" is not a valid first argument.");
				return false;
			}

			if (firstArg == LIST_ARG)
			{
				if (args.Length > 1)
				{
					DeveloperConsole.Log("Too many arguments");
					return false;
				}
			}

			if (entityNames.Contains(firstArg))
			{
				if (args.Length > 2)
				{
					DeveloperConsole.Log("Too many arguments");
					return false;
				}

				int result;
				if (args.Length == 2 && !int.TryParse(args[1], out result) && result > 0)
				{
					DeveloperConsole.Log($"Expected positive integer value after \"{firstArg}\" argument.");
					return false;
				}
			}

			return true;
		}

		private void GetEntityNames()
		{
			if (EntityPrefabs == null) return;
			for (int i = 0; i < EntityPrefabs.spawnableEntities.Count; i++)
			{
				string entityName = EntityPrefabs.spawnableEntities[i].name.ToLower();
				if (entityNames.Contains(entityName)) continue;
				entityNames.Add(entityName);
			}
		}

		private void CreateEntityNameList(List<string> names)
		{
			entityNameList = "List of entity names:\n";
			for (int i = 0; i < names.Count; i++)
			{
				entityNameList += names[i] + (i == names.Count - 1 ? string.Empty : "\n");
			}
		}
	}
}
