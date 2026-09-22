using Thunda.Auth;
using UnityEditor;
using UnityEngine;

namespace Thunda.AuthEditor
{
    public class ThundaSettingsWindow : EditorWindow
    {
        [MenuItem("Thunda/Settings")]
        public static void ShowWindow()
        {
            GetWindow<ThundaSettingsWindow>("Thunda");
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("Backend", EditorStyles.boldLabel);
            ThundaSettings.Url = EditorGUILayout.TextField("Url", ThundaSettings.Url);
            ThundaSettings.Update = EditorGUILayout.TextField("Update", ThundaSettings.Update);
            ThundaSettings.UseOculusUsername = EditorGUILayout.Toggle("Use Oculus Username", ThundaSettings.UseOculusUsername);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Session", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Logged In", ThundaClientAPI.IsClientLoggedIn().ToString());
            EditorGUILayout.LabelField("PlayFabId", string.IsNullOrEmpty(ThundaSettings.PlayFabId) ? "-" : ThundaSettings.PlayFabId);
            EditorGUILayout.LabelField("OculusId", string.IsNullOrEmpty(ThundaSettings.OculusId) ? "-" : ThundaSettings.OculusId);

            EditorGUILayout.Space();
            ThundaDebug.Enabled = EditorGUILayout.Toggle("Logging", ThundaDebug.Enabled);

            if (GUILayout.Button("Forget Credentials"))
            {
                ThundaClientAPI.ForgetAllCredentials();
            }
        }
    }
}
