using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Puzzles.BlockPush;
using CustomDataTypes;

namespace Tests
{
	public class block_push_puzzle
	{
		[Test]
		public void returns_a_grid_with_the_same_size_as_given_parameters()
		{
			//ARRANGE
			Generator gen = new Generator();
			Random.InitState(1);
			IntPair size = new IntPair(26, 14);
			int padding = 1;

			//ACT
			GridMatrix puzzle = gen.Generate(size, padding);

			//ASSERT
			Assert.AreEqual(size, puzzle.GridSize);
		}

		[Test]
		public void returns_puzzle_that_meets_the_minimum_number_of_solutions()
		{
			//ARRANGE
			Generator gen = new Generator();
			Random.InitState(1);
			IntPair size = new IntPair(26, 14);
			int padding = 1;
			int minimumSolutionCount = 2;

			//ACT
			GridMatrix puzzle = gen.Generate(size, padding, minimumSolutionCount);

			//ASSERT
			Assert.IsTrue(puzzle.solutionCount >= minimumSolutionCount);
		}

		[Test]
		public void returns_a_non_null_output()
		{
			//ARRANGE
			Generator gen = new Generator();
			Random.InitState(1);
			IntPair size = new IntPair(26, 14);
			int padding = 1;
			int minimumSolutionCount = 2;

			//ACT
			GridMatrix puzzle = gen.Generate(size, padding, minimumSolutionCount);

			//ASSERT
			Assert.IsTrue(puzzle != null);
		}

		[Test]
		public void finish_tile_is_located_near_center_of_grid()
		{
			//ARRANGE
			Generator gen = new Generator();
			Random.InitState(1);
			IntPair size = new IntPair(26, 14);
			int padding = 1;
			int minimumSolutionCount = 2;

			//ACT
			GridMatrix puzzle = gen.Generate(size, padding, minimumSolutionCount);
			IntPair finishPos = puzzle.finishTile;
			int minimumDimension = Mathf.Min(size.x, size.y);
			int range = padding + minimumDimension / 2 - 2;

			//ASSERT
			Assert.IsTrue(finishPos.x >= range
				&& finishPos.x < size.x - 1 - range
				&& finishPos.y >= range
				&& finishPos.y < size.y - 1 - range);
		}
	}
}
