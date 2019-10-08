using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcRoom : Room
{
	public NpcRoom(string[] lines, PlanetData data) : base(lines, data)
	{

	}

	public NpcRoom(IntPair position, Room previousRoom)
		: base(position, previousRoom)
	{

	}

	public override void GenerateContent()
	{
		base.GenerateContent();
	}
}
