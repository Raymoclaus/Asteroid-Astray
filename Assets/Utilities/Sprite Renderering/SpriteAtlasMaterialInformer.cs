using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[ExecuteInEditMode]
public class SpriteAtlasMaterialInformer : MonoBehaviour
{
	private const string ATLAS_X_POSITION_PROPERTY_NAME = "_AtlasXPosition",
		ATLAS_Y_POSITION_PROPERTY_NAME = "_AtlasYPosition",
		ATLAS_CELL_X_SCALE_PROPERTY_NAME = "_AtlasCellXScale",
		ATLAS_CELL_Y_SCALE_PROPERTY_NAME = "_AtlasCellYScale";
	private SpriteRenderer _sprRend;

	private void Update()
	{
		InformMaterials();
	}

	public void InformMaterials()
	{
		Rect r = SprRend.sprite.textureRect;
		Vector2 textureSize = new Vector2(SprRend.sprite.texture.width, SprRend.sprite.texture.height);
		Vector2 normalisedOffset = r.min / textureSize;
		Vector2 normalisedScale = r.size / textureSize;

		foreach (Material mat in SprRend.sharedMaterials)
		{
			InformMaterial(mat, normalisedOffset, normalisedScale);
		}
	}

	private void InformMaterial(Material mat, Vector2 offset, Vector2 scale)
	{
		if (mat == null)
		{
			Debug.Log($"Material is null", gameObject);
		}

		//x offset
		if (mat.HasProperty(ATLAS_X_POSITION_PROPERTY_NAME))
		{
			mat.SetFloat(ATLAS_X_POSITION_PROPERTY_NAME, offset.x);
		}
		//y offset
		if (mat.HasProperty(ATLAS_Y_POSITION_PROPERTY_NAME))
		{
			mat.SetFloat(ATLAS_Y_POSITION_PROPERTY_NAME, offset.y);
		}
		//x scale
		if (mat.HasProperty(ATLAS_CELL_X_SCALE_PROPERTY_NAME))
		{
			mat.SetFloat(ATLAS_CELL_X_SCALE_PROPERTY_NAME, scale.x);
		}
		//y scale
		if (mat.HasProperty(ATLAS_CELL_Y_SCALE_PROPERTY_NAME))
		{
			mat.SetFloat(ATLAS_CELL_Y_SCALE_PROPERTY_NAME, scale.y);
		}
	}

	private SpriteRenderer SprRend => _sprRend != null ? _sprRend : (_sprRend = GetComponent<SpriteRenderer>());
}
