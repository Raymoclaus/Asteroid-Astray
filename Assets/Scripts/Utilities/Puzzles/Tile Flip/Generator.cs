using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities.Puzzles.TileFlip
{
	public class Generator
	{
		public TileGrid GeneratePuzzle(IntPair size, int difficultySetting = 4)
		{
			TileGrid tg = new TileGrid(size);
			for (int i = 0; i < difficultySetting; i++)
			{
				IntPair position = IntPair.one * -1;
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
