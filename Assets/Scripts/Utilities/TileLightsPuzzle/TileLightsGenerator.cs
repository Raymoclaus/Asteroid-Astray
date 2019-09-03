using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TileLightsPuzzle
{
	public class TileLightsGenerator
	{
		public TileGrid GeneratePuzzle(Vector2Int size, int difficultySetting)
		{
			TileGrid tg = new TileGrid(size);
			for (int i = 0; i < difficultySetting; i++)
			{
				Vector2Int position = Vector2Int.one * -1;
				do
				{
					position.x = Random.Range(0, tg.GridSize.x);
					position.y = Random.Range(0, tg.GridSize.y);
				} while (tg.StartingStateContainsPosition(position));
				tg.startingState.Add(position);
			}
			tg.Reset();

			return tg;
		}
	}
}
