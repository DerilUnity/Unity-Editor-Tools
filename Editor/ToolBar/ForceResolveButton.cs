using UnityEditor;
using UnityEngine;

namespace Common.Scripts.Editor
{
    public static class ForceResolveButton
    {
        public static void Draw()
        {
            if (GUILayout.Button(new GUIContent("FR", "Force Resolve Android Dependencies"), GetStyle()))
            {
                ForceResolve();
            }
        }

        private static GUIStyle GetStyle() => new GUIStyle("Button")
        {
            fontSize = 13,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            fixedWidth = 34,
            fixedHeight = 22
        };

        private static void ForceResolve()
        {
#if UNITY_ANDROID
            if (!EditorUtility.DisplayDialog(
                    "Force Resolve Dependencies",
                    "This will delete resolved libraries and force Android dependency resolution.\n\nContinue?",
                    "Yes", "Cancel"))
                return;

            Debug.Log("[FR] Deleting resolved libraries...");
            EditorApplication.ExecuteMenuItem(
                "Assets/External Dependency Manager/Android Resolver/Delete Resolved Libraries");

            Debug.Log("[FR] Forcing resolve...");
            EditorApplication.ExecuteMenuItem(
                "Assets/External Dependency Manager/Android Resolver/Force Resolve");

            Debug.Log("[FR] Dependency resolution triggered.");
#else
            Debug.LogWarning("[FR] Android Resolver is only available when Android platform is enabled.");
#endif
        }
    }
}