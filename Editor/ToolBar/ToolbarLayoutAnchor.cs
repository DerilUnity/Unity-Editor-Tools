using UnityEngine;
using UnityEditor;
using UnityToolbarExtender;

namespace Common.Scripts.Editor
{
    [InitializeOnLoad]
    public static class CustomToolbarDrawer
    {
        static CustomToolbarDrawer()
        {
            // ONE place to draw all custom buttons
            ToolbarExtender.LeftToolbarGUI.Add(DrawToolbar);
        }

        private static void DrawToolbar()
        {
            if (EditorApplication.isPlaying) return;

            // Push custom buttons to the right side
            GUILayout.FlexibleSpace();
            
            GUILayout.FlexibleSpace();

            
            FastPlayModeButton.Draw();
            SceneDropdownButton.Draw();
            ForceResolveButton.Draw();  
            ClearPlayerPrefsButton.Draw();
            CreateScriptButton.Draw();
            MasterSceneButton.Draw();

        }
    }
}