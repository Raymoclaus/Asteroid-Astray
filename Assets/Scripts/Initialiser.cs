using UnityEngine;

/// <inheritdoc />
/// The purpose of this class is to run before everything else and make sure everything is set up before gameplay begins
public class Initialiser : MonoBehaviour
{
	public Asteroid AsteroidPfb;

	private void Awake()
	{
		AsteroidGenerator.AsteroidPfb = AsteroidPfb;
		AsteroidGenerator.AsteroidHolder = new GameObject("AsteroidHolder").transform;

		EntityNetwork.CreateGrid();
		AsteroidGenerator.FillTriggerList();
		StartCoroutine(EntityNetwork.RoutineCheckup());
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.RightBracket))
		{
			EntityNetwork.PrintStats();
		}
	}
}