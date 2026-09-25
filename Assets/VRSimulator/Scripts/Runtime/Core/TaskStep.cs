using System;
using UnityEngine;

namespace VRSimulator.Core
{
    /// <summary>Mashq topshirig'ining bitta qadami.</summary>
    [Serializable]
    public class TaskStep
    {
        [Tooltip("Qadamning ichki identifikatori (takrorlanmasin)")]
        public string id = "step";

        [Tooltip("Foydalanuvchiga ko'rsatiladigan ko'rsatma")]
        [TextArea(2, 4)]
        public string instruction = "Ko'rsatma matni";

        [Tooltip("Yordam kerak bo'lganda ko'rsatiladigan maslahat")]
        [TextArea(1, 3)]
        public string hint = "";

        [Tooltip("Shu qadamni yopadigan hodisa identifikatori (PlacementZone yoki boshqa komponentdan)")]
        public string completionEventId = "";

        [Tooltip("Qadamga ajratilgan vaqt (sekund). 0 = cheklovsiz")]
        [Min(0f)]
        public float timeLimit = 0f;

        public bool HasTimeLimit => timeLimit > 0f;
    }
}
