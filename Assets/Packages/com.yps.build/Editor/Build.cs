using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;
using System;

public class Build
{
    [MenuItem("Build/Android/Export Project")]
    public static void ExportAndroidProject()
    {
        // 设置导出路径
        string exportPath = "Builds/AndroidProject";

        // 检查并创建导出目录
        if (!System.IO.Directory.Exists(exportPath))
        {
            System.IO.Directory.CreateDirectory(exportPath);
        }

        // 切换到 Android 平台
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            Debug.Log("Switching to Android platform...");
            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
            {
                Debug.LogError("Failed to switch to Android platform!");
                return;
            }
        }

        // 设置构建选项
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = GetEnabledScenes(), // 获取所有已启用的场景
            locationPathName = exportPath, // 导出路径
            target = BuildTarget.Android, // 目标平台
            options = BuildOptions.AcceptExternalModificationsToPlayer // 导出为 Android 工程
        };

        // 开始构建
        Debug.Log("Exporting Android project...");
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);

        // 检查构建结果
        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log("Android project exported successfully to: " + exportPath);
        }
        else
        {
            Debug.LogError("Failed to export Android project. Errors: " + report.summary.totalErrors);
            return;
        }

        Debug.Log("Modify Android project...");

        // 获取 Unity 安装目录
        string unityEditorPath = EditorApplication.applicationPath;
        string unityEditorDirectory = System.IO.Path.GetDirectoryName(unityEditorPath);

        // 拼接内置 JDK 的路径
        string embeddedJdkPath = System.IO.Path.Combine(unityEditorDirectory, "Data", "PlaybackEngines", "AndroidPlayer", "OpenJDK");

        var gradle_properties = $"{exportPath}/gradle.properties";
        if(!File.ReadAllText(gradle_properties).Contains("org.gradle.java.home="))
        {
            File.AppendAllLines(gradle_properties, new[] {$"org.gradle.java.home={embeddedJdkPath.Replace("\\", "/")}" });
        }

        // Generate Readme
        File.WriteAllLines($"{exportPath}/Readme.txt", new[] {
            "确认已经安装了以下软件",
            "- gradle",
            " ",
            "执行以下步骤",
            "gradle.bat wrapper",
            @"./gradlew.bat assembleRelease"
        });

        Debug.Log("Export Finish.");
    }

    // 获取所有已启用的场景
    private static string[] GetEnabledScenes()
    {
        var scenes = EditorBuildSettings.scenes;
        var enabledScenes = new System.Collections.Generic.List<string>();

        foreach (var scene in scenes)
        {
            if (scene.enabled)
            {
                enabledScenes.Add(scene.path);
            }
        }

        return enabledScenes.ToArray();
    }
}
