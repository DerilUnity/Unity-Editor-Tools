using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

public static class ComponentTools
{
    [MenuItem("CONTEXT/Component/Copy & Remove Component")]
    private static void CopyRemoveTransform(MenuCommand command)
    {
        Component comp = command.context as Component;

        if (comp == null) return;

        // Copy
        ComponentUtility.CopyComponent(comp);

        // Remove
        Undo.DestroyObjectImmediate(comp);
    }
}