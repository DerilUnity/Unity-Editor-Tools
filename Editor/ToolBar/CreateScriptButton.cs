
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Common.Scripts.Editor
{
    public static class CreateScriptButton
    {
        private const string DefaultFolder = "Assets/Scripts/Generated/";

        public static void Draw()
        {
            if (GUILayout.Button(new GUIContent("S", "Create Script From Clipboard"), GetStyle()))
                CreateScript();
        }

        private static GUIStyle GetStyle() => new GUIStyle("Button")
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            fixedWidth = 26,
            fixedHeight = 22
        };

        private static string GetTargetFolder()
        {
            string saved = EditorPrefs.GetString("CustomScriptCreationFolder", "");
            if (!string.IsNullOrEmpty(saved) && AssetDatabase.IsValidFolder(saved))
                return saved;
            return DefaultFolder;
        }

        private static void CreateScript()
        {
            string clipboard = GUIUtility.systemCopyBuffer.Trim();
            if (string.IsNullOrEmpty(clipboard))
            {
                Debug.LogError("Clipboard is empty.");
                return;
            }

            string folderAssetPath = GetTargetFolder().TrimEnd('/') + "/";
            string ns = GenerateNamespace(folderAssetPath);

            bool isFullScript = clipboard.Contains("class ");
            string name;
            string scriptContent;

            if (isFullScript)
            {
                name = ExtractClassName(clipboard);
                if (string.IsNullOrEmpty(name))
                {
                    Debug.LogError("Could not extract class name from clipboard script.");
                    return;
                }
                scriptContent = InjectNamespace(clipboard, ns);
            }
            else
            {
                name = RemoveInvalid(clipboard);
                scriptContent = BuildTemplate(name, ns);
            }

            string folderDiskPath = Application.dataPath + "/" + folderAssetPath.Replace("Assets/", "");
            Directory.CreateDirectory(folderDiskPath);

            string fullDiskPath = Path.Combine(folderDiskPath, name + ".cs");
            string assetPath = folderAssetPath + name + ".cs";

            File.WriteAllText(fullDiskPath, scriptContent);
            AssetDatabase.Refresh();

            var asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            if (asset != null)
                AssetDatabase.OpenAsset(asset);
        }

        private static string ExtractClassName(string script)
        {
            var match = Regex.Match(script, @"\bclass\s+([A-Za-z_][A-Za-z0-9_]*)");
            return match.Success ? match.Groups[1].Value : null;
        }

        private static string InjectNamespace(string script, string ns)
        {
            // Replace existing namespace declaration
            if (Regex.IsMatch(script, @"\bnamespace\s+[\w.]+\s*\{"))
            {
                if (!string.IsNullOrEmpty(ns))
                    return Regex.Replace(script, @"\bnamespace\s+[\w.]+", $"namespace {ns}");
                return script;
            }

            // No namespace found — wrap entire script
            if (string.IsNullOrEmpty(ns)) return script;

            string indented = string.Join("\n", script.Split('\n').Select(l => "    " + l));
            return $"namespace {ns}\n{{\n{indented}\n}}";
        }

        private static string BuildTemplate(string name, string ns)
        {
            if (!string.IsNullOrEmpty(ns))
                return
$@"using UnityEngine;

namespace {ns}
{{
    public class {name} : MonoBehaviour
    {{
        void Start()
        {{

        }}

        void Update()
        {{

        }}
    }}
}}";

            return
$@"using UnityEngine;

public class {name} : MonoBehaviour
{{
    void Start()
    {{

    }}

    void Update()
    {{

    }}
}}";
        }

        private static string GenerateNamespace(string folderAssetPath)
        {
            folderAssetPath = folderAssetPath.TrimEnd('/');

            if (folderAssetPath.StartsWith("Assets/"))
                folderAssetPath = folderAssetPath.Substring("Assets/".Length);

            if (string.IsNullOrEmpty(folderAssetPath))
                return null;

            string[] parts = folderAssetPath.Split('/');

            for (int i = 0; i < parts.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(parts[i])) continue;

                string cleaned = "";
                foreach (char c in parts[i])
                {
                    if (char.IsLetterOrDigit(c) || c == '_')
                        cleaned += c;
                }

                if (string.IsNullOrEmpty(cleaned)) continue;

                cleaned = char.ToUpper(cleaned[0]) + cleaned.Substring(1);
                parts[i] = cleaned;
            }

            parts = parts.Where(p => !string.IsNullOrEmpty(p)).ToArray();
            return string.Join(".", parts);
        }

        private static string RemoveInvalid(string s)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                s = s.Replace(c.ToString(), "");
            return s;
        }
    }
}