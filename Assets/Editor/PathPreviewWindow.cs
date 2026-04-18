using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class PathPreviewWindow : EditorWindow
{
    private enum PreviewMode { SinglePath, AllPaths, WaveDefinition }

    private PreviewMode mode = PreviewMode.SinglePath;
    private PathData selectedPath;
    private WaveDefinition selectedWave;
    private Vector3 spawnPosition = new Vector3(0f, 0f, 10f);
    private bool showCameraBounds = true;
    private bool showLabels = true;
    private bool showCoastOff = true;
    private float coastOffLength = 8f;

    private float cameraHalfWidth = 5f;
    private float cameraHalfHeight = 8f;
    private float spawnZ = 10f;
    private float despawnZ = -10f;

    private PathData[] allPaths;
    private Vector2 scrollPos;

    private const string PrefsPrefix = "PathPreviewer_";

    private static readonly Color[] PathPalette =
    {
        new Color(0.2f, 0.8f, 1f),
        new Color(1f, 0.4f, 0.3f),
        new Color(0.4f, 1f, 0.4f),
        new Color(1f, 0.85f, 0.2f),
        new Color(0.8f, 0.4f, 1f),
        new Color(1f, 0.6f, 0.2f),
        new Color(0.2f, 1f, 0.8f),
        new Color(1f, 0.4f, 0.7f),
        new Color(0.6f, 0.8f, 0.2f),
        new Color(0.5f, 0.6f, 1f),
    };

    [MenuItem("Window/TakeFlight/Path Previewer")]
    public static void ShowWindow()
    {
        GetWindow<PathPreviewWindow>("Path Previewer");
    }

    void OnEnable()
    {
        LoadPrefs();
        SceneView.duringSceneGui += OnSceneGUI;
        RefreshPathList();
    }

    void OnDisable()
    {
        SavePrefs();
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    void SavePrefs()
    {
        EditorPrefs.SetInt(PrefsPrefix + "mode", (int)mode);
        EditorPrefs.SetFloat(PrefsPrefix + "spawnX", spawnPosition.x);
        EditorPrefs.SetFloat(PrefsPrefix + "spawnY", spawnPosition.y);
        EditorPrefs.SetFloat(PrefsPrefix + "spawnZ_pos", spawnPosition.z);
        EditorPrefs.SetBool(PrefsPrefix + "showCameraBounds", showCameraBounds);
        EditorPrefs.SetBool(PrefsPrefix + "showLabels", showLabels);
        EditorPrefs.SetBool(PrefsPrefix + "showCoastOff", showCoastOff);
        EditorPrefs.SetFloat(PrefsPrefix + "coastOffLength", coastOffLength);
        EditorPrefs.SetFloat(PrefsPrefix + "cameraHalfWidth", cameraHalfWidth);
        EditorPrefs.SetFloat(PrefsPrefix + "cameraHalfHeight", cameraHalfHeight);
        EditorPrefs.SetFloat(PrefsPrefix + "spawnZ", spawnZ);
        EditorPrefs.SetFloat(PrefsPrefix + "despawnZ", despawnZ);
    }

    void LoadPrefs()
    {
        mode = (PreviewMode)EditorPrefs.GetInt(PrefsPrefix + "mode", (int)PreviewMode.SinglePath);
        spawnPosition.x = EditorPrefs.GetFloat(PrefsPrefix + "spawnX", 0f);
        spawnPosition.y = EditorPrefs.GetFloat(PrefsPrefix + "spawnY", 0f);
        spawnPosition.z = EditorPrefs.GetFloat(PrefsPrefix + "spawnZ_pos", 10f);
        showCameraBounds = EditorPrefs.GetBool(PrefsPrefix + "showCameraBounds", true);
        showLabels = EditorPrefs.GetBool(PrefsPrefix + "showLabels", true);
        showCoastOff = EditorPrefs.GetBool(PrefsPrefix + "showCoastOff", true);
        coastOffLength = EditorPrefs.GetFloat(PrefsPrefix + "coastOffLength", 8f);
        cameraHalfWidth = EditorPrefs.GetFloat(PrefsPrefix + "cameraHalfWidth", 5f);
        cameraHalfHeight = EditorPrefs.GetFloat(PrefsPrefix + "cameraHalfHeight", 8f);
        spawnZ = EditorPrefs.GetFloat(PrefsPrefix + "spawnZ", 10f);
        despawnZ = EditorPrefs.GetFloat(PrefsPrefix + "despawnZ", -10f);
    }

    void RefreshPathList()
    {
        string[] guids = AssetDatabase.FindAssets("t:PathData");
        allPaths = new PathData[guids.Length];
        for (int i = 0; i < guids.Length; i++)
            allPaths[i] = AssetDatabase.LoadAssetAtPath<PathData>(AssetDatabase.GUIDToAssetPath(guids[i]));
    }

    void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        EditorGUILayout.LabelField("Path Previewer", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        mode = (PreviewMode)EditorGUILayout.EnumPopup("Preview Mode", mode);
        EditorGUILayout.Space(4);

        switch (mode)
        {
            case PreviewMode.SinglePath:
                DrawSinglePathUI();
                break;
            case PreviewMode.AllPaths:
                DrawAllPathsUI();
                break;
            case PreviewMode.WaveDefinition:
                DrawWaveUI();
                break;
        }

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Display Options", EditorStyles.boldLabel);

        showCameraBounds = EditorGUILayout.Toggle("Show Camera Bounds", showCameraBounds);
        showLabels = EditorGUILayout.Toggle("Show Waypoint Labels", showLabels);
        showCoastOff = EditorGUILayout.Toggle("Show Coast-Off Trajectory", showCoastOff);

        if (showCoastOff)
            coastOffLength = EditorGUILayout.Slider("Coast-Off Length", coastOffLength, 2f, 20f);

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Camera Bounds (world units)", EditorStyles.boldLabel);
        cameraHalfWidth = EditorGUILayout.FloatField("Half Width", cameraHalfWidth);
        cameraHalfHeight = EditorGUILayout.FloatField("Half Height", cameraHalfHeight);

        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Z Planes", EditorStyles.boldLabel);
        spawnZ = EditorGUILayout.FloatField("Spawn Z", spawnZ);
        despawnZ = EditorGUILayout.FloatField("Despawn Z", despawnZ);

        EditorGUILayout.Space(8);
        if (GUILayout.Button("Refresh Path List"))
            RefreshPathList();

        EditorGUILayout.EndScrollView();

        if (GUI.changed)
        {
            SavePrefs();
            SceneView.RepaintAll();
        }
    }

    void DrawSinglePathUI()
    {
        selectedPath = (PathData)EditorGUILayout.ObjectField("Path Data", selectedPath, typeof(PathData), false);
        spawnPosition = EditorGUILayout.Vector3Field("Spawn Position", spawnPosition);
    }

    void DrawAllPathsUI()
    {
        spawnPosition = EditorGUILayout.Vector3Field("Spawn Position", spawnPosition);
        EditorGUILayout.HelpBox(
            $"{(allPaths != null ? allPaths.Length : 0)} PathData assets found. All will be drawn from the spawn position above.",
            MessageType.Info);
    }

    void DrawWaveUI()
    {
        selectedWave = (WaveDefinition)EditorGUILayout.ObjectField("Wave Definition", selectedWave, typeof(WaveDefinition), false);

        if (selectedWave != null && selectedWave.spawnInstructions != null)
        {
            EditorGUILayout.HelpBox(
                $"{selectedWave.spawnInstructions.Length} spawn instructions. Each path drawn at its configured spawn position.",
                MessageType.Info);
        }
    }

    // ----------------------------------------------------------------
    // Scene View Drawing
    // ----------------------------------------------------------------

    void OnSceneGUI(SceneView sceneView)
    {
        if (showCameraBounds)
            DrawCameraBounds();

        switch (mode)
        {
            case PreviewMode.SinglePath:
                if (selectedPath != null)
                    DrawPath(selectedPath, spawnPosition, PathPalette[0], 0);
                break;

            case PreviewMode.AllPaths:
                if (allPaths != null)
                    for (int i = 0; i < allPaths.Length; i++)
                        if (allPaths[i] != null)
                            DrawPath(allPaths[i], spawnPosition, PathPalette[i % PathPalette.Length], i);
                break;

            case PreviewMode.WaveDefinition:
                DrawWavePaths();
                break;
        }
    }

    void DrawPath(PathData path, Vector3 origin, Color color, int pathIndex)
    {
        if (path.waypoints == null || path.waypoints.Length == 0) return;

        Vector3[] world = new Vector3[path.waypoints.Length];
        for (int i = 0; i < path.waypoints.Length; i++)
            world[i] = origin + path.waypoints[i];

        Handles.color = color;

        // Spawn point sphere
        Handles.SphereHandleCap(0, origin, Quaternion.identity, 0.5f, EventType.Repaint);

        if (showLabels)
        {
            GUIStyle spawnStyle = new GUIStyle(EditorStyles.boldLabel) { normal = { textColor = color } };
            string label = mode == PreviewMode.AllPaths ? path.name : "SPAWN";
            Handles.Label(origin + Vector3.up * 0.4f, label, spawnStyle);
        }

        // Path segments with arrows
        Vector3 prev = origin;
        for (int i = 0; i < world.Length; i++)
        {
            Handles.color = color;
            Handles.DrawLine(prev, world[i], 2f);
            DrawArrowHead(prev, world[i], color, 0.4f);

            Handles.SphereHandleCap(0, world[i], Quaternion.identity, 0.25f, EventType.Repaint);

            if (showLabels)
            {
                GUIStyle wpStyle = new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = color } };
                Handles.Label(world[i] + Vector3.up * 0.3f, $"WP {i}", wpStyle);
            }

            prev = world[i];
        }

        // Coast-off dashed line
        if (showCoastOff)
        {
            Vector3 lastWp = world[world.Length - 1];
            Vector3 lastDir;

            if (world.Length >= 2)
                lastDir = (world[world.Length - 1] - world[world.Length - 2]).normalized;
            else
                lastDir = (world[0] - origin).normalized;

            if (lastDir == Vector3.zero) lastDir = Vector3.back;

            Vector3 coastEnd = lastWp + lastDir * coastOffLength;
            Color dashed = new Color(color.r, color.g, color.b, 0.35f);
            Handles.color = dashed;
            DrawDashedLine(lastWp, coastEnd, 0.5f);
            DrawArrowHead(lastWp, coastEnd, dashed, 0.3f);
        }
    }

    void DrawWavePaths()
    {
        if (selectedWave == null || selectedWave.spawnInstructions == null) return;

        var instructions = selectedWave.spawnInstructions;
        for (int i = 0; i < instructions.Length; i++)
        {
            var inst = instructions[i];
            if (inst.pathData == null) continue;
            DrawPath(inst.pathData, inst.spawnPosition, PathPalette[i % PathPalette.Length], i);
        }
    }

    // ----------------------------------------------------------------
    // Camera Bounds
    // ----------------------------------------------------------------

    void DrawCameraBounds()
    {
        DrawBoundsRect(spawnZ, new Color(1f, 1f, 1f, 0.5f), "Spawn Line");
        DrawBoundsRect(0f, new Color(0.3f, 1f, 0.3f, 0.6f), "Player Area (Z=0)");
        DrawBoundsRect(despawnZ, new Color(1f, 0.3f, 0.3f, 0.5f), "Despawn Line");

        // Vertical edges connecting the three planes
        Color edgeColor = new Color(1f, 1f, 1f, 0.15f);
        Handles.color = edgeColor;
        float hw = cameraHalfWidth;
        float z0 = spawnZ, z1 = despawnZ;
        Handles.DrawLine(new Vector3(-hw, 0, z0), new Vector3(-hw, 0, z1));
        Handles.DrawLine(new Vector3(hw, 0, z0), new Vector3(hw, 0, z1));
    }

    void DrawBoundsRect(float z, Color color, string label)
    {
        float hw = cameraHalfWidth;
        float hh = cameraHalfHeight;

        Vector3 bl = new Vector3(-hw, 0, z - hh);
        Vector3 br = new Vector3(hw, 0, z - hh);
        Vector3 tl = new Vector3(-hw, 0, z + hh);
        Vector3 tr = new Vector3(hw, 0, z + hh);

        // For a top-down shmup the camera viewport is an X/Z rectangle.
        // Draw a simple horizontal line at this Z to mark the plane,
        // plus the full visible rect if at Z=0 (player area).
        Vector3 left = new Vector3(-hw, 0, z);
        Vector3 right = new Vector3(hw, 0, z);

        Handles.color = color;
        Handles.DrawLine(left, right, 2f);

        if (showLabels)
        {
            GUIStyle s = new GUIStyle(EditorStyles.boldLabel) { normal = { textColor = color } };
            Handles.Label(right + new Vector3(0.3f, 0, 0), label, s);
        }

        // Full rect only for the player area plane
        if (Mathf.Approximately(z, 0f))
        {
            Color fill = new Color(color.r, color.g, color.b, 0.06f);
            Handles.DrawSolidRectangleWithOutline(
                new[] { bl, tl, tr, br }, fill, color);
        }
    }

    // ----------------------------------------------------------------
    // Drawing Helpers
    // ----------------------------------------------------------------

    static void DrawArrowHead(Vector3 from, Vector3 to, Color color, float size)
    {
        Vector3 dir = (to - from).normalized;
        if (dir == Vector3.zero) return;

        Vector3 mid = Vector3.Lerp(from, to, 0.6f);
        Vector3 right = Vector3.Cross(dir, Vector3.up).normalized;
        if (right == Vector3.zero) right = Vector3.Cross(dir, Vector3.forward).normalized;

        Handles.color = color;
        Vector3 a = mid - dir * size + right * size * 0.5f;
        Vector3 b = mid - dir * size - right * size * 0.5f;
        Handles.DrawLine(mid, a);
        Handles.DrawLine(mid, b);
    }

    static void DrawDashedLine(Vector3 from, Vector3 to, float dashLength)
    {
        float dist = Vector3.Distance(from, to);
        if (dist < 0.01f) return;

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
