using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundController : MonoBehaviour
{
	public Renderer rend;
	private Material mat;
	private Texture2D tex;
	public Texture2D[] textures;
	private Color32[][] colorArrays;
	private const int width = 512;
	private const int height = 512;
	private Color32[] allBlack = new Color32[width * height];

	private void Awake()
	{
		//get references
		rend = rend == null ? GetComponent<Renderer>() : rend;
		mat = rend.material;
		//set material to fully opaque
		mat.color = Color.white;
		//create new texture
		tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
		//set all pixels to black
		for (int i = 0; i < allBlack.Length; i++)
		{
			allBlack[i] = new Color32(0, 0, 0, 255);
		}

		colorArrays = new Color32[textures.Length][];
		for (int i = 0; i < textures.Length; i++)
		{
			colorArrays[i] = textures[i].GetPixels32();
		}

		//change camera setting to view the texture properly
		Camera.main.clearFlags = CameraClearFlags.Depth;
	}

	private void Update()
	{
		tex.SetPixels32(allBlack);

		BlendTexture(width / 2 + 100, height / 2 + 100, textures[0].width, textures[0].height, 0);

		tex.Apply();
		mat.mainTexture = tex;
	}

	private void BlendTexture(int x, int y, int w, int h, int texIndex)
	{
		for (int i = 0; i < w; i++)
		{
			for (int j = 0; j < h; j++)
			{
				Color32 texCol = tex.GetPixel(x + i, y + j);
				int sprPos = i + j * w;
				Color32 sprCol = colorArrays[texIndex][sprPos];
				float alphaMultiplier = sprCol.a / 255f;
				texCol.r += (byte)(sprCol.r * alphaMultiplier);
				texCol.g += (byte)(sprCol.g * alphaMultiplier);
				texCol.b += (byte)(sprCol.b * alphaMultiplier);
				texCol.a += (byte)(sprCol.a * alphaMultiplier);
				tex.SetPixel(x + i, y + j, texCol);
			}
		}
	}
}