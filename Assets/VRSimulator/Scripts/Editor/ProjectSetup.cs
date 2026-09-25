using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR.Management;

namespace VRSimulator.Editor
{
    /// <summary>
    /// Loyihani Meta Quest uchun bir marta sozlaydi: URP aktivlari, XR yuklovchisi,
    /// Player/Quality sozlamalari. Unity'ning o'z API'lari ishlatiladi, shuning uchun
    /// natija joriy paket versiyalariga mos bo'ladi.
    /// </summary>
    public static class ProjectSetup
    {
        private const string SettingsFolder = "Assets/VRSimulator/Settings";
        private const string XrFolder = "Assets/XR";
        private const string UrpAssetPath = SettingsFolder + "/VRSimulator_URP.asset";
        private const string RendererPath = SettingsFolder + "/VRSimulator_Renderer.asset";
        private const string XrSettingsPath = XrFolder + "/XRGeneralSettings.asset";
        private const string OpenXrLoaderType = "UnityEngine.XR.OpenXR.OpenXRLoader";

        [MenuItem("Tools/VRSimulator/1. Loyihani sozlash", priority = 1)]
        public static void Run()
        {
            var report = new SetupReport();

            report.Step("URP aktivlari yaratildi va ulandi", ConfigureUrp);
            report.Step("Android Player sozlamalari (Quest)", ConfigurePlayerSettings);
            report.Step("Sifat sozlamalari (VR uchun)", ConfigureQuality);
            report.Step("XR Plug-in Management: OpenXR (Android)", ConfigureXr);

            AssetDatabase.SaveAssets();
            report.Print("VRSimulator — loyiha sozlandi");

            Debug.Log(
                "Qo'lda bajarilishi kerak bo'lgan 2 qadam:\n" +
                "  1) Project Settings > XR Plug-in Management > OpenXR > Interaction Profiles " +
                "ga 'Oculus Touch Controller Profile' qo'shing (pult tugmalari shu orqali ishlaydi).\n" +
                "  2) Window > Package Manager > XR Interaction Toolkit > Samples > 'Starter Assets' ni Import qiling " +
                "(standart input amallari va pult presetlari shu namunada keladi).\n" +
                "Keyin: Tools > VRSimulator > 2. Namunaviy sahna yaratish.");
        }

        private static void ConfigureUrp()
        {
            EnsureFolder(SettingsFolder);

            var urp = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(UrpAssetPath);
            if (urp == null)
            {
                var rendererData = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererPath);
                if (rendererData == null)
                {
                    rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
                    AssetDatabase.CreateAsset(rendererData, RendererPath);
                }

                urp = UniversalRenderPipelineAsset.Create(rendererData);
                AssetDatabase.CreateAsset(urp, UrpAssetPath);
            }

            // Quest — mobil GPU. Og'ir imkoniyatlar o'chiriladi.
            urp.supportsHDR = false;
            urp.msaaSampleCount = 4;              // VR'da qirralar uchun MSAA 4x arzon va zarur
            urp.renderScale = 1.0f;
            urp.shadowDistance = 25f;
            urp.supportsCameraDepthTexture = false;
            urp.supportsCameraOpaqueTexture = false;

            GraphicsSettings.defaultRenderPipeline = urp;
            for (int i = 0; i < QualitySettings.count; i++)
            {
                QualitySettings.SetQualityLevel(i, false);
                QualitySettings.renderPipeline = urp;
            }

            EditorUtility.SetDirty(urp);
        }

        private static void ConfigurePlayerSettings()
        {
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.gpuSkinning = true;
            PlayerSettings.companyName = "Aiziyrak";
            PlayerSettings.productName = "VRSimulator";

            var android = NamedBuildTarget.Android;
            PlayerSettings.SetApplicationIdentifier(android, "com.aiziyrak.vrsimulator");
            PlayerSettings.SetScriptingBackend(android, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetManagedStrippingLevel(android, ManagedStrippingLevel.Low);
            PlayerSettings.SetMobileMTRendering(android, true);

            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel32;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.Android.blitType = AndroidBlitType.Never;
            PlayerSettings.Android.androidIsGame = true;
            PlayerSettings.Android.optimizedFramePacing = true;

            // Quest Vulkan'da ishlaydi; GLES3 zaxira sifatida ham qoldirilmaydi,
            // aks holda Unity noto'g'ri API'ni tanlab qolishi mumkin.
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.Vulkan });

            PlayerSettings.SplashScreen.show = false;
        }

        private static void ConfigureQuality()
        {
            for (int i = 0; i < QualitySettings.count; i++)
            {
                QualitySettings.SetQualityLevel(i, false);
                QualitySettings.vSyncCount = 0;          // VR'da shlem o'zi sinxronlaydi
                QualitySettings.shadows = ShadowQuality.HardOnly;
                QualitySettings.shadowResolution = ShadowResolution.Low;
                QualitySettings.skinWeights = SkinWeights.TwoBones;
                QualitySettings.realtimeReflectionProbes = false;
                QualitySettings.softParticles = false;
            }
        }

        private static void ConfigureXr()
        {
            EnsureFolder(XrFolder);

            if (!EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey,
                    out XRGeneralSettingsPerBuildTarget perBuildTarget) || perBuildTarget == null)
            {
                perBuildTarget = AssetDatabase.LoadAssetAtPath<XRGeneralSettingsPerBuildTarget>(XrSettingsPath);
                if (perBuildTarget == null)
                {
                    perBuildTarget = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
                    AssetDatabase.CreateAsset(perBuildTarget, XrSettingsPath);
                }
                EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, perBuildTarget, true);
            }

            foreach (var group in new[] { BuildTargetGroup.Android, BuildTargetGroup.Standalone })
            {
                if (!perBuildTarget.HasManagerSettingsForBuildTarget(group))
                {
                    perBuildTarget.CreateDefaultManagerSettingsForBuildTarget(group);
                }

                var settings = perBuildTarget.SettingsForBuildTarget(group);
                if (settings == null || settings.Manager == null) continue;

                settings.InitManagerOnStart = true;
                XRPackageMetadataStore.AssignLoader(settings.Manager, OpenXrLoaderType, group);
                EditorUtility.SetDirty(settings);
            }

            EditorUtility.SetDirty(perBuildTarget);
        }

        /// <summary>Ko'p qatlamli papkani AssetDatabase uchun yaratadi.</summary>
        internal static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            var parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            var leaf = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && parent != "Assets") EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
