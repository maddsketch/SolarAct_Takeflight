using UnityEditor;
using UnityEngine;
using System.IO;

// Window > TakeFlight > Generate Level Definition
// Generates Sector1_Level01_Generated.asset in Assets/Prefab/Level_ID/
public class LevelDefinitionGenerator : EditorWindow
{
    private const string OutputFolder  = "Assets/Prefab/Level_ID";
    private const string PathDataFolder = "Assets/ScriptableObjects/PathData";
    private const string EnemyPrefabPath = "Assets/Prefab/EnemyAssets/Enemy01.prefab";

    [MenuItem("Window/TakeFlight/Generate Level Definition")]
    public static void ShowWindow() =>
        GetWindow<LevelDefinitionGenerator>("Generate Level Definition");

    void OnGUI()
    {
        GUILayout.Label("Generates Sector1_Level01_Generated.asset\nwith all 15 waves pre-filled.", EditorStyles.wordWrappedLabel);
        GUILayout.Space(8);
        if (GUILayout.Button("Generate Level Definition", GUILayout.Height(36)))
            Generate();
    }

    private static void Generate()
    {
        var enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(EnemyPrefabPath);
        if (enemyPrefab == null)
        {
            EditorUtility.DisplayDialog("Error", $"Enemy prefab not found at:\n{EnemyPrefabPath}", "OK");
            return;
        }

        // --- Load all PathData assets ---
        PathData Pd(string name) =>
            AssetDatabase.LoadAssetAtPath<PathData>($"{PathDataFolder}/{name}.asset");

        var straight     = Pd("Path_StraightDown");
        var sweepL       = Pd("Path_SweepLeft");
        var sweepR       = Pd("Path_SweepRight");
        var sCurve       = Pd("Path_SCurve");
        var sCurveMirror = Pd("Path_SCurve_Mirror");
        var hook         = Pd("Path_Hook");
        var hookMirror   = Pd("Path_Hook_Mirror");
        var zigzag       = Pd("Path_Zigzag");
        var zigzagMirror = Pd("Path_Zigzag_Mirror");
        var wideLoop     = Pd("Path_WideLoopSweep");
        var wideLoopM    = Pd("Path_WideLoopSweep_Mirror");
        var diveBomb     = Pd("Path_DiveBomb");
        var diveBombM    = Pd("Path_DiveBomb_Mirror");
        var figureApp    = Pd("Path_FigureApproach");
        var figureAppM   = Pd("Path_FigureApproach_Mirror");
        var stall        = Pd("Path_StallAndDrop");

        // --- Helper to build a SpawnInstruction ---
        SpawnInstruction SI(float delay, float x, float y, float z, PathData path) => new SpawnInstruction
        {
            enemyPrefab   = enemyPrefab,
            spawnPosition = new Vector3(x, y, z),
            delay         = delay,
            pathData      = path
        };

        // --- Define all 15 waves ---
        var waveDefs = new (string name, float delay, SpawnInstruction[] instructions)[]
        {
            // Wave 1
            ("Wave_01", 4.0f, new[]
            {
                SI(0.0f, -1,  0, 9, straight),
                SI(1.5f,  1,  0, 9, straight),
            }),
            // Wave 2
            ("Wave_02", 4.0f, new[]
            {
                SI(0.0f, -3, 0, 9, sweepL),
                SI(2.0f,  3, 0, 9, sweepR),
            }),
            // Wave 3
            ("Wave_03", 3.5f, new[]
            {
                SI(0.0f, -3, 0,  9, sweepL),
                SI(1.5f,  0, 0,  9, straight),
                SI(3.0f,  3, 0,  9, sweepR),
            }),
            // Wave 4
            ("Wave_04", 3.5f, new[]
            {
                SI(0.0f, -2, 0,  9, sCurve),
                SI(1.5f,  0, 0, 10, straight),
                SI(3.0f,  2, 0,  9, sCurveMirror),
            }),
            // Wave 5
            ("Wave_05", 3.0f, new[]
            {
                SI(0.0f,  -4,   0, 9, sweepL),
                SI(1.0f,  -1.5f,0, 9, sCurve),
                SI(2.0f,   1.5f,0, 9, sCurveMirror),
                SI(3.0f,   4,   0, 9, sweepR),
            }),
            // Wave 6
            ("Wave_06", 3.0f, new[]
            {
                SI(0.0f, -3, 0, 10, hook),
                SI(0.8f, -1, 0,  9, straight),
                SI(1.6f,  1, 0,  9, straight),
                SI(2.5f,  3, 0, 10, hookMirror),
            }),
            // Wave 7
            ("Wave_07", 3.0f, new[]
            {
                SI(0.0f, -5,   0,  9, sweepL),
                SI(0.8f, -2.5f,0,  9, sCurve),
                SI(1.5f,  0,   0, 10, zigzag),
                SI(2.2f,  2.5f,0,  9, sCurveMirror),
                SI(3.0f,  5,   0,  9, sweepR),
            }),
            // Wave 8
            ("Wave_08", 2.0f, new[]
            {
                SI(0.0f, -5, 0, 10, hook),
                SI(0.5f, -3, 0,  9, sCurve),
                SI(1.0f, -1, 0, 11, diveBomb),
                SI(1.5f,  1, 0, 11, diveBombM),
                SI(2.0f,  3, 0,  9, sCurveMirror),
                SI(2.8f,  5, 0, 10, hookMirror),
            }),
            // Wave 9
            ("Wave_09", 2.5f, new[]
            {
                SI(0.0f, -6, 0,  9, wideLoop),
                SI(0.8f, -3, 0, 10, sweepL),
                SI(1.5f, -1, 0,  9, straight),
                SI(2.2f,  1, 0,  9, straight),
                SI(3.0f,  3, 0, 10, sweepR),
                SI(3.8f,  6, 0,  9, wideLoopM),
            }),
            // Wave 10
            ("Wave_10", 1.5f, new[]
            {
                SI(0.0f, -5,   0, 11, diveBomb),
                SI(0.4f, -3.5f,0,  9, sCurve),
                SI(0.8f, -2,   0, 10, hook),
                SI(1.2f,  0,   0,  9, zigzag),
                SI(1.6f,  2,   0, 10, hookMirror),
                SI(2.2f,  3.5f,0,  9, sCurveMirror),
                SI(3.0f,  5,   0, 11, diveBombM),
            }),
            // Wave 11
            ("Wave_11", 2.5f, new[]
            {
                SI(0.0f, -6, 0,  9, wideLoop),
                SI(0.7f, -4, 0,  9, sCurve),
                SI(1.4f, -2, 0, 10, figureApp),
                SI(2.1f,  0, 0, 11, straight),
                SI(2.8f,  2, 0, 10, figureAppM),
                SI(3.5f,  4, 0,  9, sCurveMirror),
                SI(4.2f,  6, 0,  9, wideLoopM),
            }),
            // Wave 12
            ("Wave_12", 1.5f, new[]
            {
                SI(0.0f, -6,    0, 11, diveBomb),
                SI(0.4f, -4,    0, 10, hook),
                SI(0.8f, -2.5f, 0,  9, zigzag),
                SI(1.2f, -0.5f, 0,  9, sCurve),
                SI(1.6f,  0.5f, 0,  9, sCurveMirror),
                SI(2.0f,  2.5f, 0,  9, zigzagMirror),
                SI(2.5f,  4,    0, 10, hookMirror),
                SI(3.2f,  6,    0, 11, diveBombM),
            }),
            // Wave 13
            ("Wave_13", 2.5f, new[]
            {
                SI(0.0f, -5,    0,  9, sweepL),
                SI(0.6f, -4,    0, 10, wideLoop),
                SI(1.2f, -2,    0, 12, stall),
                SI(1.8f, -0.5f, 0,  9, sCurve),
                SI(2.4f,  0.5f, 0,  9, sCurveMirror),
                SI(3.0f,  2,    0, 12, stall),
                SI(3.6f,  4,    0, 10, wideLoopM),
                SI(4.2f,  5,    0,  9, sweepR),
            }),
            // Wave 14
            ("Wave_14", 1.5f, new[]
            {
                SI(0.0f, -6,    0, 11, diveBomb),
                SI(0.3f, -4.5f, 0, 10, hook),
                SI(0.6f, -3,    0,  9, sCurve),
                SI(0.9f, -1.5f, 0,  9, zigzag),
                SI(1.2f,  0,    0, 10, figureApp),
                SI(1.6f,  1.5f, 0,  9, zigzagMirror),
                SI(2.0f,  3,    0,  9, sCurveMirror),
                SI(2.5f,  4.5f, 0, 10, hookMirror),
                SI(3.2f,  6,    0, 11, diveBombM),
            }),
            // Wave 15
            ("Wave_15", 0.0f, new[]
            {
                SI(0.0f, -4,    0, 11, diveBomb),
                SI(0.3f, -1.5f, 0, 11, hook),
                SI(0.6f,  1.5f, 0, 11, hookMirror),
                SI(0.9f,  4,    0, 11, diveBombM),
                SI(1.5f, -5,    0,  9, zigzag),
                SI(1.8f, -2.5f, 0,  9, sCurve),
                SI(2.1f,  0,    0, 10, figureApp),
                SI(2.4f,  2.5f, 0,  9, sCurveMirror),
                SI(3.0f,  5,    0,  9, zigzagMirror),
                SI(3.3f,  0,    0, 12, stall),
            }),
        };

        // --- Build the LevelDefinition asset ---
        string assetPath = $"{OutputFolder}/Sector1_Level01_Generated.asset";

        if (AssetDatabase.LoadAssetAtPath<LevelDefinition>(assetPath) != null)
        {
            if (!EditorUtility.DisplayDialog("Already Exists",
                "Sector1_Level01_Generated.asset already exists. Overwrite?", "Overwrite", "Cancel"))
                return;

            AssetDatabase.DeleteAsset(assetPath);
        }

        var levelDef = ScriptableObject.CreateInstance<LevelDefinition>();
        levelDef.levelID     = "sector1_level01";
        levelDef.displayName = "Sector 1 — Level 01";
        levelDef.sceneName   = "Sector1_Level01";
        levelDef.waves       = new WaveDefinition[waveDefs.Length];

        AssetDatabase.CreateAsset(levelDef, assetPath);

        for (int i = 0; i < waveDefs.Length; i++)
        {
            var (name, delay, instructions) = waveDefs[i];

            var waveDef = ScriptableObject.CreateInstance<WaveDefinition>();
            waveDef.name                = name;
            waveDef.spawnInstructions   = instructions;
            waveDef.clearCondition      = WaveClearCondition.KillAll;
            waveDef.delayBeforeNextWave = delay;

            AssetDatabase.AddObjectToAsset(waveDef, assetPath);
            levelDef.waves[i] = waveDef;
        }

        EditorUtility.SetDirty(levelDef);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[LevelDefinitionGenerator] Created {assetPath} with {waveDefs.Length} waves.");
        EditorUtility.DisplayDialog("Done",
            $"Created Sector1_Level01_Generated.asset\nwith {waveDefs.Length} waves.", "OK");
    }
}
