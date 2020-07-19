using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class MultiMaterialSpriteController : MonoBehaviour
{
	[SerializeField] private List<Material> materials;
	private SpriteRenderer _sprRend;

	public void ApplyMaterials()
	{
		SprRend.sharedMaterials = materials.ToArray();
	}

	public void RevertToArray()
	{
		materials = SprRend.sharedMaterials.ToList();
	}

	/// <summary>
	/// checks if there are any difference between the contents of the material list and the material array on the sprite renderer component
	/// </summary>
	public bool MaterialListMatchesSpriteRendererMaterialArray
	{
		get
		{
			int listCount = materials.Count;
			int arrayCount = SprRend.sharedMaterials.Length;
			if (listCount != arrayCount) return false;

			for (int i = 0; i < listCount; i++)
			{
				if (materials[i] != SprRend.sharedMaterials[i]) return false;
			}

			return true;
		}
	}

	public void AddMaterial(Material mat)
	{
		materials.Add(mat);
	}

	public void InsertMaterial(Material mat, int index)
	{
		//ensure index is at least 0
		index = Mathf.Max(0, index);
		//if index is higher than list count, just add it on top
		if (index >= materials.Count)
		{
			AddMaterial(mat);
		}
		else
		{
			materials.Insert(index, mat);
		}
	}

	public void RemoveMaterial(Material mat)
	{
		materials.Remove(mat);
	}

	public void RemoveMaterialAtIndex(int index)
	{
		materials.RemoveAt(index);
	}

	/// <summary>
	/// Swaps the materials at the given indexes. If either input is not within the valid range of indexes then nothing changes.
	/// </summary>
	/// <param name="a"></param>
	/// <param name="b"></param>
	public void SwapIndexes(int a, int b)
	{
		if (a < 0 || b < 0 || a >= materials.Count || b >= materials.Count) return;

		Material temp = materials[a];
		materials[a] = materials[b];
		materials[b] = temp;
	}

	public void MoveMaterialUp(int index)
	{
		SwapIndexes(index, index - 1);
	}

	public void MoveMaterialDown(int index)
	{
		SwapIndexes(index, index + 1);
	}

	private SpriteRenderer SprRend => _sprRend != null ? _sprRend : (_sprRend = GetComponent<SpriteRenderer>());
}
