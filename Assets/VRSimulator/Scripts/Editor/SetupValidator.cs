using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering;

namespace VRSimulator.Editor
{
    /// <summary>
    /// Loyiha Quest uchun to'g'ri sozlanganini tekshiradi va nimani tuzatish
    /// kerakligini aytadi. Build qilishdan oldin ishlatish tavsiya etiladi.
    /// </summary>
    public static class SetupValidator
    {
        [MenuItem("Tools/VRSimulator/4. Sozlamalarni tekshirish", priority = 4)]
        public static void Validate()
        {
            var problems = new List<string>();

            if (GraphicsSettings.defaultRenderPipeline == null)
                problems.Add("URP aktivi ulanmagan (Graphics > Default Render Pipeline bo'sh).");

            if (PlayerSettings.colorSpace != ColorSpace.Linear)
                problems.Add("Color Space = Linear bo'lishi kerak (Gamma VR'da noto'g'ri yorug'lik beradi).");

            if (PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android) != ScriptingImplementation.IL2CPP)
                problems.Add("Android uchun Scripting Backend = IL2CPP bo'lishi kerak.");

            if (PlayerSettings.Android.targetArchitectures != AndroidArchitecture.ARM64)
                problems.Add("Target Architectures = ARM64 bo'lishi kerak (Quest faqat ARM64 qabul qiladi).");

            if ((int)PlayerSettings.Android.minSdkVersion < 32)
                problems.Add("Minimum API Level 32 yoki undan yuqori bo'lishi kerak (Meta talabi).");

            var apis = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android);
            if (apis.Length == 0 || apis[0] != GraphicsDeviceType.Vulkan)
                problems.Add("Android grafika API ro'yxatida Vulkan birinchi bo'lishi kerak.");

            if (Mathf.Abs(Time.fixedDeltaTime - 1f / 72f) > 0.001f)
                problems.Add($"Fixed Timestep {Time.fixedDeltaTime:F4} — Quest uchun 1/72 (0.01389) tavsiya etiladi.");

            var enabledScenes = EditorBuildSettings.scenes.Count(scene => scene.enabled);
            if (enabledScenes == 0)
                problems.Add("Build Settings da yoqilgan sahna yo'q.");

            if (EditorTypeUtility.FindType("UnityEngine.XR.OpenXR.OpenXRSettings") == null)
                problems.Add("OpenXR paketi topilmadi (com.unity.xr.openxr).");

            if (EditorTypeUtility.FindType(
                    "UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable",
                    "UnityEngine.XR.Interaction.Toolkit.XRGrabInteractable") == null)
                problems.Add("XR Interaction Toolkit topilmadi (com.unity.xr.interaction.toolkit).");

            if (problems.Count == 0)
            {
                Debug.Log("[VRSimulator] Sozlamalar tekshiruvi: hammasi joyida. Build qilishga tayyor.");
                return;
            }

            var message = "[VRSimulator] Sozlamalar tekshiruvi — e'tibor talab qiladi:\n" +
                          string.Join("\n", problems.Select(p => "  • " + p)) +
                          "\nKo'pini Tools > VRSimulator > 1. Loyihani sozlash tuzatadi.";
            Debug.LogWarning(message);
        }
    }
}
