using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlockPushPuzzle
{
	public class BlockPushGenerator
	{
		private List<Vector2Int> visitedSpots = new List<Vector2Int>();
		private List<Vector2Int> trimmedVisitedSpots = new List<Vector2Int>();
		private int trimSize = 4;
		private Vector2Int currentPos, previousPos;
		private Vector2Int previousDirection;
		private List<Vector2Int> startingDirections;

		public PushPuzzle Generate(Vector2Int size, int padding, int minimumSolutionCount = 2)
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
					List<Vector2Int> possibleDirections = new List<Vector2Int>()
					{
						new Vector2Int(1, 0),
						new Vector2Int(-1, 0),
						new Vector2Int(0, 1),
						new Vector2Int(0, -1)
					};
					List<Vector2Int> emergencyDirections = new List<Vector2Int>();
					Vector2Int randomDirection = Vector2Int.zero;
					Vector2Int nextPos = currentPos + randomDirection;

					bool foundValidDirection = false;
					while (possibleDirections.Count > 0
						|| emergencyDirections.Count > 0)
					{
						List<Vector2Int> listToUse = possibleDirections.Count > 0 ?
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
					Vector2Int oppositePos = currentPos - randomDirection;

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
				Vector2Int forwardCheck =
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
			startingDirections = new List<Vector2Int>()
			{
				new Vector2Int(1, 0),
				new Vector2Int(-1, 0),
				new Vector2Int(0, 1),
				new Vector2Int(0, -1)
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
			previousDirection = Vector2Int.zero;
		}

		private void VisitSpot(Vector2Int position)
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
