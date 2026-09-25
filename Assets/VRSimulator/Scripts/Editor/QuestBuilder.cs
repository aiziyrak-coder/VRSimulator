using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace VRSimulator.Editor
{
    /// <summary>
    /// Meta Quest uchun APK yig'adi. Menyudan yoki buyruq satridan ishlatiladi:
    /// Unity.exe -quit -batchmode -projectPath . -executeMethod VRSimulator.Editor.QuestBuilder.BuildFromCommandLine
    /// </summary>
    public static class QuestBuilder
    {
        private const string OutputFolder = "Builds/Android";
        private const string ApkName = "VRSimulator.apk";

        [MenuItem("Tools/VRSimulator/3. Quest uchun APK yig'ish", priority = 3)]
        public static void BuildFromMenu()
        {
            var summary = BuildApk(development: true);
            if (summary == null) return;

            if (summary.Value.result == BuildResult.Succeeded)
            {
                EditorUtility.RevealInFinder(Path.Combine(OutputFolder, ApkName));
            }
        }

        /// <summary>Buyruq satridan ishga tushirish uchun kirish nuqtasi (CI).</summary>
        public static void BuildFromCommandLine()
        {
            var development = Environment.GetCommandLineArgs().Contains("-vrsimDevelopment");
            var summary = BuildApk(development);

            if (summary == null || summary.Value.result != BuildResult.Succeeded)
            {
                EditorApplication.Exit(1);
                return;
            }
            EditorApplication.Exit(0);
        }

        private static BuildSummary? BuildApk(bool development)
        {
            var scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                Debug.LogError("[VRSimulator] Build Settings da yoqilgan sahna yo'q. " +
                               "Tools > VRSimulator > 2. Namunaviy sahna yaratish ni bajaring.");
                return null;
            }

            Directory.CreateDirectory(OutputFolder);

            var options = BuildOptions.None;
            if (development)
            {
                // Qurilmada log ko'rish va profiler ulash uchun.
                options |= BuildOptions.Development | BuildOptions.ConnectWithProfiler | BuildOptions.AllowDebugging;
            }

            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = Path.Combine(OutputFolder, ApkName),
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = options,
            };

            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            var summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[VRSimulator] APK tayyor: {buildPlayerOptions.locationPathName} " +
                          $"({summary.totalSize / (1024 * 1024)} MB, {summary.totalTime.TotalSeconds:F0} s)\n" +
                          "Qurilmaga o'rnatish: adb install -r " + buildPlayerOptions.locationPathName);
            }
            else
            {
                Debug.LogError($"[VRSimulator] Build muvaffaqiyatsiz: {summary.result}, " +
                               $"{summary.totalErrors} xato.");
            }

            return summary;
        }
    }
}
