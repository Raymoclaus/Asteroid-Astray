using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlockPushPuzzle
{
	public class BlockPushGenerator
	{
		private List<IntPair> visitedSpots = new List<IntPair>();
		private List<IntPair> trimmedVisitedSpots = new List<IntPair>();
		private int trimSize = 4;
		private IntPair currentPos, previousPos;
		private IntPair previousDirection;
		private List<IntPair> startingDirections;

		public PushPuzzle Generate(IntPair size, int padding, int minimumSolutionCount = 2)
		{
			PushPuzzle puzzle = new PushPuzzle(size, padding);
			Reset(puzzle);

			int freezeCount = 0;
			while (true)
			{
				freezeCount++;
				if (freezeCount >= 10000)
				{
					Debug.Log("Failed");
					return null;
				}

				if (!puzzle.IsNearOuterEdge(currentPos, 2))
				{
					List<IntPair> possibleDirections = new List<IntPair>()
					{
						new IntPair(1, 0),
						new IntPair(-1, 0),
						new IntPair(0, 1),
						new IntPair(0, -1)
					};
					List<IntPair> emergencyDirections = new List<IntPair>();
					IntPair randomDirection = IntPair.zero;
					IntPair nextPos = currentPos + randomDirection;

					bool foundValidDirection = false;
					while (possibleDirections.Count > 0
						|| emergencyDirections.Count > 0)
					{
						List<IntPair> listToUse = possibleDirections.Count > 0 ?
							possibleDirections : emergencyDirections;
						int randomIndex = Random.Range(0, listToUse.Count);
						randomDirection = listToUse[randomIndex];
						listToUse.RemoveAt(randomIndex);

						nextPos = currentPos + randomDirection;
						bool meetsRequirements =
							!puzzle.IsNearTo(nextPos, puzzle.finishTile, 2)
							&& !ListContainsItem(visitedSpots, nextPos)
							&& !puzzle.ListContainsAdjacentPosition(
								trimmedVisitedSpots, nextPos, 2);
						if (!meetsRequirements)
						{
							if (possibleDirections.Count == 0
								&& emergencyDirections.Count == 0)
							{
								if (startingDirections.Count > 0)
								{
									StartFromNewDirection(puzzle);
								}
								else if (puzzle.solutionCount >= minimumSolutionCount)
								{
									puzzle.FinishGeneration();
									return puzzle;
								}
								else
								{
									return Generate(size, padding, minimumSolutionCount);
								}
							}
							continue;
						}
						if (puzzle.IsNearOuterEdge(nextPos, 2) && possibleDirections.Count > 0)
						{
							emergencyDirections.Add(randomDirection);
							continue;
						}
						foundValidDirection = true;
						break;
					}
					if (!foundValidDirection) continue;

					previousDirection = randomDirection;
					IntPair oppositePos = currentPos - randomDirection;

					if (!puzzle.IsNearOuterEdge(oppositePos, 2)
						&& puzzle.BlockExists(oppositePos))
					{
						puzzle.SetBlock(oppositePos, false);
						puzzle.SetBlock(currentPos, true);
						VisitSpot(oppositePos);
					}
					puzzle.SetBlock(nextPos, false);

					previousPos = currentPos;
					currentPos = nextPos;
					VisitSpot(currentPos);
				}
				else if (startingDirections.Count > 0)
				{
					if (puzzle.BlockExists(previousPos))
					{
						puzzle.SetBlock(previousPos, false);
						puzzle.SetBlock(currentPos, true);
					}
					currentPos += previousDirection;
					puzzle.SetBlock(currentPos, false);
					StartFromNewDirection(puzzle);
					puzzle.solutionCount++;
				}
				else if (puzzle.solutionCount >= minimumSolutionCount)
				{
					if (puzzle.BlockExists(previousPos))
					{
						puzzle.SetBlock(previousPos, false);
						puzzle.SetBlock(currentPos, true);
					}
					currentPos += previousDirection;
					puzzle.SetBlock(currentPos, false);
					break;
				}
				else
				{
					return Generate(size, padding, minimumSolutionCount);
				}
			}

			puzzle.FinishGeneration();
			return puzzle;
		}

		private bool StartFromNewDirection(PushPuzzle puzzle)
		{
			while (startingDirections.Count > 0)
			{
				currentPos = puzzle.finishTile;
				IntPair forwardCheck =
					puzzle.finishTile + startingDirections[0] * 3;
				if (!puzzle.ListContainsNearPosition(
					trimmedVisitedSpots, forwardCheck, 2))
				{
					for (int i = 0; i < 2; i++)
					{
						currentPos += startingDirections[0];
						puzzle.SetBlock(currentPos, false);
						VisitSpot(currentPos);
					}
					startingDirections.RemoveAt(0);
					return true;
				}
				else
				{
					startingDirections.RemoveAt(0);
				}
			}
			return false;
		}

		private void Reset(PushPuzzle puzzle)
		{
			puzzle.Reset();
			startingDirections = new List<IntPair>()
			{
				new IntPair(1, 0),
				new IntPair(-1, 0),
				new IntPair(0, 1),
				new IntPair(0, -1)
			};
			visitedSpots.Clear();
			trimmedVisitedSpots.Clear();

			currentPos = puzzle.finishTile;
			previousPos = currentPos;
			for (int i = 0; i < 2; i++)
			{
				currentPos.x += startingDirections[0].x;
				currentPos.y += startingDirections[0].y;
				puzzle.SetBlock(currentPos, false);
				VisitSpot(currentPos);
			}
			startingDirections.RemoveAt(0);
			previousDirection = IntPair.zero;
		}

		private void VisitSpot(IntPair position)
		{
			AddToListExcludingDuplicates(visitedSpots, currentPos);
			if (visitedSpots.Count >= trimSize)
			{
				trimmedVisitedSpots.Add(visitedSpots[visitedSpots.Count - trimSize]);
			}
		}

		private void AddToListExcludingDuplicates<T>(List<T> list, T item)
		{
			if (ListContainsItem<T>(list, item)) return;
			list.Add(item);
		}

		private bool ListContainsItem<T>(List<T> list, T item, int excludeEnding = 0)
		{
			for (int i = 0; i < list.Count - excludeEnding; i++)
			{
				if (list[i].Equals(item)) return true;
			}
			return false;
		}
	}
}
