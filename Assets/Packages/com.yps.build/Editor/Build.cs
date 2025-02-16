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
        // ���õ���·��
        string exportPath = "Builds/AndroidProject";

        // ��鲢��������Ŀ¼
        if (!System.IO.Directory.Exists(exportPath))
        {
            System.IO.Directory.CreateDirectory(exportPath);
        }

        // �л��� Android ƽ̨
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            Debug.Log("Switching to Android platform...");
            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
            {
                Debug.LogError("Failed to switch to Android platform!");
                return;
            }
        }

        // ���ù���ѡ��
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = GetEnabledScenes(), // ��ȡ���������õĳ���
            locationPathName = exportPath, // ����·��
            target = BuildTarget.Android, // Ŀ��ƽ̨
            options = BuildOptions.AcceptExternalModificationsToPlayer // ����Ϊ Android ����
        };

        // ��ʼ����
        Debug.Log("Exporting Android project...");
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);

        // ��鹹�����
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

        // ��ȡ Unity ��װĿ¼
        string unityEditorPath = EditorApplication.applicationPath;
        string unityEditorDirectory = System.IO.Path.GetDirectoryName(unityEditorPath);

        // ƴ������ JDK ��·��
        string embeddedJdkPath = System.IO.Path.Combine(unityEditorDirectory, "Data", "PlaybackEngines", "AndroidPlayer", "OpenJDK");

        var gradle_properties = $"{exportPath}/gradle.properties";
        if(!File.ReadAllText(gradle_properties).Contains("org.gradle.java.home="))
        {
            File.AppendAllLines(gradle_properties, new[] {$"org.gradle.java.home={embeddedJdkPath.Replace("\\", "/")}" });
        }

        // Generate Readme
        File.WriteAllLines($"{exportPath}/Readme.txt", new[] {
            "ȷ���Ѿ���װ���������",
            "- gradle",
            " ",
            "ִ�����²���",
            "gradle.bat wrapper",
            @"./gradlew.bat assembleRelease"
        });

        Debug.Log("Export Finish.");
    }

    // ��ȡ���������õĳ���
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
