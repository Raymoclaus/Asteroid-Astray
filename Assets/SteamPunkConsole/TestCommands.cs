using UnityEditor;
using UnityEngine;

public static class TestCommands
{
	[SteamPunkConsoleCommand(command = "TestCommand", info = "This is a test command, it used to test three parameters.")]
	static public void TestCommand(int valueInt, float valueFloat, string valueString)
	{
		Debug.Log("TEST WORKS!");
	}

	[SteamPunkConsoleCommand(command = "Quit", info = "This quits the game.")]
	static public void CommandQuit()
	{
		EditorApplication.isPlaying = false;
	}
}
