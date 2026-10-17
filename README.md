[README_Unity_Editor_Tools.md](https://github.com/user-attachments/files/33598110/README_Unity_Editor_Tools.md)
# Unity Editor Tools

This folder contains custom Unity Editor tools for faster scene editing, script creation, sprite creation, and project setup.

## Requirements

- Unity Editor.
- The **UnityToolbarExtender** package for the custom toolbar buttons.
- **External Dependency Manager for Unity (EDM4U)** for the Android dependency resolver button.
- Keep Editor-only scripts inside an `Editor` folder so Unity does not include them in player builds.

## Installation

1. Copy the `Editor` folder into your Unity project's `Assets` folder.
2. Make sure UnityToolbarExtender and External Dependency Manager for Unity are installed if you want to use those features.
3. Wait for Unity to compile the scripts.
4. Use the toolbar buttons and menu items described below.

## Custom Toolbar

The toolbar tools are drawn by `Editor/ToolBar/ToolbarLayoutAnchor.cs`. The buttons are shown when the Editor is not in Play mode.

| Button | Tool | What it does |
|---|---|---|
| **Fast Play** | `FastPlayModeButton.cs` | Starts Play mode with Domain Reload and Scene Reload disabled. Restores the previous Enter Play Mode settings after returning to Edit mode. Use this only when your project works correctly without those reloads. |
| **Scenes** | `SceneDropdownButton.cs` | Shows enabled scenes from **File > Build Settings**. Select a scene to open it. Unity asks whether to save modified scenes first. |
| **FR** | `ForceResolveButton.cs` | On Android, asks for confirmation, deletes resolved libraries, and triggers **Delete Resolved Libraries** and **Force Resolve** in External Dependency Manager. On other platforms, it logs a warning. |
| **C** | `ClearPlayerPrefsButton.cs` | Deletes all PlayerPrefs and saves the change. This affects all PlayerPrefs for the current application, so use it carefully. |
| **S** | `CreateScriptButton.cs` | Creates a C# script from the system clipboard. See **Create Script From Clipboard** below. |
| **M** | `MasterSceneButton.cs` | Enters Play mode and temporarily enables the script's master-scene-on-play flag. The actual scene-loading behavior depends on the project code that reads the saved `SceneAutoLoader.LoadMasterOnPlay` and `SceneAutoLoader.LoadMasterStatus` EditorPrefs keys. |

### Create Script From Clipboard

1. Copy either a C# class name or a complete C# script to the system clipboard.
2. Click **S** in the Unity toolbar.
3. The tool writes the script into the selected script-creation folder, or into `Assets/Scripts/Generated/` if no valid folder has been configured.
4. Unity refreshes the Asset Database and opens the created script when it can load the asset.

If the clipboard contains `class `, the tool treats it as a complete script and extracts the first class name it finds. Otherwise, it uses the clipboard text as the new class name and creates a `MonoBehaviour` template with `Start` and `Update` methods.

The tool generates a namespace from the target folder path. If a complete script already has a namespace, the tool replaces that namespace with the generated namespace. If it has no namespace, the tool wraps the script in the generated namespace when one can be generated.

To choose the target folder, select a folder in the Project window and use **Assets > Extras > Set Script Creation Folder**. The setting is stored in EditorPrefs.

## Other Editor Tools

### Clever Clicker

**File:** `Editor/CleverClicker/CleverClicker.cs`

In the Scene view, right-click an object area without holding Alt to open a menu of GameObjects under the mouse. Select an entry to select that GameObject. The menu shows hierarchy paths to help distinguish objects with similar names. The tool uses a pick radius of 40 pixels and lists up to 50 results.

### Copy & Remove Component

**File:** `Editor/CopyRemoveTransforComponent/CopyRemoveTransform.cs`

In the Inspector, open the context menu for a component and select **Copy & Remove Component**. The tool copies the component using Unity's component utility, then removes it from the GameObject. This is useful when you want to paste the copied component elsewhere.

**Important:** The component is removed immediately after it is copied. Paste it onto the intended target before you lose the copied component data.

### Shape Sprite Creator

**File:** `Editor/CustomSpriteCreator/CustomSpriteCreator.cs`

Open **Tools > Shape Sprite Creator**.

The window can generate these shapes:

- Rectangle
- Rounded rectangle
- Circle
- Ellipse
- Triangle
- N-gon
- Star
- Line
- Capsule

The window includes shape settings, size presets, preview options, fill and outline settings, export settings, and a **Create Sprite** button. You can configure pixel dimensions, colors, outline thickness, pixels per unit, and the output file name. The default fallback output folder is `Assets/GeneratedShapes`; when supported by the tool's save logic, a selected Project folder can be used instead.

## HotKeyShortcuts

**File:** `HotKeyShortcuts.cs`  
**Namespace:** `IDZBase.Core.Scripts.Editor`

This script adds menu items under **Assets > Extras**.

| Menu item | Shortcut | What it does |
|---|---|---|
| **Create Dummy Folders** | None | Creates `Scenes`, `Sounds`, `Prefabs`, `Scripts`, `Materials`, `Sprites`, and `Animations` inside the selected asset's path when those folders do not already exist. `Sounds` appears twice in the source list, but the duplicate does not create a second folder. Select a folder in the Project window before using this command. |
| **ToggleAllColliderGizmos** | `Shift+Alt+C` | Toggles the Physics 2D **Always Show Colliders** setting and logs the new state. |
| **Set Script Creation Folder** | `Shift+Alt+S` | Stores the selected Project folder as the target for the **S** toolbar button. Select a folder first. If you select a file, the tool uses its containing folder. |
| **Create Folder Structure** | `Shift+Alt+F` | Reads a folder tree from the system clipboard and creates that structure inside the selected root folder. |

### Create Folder Structure: Clipboard Format

1. Copy a folder tree into the system clipboard.
2. Select the destination root folder in the Project window.
3. Run **Assets > Extras > Create Folder Structure**, or press **Shift+Alt+F**.
4. The tool creates the folders under the selected root and refreshes the Asset Database.

Example clipboard content:

```text
Assets
├── Scripts
│   ├── Player
│   └── UI
├── Prefabs
│   └── Characters
└── Sprites
```

The parser uses tree branches (`├──` and `└──`) and indentation to determine folder depth. It skips lines named `Assets` and entries containing `.cs`. Existing folders are reused.

## Notes and Limitations

- These tools run in the Unity Editor. They are not runtime gameplay systems.
- `Clear PlayerPrefs` deletes all PlayerPrefs. It does not clear only selected keys.
- Fast Play disables Domain Reload and Scene Reload for that Play session. Static state and scene state may therefore behave differently from a normal Play session.
- The **M** button sets EditorPrefs flags. A separate project scene-loader implementation must read those flags and load the master scene for that behavior to occur.
- The Android resolver button depends on EDM4U menu commands being available and the Android platform being active.
- Back up important project data before using destructive tools.
