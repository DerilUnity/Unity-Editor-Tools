using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ShapeSpriteCreator : EditorWindow
{
    public enum ShapeType
    {
        Rectangle,
        RoundedRectangle,
        Circle,
        Ellipse,
        Triangle,
        NGon,
        Star,
        Line,
        Capsule
    }

    public enum FillMode
    {
        Fill,
        Outline,
        FillAndOutline
    }

    public enum CapsuleOrientation
    {
        Horizontal,
        Vertical
    }

    // ------------------------------
    // General settings / state
    // ------------------------------
    ShapeType shapeType = ShapeType.Rectangle;
    FillMode fillMode = FillMode.Fill;

    Color fillColor = Color.white;
    Color outlineColor = Color.black;
    int outlineThickness = 1;

    string fileName = "NewShapeSprite";
    int pixelsPerUnit = 16;

    // Save folder (fallback if no folder selected in Project)
    string customSaveFolder = "Assets/GeneratedShapes";

    // Shape parameters
    int width = 8;
    int height = 8;

    int cornerRadius = 2;

    int radius = 4;
    int radiusX = 4;
    int radiusY = 6;

    int sides = 5;
    float innerRadiusFactor = 0.5f; // star inner factor

    int lineLength = 8;
    int lineThickness = 2;
    float lineAngle = 0f;

    CapsuleOrientation capsuleOrientation = CapsuleOrientation.Horizontal;
    int capsuleRadius = 4;

    // Preview
    Texture2D previewTexture;
    bool pixelPerfectPreview = false;

    [MenuItem("Tools/Shape Sprite Creator")]
    public static void ShowWindow()
    {
        GetWindow<ShapeSpriteCreator>("Shape Sprite Creator");
    }

    void OnGUI()
    {
        GUILayout.Label("Shape Sprite Settings", EditorStyles.boldLabel);

        shapeType = (ShapeType)EditorGUILayout.EnumPopup("Shape", shapeType);

        EditorGUILayout.Space(4);

        DrawShapeSpecificUI();

        EditorGUILayout.Space(6);
        DrawPresetsUI();

        EditorGUILayout.Space(10);
        DrawPreviewUI();

        EditorGUILayout.Space(10);
        DrawFillOutlineUI();

        EditorGUILayout.Space(10);
        DrawExportUI();

        EditorGUILayout.Space(10);
        if (GUILayout.Button("Create Sprite"))
        {
            CreateSprite();
        }
    }

    // ------------------------------
    // Shape-specific controls
    // ------------------------------
    void DrawShapeSpecificUI()
    {
        switch (shapeType)
        {
            case ShapeType.Rectangle:
                width = Mathf.Max(1, EditorGUILayout.IntField("Width (px)", width));
                height = Mathf.Max(1, EditorGUILayout.IntField("Height (px)", height));
                break;

            case ShapeType.RoundedRectangle:
                width = Mathf.Max(1, EditorGUILayout.IntField("Width (px)", width));
                height = Mathf.Max(1, EditorGUILayout.IntField("Height (px)", height));
                cornerRadius = Mathf.Max(0, EditorGUILayout.IntField("Corner Radius (px)", cornerRadius));
                break;

            case ShapeType.Circle:
                radius = Mathf.Max(1, EditorGUILayout.IntField("Radius (px)", radius));
                break;

            case ShapeType.Ellipse:
                radiusX = Mathf.Max(1, EditorGUILayout.IntField("Radius X (px)", radiusX));
                radiusY = Mathf.Max(1, EditorGUILayout.IntField("Radius Y (px)", radiusY));
                break;

            case ShapeType.Triangle:
                radius = Mathf.Max(1, EditorGUILayout.IntField("Radius (px)", radius));
                break;

            case ShapeType.NGon:
                sides = Mathf.Max(3, EditorGUILayout.IntField("Sides", sides));
                radius = Mathf.Max(1, EditorGUILayout.IntField("Radius (px)", radius));
                break;

            case ShapeType.Star:
                sides = Mathf.Max(3, EditorGUILayout.IntField("Points", sides));
                radius = Mathf.Max(1, EditorGUILayout.IntField("Outer Radius (px)", radius));
                innerRadiusFactor = EditorGUILayout.Slider("Inner Radius Factor", innerRadiusFactor, 0.1f, 0.9f);
                break;

            case ShapeType.Line:
                lineLength = Mathf.Max(1, EditorGUILayout.IntField("Length (px)", lineLength));
                lineThickness = Mathf.Max(1, EditorGUILayout.IntField("Thickness (px)", lineThickness));
                lineAngle = EditorGUILayout.FloatField("Angle (deg)", lineAngle);
                break;

            case ShapeType.Capsule:
                capsuleOrientation = (CapsuleOrientation)EditorGUILayout.EnumPopup("Orientation", capsuleOrientation);
                width = Mathf.Max(1, EditorGUILayout.IntField("Width (px)", width));
                height = Mathf.Max(1, EditorGUILayout.IntField("Height (px)", height));
                capsuleRadius = Mathf.Max(1, EditorGUILayout.IntField("Cap Radius (px)", capsuleRadius));
                break;
        }
    }

    // ------------------------------
    // Presets
    // ------------------------------
    void DrawPresetsUI()
    {
        GUILayout.Label("Presets", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (shapeType == ShapeType.Rectangle || shapeType == ShapeType.RoundedRectangle || shapeType == ShapeType.Capsule)
        {
            if (GUILayout.Button("2×2")) { width = 2; height = 2; }
            if (GUILayout.Button("4×4")) { width = 4; height = 4; }
            if (GUILayout.Button("8×8")) { width = 8; height = 8; }
            if (GUILayout.Button("16×16")) { width = 16; height = 16; }
            if (GUILayout.Button("32×32")) { width = 32; height = 32; }
            if (GUILayout.Button("64×64")) { width = 64; height = 64; }
        }
        else if (shapeType == ShapeType.Circle || shapeType == ShapeType.Triangle || shapeType == ShapeType.NGon || shapeType == ShapeType.Star)
        {
            if (GUILayout.Button("r = 4")) { radius = 4; }
            if (GUILayout.Button("r = 8")) { radius = 8; }
            if (GUILayout.Button("r = 16")) { radius = 16; }
            if (GUILayout.Button("r = 32")) { radius = 32; }
        }
        else if (shapeType == ShapeType.Ellipse)
        {
            if (GUILayout.Button("8×8")) { radiusX = 8; radiusY = 8; }
            if (GUILayout.Button("16×8")) { radiusX = 16; radiusY = 8; }
            if (GUILayout.Button("16×16")) { radiusX = 16; radiusY = 16; }
            if (GUILayout.Button("32×16")) { radiusX = 32; radiusY = 16; }
        }
        else if (shapeType == ShapeType.Line)
        {
            if (GUILayout.Button("Len 8")) { lineLength = 8; }
            if (GUILayout.Button("Len 16")) { lineLength = 16; }
            if (GUILayout.Button("Len 32")) { lineLength = 32; }
            if (GUILayout.Button("Horiz")) { lineAngle = 0f; }
            if (GUILayout.Button("Vert")) { lineAngle = 90f; }
        }
        EditorGUILayout.EndHorizontal();
    }

    // ------------------------------
    // Preview
    // ------------------------------
    void DrawPreviewUI()
    {
        GUILayout.Label("Preview", EditorStyles.boldLabel);
        pixelPerfectPreview = EditorGUILayout.Toggle("Pixel Perfect Preview", pixelPerfectPreview);

        // Generate preview texture on repaint
        if (Event.current.type == EventType.Repaint)
        {
            GeneratePreviewTexture();
        }

        if (previewTexture == null)
        {
            GUILayout.Label("No preview yet.");
            return;
        }

        Rect rect = GUILayoutUtility.GetRect(128, 128, GUILayout.ExpandWidth(true));

        if (pixelPerfectPreview)
        {
            float texW = previewTexture.width;
            float texH = previewTexture.height;

            float drawW = Mathf.Min(texW, rect.width);
            float drawH = Mathf.Min(texH, rect.height);

            Rect drawRect = new Rect(
                rect.x + (rect.width - drawW) * 0.5f,
                rect.y + (rect.height - drawH) * 0.5f,
                drawW,
                drawH
            );

            GUI.DrawTexture(drawRect, previewTexture, ScaleMode.ScaleAndCrop, true);
        }
        else
        {
            GUI.DrawTexture(rect, previewTexture, ScaleMode.ScaleToFit, true);
        }
    }

    void GeneratePreviewTexture()
    {
        int tw, th;
        Texture2D tex = GenerateShapeTexture(out tw, out th);

        if (previewTexture != null)
        {
            DestroyImmediate(previewTexture);
        }

        previewTexture = tex;
    }

    // ------------------------------
    // Fill / Outline UI
    // ------------------------------
    void DrawFillOutlineUI()
    {
        GUILayout.Label("Fill & Outline", EditorStyles.boldLabel);
        fillMode = (FillMode)EditorGUILayout.EnumPopup("Mode", fillMode);
        fillColor = EditorGUILayout.ColorField("Fill Color", fillColor);
        outlineColor = EditorGUILayout.ColorField("Outline Color", outlineColor);
        outlineThickness = Mathf.Max(1, EditorGUILayout.IntField("Outline Thickness", outlineThickness));
    }

    // ------------------------------
    // Export UI
    // ------------------------------
    void DrawExportUI()
    {
        GUILayout.Label("Export Settings", EditorStyles.boldLabel);
        fileName = EditorGUILayout.TextField("File Name", fileName);
        pixelsPerUnit = Mathf.Max(1, EditorGUILayout.IntField("Pixels Per Unit", pixelsPerUnit));

        EditorGUILayout.Space(2);
        GUILayout.Label("Save Folder (fallback, if no Project folder selected)", EditorStyles.miniBoldLabel);
        customSaveFolder = EditorGUILayout.TextField("Folder (under Assets)", customSaveFolder);
    }

    // ------------------------------
    // Create Sprite
    // ------------------------------
    void CreateSprite()
    {
        int texWidth, texHeight;
        Texture2D tex = GenerateShapeTexture(out texWidth, out texHeight);

        // Determine destination folder
        string projectPath = Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length);
        string destFolder = GetDestinationFolder(projectPath);
        string filePath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(destFolder, fileName + ".png"));

        string fullPath = Path.Combine(projectPath, filePath);

        // Ensure directory exists
        string fullDir = Path.GetDirectoryName(fullPath);
        if (!Directory.Exists(fullDir))
        {
            Directory.CreateDirectory(fullDir);
        }

        File.WriteAllBytes(fullPath, tex.EncodeToPNG());
        AssetDatabase.Refresh();

        TextureImporter importer = AssetImporter.GetAtPath(filePath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;

            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
        }

        Debug.Log("Shape sprite created at: " + filePath);
    }

    string GetDestinationFolder(string projectPath)
    {
        string destFolder = customSaveFolder;

        Object activeObj = Selection.activeObject;
        if (activeObj != null)
        {
            string selPath = AssetDatabase.GetAssetPath(activeObj);
            if (!string.IsNullOrEmpty(selPath))
            {
                if (Directory.Exists(selPath))
                {
                    destFolder = selPath;
                }
                else
                {
                    string dir = Path.GetDirectoryName(selPath);
                    if (!string.IsNullOrEmpty(dir))
                        destFolder = dir;
                }
            }
        }

        if (string.IsNullOrEmpty(destFolder) || !destFolder.StartsWith("Assets"))
        {
            destFolder = "Assets";
        }

        // Make sure folder exists on disk
        string fullDir = Path.Combine(projectPath, destFolder);
        if (!Directory.Exists(fullDir))
        {
            Directory.CreateDirectory(fullDir);
            AssetDatabase.Refresh();
        }

        return destFolder;
    }

    // ------------------------------
    // Shape texture generation
    // ------------------------------
    Texture2D GenerateShapeTexture(out int texWidth, out int texHeight)
    {
        // Decide texture size based on shape
        switch (shapeType)
        {
            case ShapeType.Rectangle:
            case ShapeType.RoundedRectangle:
            case ShapeType.Capsule:
                texWidth = Mathf.Max(1, width);
                texHeight = Mathf.Max(1, height);
                break;

            case ShapeType.Circle:
                texWidth = texHeight = Mathf.Max(1, radius * 2);
                break;

            case ShapeType.Ellipse:
                texWidth = Mathf.Max(1, radiusX * 2);
                texHeight = Mathf.Max(1, radiusY * 2);
                break;

            case ShapeType.Triangle:
            case ShapeType.NGon:
            case ShapeType.Star:
                texWidth = texHeight = Mathf.Max(1, radius * 2);
                break;

            case ShapeType.Line:
                texWidth = Mathf.Max(1, lineLength + lineThickness * 2);
                texHeight = Mathf.Max(1, lineLength + lineThickness * 2);
                break;

            default:
                texWidth = texHeight = 8;
                break;
        }

        Texture2D tex = new Texture2D(texWidth, texHeight, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[texWidth * texHeight];
        bool[] inside = new bool[texWidth * texHeight];

        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = new Color(0, 0, 0, 0);
            inside[i] = false;
        }

        FillInsideMask(shapeType, texWidth, texHeight, inside);
        ApplyFillAndOutline(texWidth, texHeight, pixels, inside);

        tex.SetPixels(pixels);
        tex.Apply();

        return tex;
    }

    // ------------------------------
    // Shape mask generation
    // ------------------------------
    void FillInsideMask(ShapeType type, int texWidth, int texHeight, bool[] inside)
    {
        switch (type)
        {
            case ShapeType.Rectangle:
                FillRectangleMask(texWidth, texHeight, inside);
                break;

            case ShapeType.RoundedRectangle:
                FillRoundedRectMask(texWidth, texHeight, inside);
                break;

            case ShapeType.Circle:
                FillCircleMask(texWidth, texHeight, inside, radius);
                break;

            case ShapeType.Ellipse:
                FillEllipseMask(texWidth, texHeight, inside, radiusX, radiusY);
                break;

            case ShapeType.Triangle:
                FillPolygonMask(texWidth, texHeight, inside, BuildRegularPolygon(texWidth, texHeight, 3, radius));
                break;

            case ShapeType.NGon:
                FillPolygonMask(texWidth, texHeight, inside, BuildRegularPolygon(texWidth, texHeight, sides, radius));
                break;

            case ShapeType.Star:
                FillPolygonMask(texWidth, texHeight, inside, BuildStarPolygon(texWidth, texHeight, sides, radius, innerRadiusFactor));
                break;

            case ShapeType.Line:
                FillPolygonMask(texWidth, texHeight, inside, BuildLineQuad(texWidth, texHeight, lineLength, lineThickness, lineAngle));
                break;

            case ShapeType.Capsule:
                FillCapsuleMask(texWidth, texHeight, inside);
                break;
        }
    }

    void FillRectangleMask(int w, int h, bool[] inside)
    {
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                inside[y * w + x] = true;
            }
        }
    }

    void FillRoundedRectMask(int w, int h, bool[] inside)
    {
        float cx = (w - 1) / 2f;
        float cy = (h - 1) / 2f;
        float halfW = w / 2f;
        float halfH = h / 2f;
        float r = Mathf.Clamp(cornerRadius, 0, Mathf.Min(halfW, halfH));

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float px = x - cx;
                float py = y - cy;
                float ax = Mathf.Abs(px);
                float ay = Mathf.Abs(py);

                float innerW = halfW - r;
                float innerH = halfH - r;

                bool insideRect = (ax <= innerW && ay <= halfH) || (ay <= innerH && ax <= halfW);

                bool insideCorner = false;
                if (!insideRect)
                {
                    float dx = Mathf.Max(ax - innerW, 0);
                    float dy = Mathf.Max(ay - innerH, 0);
                    insideCorner = dx * dx + dy * dy <= r * r;
                }

                inside[y * w + x] = insideRect || insideCorner;
            }
        }
    }

    void FillCircleMask(int w, int h, bool[] inside, int r)
    {
        float cx = (w - 1) / 2f;
        float cy = (h - 1) / 2f;
        float rr = r * r;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float dx = x - cx;
                float dy = y - cy;
                inside[y * w + x] = dx * dx + dy * dy <= rr;
            }
        }
    }

    void FillEllipseMask(int w, int h, bool[] inside, int rx, int ry)
    {
        float cx = (w - 1) / 2f;
        float cy = (h - 1) / 2f;
        float rx2 = rx * rx;
        float ry2 = ry * ry;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float dx = x - cx;
                float dy = y - cy;
                float v = (dx * dx) / rx2 + (dy * dy) / ry2;
                inside[y * w + x] = v <= 1f;
            }
        }
    }

    void FillCapsuleMask(int w, int h, bool[] inside)
    {
        float cx = (w - 1) / 2f;
        float cy = (h - 1) / 2f;
        float halfW = w / 2f;
        float halfH = h / 2f;
        float r = capsuleRadius;

        if (capsuleOrientation == CapsuleOrientation.Horizontal)
            r = Mathf.Min(r, halfH);
        else
            r = Mathf.Min(r, halfW);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float px = x - cx;
                float py = y - cy;
                bool insideShape = false;

                if (capsuleOrientation == CapsuleOrientation.Horizontal)
                {
                    if (Mathf.Abs(py) <= halfH && Mathf.Abs(px) <= halfW - r)
                    {
                        insideShape = true;
                    }
                    else
                    {
                        Vector2 leftCenter = new Vector2(-halfW + r, 0);
                        Vector2 rightCenter = new Vector2(halfW - r, 0);
                        Vector2 p = new Vector2(px, py);

                        if ((p - leftCenter).sqrMagnitude <= r * r ||
                            (p - rightCenter).sqrMagnitude <= r * r)
                            insideShape = true;
                    }
                }
                else
                {
                    if (Mathf.Abs(px) <= halfW && Mathf.Abs(py) <= halfH - r)
                    {
                        insideShape = true;
                    }
                    else
                    {
                        Vector2 bottomCenter = new Vector2(0, -halfH + r);
                        Vector2 topCenter = new Vector2(0, halfH - r);
                        Vector2 p = new Vector2(px, py);

                        if ((p - bottomCenter).sqrMagnitude <= r * r ||
                            (p - topCenter).sqrMagnitude <= r * r)
                            insideShape = true;
                    }
                }

                inside[y * w + x] = insideShape;
            }
        }
    }

    void FillPolygonMask(int w, int h, bool[] inside, List<Vector2> vertices)
    {
        if (vertices == null || vertices.Count < 3)
            return;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (PointInPolygon(new Vector2(x + 0.5f, y + 0.5f), vertices))
                    inside[y * w + x] = true;
            }
        }
    }

    bool PointInPolygon(Vector2 p, List<Vector2> vertices)
    {
        bool result = false;
        int j = vertices.Count - 1;

        for (int i = 0; i < vertices.Count; i++)
        {
            Vector2 vi = vertices[i];
            Vector2 vj = vertices[j];

            if ((vi.y > p.y) != (vj.y > p.y) &&
                p.x < (vj.x - vi.x) * (p.y - vi.y) / (vj.y - vi.y) + vi.x)
            {
                result = !result;
            }

            j = i;
        }

        return result;
    }

    // ------------------------------
    // Polygon builders
    // ------------------------------
    List<Vector2> BuildRegularPolygon(int w, int h, int sides, int radius)
    {
        List<Vector2> verts = new List<Vector2>();
        float cx = (w - 1) / 2f;
        float cy = (h - 1) / 2f;

        for (int i = 0; i < sides; i++)
        {
            float angle = (Mathf.PI * 2f * i) / sides - Mathf.PI / 2f;
            float x = cx + Mathf.Cos(angle) * radius;
            float y = cy + Mathf.Sin(angle) * radius;
            verts.Add(new Vector2(x, y));
        }

        return verts;
    }

    List<Vector2> BuildStarPolygon(int w, int h, int points, int outerRadius, float innerFactor)
    {
        List<Vector2> verts = new List<Vector2>();
        float cx = (w - 1) / 2f;
        float cy = (h - 1) / 2f;
        float innerRadius = outerRadius * innerFactor;
        int total = points * 2;

        for (int i = 0; i < total; i++)
        {
            float angle = (Mathf.PI * 2f * i) / total - Mathf.PI / 2f;
            float r = (i % 2 == 0) ? outerRadius : innerRadius;
            float x = cx + Mathf.Cos(angle) * r;
            float y = cy + Mathf.Sin(angle) * r;
            verts.Add(new Vector2(x, y));
        }

        return verts;
    }

    List<Vector2> BuildLineQuad(int w, int h, int length, int thickness, float angleDeg)
    {
        List<Vector2> verts = new List<Vector2>();
        float cx = (w - 1) / 2f;
        float cy = (h - 1) / 2f;

        float angle = angleDeg * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        Vector2 perp = new Vector2(-dir.y, dir.x);

        float halfLen = length / 2f;
        float halfThick = thickness / 2f;

        Vector2 center = new Vector2(cx, cy);

        Vector2 p1 = center + dir * halfLen + perp * halfThick;
        Vector2 p2 = center + dir * halfLen - perp * halfThick;
        Vector2 p3 = center - dir * halfLen - perp * halfThick;
        Vector2 p4 = center - dir * halfLen + perp * halfThick;

        verts.Add(p1);
        verts.Add(p2);
        verts.Add(p3);
        verts.Add(p4);

        return verts;
    }

    // ------------------------------
    // Fill & outline application
    // ------------------------------
    void ApplyFillAndOutline(int w, int h, Color[] pixels, bool[] inside)
    {
        // Fill
        if (fillMode == FillMode.Fill || fillMode == FillMode.FillAndOutline)
        {
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int idx = y * w + x;
                    if (inside[idx])
                    {
                        pixels[idx] = fillColor;
                    }
                }
            }
        }

        // Outline
        if (fillMode == FillMode.Outline || fillMode == FillMode.FillAndOutline)
        {
            int maxOffset = outlineThickness;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int idx = y * w + x;
                    if (!inside[idx])
                        continue;

                    bool isBorder = false;

                    for (int oy = -maxOffset; oy <= maxOffset && !isBorder; oy++)
                    {
                        for (int ox = -maxOffset; ox <= maxOffset && !isBorder; ox++)
                        {
                            if (ox == 0 && oy == 0) continue;
                            int nx = x + ox;
                            int ny = y + oy;
                            if (nx < 0 || nx >= w || ny < 0 || ny >= h)
                            {
                                isBorder = true;
                            }
                            else
                            {
                                int nIdx = ny * w + nx;
                                if (!inside[nIdx])
                                    isBorder = true;
                            }
                        }
                    }

                    if (isBorder)
                    {
                        pixels[idx] = outlineColor;
                    }
                    else if (fillMode == FillMode.Outline)
                    {
                        pixels[idx] = new Color(0, 0, 0, 0);
                    }
                }
            }
        }

        if (fillMode == FillMode.Outline)
        {
            for (int i = 0; i < pixels.Length; i++)
            {
                if (!inside[i])
                    pixels[i] = new Color(0, 0, 0, 0);
            }
        }
    }
}



/*
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ShapeSpriteCreator : EditorWindow
{
    public enum ShapeType
    {
        Rectangle,
        RoundedRectangle,
        Circle,
        Ellipse,
        Triangle,
        NGon,
        Star,
        Line,
        Capsule
    }

    public enum FillMode
    {
        Fill,
        Outline,
        FillAndOutline
    }

    public enum CapsuleOrientation
    {
        Horizontal,
        Vertical
    }

    // General settings
    ShapeType shapeType = ShapeType.Rectangle;
    FillMode fillMode = FillMode.Fill;
    Color fillColor = Color.white;
    Color outlineColor = Color.black;
    int outlineThickness = 1;

    string fileName = "NewShapeSprite";
    int pixelsPerUnit = 16;

    // Rectangle / RoundedRect / Capsule / Line base size
    int width = 8;
    int height = 8;

    // Rounded rectangle
    int cornerRadius = 2;

    // Circle / Ellipse / Polygon / Star radius
    int radius = 4;
    int radiusX = 4;
    int radiusY = 6;

    // NGon / Star
    int sides = 5;
    float innerRadiusFactor = 0.5f; // for star

    // Line
    int lineLength = 8;
    int lineThickness = 2;
    float lineAngle = 0f;

    // Capsule
    CapsuleOrientation capsuleOrientation = CapsuleOrientation.Horizontal;
    int capsuleRadius = 4;

    [MenuItem("Tools/Shape Sprite Creator")]
    public static void ShowWindow()
    {
        GetWindow<ShapeSpriteCreator>("Shape Sprite Creator");
    }

    void OnGUI()
    {
        GUILayout.Label("Shape Sprite Settings", EditorStyles.boldLabel);

        shapeType = (ShapeType)EditorGUILayout.EnumPopup("Shape", shapeType);

        EditorGUILayout.Space(5);

        // Shape-specific UI
        DrawShapeSpecificUI();

        EditorGUILayout.Space(10);

        // Fill & Outline settings
        GUILayout.Label("Fill & Outline", EditorStyles.boldLabel);
        fillMode = (FillMode)EditorGUILayout.EnumPopup("Mode", fillMode);
        fillColor = EditorGUILayout.ColorField("Fill Color", fillColor);
        outlineColor = EditorGUILayout.ColorField("Outline Color", outlineColor);
        outlineThickness = Mathf.Max(1, EditorGUILayout.IntField("Outline Thickness", outlineThickness));

        EditorGUILayout.Space(10);

        // Export options
        GUILayout.Label("Export Settings", EditorStyles.boldLabel);
        fileName = EditorGUILayout.TextField("File Name", fileName);
        pixelsPerUnit = Mathf.Max(1, EditorGUILayout.IntField("Pixels Per Unit", pixelsPerUnit));

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Create Sprite"))
        {
            CreateSprite();
        }
    }

    void DrawShapeSpecificUI()
    {
        switch (shapeType)
        {
            case ShapeType.Rectangle:
                width = Mathf.Max(1, EditorGUILayout.IntField("Width (px)", width));
                height = Mathf.Max(1, EditorGUILayout.IntField("Height (px)", height));
                break;

            case ShapeType.RoundedRectangle:
                width = Mathf.Max(1, EditorGUILayout.IntField("Width (px)", width));
                height = Mathf.Max(1, EditorGUILayout.IntField("Height (px)", height));
                cornerRadius = Mathf.Max(0, EditorGUILayout.IntField("Corner Radius (px)", cornerRadius));
                break;

            case ShapeType.Circle:
                radius = Mathf.Max(1, EditorGUILayout.IntField("Radius (px)", radius));
                break;

            case ShapeType.Ellipse:
                radiusX = Mathf.Max(1, EditorGUILayout.IntField("Radius X (px)", radiusX));
                radiusY = Mathf.Max(1, EditorGUILayout.IntField("Radius Y (px)", radiusY));
                break;

            case ShapeType.Triangle:
                radius = Mathf.Max(1, EditorGUILayout.IntField("Radius (px)", radius));
                break;

            case ShapeType.NGon:
                sides = Mathf.Max(3, EditorGUILayout.IntField("Sides", sides));
                radius = Mathf.Max(1, EditorGUILayout.IntField("Radius (px)", radius));
                break;

            case ShapeType.Star:
                sides = Mathf.Max(3, EditorGUILayout.IntField("Points", sides));
                radius = Mathf.Max(1, EditorGUILayout.IntField("Outer Radius (px)", radius));
                innerRadiusFactor = EditorGUILayout.Slider("Inner Radius Factor", innerRadiusFactor, 0.1f, 0.9f);
                break;

            case ShapeType.Line:
                lineLength = Mathf.Max(1, EditorGUILayout.IntField("Length (px)", lineLength));
                lineThickness = Mathf.Max(1, EditorGUILayout.IntField("Thickness (px)", lineThickness));
                lineAngle = EditorGUILayout.FloatField("Angle (deg)", lineAngle);
                break;

            case ShapeType.Capsule:
                capsuleOrientation = (CapsuleOrientation)EditorGUILayout.EnumPopup("Orientation", capsuleOrientation);
                width = Mathf.Max(1, EditorGUILayout.IntField("Width (px)", width));
                height = Mathf.Max(1, EditorGUILayout.IntField("Height (px)", height));
                capsuleRadius = Mathf.Max(1, EditorGUILayout.IntField("Cap Radius (px)", capsuleRadius));
                break;
        }
    }

    void CreateSprite()
    {
        int texWidth, texHeight;

        // Decide texture size based on shape
        switch (shapeType)
        {
            case ShapeType.Rectangle:
            case ShapeType.RoundedRectangle:
            case ShapeType.Capsule:
                texWidth = Mathf.Max(1, width);
                texHeight = Mathf.Max(1, height);
                break;

            case ShapeType.Circle:
                texWidth = texHeight = Mathf.Max(1, radius * 2);
                break;

            case ShapeType.Ellipse:
                texWidth = Mathf.Max(1, radiusX * 2);
                texHeight = Mathf.Max(1, radiusY * 2);
                break;

            case ShapeType.Triangle:
            case ShapeType.NGon:
            case ShapeType.Star:
                texWidth = texHeight = Mathf.Max(1, radius * 2);
                break;

            case ShapeType.Line:
                // Give some padding around the line
                texWidth = Mathf.Max(1, lineLength + lineThickness * 2);
                texHeight = Mathf.Max(1, lineLength + lineThickness * 2);
                break;

            default:
                texWidth = texHeight = 8;
                break;
        }

        Texture2D tex = new Texture2D(texWidth, texHeight, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[texWidth * texHeight];
        bool[] inside = new bool[texWidth * texHeight];

        // Initialize all pixels as transparent
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = new Color(0, 0, 0, 0);
            inside[i] = false;
        }

        // Fill inside[] mask for the selected shape
        FillInsideMask(shapeType, texWidth, texHeight, inside);

        // Apply fill and outline based on fillMode
        ApplyFillAndOutline(texWidth, texHeight, pixels, inside);

        tex.SetPixels(pixels);
        tex.Apply();

        // Save PNG inside project
        string path = EditorUtility.SaveFilePanelInProject("Save Sprite", fileName, "png", "Save sprite as PNG");
        if (string.IsNullOrEmpty(path))
            return;

        File.WriteAllBytes(path, tex.EncodeToPNG());
        AssetDatabase.Refresh();

        // Import as sprite
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;

            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
        }

        Debug.Log("Shape sprite created at: " + path);
    }

    // ------------------------------------------------------
    //  Shape mask generation
    // ------------------------------------------------------

    void FillInsideMask(ShapeType type, int texWidth, int texHeight, bool[] inside)
    {
        switch (type)
        {
            case ShapeType.Rectangle:
                FillRectangleMask(texWidth, texHeight, inside);
                break;
            case ShapeType.RoundedRectangle:
                FillRoundedRectMask(texWidth, texHeight, inside);
                break;
            case ShapeType.Circle:
                FillCircleMask(texWidth, texHeight, inside, radius);
                break;
            case ShapeType.Ellipse:
                FillEllipseMask(texWidth, texHeight, inside, radiusX, radiusY);
                break;
            case ShapeType.Triangle:
                FillPolygonMask(texWidth, texHeight, inside, BuildRegularPolygon(texWidth, texHeight, 3, radius));
                break;
            case ShapeType.NGon:
                FillPolygonMask(texWidth, texHeight, inside, BuildRegularPolygon(texWidth, texHeight, sides, radius));
                break;
            case ShapeType.Star:
                FillPolygonMask(texWidth, texHeight, inside, BuildStarPolygon(texWidth, texHeight, sides, radius, innerRadiusFactor));
                break;
            case ShapeType.Line:
                FillPolygonMask(texWidth, texHeight, inside, BuildLineQuad(texWidth, texHeight, lineLength, lineThickness, lineAngle));
                break;
            case ShapeType.Capsule:
                FillCapsuleMask(texWidth, texHeight, inside);
                break;
        }
    }

    void FillRectangleMask(int w, int h, bool[] inside)
    {
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                inside[y * w + x] = true;
            }
        }
    }

    void FillRoundedRectMask(int w, int h, bool[] inside)
    {
        float cx = (w - 1) / 2f;
        float cy = (h - 1) / 2f;
        float halfW = w / 2f;
        float halfH = h / 2f;
        float r = Mathf.Clamp(cornerRadius, 0, Mathf.Min(halfW, halfH));

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                // position relative to center
                float px = x - cx;
                float py = y - cy;
                float ax = Mathf.Abs(px);
                float ay = Mathf.Abs(py);

                float innerW = halfW - r;
                float innerH = halfH - r;

                bool insideRect = (ax <= innerW && ay <= halfH) || (ay <= innerH && ax <= halfW);

                bool insideCorner = false;
                if (!insideRect)
                {
                    float dx = Mathf.Max(ax - innerW, 0);
                    float dy = Mathf.Max(ay - innerH, 0);
                    insideCorner = dx * dx + dy * dy <= r * r;
                }

                inside[y * w + x] = insideRect || insideCorner;
            }
        }
    }

    void FillCircleMask(int w, int h, bool[] inside, int r)
    {
        float cx = (w - 1) / 2f;
        float cy = (h - 1) / 2f;
        float rr = r * r;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float dx = x - cx;
                float dy = y - cy;
                inside[y * w + x] = dx * dx + dy * dy <= rr;
            }
        }
    }

    void FillEllipseMask(int w, int h, bool[] inside, int rx, int ry)
    {
        float cx = (w - 1) / 2f;
        float cy = (h - 1) / 2f;
        float rx2 = rx * rx;
        float ry2 = ry * ry;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float dx = x - cx;
                float dy = y - cy;
                float v = (dx * dx) / rx2 + (dy * dy) / ry2;
                inside[y * w + x] = v <= 1f;
            }
        }
    }

    void FillCapsuleMask(int w, int h, bool[] inside)
    {
        float cx = (w - 1) / 2f;
        float cy = (h - 1) / 2f;
        float halfW = w / 2f;
        float halfH = h / 2f;
        float r = capsuleRadius;

        // Clamp radius
        if (capsuleOrientation == CapsuleOrientation.Horizontal)
            r = Mathf.Min(r, halfH);
        else
            r = Mathf.Min(r, halfW);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float px = x - cx;
                float py = y - cy;
                bool insideShape = false;

                if (capsuleOrientation == CapsuleOrientation.Horizontal)
                {
                    // center rect
                    if (Mathf.Abs(py) <= halfH && Mathf.Abs(px) <= halfW - r)
                    {
                        insideShape = true;
                    }
                    else
                    {
                        // left and right caps
                        Vector2 leftCenter = new Vector2(-halfW + r, 0);
                        Vector2 rightCenter = new Vector2(halfW - r, 0);
                        Vector2 p = new Vector2(px, py);

                        if ((p - leftCenter).sqrMagnitude <= r * r ||
                            (p - rightCenter).sqrMagnitude <= r * r)
                            insideShape = true;
                    }
                }
                else // Vertical
                {
                    if (Mathf.Abs(px) <= halfW && Mathf.Abs(py) <= halfH - r)
                    {
                        insideShape = true;
                    }
                    else
                    {
                        Vector2 bottomCenter = new Vector2(0, -halfH + r);
                        Vector2 topCenter = new Vector2(0, halfH - r);
                        Vector2 p = new Vector2(px, py);

                        if ((p - bottomCenter).sqrMagnitude <= r * r ||
                            (p - topCenter).sqrMagnitude <= r * r)
                            insideShape = true;
                    }
                }

                inside[y * w + x] = insideShape;
            }
        }
    }

    void FillPolygonMask(int w, int h, bool[] inside, List<Vector2> vertices)
    {
        if (vertices == null || vertices.Count < 3)
            return;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (PointInPolygon(new Vector2(x + 0.5f, y + 0.5f), vertices))
                    inside[y * w + x] = true;
            }
        }
    }

    bool PointInPolygon(Vector2 p, List<Vector2> vertices)
    {
        bool result = false;
        int j = vertices.Count - 1;

        for (int i = 0; i < vertices.Count; i++)
        {
            Vector2 vi = vertices[i];
            Vector2 vj = vertices[j];

            if ((vi.y > p.y) != (vj.y > p.y) &&
                p.x < (vj.x - vi.x) * (p.y - vi.y) / (vj.y - vi.y) + vi.x)
            {
                result = !result;
            }

            j = i;
        }

        return result;
    }

    // ------------------------------------------------------
    //  Polygon builders
    // ------------------------------------------------------

    List<Vector2> BuildRegularPolygon(int w, int h, int sides, int radius)
    {
        List<Vector2> verts = new List<Vector2>();
        float cx = (w - 1) / 2f;
        float cy = (h - 1) / 2f;

        for (int i = 0; i < sides; i++)
        {
            float angle = (Mathf.PI * 2f * i) / sides - Mathf.PI / 2f;
            float x = cx + Mathf.Cos(angle) * radius;
            float y = cy + Mathf.Sin(angle) * radius;
            verts.Add(new Vector2(x, y));
        }

        return verts;
    }

    List<Vector2> BuildStarPolygon(int w, int h, int points, int outerRadius, float innerFactor)
    {
        List<Vector2> verts = new List<Vector2>();
        float cx = (w - 1) / 2f;
        float cy = (h - 1) / 2f;
        float innerRadius = outerRadius * innerFactor;
        int total = points * 2;

        for (int i = 0; i < total; i++)
        {
            float angle = (Mathf.PI * 2f * i) / total - Mathf.PI / 2f;
            float r = (i % 2 == 0) ? outerRadius : innerRadius;
            float x = cx + Mathf.Cos(angle) * r;
            float y = cy + Mathf.Sin(angle) * r;
            verts.Add(new Vector2(x, y));
        }

        return verts;
    }

    List<Vector2> BuildLineQuad(int w, int h, int length, int thickness, float angleDeg)
    {
        List<Vector2> verts = new List<Vector2>();
        float cx = (w - 1) / 2f;
        float cy = (h - 1) / 2f;

        float angle = angleDeg * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        Vector2 perp = new Vector2(-dir.y, dir.x);

        float halfLen = length / 2f;
        float halfThick = thickness / 2f;

        Vector2 center = new Vector2(cx, cy);

        Vector2 p1 = center + dir * halfLen + perp * halfThick;
        Vector2 p2 = center + dir * halfLen - perp * halfThick;
        Vector2 p3 = center - dir * halfLen - perp * halfThick;
        Vector2 p4 = center - dir * halfLen + perp * halfThick;

        verts.Add(p1);
        verts.Add(p2);
        verts.Add(p3);
        verts.Add(p4);

        return verts;
    }

    // ------------------------------------------------------
    //  Fill & outline application
    // ------------------------------------------------------

    void ApplyFillAndOutline(int w, int h, Color[] pixels, bool[] inside)
    {
        // First apply fill (if needed)
        if (fillMode == FillMode.Fill || fillMode == FillMode.FillAndOutline)
        {
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int idx = y * w + x;
                    if (inside[idx])
                    {
                        pixels[idx] = fillColor;
                    }
                }
            }
        }

        // Then apply outline
        if (fillMode == FillMode.Outline || fillMode == FillMode.FillAndOutline)
        {
            // We'll outline pixels that are inside and have an outside neighbor
            int maxOffset = outlineThickness;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int idx = y * w + x;
                    if (!inside[idx])
                        continue;

                    bool isBorder = false;

                    for (int oy = -maxOffset; oy <= maxOffset && !isBorder; oy++)
                    {
                        for (int ox = -maxOffset; ox <= maxOffset && !isBorder; ox++)
                        {
                            if (ox == 0 && oy == 0) continue;
                            int nx = x + ox;
                            int ny = y + oy;
                            if (nx < 0 || nx >= w || ny < 0 || ny >= h)
                            {
                                isBorder = true; // edge of texture counts as border
                            }
                            else
                            {
                                int nIdx = ny * w + nx;
                                if (!inside[nIdx])
                                    isBorder = true;
                            }
                        }
                    }

                    if (isBorder)
                    {
                        pixels[idx] = outlineColor;
                    }
                    else if (fillMode == FillMode.Outline) // no fill
                    {
                        // ensure non-border inside pixels are transparent
                        pixels[idx] = new Color(0, 0, 0, 0);
                    }
                }
            }
        }

        // If mode is Outline only and we never filled, ensure outside pixels are transparent
        if (fillMode == FillMode.Outline)
        {
            for (int i = 0; i < pixels.Length; i++)
            {
                if (!inside[i])
                    pixels[i] = new Color(0, 0, 0, 0);
            }
        }
    }
}
*/
