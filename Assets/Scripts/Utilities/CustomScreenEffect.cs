using UnityEngine;

[ExecuteInEditMode]
public class CustomScreenEffect : MonoBehaviour
{
	public Material[] effectMaterials;
	public Material defaultMaterial;
	public Camera cam;

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		bool needDefault = true;
		foreach (Material mat in effectMaterials)
		{
			if (mat == null) continue;
			needDefault = false;
			Graphics.Blit(source, destination, mat);
			source = destination;
		}
		if (needDefault)
		{
			Graphics.Blit(source, destination, defaultMaterial);
		}
	}
}