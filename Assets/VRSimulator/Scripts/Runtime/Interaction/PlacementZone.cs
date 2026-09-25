using UnityEngine;
using UnityEngine.Events;
using VRSimulator.Core;

namespace VRSimulator.Interaction
{
    /// <summary>
    /// Buyum qo'yilishi kerak bo'lgan joy. Trigger kolliderga tayanadi,
    /// shuning uchun XR Interaction Toolkit versiyasiga bog'liq emas.
    /// To'g'ri buyum qo'yilsa <see cref="SimulationBus"/> ga hodisa chiqaradi.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class PlacementZone : MonoBehaviour
    {
        [Header("Shartlar")]
        [Tooltip("Shu zonaga qo'yilishi kerak bo'lgan buyum identifikatori")]
        public string expectedObjectId = "buyum";

        [Tooltip("Qadamni yopadigan hodisa identifikatori (TaskStep.completionEventId bilan bir xil bo'lsin)")]
        public string completionEventId = "";

        [Tooltip("Buyum qancha vaqt zonada turgandan keyin qabul qilinadi (sekund)")]
        [Min(0f)] public float holdDuration = 0.35f;

        [Tooltip("Bir marta bajarilgandan keyin qayta ishlamasin")]
        public bool oneShot = true;

        [Header("Hodisalar")]
        public UnityEvent OnAccepted = new UnityEvent();
        public UnityEvent<string> OnRejected = new UnityEvent<string>();

        private SimObject candidate;
        private float candidateEnteredAt;
        private bool consumed;

        private void Reset()
        {
            var col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (consumed && oneShot) return;

            var simObject = SimObject.Find(other);
            if (simObject == null) return;

            if (simObject.objectId != expectedObjectId)
            {
                var reason = $"'{simObject.DisplayName}' bu joyga to'g'ri kelmaydi.";
                OnRejected.Invoke(reason);
                SimulationBus.RaiseFailed(completionEventId, reason);
                return;
            }

            candidate = simObject;
            candidateEnteredAt = Time.time;
        }

        private void OnTriggerExit(Collider other)
        {
            var simObject = SimObject.Find(other);
            if (simObject != null && simObject == candidate) candidate = null;
        }

        private void Update()
        {
            if (candidate == null || (consumed && oneShot)) return;
            if (Time.time - candidateEnteredAt < holdDuration) return;

            consumed = true;
            candidate = null;
            OnAccepted.Invoke();
            SimulationBus.RaiseCompleted(completionEventId);
        }

        /// <summary>Topshiriq qayta boshlanganda zonani tozalaydi.</summary>
        public void ResetZone()
        {
            consumed = false;
            candidate = null;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.35f);
            var col = GetComponent<Collider>();
            if (col != null) Gizmos.DrawCube(col.bounds.center, col.bounds.size);
        }
    }
}
