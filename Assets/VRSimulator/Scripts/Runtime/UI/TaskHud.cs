using UnityEngine;
using UnityEngine.UI;

namespace VRSimulator.UI
{
    /// <summary>
    /// Mashq ko'rsatmalarini VR ichidagi panelga chiqaradi.
    /// Matnlar <see cref="VRSimulator.Core.TaskRunner"/> hodisalaridan keladi —
    /// ular Inspector orqali yoki sahna generatori tomonidan ulanadi.
    /// </summary>
    public class TaskHud : MonoBehaviour
    {
        [Header("Matn maydonlari")]
        [SerializeField] private Text instructionLabel;
        [SerializeField] private Text statusLabel;

        [Header("Bajarilish darajasi")]
        [SerializeField] private Image progressFill;

        [Header("Xato xabari")]
        [SerializeField, Min(0.5f), Tooltip("Xato xabari ekranda qancha turadi (sekund)")]
        private float mistakeDisplayTime = 3f;

        private float mistakeHideAt = -1f;

        private void Update()
        {
            if (mistakeHideAt > 0f && Time.time >= mistakeHideAt)
            {
                mistakeHideAt = -1f;
                SetStatus("");
            }
        }

        /// <summary>TaskRunner.OnInstructionChanged shuni chaqiradi.</summary>
        public void SetInstruction(string text)
        {
            if (instructionLabel != null) instructionLabel.text = text;
        }

        /// <summary>TaskRunner.OnProgressChanged shuni chaqiradi.</summary>
        public void SetProgress(float normalized)
        {
            if (progressFill != null) progressFill.fillAmount = Mathf.Clamp01(normalized);
        }

        /// <summary>TaskRunner.OnMistake shuni chaqiradi.</summary>
        public void ShowMistake(string reason)
        {
            SetStatus(reason);
            mistakeHideAt = Time.time + mistakeDisplayTime;
        }

        /// <summary>TaskRunner.OnTaskCompleted shuni chaqiradi.</summary>
        public void ShowCompleted()
        {
            SetStatus("Topshiriq bajarildi.");
            SetProgress(1f);
            mistakeHideAt = -1f;
        }

        private void SetStatus(string text)
        {
            if (statusLabel != null) statusLabel.text = text;
        }
    }
}
