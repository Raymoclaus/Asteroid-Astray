﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Item
{
	public enum Type
	{
		Blank,
		Stone
	}

	public static int TypeRarity(Type type)
	{
		switch (type)
		{
			case Type.Blank: return 0;
			case Type.Stone: return 1;

			default: return 1;
		}
	}

	public static int StackLimit(Type type)
	{
		switch (type)
		{
			case Type.Blank: return 0;
			case Type.Stone: return 99;

			default: return 99;
		}
	}
}