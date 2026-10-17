using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using System.Linq;

namespace Common.Scripts.Editor
{
    public static class SceneDropdownButton
    {
        public static void Draw()
        {
            if (GUILayout.Button(new GUIContent("Scenes", "Open Scene From Build Settings"), GetStyle()))
            {
                ShowSceneMenu();
            }
        }

        private static void ShowSceneMenu()
        {
            var menu = new GenericMenu();

            var scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .ToArray();

            foreach (var scene in scenes)
            {
                string scenePath = scene.path;
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

                menu.AddItem(new GUIContent(sceneName), false, () =>
                {
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                        EditorSceneManager.OpenScene(scenePath);
                });
            }

            menu.ShowAsContext();
        }

        private static GUIStyle GetStyle() => new GUIStyle("Button")
        {
            fontSize = 13,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            fixedWidth = 80,   // wider button
            fixedHeight = 22
        };
    }
}