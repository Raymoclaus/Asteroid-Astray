//#define CI_DEBUG
using System.IO;
using UnityEngine;

namespace UsingTheirs.CompilingIndicator
{
    public class Logger : MonoBehaviour
    {
        const int logFileMaxBytes = 1024 * 1024;

        private static string _logFilePath = null;
        static string logFilePath
        {
            get
            {
                if (_logFilePath == null)
                    _logFilePath = Path.GetFullPath(Path.Combine(logFileDir, "Log.txt"));

                return _logFilePath;
            }
        }
        
        private static string _logBackFilePath = null;
        static string logBackFilePath
        {
            get
            {
                if (_logBackFilePath == null)
                    _logBackFilePath = Path.GetFullPath(Path.Combine(logFileDir, "Log.txt.Bak"));

                return _logBackFilePath;
            }
        }
        
        private static string _logFileDir = null;
        static string logFileDir
        {
            get
            {
                if (_logFileDir == null)
                    _logFileDir = GetLogFileDir();

                return _logFileDir;
            }
        }

        private static string GetLogFileDir()
        {
            var dir = Path.Combine(Application.dataPath, "..");
            dir = Path.Combine(dir, "Library");
            dir = Path.Combine(dir, "UsingTheirsCompilingIndicator");
            return dir;
        }

        private static void CreateLogFileDir()
        {
            if (Directory.Exists(logFileDir))
                return;

            Directory.CreateDirectory(logFileDir);
        }

        private static void LogImpl(LogType type, string msg)
        {
            if (  File.Exists(logFilePath))
            {
                if (new System.IO.FileInfo(logFilePath).Length > logFileMaxBytes)
                {
                    if ( File.Exists(logBackFilePath))
                        File.Delete(logBackFilePath);
                    
                    File.Move(logFilePath, logBackFilePath);
                }
            }
            else
            {
                CreateLogFileDir();
            }

            using (var sw = File.AppendText(logFilePath))
            {
                sw.WriteLine(string.Format("[{0}] {1}", type, msg));
            }
        }

        public static void Log(string format, params object[] args)
        {
            var msg = string.Format(format, args);
            LogImpl(LogType.Log, msg);
#if CI_DEBUG
            Debug.Log(msg);
#endif
        }

        public static void LogWarning(string format, params object[] args)
        {
            var msg = string.Format(format, args);
            LogImpl(LogType.Warning, msg);
#if CI_DEBUG
            Debug.LogWarning(msg);
#endif
        }
        public static void LogError(string format, params object[] args)
        {
            var msg = string.Format(format, args);
            LogImpl(LogType.Error, msg);
#if CI_DEBUG
            Debug.LogError(msg);
#endif
        }
    }
}