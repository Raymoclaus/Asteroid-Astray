using UnityEngine;

namespace Utilities
{
    public class FunctionTimer
    {
        public float StartTime;

        public FunctionTimer()
        {
            Restart();
        }

        public void Restart()
        {
            StartTime = Time.realtimeSinceStartup;
        }

        public float Log()
        {
            return Time.realtimeSinceStartup - StartTime;
        }
    }
}