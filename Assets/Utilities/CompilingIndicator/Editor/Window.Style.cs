using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace UsingTheirs.CompilingIndicator
{

    public partial class Window
    {
        private class Styles
        {
            public GUIStyle header;
            public GUIStyle desc;
            public GUIStyle log;
            public GUIStyle scroll;
            public GUIStyle bottomBtn;
            public Texture2D bgTexture;
            public Texture2D icon;

            public Styles()
            {
            }

            public void Build(ScriptableObject parent)
            {
                if ( bgTexture == null)
                    bgTexture = MakeTex(1, 1, Color.black);

                if (header == null)
                {
                    header = new GUIStyle(GUI.skin.label);
                    header.fontSize = 40;
                    header.fontStyle = FontStyle.Bold;
                    header.alignment = TextAnchor.LowerLeft;
                    SetAllBackground(header, bgTexture);
                    SetAllTextColor(header, new Color(0.9f, 0.9f, 0.9f));
                    header.margin = new RectOffset(10, 10, 0, -5);
                }

                if (desc == null)
                {
                    desc = new GUIStyle(GUI.skin.label);
                    desc.fontSize = 15;
                    desc.alignment = TextAnchor.UpperLeft;
                    SetAllBackground(desc, bgTexture);
                    SetAllTextColor(desc, Color.white);
                    desc.margin = new RectOffset(10, 10, -5, 5);
                    desc.richText = true;
                }

                if (log == null)
                {
                    log = new GUIStyle(GUI.skin.textArea);
                    log.richText = true;
                    SetAllBackground(log, MakeTex(1, 1, new Color(0.1f, 0.1f, 0.1f)));
                }

                if (scroll == null)
                {
                    scroll = new GUIStyle(GUI.skin.scrollView);
                    scroll.margin = new RectOffset(0, 0, 2, 2);
                }

                if (bottomBtn == null)
                {
                    bottomBtn = new GUIStyle(GUI.skin.button);
                    bottomBtn.alignment = TextAnchor.LowerCenter;
                    SetAllBackground(bottomBtn, bgTexture);
                    SetAllTextColor(bottomBtn, Color.gray);
                    bottomBtn.hover.textColor = Color.white;
                    bottomBtn.active.textColor = Color.white;
                }

                if (icon == null)
                {
                    try
                    {
                        var iconPath = Path.Combine(GetIconDirectoryPath(parent), "Logo.png");
                        icon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);
                    }
                    catch (Exception)
                    {
                     Debug.LogError("[CompilationIndicator] Icon Not Found.");
                    }
                }
            }

            static void SetAllBackground(GUIStyle style, Texture2D tx)
            {
                style.active.background = style.focused.background =
                    style.hover.background = style.normal.background = tx;
            }
            
            static void SetAllTextColor(GUIStyle style, Color c)
            {
                style.active.textColor = style.focused.textColor =
                    style.hover.textColor = style.normal.textColor = c;
            }

            static Texture2D MakeTex(int width, int height, Color col)
            {
                Color[] pix = new Color[width * height];

                for (int i = 0; i < pix.Length; i++)
                    pix[i] = col;

                Texture2D result = new Texture2D(width, height);
                result.SetPixels(pix);
                result.Apply();

                return result;
            }

            static string GetIconDirectoryPath(ScriptableObject parent)
            {
                var scriptAsset = MonoScript.FromScriptableObject(parent);
                var path = AssetDatabase.GetAssetPath(scriptAsset);
                return Path.Combine(Path.GetDirectoryName(path), "Icon");
            }
        }
    }

}

