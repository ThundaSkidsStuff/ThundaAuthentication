using UnityEngine;

namespace Thunda.Auth
{
    public static class ThundaDebug
    {
        public static bool Enabled = true;

        public static void Log(string msg)
        {
            if (Enabled)
            {
                Debug.Log("[Thunda] " + msg);
            }
        }

        public static void Warn(string msg)
        {
            if (Enabled)
            {
                Debug.LogWarning("[Thunda] " + msg);
            }
        }

        public static void Error(ThundaError error)
        {
            if (Enabled)
            {
                Debug.LogError("[Thunda] " + error.GenerateErrorReport());
            }
        }
    }
}
