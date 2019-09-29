using UnityEngine.Tilemaps;
using UnityEngine;
using MazePuzzle;

public class TestingTilemaps : MonoBehaviour
{
	public Grid grid;
	public Tilemap tilemap;
	public TileBase wallTile, floorTile;

	private void Start()
	{
		MazePuzzle.MazeGenerator gen = new MazePuzzle.MazeGenerator();
		Maze maze = gen.GeneratePuzzle(new IntPair(28, 16), new IntPair[]
		{
			new IntPair(1, 5),
			new IntPair(10, 9)
		});

		for (int i = 0; i < maze.ArrayLength(); i++)
		{
			IntPair pos = maze.GetPos(i);
			bool isWall = maze.IsWall(pos);
			for (int x = 0; x < 2; x++)
			{
				for (int y = 0; y < 2; y++)
				{
					IntPair adjustedPos =
						new IntPair(pos.x * 2 + x, pos.y * 2 + y);
					tilemap.SetTile(new Vector3Int(adjustedPos.x, adjustedPos.y, 0),
						isWall ? wallTile : floorTile);
				}
			}
		}
		tilemap.RefreshAllTiles();
	}
}
