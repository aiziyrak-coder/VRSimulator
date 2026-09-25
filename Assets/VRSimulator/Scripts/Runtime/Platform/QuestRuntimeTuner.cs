using UnityEngine;
using UnityEngine.XR;

namespace VRSimulator.Platform
{
    /// <summary>
    /// Meta Quest'da barqaror kadrlar tezligi uchun ishga tushirishda
    /// render sozlamalarini qo'yadi. Faqat barqaror Unity API'laridan foydalanadi.
    /// </summary>
    public class QuestRuntimeTuner : MonoBehaviour
    {
        [Header("Render masshtabi")]
        [SerializeField, Range(0.7f, 1.4f), Tooltip("Ko'z teksturasi masshtabi. 1.0 — standart, 1.2 — aniqroq lekin qimmatroq")]
        private float eyeTextureScale = 1.0f;

        [Header("Kadrlar tezligi")]
        [SerializeField, Tooltip("Maqsadli FPS. Quest 2/3 uchun 72 yoki 90")]
        private int targetFrameRate = 72;

        [SerializeField, Tooltip("VR'da vSync o'chirilishi kerak — shlem o'zi sinxronlaydi")]
        private bool disableVSync = true;

        [Header("Diagnostika")]
        [SerializeField, Tooltip("Ishga tushirishda qurilma ma'lumotini logga yozadi")]
        private bool logDeviceInfo = true;

        private void Awake()
        {
            if (disableVSync) QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = targetFrameRate;

            if (XRSettings.enabled)
            {
                XRSettings.eyeTextureResolutionScale = eyeTextureScale;
            }

            if (logDeviceInfo) LogDeviceInfo();
        }

        private void LogDeviceInfo()
        {
            Debug.Log($"[VRSimulator] XR yoqilgan: {XRSettings.enabled}, " +
                      $"qurilma: '{XRSettings.loadedDeviceName}', " +
                      $"ko'z teksturasi: {XRSettings.eyeTextureWidth}x{XRSettings.eyeTextureHeight}, " +
                      $"grafika: {SystemInfo.graphicsDeviceType}, " +
                      $"protsessor: {SystemInfo.processorType} ({SystemInfo.processorCount} oqim), " +
                      $"RAM: {SystemInfo.systemMemorySize} MB");
        }
    }
}
