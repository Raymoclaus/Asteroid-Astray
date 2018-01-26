using UnityEngine;

namespace Utilities.FPS_Display
{
    public class FpsDisplay : MonoBehaviour
    {
        float _deltaTime;
        int _screenW, _screenH;
        Rect _rect;
        GUIStyle _style = new GUIStyle();

        void Awake()
        {
            _screenW = Screen.width;
            _screenH = Screen.height;
            _rect = new Rect(0, 0, _screenW, _screenH / 25);
            _style.alignment = TextAnchor.UpperLeft;
            _style.fontSize = _screenH / 25;
            _style.normal.textColor = new Color(0.0f, 0.0f, 0.5f, 1.0f);
        }

        void Update()
        {
            _deltaTime += (Time.deltaTime - _deltaTime) * 0.1f;
        }

        void OnGUI()
        {
            float msec = _deltaTime * 1000.0f;
            float fps = 1.0f / _deltaTime;
            string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
            GUI.Label(_rect, text, _style);
        }
    }
}