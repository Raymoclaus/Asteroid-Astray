using UnityEngine;
using UnityEditor;

namespace UsingTheirs.CompilingIndicator
{
    public partial class Window : EditorWindow
    {
        public enum Phase
        {
            Compile,
            Reload,
            Success,
            Fail,
        };

        private const float windowWidth = 500;
        private const float windowHeight = 200;

        public static void CreatePopup()
        {
            if (_instance != null)
                _instance.Close();

            var win = CreateInstance<Window>();
            Util.ShowDropDown(win, new Vector2(windowWidth, windowHeight), false);
            win.ChangePhase(Phase.Compile);

            _instance = win;
        }

        private static Window _instance;
        public static Window Instance
        {
            get { return _instance; }
        }

        private string log = string.Empty;
        private float progress;
        private string leftSec;


        private Styles styles;
        private Vector2 scrollPos = Vector2.zero;
        private string headerText;
        private string descText;
        private Phase phase;

        void OnGUI()
        {
            if (styles == null)
                styles = new Styles();
            styles.Build(this);

            GUI.DrawTexture(new Rect(0, 0, position.width, position.height), styles.bgTexture, ScaleMode.StretchToFill);

            GUILayout.Label( headerText, styles.header);
                
            if ( phase == Phase.Success )
            {
                Rect rcIcon = new Rect(position.width - 40, 8, 32, 32);
                GUI.DrawTexture(rcIcon, styles.icon, ScaleMode.ScaleAndCrop);
            }
            
            GUILayout.Label(descText, styles.desc);

            Rect r = EditorGUILayout.BeginVertical();
            EditorGUI.ProgressBar(r, progress, string.Format("{0}% ({1})", (int) (progress * 100), leftSec));
            GUILayout.Space(30);
            EditorGUILayout.EndVertical();

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, styles.scroll, GUILayout.ExpandHeight(true));
            GUILayout.TextArea(log, styles.log, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();

            // Auto Scroll
            if (phase != Phase.Success && phase != Phase.Fail)
                scrollPos.y = 999f; // large number

            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(windowWidth - 200);
                
                if (GUILayout.Button("Help", styles.bottomBtn))
                {
                    Application.OpenURL("https://codingdad.me/2019/10/31/compiling-indicator/");
                }

                if (GUILayout.Button("Clear Database", styles.bottomBtn))
                {
                    Logic.ClearDatabase();
                    Logic.LoadDatabase();
                }

                if (GUILayout.Button("Close", styles.bottomBtn))
                    Close();
            }
            GUILayout.EndHorizontal();
        }

        private void OnEnable()
        {
            // When assemblies are reloaded, _instance losts its value.
            _instance = this;
        }

        public void UpdateProgress(float progress, float leftSec, bool noInfo, bool initProgress = false)
        {
            this.progress = progress;
            this.leftSec = initProgress ? "Calculating..." : ComposeEstimatedTime(leftSec, noInfo);

            Repaint();
        }

        public void UpdateLog(string log, bool clearLogs = false)
        {
            if (clearLogs)
                this.log = string.Empty;

            this.log += string.Format("<color=silver>{0}</color>\n", log);

            Repaint();
        }

        public void ChangePhase(Phase phase, string extra = null)
        {
            this.phase = phase;

            if (phase == Phase.Compile)
            {
                headerText = "Compiling...";
                if (extra != null)
                    descText = string.Format("<color=grey>{0} Second(s) Estimated</color>", extra);
                else
                    descText = "<color=grey>Calculating...</color>";
            }
            else if (phase == Phase.Reload)
            {
                headerText = "Reloading...";
                descText = string.Format("<color=grey>{0} Second(s) Left</color>", extra);
            }
            else if (phase == Phase.Success)
            {
                headerText = "Done";
                descText = string.Format("<color=grey>{0} Second(s) Elapsed</color>", extra);
            }
            else if (phase == Phase.Fail)
            {
                headerText = "Failed";
                descText = string.Format("<color=red>{0} Error(s)</color>", extra);
            }

            Repaint();
        }

        static string ComposeEstimatedTime(float et, bool noInfo)
        {
            return noInfo ? "Calculating..." : string.Format("{0:F2} sec.", et);
        }
    }

}
