using UnityEditor;
using UnityEngine;

namespace Common.Scripts.Editor
{
    [InitializeOnLoad]
    public static class FastPlayModeButton
    {
        private static bool previousEnterPlayModeEnabled;
        private static EnterPlayModeOptions previousOptions;
        private static bool restoreAfterPlay;

        static FastPlayModeButton()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        public static void Draw()
        {
            if (EditorApplication.isPlaying) return;

            if (GUILayout.Button(new GUIContent("Fast Play", "Play Without Domain Reload"), GetStyle()))
            {
                StartFastPlay();
            }
        }

        private static void StartFastPlay()
        {
            previousEnterPlayModeEnabled = EditorSettings.enterPlayModeOptionsEnabled;
            previousOptions = EditorSettings.enterPlayModeOptions;

            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions =
                EnterPlayModeOptions.DisableDomainReload |
                EnterPlayModeOptions.DisableSceneReload;

            restoreAfterPlay = true;
            EditorApplication.isPlaying = true;
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (!restoreAfterPlay) return;

            if (state == PlayModeStateChange.EnteredEditMode)
            {
                EditorSettings.enterPlayModeOptionsEnabled = previousEnterPlayModeEnabled;
                EditorSettings.enterPlayModeOptions = previousOptions;
                restoreAfterPlay = false;
            }
        }

        private static GUIStyle GetStyle() => new GUIStyle("Button")
        {
            fontSize = 13,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            fixedWidth = 95,
            fixedHeight = 22
        };
    }
}