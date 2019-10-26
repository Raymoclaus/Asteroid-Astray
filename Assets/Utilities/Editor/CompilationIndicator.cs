using UnityEditor;
using UnityEngine;

public class CompilationIndicator : EditorWindow
{
	private static Texture2D greenTex, redTex;

	[MenuItem("Window/Compilation Indicator")]
	public static void OpenWindow()
	{
		CreateTextures();

		Rect r = new Rect(0, 0, 200, 200);
		EditorWindow window = GetWindowWithRect(typeof(CompilationIndicator), r);
		window.Show();
	}

	private void OnGUI()
	{
		if (greenTex == null || redTex == null)
		{
			CreateTextures();
		}
		GUI.DrawTexture(new Rect(0, 0, maxSize.x, maxSize.y),
			EditorApplication.isCompiling ? redTex : greenTex,
			ScaleMode.StretchToFill);
		string text = EditorApplication.isCompiling ?
			"Not ready" : "Ready";
		GUILayout.Label(text);
		Repaint();
	}

	private static void CreateTextures()
	{
		greenTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
		greenTex.SetPixel(0, 0, new Color(0.25f, 0.5f, 0.25f));
		greenTex.Apply();

		redTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
		redTex.SetPixel(0, 0, new Color(0.5f, 0.25f, 0.25f));
		redTex.Apply();
	}
}
