using UnityEditor;
using UnityEngine;
using System.IO;

// Window > TakeFlight > Generate Path Data
// Creates all path pattern assets in Assets/ScriptableObjects/PathData/
public class PathDataGenerator : EditorWindow
{
    private const string OutputFolder = "Assets/ScriptableObjects/PathData";

    [MenuItem("Window/TakeFlight/Generate Path Data")]
    public static void ShowWindow()
    {
        GetWindow<PathDataGenerator>("Generate Path Data");
    }

    void OnGUI()
    {
        GUILayout.Label("Generates all 10 path patterns + mirrored variants.", EditorStyles.wordWrappedLabel);
        GUILayout.Space(8);

        if (GUILayout.Button("Generate All Path Data Assets", GUILayout.Height(36)))
            Generate();
    }

    private static void Generate()
    {
        if (!AssetDatabase.IsValidFolder(OutputFolder))
        {
            Directory.CreateDirectory(Path.Combine(Application.dataPath, "ScriptableObjects/PathData"));
            AssetDatabase.Refresh();
        }

        // (name, speed, waypoints)
        var patterns = new (string name, float speed, Vector3[] waypoints)[]
        {
            (
                "Path_StraightDown", 4f,
                new[] { new Vector3(0, 0, -20) }
            ),
            (
                "Path_SweepLeft", 4f,
                new[] { new Vector3(-4, 0, -4), new Vector3(-4, 0, -20) }
            ),
            (
                "Path_SweepRight", 4f,
                new[] { new Vector3(4, 0, -4), new Vector3(4, 0, -20) }
            ),
            (
                "Path_SCurve", 4f,
                new[] {
                    new Vector3(3, 0, -4), new Vector3(3, 0, -8),
                    new Vector3(-3, 0, -12), new Vector3(-3, 0, -20)
                }
            ),
            (
                "Path_SCurve_Mirror", 4f,
                new[] {
                    new Vector3(-3, 0, -4), new Vector3(-3, 0, -8),
                    new Vector3(3, 0, -12), new Vector3(3, 0, -20)
                }
            ),
            (
                "Path_Hook", 4f,
                new[] {
                    new Vector3(0, 0, -6), new Vector3(5, 0, -6), new Vector3(5, 0, -20)
                }
            ),
            (
                "Path_Hook_Mirror", 4f,
                new[] {
                    new Vector3(0, 0, -6), new Vector3(-5, 0, -6), new Vector3(-5, 0, -20)
                }
            ),
            (
                "Path_Zigzag", 4f,
                new[] {
                    new Vector3(3, 0, -3), new Vector3(-3, 0, -6),
                    new Vector3(3, 0, -9), new Vector3(-3, 0, -12),
                    new Vector3(0, 0, -20)
                }
            ),
            (
                "Path_Zigzag_Mirror", 4f,
                new[] {
                    new Vector3(-3, 0, -3), new Vector3(3, 0, -6),
                    new Vector3(-3, 0, -9), new Vector3(3, 0, -12),
                    new Vector3(0, 0, -20)
                }
            ),
            (
                "Path_WideLoopSweep", 4f,
                new[] {
                    new Vector3(-2, 0, -3), new Vector3(-5, 0, -6),
                    new Vector3(-5, 0, -10), new Vector3(-2, 0, -14),
                    new Vector3(2, 0, -20)
                }
            ),
            (
                "Path_WideLoopSweep_Mirror", 4f,
                new[] {
                    new Vector3(2, 0, -3), new Vector3(5, 0, -6),
                    new Vector3(5, 0, -10), new Vector3(2, 0, -14),
                    new Vector3(-2, 0, -20)
                }
            ),
            (
                "Path_DiveBomb", 5f,
                new[] {
                    new Vector3(0, 0, -3), new Vector3(0, 0, -8),
                    new Vector3(6, 0, -10), new Vector3(6, 0, -20)
                }
            ),
            (
                "Path_DiveBomb_Mirror", 5f,
                new[] {
                    new Vector3(0, 0, -3), new Vector3(0, 0, -8),
                    new Vector3(-6, 0, -10), new Vector3(-6, 0, -20)
                }
            ),
            (
                "Path_FigureApproach", 4f,
                new[] {
                    new Vector3(4, 0, -4), new Vector3(-4, 0, -10),
                    new Vector3(4, 0, -16), new Vector3(0, 0, -22)
                }
            ),
            (
                "Path_FigureApproach_Mirror", 4f,
                new[] {
                    new Vector3(-4, 0, -4), new Vector3(4, 0, -10),
                    new Vector3(-4, 0, -16), new Vector3(0, 0, -22)
                }
            ),
            (
                "Path_StallAndDrop", 3f,
                new[] {
                    new Vector3(0, 0, -5), new Vector3(0, 0, -5),
                    new Vector3(0, 0, -5), new Vector3(0, 0, -20)
                }
            ),
        };

        int created = 0;
        int skipped = 0;

        foreach (var (name, speed, waypoints) in patterns)
        {
            string assetPath = $"{OutputFolder}/{name}.asset";

            if (AssetDatabase.LoadAssetAtPath<PathData>(assetPath) != null)
            {
                skipped++;
                continue;
            }

            var asset = CreateInstance<PathData>();
            asset.moveSpeed = speed;
            asset.waypoints = waypoints;

            AssetDatabase.CreateAsset(asset, assetPath);
            created++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[PathDataGenerator] Done — {created} created, {skipped} already existed. Output: {OutputFolder}");
        EditorUtility.DisplayDialog("Path Data Generator",
            $"{created} assets created.\n{skipped} already existed.\n\nOutput: {OutputFolder}", "OK");
    }
}
