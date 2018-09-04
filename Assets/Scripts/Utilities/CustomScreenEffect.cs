using UnityEngine;

[ExecuteInEditMode]
public class CustomScreenEffect : MonoBehaviour
{
	public Material effectMaterial;

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		Graphics.Blit(source, destination, effectMaterial);
	}
}