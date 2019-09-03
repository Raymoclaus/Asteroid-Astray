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
		Maze maze = gen.GeneratePuzzle(new Vector2Int(28, 16), new Vector2Int[]
		{
			new Vector2Int(1, 5),
			new Vector2Int(10, 9)
		});

		for (int i = 0; i < maze.ArrayLength(); i++)
		{
			Vector2Int pos = maze.GetPos(i);
			bool isWall = maze.IsWall(pos);
			for (int x = 0; x < 2; x++)
			{
				for (int y = 0; y < 2; y++)
				{
					Vector2Int adjustedPos =
						new Vector2Int(pos.x * 2 + x, pos.y * 2 + y);
					tilemap.SetTile((Vector3Int)adjustedPos,
						isWall ? wallTile : floorTile);
				}
			}
		}
		tilemap.RefreshAllTiles();
	}
}
