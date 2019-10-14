using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Puzzles.TileFlip;
using CustomDataTypes;

namespace Tests
{
	public class tile_lights_puzzle
	{
		// A Test behaves as an ordinary method
		[Test]
		public void returns_a_grid_with_the_same_size_as_given_parameters()
		{
			//ARRANGE
			Generator generator = new Generator();
			int width = Random.Range(1, 10);
			int height = Random.Range(1, 10);
			IntPair size = new IntPair(width, height);

			//ACT
			GridMatrix grid = generator.GeneratePuzzle(size);

			//ASSERT
			Assert.AreEqual(size, grid.GridSize);
		}
	}
}
