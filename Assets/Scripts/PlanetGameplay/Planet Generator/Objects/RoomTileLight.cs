using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileLightsPuzzle;

public class RoomTileLight : RoomObject
{
	public bool flipped;
	private TileGrid tileGrid;
	private int index;

	public delegate void TileFlippedEventHandler(bool flipped);
	public event TileFlippedEventHandler OnTileFlipped;

	public RoomTileLight(TileGrid tileGrid, int index)
	{
		this.tileGrid = tileGrid;
		tileGrid.OnTileFlipped += Flip;
		this.index = index;
		flipped = tileGrid.IsFlipped(index);
	}
	
	public void Flip(int index)
	{
		if (this.index != index) return;
		flipped = !flipped;
		OnTileFlipped?.Invoke(flipped);
	}

	public void Interact() => tileGrid.TileFlipped(index);

	public override ObjType GetObjectType() => ObjType.TileLight;
}
