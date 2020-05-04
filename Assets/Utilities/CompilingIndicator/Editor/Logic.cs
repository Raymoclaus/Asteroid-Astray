using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Compilation;

namespace UsingTheirs.CompilingIndicator
{
    [InitializeOnLoad]
    public class Logic
    {
        static Logic()
        {
            RegisterCallbacks();
            LoadDatabase();
        }
        
        #region Stored
        // Key: First Assembly, Value: Estimated Total Compilation Time
        static private Dictionary<string, float> estimatedTimes;        
        // Key: First Assembly, Value: Expected Last Assembly
        static private Dictionary<string, string> expectedLastAssemblies;
        static private float estimatedReloadTime;
        
        // For Assembly Reloading
        static private float assemblyReloadStartTime;
        static private float totalCompilationTime;
        static private float compilationEstimatedTime;
        static private string expectedLastAssembly;
        #endregion

        // Key: Assembly, Value: Assembly Compilation Start Time
        static private Dictionary<string, float> assemblyStartTime = new Dictionary<string, float>();
        static private string firstAssemblyName;
        static private float compilationStartTime;
        static private int compilationErrorCount;
        static private bool noInfo;
        static private float totalEstimatedTime;
        static private string lastAssemblyCandidate;
        
        
        private const float timeLerpRatio = 0.9f;
        private static Window window { get { return Window.Instance; } }

        static void SaveDatabase()
        {
            EditorPrefs.SetString("CI.ETKeys", String.Join(":", estimatedTimes.Keys.ToArray()));
            foreach (var k in estimatedTimes.Keys)
            {
                EditorPrefs.SetFloat("CI.ET." + k, estimatedTimes[k]);
            }
            
            EditorPrefs.SetString("CI.ELAKeys", String.Join(":", expectedLastAssemblies.Keys.ToArray()));
            foreach (var k in expectedLastAssemblies.Keys)
            {
                EditorPrefs.SetString("CI.ELA." + k, expectedLastAssemblies[k]);
            }

            EditorPrefs.SetFloat("CI.ERT", estimatedReloadTime);
            EditorPrefs.SetFloat("CI.RST", assemblyReloadStartTime);
            EditorPrefs.SetFloat("CI.TCT", totalCompilationTime);
            EditorPrefs.SetFloat("CI.CET", compilationEstimatedTime);
            EditorPrefs.SetString("CI.ELA", expectedLastAssembly);
        }

        internal static void LoadDatabase()
        {
            if (estimatedTimes == null)
                estimatedTimes = new Dictionary<string, float>();
            else
                estimatedTimes.Clear();
            
            var rawETKeys = EditorPrefs.GetString("CI.ETKeys", null);
            if (rawETKeys != null)
            {
                var splitETKeys = rawETKeys.Split(':');
                foreach (var k in splitETKeys)
                {
                    estimatedTimes.Add( k, EditorPrefs.GetFloat("CI.ET." + k, 0));
                }
            }
            
            if (expectedLastAssemblies == null)
                expectedLastAssemblies = new Dictionary<string, string>();
            else
                expectedLastAssemblies.Clear();
            
            var rawELAKeys = EditorPrefs.GetString("CI.ELAKeys", null);
            if (rawELAKeys != null)
            {
                var splitELAKeys = rawELAKeys.Split(':');
                foreach (var k in splitELAKeys)
                {
                    expectedLastAssemblies.Add( k, EditorPrefs.GetString("CI.ELA." + k, string.Empty));
                }
            }
            
            estimatedReloadTime = EditorPrefs.GetFloat("CI.ERT", 0);
            assemblyReloadStartTime = EditorPrefs.GetFloat("CI.RST", 0);
            totalCompilationTime = EditorPrefs.GetFloat("CI.TCT", 0);
            compilationEstimatedTime = EditorPrefs.GetFloat("CI.CET", 0);
            expectedLastAssembly = EditorPrefs.GetString("CI.ELA", string.Empty);
        }

        internal static void ClearDatabase()
        {
            var rawETKeys = EditorPrefs.GetString("CI.ETKeys", null);
            if (rawETKeys != null)
            {
                var splitETKeys = rawETKeys.Split(':');
                foreach (var k in splitETKeys)
                {
                    EditorPrefs.DeleteKey("CI.ET." + k);
                }
            }
            
            var rawELAKeys = EditorPrefs.GetString("CI.ELAKeys", null);
            if (rawELAKeys != null)
            {
                var splitELAKeys = rawELAKeys.Split(':');
                foreach (var k in splitELAKeys)
                {
                    EditorPrefs.DeleteKey("CI.ELA." + k);
                }
            }
            
            EditorPrefs.DeleteKey("CI.ETKeys");
            EditorPrefs.DeleteKey("CI.ERT");
            EditorPrefs.DeleteKey("CI.RST");
            EditorPrefs.DeleteKey("CI.TCT");
            EditorPrefs.DeleteKey("CI.CET");
            EditorPrefs.DeleteKey("CI.ELA");
        }

        static void RegisterCallbacks()
        {
            CompilationPipeline.assemblyCompilationStarted += OnAssemblyCompilationStarted;
            CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompilationFinished;
            #if UNITY_2019_1_OR_NEWER
            CompilationPipeline.compilationStarted += OnCompilationStarted;
            CompilationPipeline.compilationFinished += OnCompilationFinished;
            #endif
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
        }

        static void UnregisterCallbacks()
        {
            CompilationPipeline.assemblyCompilationStarted -= OnAssemblyCompilationStarted;
            CompilationPipeline.assemblyCompilationFinished -= OnAssemblyCompilationFinished;
            #if UNITY_2019_1_OR_NEWER
            CompilationPipeline.compilationStarted -= OnCompilationStarted;
            CompilationPipeline.compilationFinished -= OnCompilationFinished;
            #endif
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;
        }

        static void ClearVolatiles()
        {
            compilationErrorCount = 0;
            firstAssemblyName = string.Empty;
            noInfo = false;
            assemblyStartTime.Clear();
            expectedLastAssembly = string.Empty;
        }

        static void OnCompilationStarted(object context)
        {
            Logger.Log("OnCompilationStarted");
            
            Window.CreatePopup();
            
            if ( window != null )
                window.UpdateProgress(0, 0, noInfo, true);
            if ( window != null )
                window.UpdateLog("Compilation Started", true);
            
            compilationStartTime = Time.realtimeSinceStartup;
            
            EditorApplication.update += EditorUpdate;

            ClearVolatiles();
        }

        static void OnAssemblyCompilationStarted(string assembly)
        {
            Logger.Log("OnAssemblyCompilationStarted - {0}", assembly);
            
            #if UNITY_2019_1_OR_NEWER
            #else
            if (string.IsNullOrEmpty(firstAssemblyName))
            {
                OnCompilationStarted(null);
            }
            #endif
                
            if (window != null)
                window.UpdateLog(string.Format("Assembly <color=yellow><i>\'{0}'</i></color> Compilation Started", AssemblyDisplayName(assembly)),false);

            if (assemblyStartTime.ContainsKey(assembly))
            {
                Logger.LogError("[CompilingIndicator] Something wrong happend. Duplicated assembly - {0}", assembly);
                return;
            }

            assemblyStartTime.Add(assembly, Time.realtimeSinceStartup);

            if (string.IsNullOrEmpty(firstAssemblyName))
            {
                firstAssemblyName = assembly;

                float oldET;
                if (estimatedTimes.TryGetValue(assembly, out oldET))
                {
                    noInfo = false;
                    totalEstimatedTime = oldET + estimatedReloadTime;
                    compilationEstimatedTime = oldET;
                }
                else
                {
                    noInfo = true;
                    totalEstimatedTime = estimatedReloadTime;
                    compilationEstimatedTime = 0;
                }

                totalEstimatedTime = Mathf.Max(0, totalEstimatedTime);

                if (window != null)
                {
                    window.UpdateProgress(0,  totalEstimatedTime, noInfo);
                    window.ChangePhase(Window.Phase.Compile, noInfo ? null : totalEstimatedTime.ToString("F1"));
                }

                expectedLastAssemblies.TryGetValue(assembly, out expectedLastAssembly);
            }
            
            lastAssemblyCandidate = assembly;
            
            // The Unity Editor UI freezes before we get OnAssemblyCompilationFinished callback.
            if (expectedLastAssembly == assembly)
            {
                estimatedReloadTime = Mathf.Max(0, estimatedReloadTime);
                if (window != null)
                    window.ChangePhase( Window.Phase.Reload, estimatedReloadTime.ToString("F2") );
            }
        }

        static void OnAssemblyCompilationFinished(string assembly, CompilerMessage[] messages)
        {
            Logger.Log("OnAssemblyCompilationFinished - {0}", assembly);
            
            var elapsed = Time.realtimeSinceStartup - assemblyStartTime[assembly];

            if (window != null)
            {
                var log = string.Format(
                    "Assembly <color=yellow><i>\'{0}'</i></color> Compilation Finished ({1:F2} sec.)", 
                    AssemblyDisplayName(assembly), elapsed);
                window.UpdateLog(log, false);
            }
            
            foreach (var m in messages)
            {
                if (m.type != CompilerMessageType.Error) continue;
                
                compilationErrorCount++;
                if (window != null)
                    window.UpdateLog(string.Format("<color=red>{0}</color>",m.message), false);
            }
            
            #if UNITY_2019_1_OR_NEWER
            #else
            if ( compilationErrorCount > 0 )
            {
                OnCompilationFinished(null);
            }
            #endif
        }

        static void OnCompilationFinished(object context)
        {
            Logger.Log("OnCompilationFinished");
            
            totalCompilationTime = Time.realtimeSinceStartup - compilationStartTime;

            if (compilationErrorCount > 0)
            {
                if (window != null)
                {
                    window.UpdateLog(string.Format("Compilation Failed - {0} Error(s)", compilationErrorCount), false);
                    window.ChangePhase(Window.Phase.Fail, compilationErrorCount.ToString());
                }

                EditorApplication.update -= EditorUpdate;
            }
            else
            {
                if ( window != null)
                    window.UpdateLog(string.Format("Compilation Finished ({0} sec.)",totalCompilationTime.ToString("F2")), false);

                if (firstAssemblyName != null)
                {
                    expectedLastAssemblies[firstAssemblyName] = lastAssemblyCandidate;

                    float oldET;
                    if (estimatedTimes.TryGetValue(firstAssemblyName, out oldET))
                        estimatedTimes[firstAssemblyName] = Mathf.Lerp(oldET, totalCompilationTime, timeLerpRatio);
                    else
                        estimatedTimes.Add(firstAssemblyName, totalCompilationTime);
                }
            }
            
        }
        
        static void OnBeforeAssemblyReload()
        {
            Logger.Log("OnBeforeAssemblyReload");
            
            #if UNITY_2019_1_OR_NEWER
            #else
            OnCompilationFinished(null);
            #endif
            
            if (window != null)
                window.UpdateLog("Assembly Reload Started", false);

            assemblyReloadStartTime = Time.realtimeSinceStartup;
            
            UnregisterCallbacks();
            SaveDatabase();
        }

        static void OnAfterAssemblyReload()
        {
            Logger.Log("OnAfterAssemblyReload");
            
            // Do not need to call LoadDatabase() and RegisterCallbacks() here.
            // The static constructor is called before this.
            
            float elapsed = Time.realtimeSinceStartup - assemblyReloadStartTime;
            estimatedReloadTime = Mathf.Lerp(estimatedReloadTime, elapsed, timeLerpRatio);

            float totalElapsed = totalCompilationTime + elapsed;
            totalElapsed = Mathf.Max(0, totalElapsed);

            if (window != null)
            {
                window.UpdateLog(string.Format("Assembly Reload Finished ({0:F2} sec.)", elapsed), false);
                window.UpdateLog(string.Format("Total ({0:F2} sec.)", totalElapsed), false);
                window.ChangePhase(Window.Phase.Success, totalElapsed.ToString("F2") );
                window.UpdateProgress(1, 0, false);
            }

            EditorApplication.update -= EditorUpdate;
            
            SaveDatabase();
        }
        
        [PostProcessBuild(1)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) 
        {
            Logger.Log("OnPostprocessBuild");
            
            // After this callback, Editor dlls will be compiled.
            ClearVolatiles();
        }

        static void EditorUpdate()
        {
            var elapsed = Time.realtimeSinceStartup - compilationStartTime;
            
            float progress = Mathf.Clamp01(elapsed / totalEstimatedTime);
            float leftSec = Mathf.Max( 0, totalEstimatedTime - elapsed);

            if (window != null)
                window.UpdateProgress(progress, leftSec, noInfo);
        }

        static string AssemblyDisplayName(string assembly)
        {
            return Path.GetFileName(assembly);
        }
    }
}
