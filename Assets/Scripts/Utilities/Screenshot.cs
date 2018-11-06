using UnityEditor;
using UnityEngine;
using System.IO;

public static class Screenshot
{
	public static string fileName = "Screenshot{0}.png";

	[MenuItem("Screenshot/Take screenshot")]
	public static void Take()
	{
		for (int count = 0;; count++)
		{
			if (!File.Exists(string.Format(fileName, count)))
			{
				string name = string.Format(fileName, count);
				Debug.Log(name);
				ScreenCapture.CaptureScreenshot(name);
				return;
			}
		}
	}
}
