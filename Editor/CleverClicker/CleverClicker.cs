using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Common.Scripts.Editor
{
    [InitializeOnLoad]
    public static class CleverClicker
    {
        // chnage these values accordingly 
        const int MouseButton = 1;
        const float PickRadius = 40f;
        const int MaxResults = 50;

        static CleverClicker()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        static void OnSceneGUI(SceneView sceneView)
        {
            Event e = Event.current;

            if (e == null)
                return;

            if (e.type != EventType.MouseDown)
                return;

            if (e.button != MouseButton)
                return;

            if (e.alt)
                return;

            List<GameObject> objects =
                GetObjectsUnderMouse(e.mousePosition);

            if (objects.Count == 0)
                return;

            GenericMenu menu = new GenericMenu();

            for (int i = 0; i < objects.Count; i++)
            {
                GameObject cachedGo = objects[i];

                if (i < 10)
                {
                    menu.AddItem(
                        new GUIContent(GetHierarchyPath(cachedGo)),
                        false,
                        () => SelectObject(cachedGo));
                }
                else
                {
                    menu.AddItem(
                        new GUIContent($"More/{GetHierarchyPath(cachedGo)}"),
                        false,
                        () => SelectObject(cachedGo));
                }
            }

            menu.ShowAsContext();

            e.Use();
        }

        static List<GameObject> GetObjectsUnderMouse(Vector2 mousePosition)
        {
            List<(GameObject go, float distance)> found =
                new List<(GameObject, float)>();

            GameObject[] allObjects =
                Object.FindObjectsByType<GameObject>(
                    FindObjectsSortMode.None);

            foreach (GameObject go in allObjects)
            {
                if (go == null)
                    continue;

                if (!go.activeInHierarchy)
                    continue;

                Vector2 guiPoint =
                    HandleUtility.WorldToGUIPoint(
                        go.transform.position);

                float distance =
                    Vector2.Distance(
                        mousePosition,
                        guiPoint);

                if (distance > PickRadius)
                    continue;

                found.Add((go, distance));
            }

            found.Sort((a, b) =>
                a.distance.CompareTo(b.distance));

            List<GameObject> results =
                new List<GameObject>();

            foreach (var item in found)
            {
                if (!results.Contains(item.go))
                    results.Add(item.go);

                if (results.Count >= MaxResults)
                    break;
            }

            return results;
        }

        static void SelectObject(GameObject go)
        {
            Selection.activeGameObject = go;
            EditorGUIUtility.PingObject(go);
        }

        
   
        // simple single List 
        
        /*static string GetHierarchyPath(GameObject go)
        {
            return go.name;
        }*/
        
        
        // List with context of its parent 
        static string GetHierarchyPath(GameObject go)
        {
            if (go.transform.parent == null)
                return go.name;

            return $"{go.name}  ({go.transform.parent.name})";
        }
    }
}