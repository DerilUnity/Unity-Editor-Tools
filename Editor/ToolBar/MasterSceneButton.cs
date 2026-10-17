using UnityEditor;
using UnityEngine;

namespace Common.Scripts.Editor
{
    public static class MasterSceneButton
    {
        private const string PREF_MASTER_ON_PLAY = "SceneAutoLoader.LoadMasterOnPlay";
        private const string PREF_MASTER_STATUS = "SceneAutoLoader.LoadMasterStatus";

        private static bool LoadMasterOnPlay
        {
            get => EditorPrefs.GetBool(PREF_MASTER_ON_PLAY, false);
            set => EditorPrefs.SetBool(PREF_MASTER_ON_PLAY, value);
        }

        private static bool LoadMasterStatus
        {
            get => EditorPrefs.GetBool(PREF_MASTER_STATUS, false);
            set => EditorPrefs.SetBool(PREF_MASTER_STATUS, value);
        }

        static MasterSceneButton()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        public static void Draw()
        {
            if (GUILayout.Button(new GUIContent("M", "Play from Master Scene"), GetStyle()))
            {
                LoadMasterStatus = LoadMasterOnPlay;
                LoadMasterOnPlay = true;
                EditorApplication.isPlaying = true;
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

        private static void OnPlayModeChanged(PlayModeStateChange change)
        {
            if (!LoadMasterOnPlay) return;

            if (!EditorApplication.isPlaying &&
                !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                LoadMasterOnPlay = LoadMasterStatus;
            }
        }
    }
}