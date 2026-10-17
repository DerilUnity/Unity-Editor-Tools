using UnityEditor;
using UnityEngine;

namespace Common.Scripts.Editor
{
    public static class ClearPlayerPrefsButton
    {
        public static void Draw()
        {
            if (GUILayout.Button(new GUIContent("C", "Clear PlayerPrefs"), GetStyle()))
            {
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
                Debug.Log("<color=yellow>PlayerPrefs Cleared</color>");
            }
        }

        private static GUIStyle GetStyle() => new GUIStyle("Button")
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            fixedWidth = 26,
            fixedHeight = 22
        };
    }
}