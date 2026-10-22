using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class GameBuildPipeline
{
    private static readonly string[] Scenes = new string[] { "Assets/Scenes/MainScene.unity" };

    [MenuItem("MiniGolf/Build iOS Xcode Project")]
    public static void BuildIOS()
    {
        string buildDir = Path.Combine(Directory.GetCurrentDirectory(), "Builds/iOS");
        if (!Directory.Exists(buildDir))
        {
            Directory.CreateDirectory(buildDir);
        }

        Debug.Log($"[GameBuildPipeline] Building iOS Xcode project to: {buildDir}");

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = Scenes,
            locationPathName = buildDir,
            target = BuildTarget.iOS,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[GameBuildPipeline] iOS build SUCCESS! Total size: {summary.totalSize} bytes, time: {summary.totalTime.TotalSeconds:F1}s");
        }
        else
        {
            Debug.LogError($"[GameBuildPipeline] iOS build FAILED! Result: {summary.result}, Errors: {summary.totalErrors}");
            EditorApplication.Exit(1);
        }
    }

    [MenuItem("MiniGolf/Build macOS Standalone")]
    public static void BuildMacOS()
    {
        string buildDir = Path.Combine(Directory.GetCurrentDirectory(), "Builds/macOS");
        if (!Directory.Exists(buildDir))
        {
            Directory.CreateDirectory(buildDir);
        }

        string appPath = Path.Combine(buildDir, "MiniGolf3D.app");
        Debug.Log($"[GameBuildPipeline] Building macOS app to: {appPath}");

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = Scenes,
            locationPathName = appPath,
            target = BuildTarget.StandaloneOSX,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[GameBuildPipeline] macOS Standalone build SUCCESS! Total size: {summary.totalSize} bytes");
        }
        else
        {
            Debug.LogError($"[GameBuildPipeline] macOS build FAILED! Result: {summary.result}");
            EditorApplication.Exit(1);
        }
    }
}
