using CustomDataTypes;
using UnityEngine;

namespace Puzzles.TileFlip
{
	public class Generator
	{
		public GridMatrix GeneratePuzzle(IntPair size, int difficultySetting = 4)
		{
			GridMatrix tg = new GridMatrix(size);
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
