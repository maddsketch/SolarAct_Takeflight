using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class SinglePathDesignerWindow : EditorWindow
{
    private const string OutputFolder = "Assets/ScriptableObjects/PathData";

    private sealed class PathPreset
    {
        public string Name;
        public float Speed;
        public Vector3[] Waypoints;

        public PathPreset(string name, float speed, Vector3[] waypoints)
        {
            Name = name;
            Speed = speed;
            Waypoints = waypoints;
        }
    }

    private static readonly PathPreset[] Presets =
    {
        new PathPreset("StraightDown", 4f, new[] { new Vector3(0, 0, -20) }),
        new PathPreset("SweepLeft", 4f, new[] { new Vector3(-4, 0, -4), new Vector3(-4, 0, -20) }),
        new PathPreset("SweepRight", 4f, new[] { new Vector3(4, 0, -4), new Vector3(4, 0, -20) }),
        new PathPreset("SCurve", 4f, new[] {
            new Vector3(3, 0, -4), new Vector3(3, 0, -8),
            new Vector3(-3, 0, -12), new Vector3(-3, 0, -20)
        }),
        new PathPreset("Hook", 4f, new[] {
            new Vector3(0, 0, -6), new Vector3(5, 0, -6), new Vector3(5, 0, -20)
        }),
        new PathPreset("Zigzag", 4f, new[] {
            new Vector3(3, 0, -3), new Vector3(-3, 0, -6),
            new Vector3(3, 0, -9), new Vector3(-3, 0, -12), new Vector3(0, 0, -20)
        }),
        new PathPreset("DiveBomb", 5f, new[] {
            new Vector3(0, 0, -3), new Vector3(0, 0, -8),
            new Vector3(6, 0, -10), new Vector3(6, 0, -20)
        }),
        new PathPreset("FigureApproach", 4f, new[] {
            new Vector3(4, 0, -4), new Vector3(-4, 0, -10),
            new Vector3(4, 0, -16), new Vector3(0, 0, -22)
        })
    };

    private int presetIndex;
    private string pathName = "Path_Custom";
    private float moveSpeed = 4f;
    private Vector3 spawnPosition = new Vector3(0f, 0f, 10f);
    private readonly List<Vector3> editableWaypoints = new List<Vector3>();

    private bool showLabels = true;
    private bool showCoastOff = true;
    private float coastOffLength = 8f;

    private Vector2 scrollPos;

    [MenuItem("Window/TakeFlight/Single Path Designer")]
    public static void ShowWindow()
    {
        GetWindow<SinglePathDesignerWindow>("Single Path Designer");
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        ApplyPreset(0);
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        EditorGUILayout.LabelField("Single Path Designer", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Select a preset, tweak waypoints, preview in Scene view, then create one PathData asset.", MessageType.Info);

        EditorGUILayout.Space(6);
        DrawPresetControls();

        EditorGUILayout.Space(10);
        DrawPathEditingControls();

        EditorGUILayout.Space(10);
        DrawPreviewOptions();

        EditorGUILayout.Space(10);
        DrawCreateControls();

        EditorGUILayout.EndScrollView();

        if (GUI.changed)
            SceneView.RepaintAll();
    }

    private void DrawPresetControls()
    {
        EditorGUILayout.LabelField("Preset Seed", EditorStyles.boldLabel);
        string[] names = new string[Presets.Length];
        for (int i = 0; i < Presets.Length; i++)
            names[i] = Presets[i].Name;

        int newPresetIndex = EditorGUILayout.Popup("Preset", presetIndex, names);
        if (newPresetIndex != presetIndex)
            ApplyPreset(newPresetIndex);

        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Reset From Preset", GUILayout.Width(160)))
                ApplyPreset(presetIndex);
        }
    }

    private void DrawPathEditingControls()
    {
        EditorGUILayout.LabelField("Path Data", EditorStyles.boldLabel);
        pathName = EditorGUILayout.TextField("Path Name", pathName);
        moveSpeed = EditorGUILayout.FloatField("Move Speed", moveSpeed);
        spawnPosition = EditorGUILayout.Vector3Field("Spawn Position", spawnPosition);

        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Waypoints (relative offsets)", EditorStyles.boldLabel);

        for (int i = 0; i < editableWaypoints.Count; i++)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                editableWaypoints[i] = EditorGUILayout.Vector3Field($"WP {i}", editableWaypoints[i]);

                GUI.enabled = i > 0;
                if (GUILayout.Button("Up", GUILayout.Width(40)))
                    SwapWaypoints(i, i - 1);

                GUI.enabled = i < editableWaypoints.Count - 1;
                if (GUILayout.Button("Dn", GUILayout.Width(40)))
                    SwapWaypoints(i, i + 1);

                GUI.enabled = true;
                if (GUILayout.Button("X", GUILayout.Width(24)))
                {
                    editableWaypoints.RemoveAt(i);
                    i--;
                }
            }
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Add Waypoint"))
                editableWaypoints.Add(GetSuggestedNewWaypoint());

            if (GUILayout.Button("Clear Waypoints"))
                editableWaypoints.Clear();
        }
    }

    private void DrawPreviewOptions()
    {
        EditorGUILayout.LabelField("Preview Options", EditorStyles.boldLabel);
        showLabels = EditorGUILayout.Toggle("Show Labels", showLabels);
        showCoastOff = EditorGUILayout.Toggle("Show Coast-Off", showCoastOff);

        if (showCoastOff)
            coastOffLength = EditorGUILayout.Slider("Coast-Off Length", coastOffLength, 2f, 20f);
    }

    private void DrawCreateControls()
    {
        EditorGUILayout.LabelField("Create Asset", EditorStyles.boldLabel);

        string validationError = GetValidationError();
        if (!string.IsNullOrEmpty(validationError))
            EditorGUILayout.HelpBox(validationError, MessageType.Warning);

        string assetName = SanitizeFileName(pathName);
        string assetPath = $"{OutputFolder}/{assetName}.asset";
        EditorGUILayout.LabelField("Target", assetPath);

        GUI.enabled = string.IsNullOrEmpty(validationError);
        if (GUILayout.Button("Create Path Asset", GUILayout.Height(32)))
            CreatePathAsset(assetPath);
        GUI.enabled = true;
    }

    private void ApplyPreset(int index)
    {
        presetIndex = Mathf.Clamp(index, 0, Presets.Length - 1);
        PathPreset preset = Presets[presetIndex];

        pathName = $"Path_{preset.Name}";
        moveSpeed = preset.Speed;
        editableWaypoints.Clear();
        editableWaypoints.AddRange(preset.Waypoints);
    }

    private void SwapWaypoints(int a, int b)
    {
        Vector3 tmp = editableWaypoints[a];
        editableWaypoints[a] = editableWaypoints[b];
        editableWaypoints[b] = tmp;
    }

    private Vector3 GetSuggestedNewWaypoint()
    {
        if (editableWaypoints.Count == 0)
            return new Vector3(0f, 0f, -4f);

        Vector3 last = editableWaypoints[editableWaypoints.Count - 1];
        return new Vector3(last.x, last.y, last.z - 4f);
    }

    private string GetValidationError()
    {
        string sanitized = SanitizeFileName(pathName);
        if (string.IsNullOrWhiteSpace(sanitized))
            return "Path name is required.";

        if (editableWaypoints.Count == 0)
            return "At least one waypoint is required.";

        if (moveSpeed <= 0f)
            return "Move speed must be greater than zero.";

        string assetPath = $"{OutputFolder}/{sanitized}.asset";
        if (AssetDatabase.LoadAssetAtPath<PathData>(assetPath) != null)
            return "A PathData asset with this name already exists.";

        return null;
    }

    private void CreatePathAsset(string assetPath)
    {
        if (!AssetDatabase.IsValidFolder(OutputFolder))
        {
            Directory.CreateDirectory(Path.Combine(Application.dataPath, "ScriptableObjects/PathData"));
            AssetDatabase.Refresh();
        }

        PathData asset = CreateInstance<PathData>();
        asset.moveSpeed = moveSpeed;
        asset.waypoints = editableWaypoints.ToArray();

        AssetDatabase.CreateAsset(asset, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorGUIUtility.PingObject(asset);
        Selection.activeObject = asset;
        Debug.Log($"[SinglePathDesigner] Created {assetPath}");
    }

    private static string SanitizeFileName(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return string.Empty;

        string sanitized = raw.Trim();
        foreach (char c in Path.GetInvalidFileNameChars())
            sanitized = sanitized.Replace(c.ToString(), string.Empty);

        return sanitized.Replace(" ", "_");
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (editableWaypoints.Count == 0)
            return;

        Vector3[] world = new Vector3[editableWaypoints.Count];
        for (int i = 0; i < editableWaypoints.Count; i++)
            world[i] = spawnPosition + editableWaypoints[i];

        Color color = new Color(0.2f, 0.8f, 1f);

        Handles.color = color;
        Handles.SphereHandleCap(0, spawnPosition, Quaternion.identity, 0.5f, EventType.Repaint);
        if (showLabels)
            Handles.Label(spawnPosition + Vector3.up * 0.4f, "SPAWN", EditorStyles.boldLabel);

        Vector3 prev = spawnPosition;
        for (int i = 0; i < world.Length; i++)
        {
            Handles.DrawLine(prev, world[i], 2f);
            DrawArrowHead(prev, world[i], color, 0.4f);
            Handles.SphereHandleCap(0, world[i], Quaternion.identity, 0.25f, EventType.Repaint);

            if (showLabels)
                Handles.Label(world[i] + Vector3.up * 0.3f, $"WP {i}", EditorStyles.miniLabel);

            prev = world[i];
        }

        if (!showCoastOff)
            return;

        Vector3 lastWp = world[world.Length - 1];
        Vector3 lastDir = world.Length >= 2
            ? (world[world.Length - 1] - world[world.Length - 2]).normalized
            : (world[0] - spawnPosition).normalized;

        if (lastDir == Vector3.zero)
            lastDir = Vector3.back;

        Vector3 coastEnd = lastWp + lastDir * coastOffLength;
        Color coastColor = new Color(color.r, color.g, color.b, 0.35f);
        Handles.color = coastColor;
        DrawDashedLine(lastWp, coastEnd, 0.5f);
        DrawArrowHead(lastWp, coastEnd, coastColor, 0.3f);
    }

    private static void DrawArrowHead(Vector3 from, Vector3 to, Color color, float size)
    {
        Vector3 dir = (to - from).normalized;
        if (dir == Vector3.zero)
            return;

        Vector3 mid = Vector3.Lerp(from, to, 0.6f);
        Vector3 right = Vector3.Cross(dir, Vector3.up).normalized;
        if (right == Vector3.zero)
            right = Vector3.Cross(dir, Vector3.forward).normalized;

        Handles.color = color;
        Vector3 a = mid - dir * size + right * size * 0.5f;
        Vector3 b = mid - dir * size - right * size * 0.5f;
        Handles.DrawLine(mid, a);
        Handles.DrawLine(mid, b);
    }

    private static void DrawDashedLine(Vector3 from, Vector3 to, float dashLength)
    {
        float dist = Vector3.Distance(from, to);
        if (dist < 0.01f)
            return;

        Vector3 dir = (to - from) / dist;
        float drawn = 0f;
        bool draw = true;

        while (drawn < dist)
        {
            float step = Mathf.Min(dashLength, dist - drawn);
            Vector3 segStart = from + dir * drawn;
            Vector3 segEnd = from + dir * (drawn + step);

            if (draw)
                Handles.DrawLine(segStart, segEnd);

            drawn += step;
            draw = !draw;
        }
    }
}
