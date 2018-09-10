using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

public static class CosmicItemFileReader
{
	private static string path = Application.dataPath + "/../BackgroundInfo.txt";
	private const char
		quadSeparator = '/',
		xSeparator = '?',
		ySeparator = '*',
		itemSeparator = '#',
		propertySeparator = ',';
	private static string propertyLine =
		"{0}" + propertySeparator +
		"{1}" + propertySeparator +
		"{2}" + propertySeparator +
		"{3}" + propertySeparator +
		"{4}" + propertySeparator +
		"{5}" + propertySeparator +
		"{6}";
	private static string newLine = System.Environment.NewLine;

	public static void Save(List<List<List<List<CosmicItem>>>> background)
	{
		StringBuilder text = new StringBuilder(100000000);
		CosmicItem c;

		for (int quad = 0; quad < background.Count; quad++)
		{
			for (int x = 0; x < background[quad].Count; x++)
			{
				bool xFound = false;
				for (int y = 0; y < background[quad][x].Count; y++)
				{
					bool yFound = false;
					for (int i = 0; i < background[quad][x][y].Count; i++)
					{
						yFound = xFound = true;
						c = background[quad][x][y][i];
						text.AppendFormat(propertyLine + newLine + itemSeparator.ToString() + newLine, c.type,
							c.pos.x, c.pos.y, c.pos.z, c.size, c.rotation, c.common);
					}
					if (yFound) text.Append(ySeparator.ToString() + newLine);
				}
				if (xFound) text.Append(xSeparator.ToString() + newLine);
			}
			if (quad <= background.Count - 1) text.Append(quadSeparator.ToString() + newLine);
		}

		Debug.Log(text.Length);
		File.WriteAllText(path, text.ToString());
	}

	public static List<List<List<List<CosmicItem>>>> Load(List<List<List<List<CosmicItem>>>> items,
		int largeDistance, int reserveSize)
	{
		if (!File.Exists(path)) return items;

		string[] lines = File.ReadAllLines(path);
		int quad = 0, x = 0, y = 0, i = 0;
		CosmicItem c;

		foreach (string line in lines)
		{
			switch(line[0])
			{
				case quadSeparator: quad++; x = 0; y = 0; i = 0; break;
				case xSeparator: x++; y = 0; i = 0; break;
				case ySeparator: y++; i = 0; break;
				case itemSeparator: i++; break;
				default:
					{
						string[] item = line.Split(propertySeparator);
						c.type = byte.Parse(item[0]);
						c.pos = new Vector3(float.Parse(item[1]), float.Parse(item[2]), float.Parse(item[3]));
						c.size = float.Parse(item[4]);
						c.rotation = byte.Parse(item[5]);
						c.common = bool.Parse(item[6]);
						AddItem(c, items, quad, x, y, i, largeDistance, reserveSize);
						break;
					}
			}
		}

		return items;
	}

	private static void AddItem(
		CosmicItem item, List<List<List<List<CosmicItem>>>> items, int quad, int x, int y, int i, int largeDistance,
		int reserveSize)
	{
		while (items[quad].Count <= x || items[quad][x].Count <= y)
		{
			if (items[quad].Count <= x)
			{
				List<List<CosmicItem>> xList = new List<List<CosmicItem>>(largeDistance);
				for (int j = 0; j < largeDistance; j++)
				{
					xList.Add(new List<CosmicItem>(reserveSize));
				}
				items[quad].Add(xList);
			}
			else
			{
				items[quad][x].Add(new List<CosmicItem>(reserveSize));
			}
		}
		items[quad][x][y].Add(item);
	}
}