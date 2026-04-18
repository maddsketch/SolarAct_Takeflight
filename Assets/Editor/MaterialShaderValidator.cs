using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Reimports shaders under Assets/Shaders and reports materials with missing or error shaders.
/// Batch: Unity -batchmode -quit -projectPath ... -executeMethod MaterialShaderValidator.ReimportShadersBatch -logFile ...
/// </summary>
public static class MaterialShaderValidator
{
    const string ErrorShaderName = "Hidden/InternalErrorShader";

    [MenuItem("TakeFlight/Validation/Reimport Shaders Under Assets/Shaders")]
    public static void ReimportShadersMenu()
    {
        ReimportShaders();
        Debug.Log("[MaterialShaderValidator] Reimport finished for Assets/Shaders.");
    }

    [MenuItem("TakeFlight/Validation/Log Material Shader Issues")]
    public static void ValidateMaterialsMenu()
    {
        string report = BuildReport();
        Debug.Log(report);
    }

    public static void ReimportShadersBatch()
    {
        ReimportShaders();
        string report = BuildReport();
        string logDir = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
        Directory.CreateDirectory(logDir);
        string outPath = Path.Combine(logDir, "MaterialShaderValidation.txt");
        File.WriteAllText(outPath, report, Encoding.UTF8);
        Debug.Log(report);
        Debug.Log($"[MaterialShaderValidator] Wrote {outPath}");
    }

    static void ReimportShaders()
    {
        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (string guid in AssetDatabase.FindAssets("glob:\"*.shader\"", new[] { "Assets/Shaders" }))
            paths.Add(AssetDatabase.GUIDToAssetPath(guid));
        foreach (string guid in AssetDatabase.FindAssets("glob:\"*.shadergraph\"", new[] { "Assets/Shaders" }))
            paths.Add(AssetDatabase.GUIDToAssetPath(guid));

        foreach (string path in paths)
        {
            if (string.IsNullOrEmpty(path))
                continue;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        AssetDatabase.Refresh();
    }

    static string BuildReport()
    {
        var issues = new List<string>();
        foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { "Assets" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
                continue;

            if (mat.shader == null)
            {
                issues.Add($"Missing shader: {path}");
                continue;
            }

            if (mat.shader.name == ErrorShaderName)
                issues.Add($"Error shader (compile failure): {path}");
        }

        var sb = new StringBuilder();
        sb.AppendLine($"Material shader scan — {DateTime.UtcNow:O} (UTC)");
        sb.AppendLine($"Issues found: {issues.Count}");
        foreach (string line in issues)
            sb.AppendLine(line);
        if (issues.Count == 0)
            sb.AppendLine("No missing or InternalErrorShader materials under Assets.");
        return sb.ToString();
    }
}
