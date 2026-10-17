using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace IDZBase.Core.Scripts.Editor
{
    public class HotKeyShortcuts : MonoBehaviour
    {
        [MenuItem("Assets/Extras/Create Dummy Folders")]
        private static void Create_Dummy_Folders()
        {
            var folderNames = new[]
            {
                "Scenes",
                "Sounds",
                "Prefabs",
                "Sounds",
                "Scripts",
                "Materials",
                "Sprites",
                "Animations"
            };

            foreach (var folderName in folderNames)
            {
                if (!AssetDatabase.IsValidFolder(
                        $"{AssetDatabase.GetAssetPath(Selection.activeObject)}/{folderName}"))
                {
                    AssetDatabase.CreateFolder(
                        AssetDatabase.GetAssetPath(Selection.activeObject),
                        folderName);
                }
            }
        }

        [MenuItem("Assets/Extras/ToggleAllColliderGizmos #&C")]
        public static void ToggleAllColliderGizmos()
        {
            const string physics2DAssetPath =
                "ProjectSettings/Physics2DSettings.asset";

            SerializedObject physics2DManager =
                new SerializedObject(
                    AssetDatabase.LoadAllAssetsAtPath(physics2DAssetPath)[0]);

            SerializedProperty showColliderGizmos =
                physics2DManager.FindProperty("m_AlwaysShowColliders");

            showColliderGizmos.boolValue =
                !showColliderGizmos.boolValue;

            physics2DManager.ApplyModifiedProperties();

            Debug.Log(
                $"Always Show Collider Gizmos has been set to {showColliderGizmos.boolValue}");
        }

        [MenuItem("Assets/Extras/Set Script Creation Folder #&S")]
        public static void SetScriptCreationFolder()
        {
            Object selectedObject = Selection.activeObject;

            if (selectedObject == null)
            {
                Debug.LogWarning(
                    "No folder selected. Select a folder in the Project window first.");
                return;
            }

            string path = AssetDatabase.GetAssetPath(selectedObject);

            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning("Could not determine the selected asset path.");
                return;
            }

            if (!AssetDatabase.IsValidFolder(path))
            {
                path = Path.GetDirectoryName(path)?.Replace("\\", "/");
            }

            if (string.IsNullOrEmpty(path) || !AssetDatabase.IsValidFolder(path))
            {
                Debug.LogWarning(
                    "Please select a valid folder in the Project window.");
                return;
            }

            EditorPrefs.SetString(
                "CustomScriptCreationFolder",
                path);

            Debug.Log(
                $"Script creation folder set to: {path}");
        }

        [MenuItem("Assets/Extras/Create Folder Structure #&F")]
        public static void CreateFolderStructure()
        {
            Object selectedObject = Selection.activeObject;

            if (selectedObject == null)
            {
                Debug.LogWarning(
                    "No folder selected. Select the main folder in the Project window first.");
                return;
            }

            string rootPath = AssetDatabase.GetAssetPath(selectedObject);

            if (!AssetDatabase.IsValidFolder(rootPath))
            {
                Debug.LogWarning(
                    "Please select a valid folder in the Project window.");
                return;
            }

            string clipboard = GUIUtility.systemCopyBuffer;

            if (string.IsNullOrWhiteSpace(clipboard))
            {
                Debug.LogWarning("Clipboard is empty.");
                return;
            }

            CreateFoldersFromClipboard(rootPath, clipboard);

            AssetDatabase.Refresh();

            Debug.Log(
                $"Folder structure created inside: {rootPath}");
        }

        private static void CreateFoldersFromClipboard(
            string rootPath,
            string clipboard)
        {
            string[] lines = clipboard.Split(
                new[] { '\r', '\n' },
                System.StringSplitOptions.RemoveEmptyEntries);

            var folderStack = new List<string>();

            foreach (string rawLine in lines)
            {
                string line = rawLine.TrimEnd();

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                string folderName = ExtractFolderName(line);

                if (string.IsNullOrEmpty(folderName))
                    continue;

                if (folderName.Contains(".cs"))
                    continue;

                if (folderName == "Assets")
                    continue;

                int depth = GetDepth(line);

                while (folderStack.Count > depth)
                    folderStack.RemoveAt(folderStack.Count - 1);

                string parentPath = rootPath;

                foreach (string parentFolder in folderStack)
                {
                    parentPath = GetOrCreateFolder(
                        parentPath,
                        parentFolder);
                }

                string createdPath = GetOrCreateFolder(
                    parentPath,
                    folderName);

                while (folderStack.Count < depth)
                    folderStack.Add("");

                if (folderStack.Count == depth)
                    folderStack.Add(folderName);
                else
                    folderStack[depth] = folderName;
            }
        }

        private static string ExtractFolderName(string line)
        {
            int branchIndex = line.LastIndexOf("├──");
            
            if (branchIndex < 0)
                branchIndex = line.LastIndexOf("└──");

            if (branchIndex >= 0)
                line = line.Substring(branchIndex + 3);

            line = line.Trim();

            int commentIndex = line.IndexOf('#');

            if (commentIndex >= 0)
                line = line.Substring(0, commentIndex);

            return line.Trim().TrimEnd('/');
        }

        private static int GetDepth(string line)
        {
            int branchIndex = line.LastIndexOf("├──");

            if (branchIndex < 0)
                branchIndex = line.LastIndexOf("└──");

            if (branchIndex < 0)
                return 0;

            string prefix = line.Substring(0, branchIndex);

            int depth = 0;

            for (int i = 0; i < prefix.Length;)
            {
                if (prefix.Substring(i).StartsWith("│   "))
                {
                    depth++;
                    i += 4;
                }
                else if (prefix.Substring(i).StartsWith("    "))
                {
                    depth++;
                    i += 4;
                }
                else
                {
                    i++;
                }
            }

            return depth;
        }

        private static string GetOrCreateFolder(
            string parentPath,
            string folderName)
        {
            string folderPath =
                $"{parentPath}/{folderName}";

            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder(
                    parentPath,
                    folderName);
            }

            return folderPath;
        }
    }
}