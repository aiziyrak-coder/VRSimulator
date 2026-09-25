using UnityEngine;

namespace VRSimulator.Interaction
{
    /// <summary>
    /// Buyum ushlanganda/qo'yib yuborilganda oddiy ovozli va vizual javob beradi.
    /// XR Interaction Toolkit'ning XRGrabInteractable komponentidagi
    /// "Select Entered" / "Select Exited" hodisalariga Inspector orqali ulanadi —
    /// shu sababli bu skript XRI versiyasiga bog'liq emas.
    /// </summary>
    public class InteractionFeedback : MonoBehaviour
    {
        [Header("Ovoz")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip grabClip;
        [SerializeField] private AudioClip releaseClip;

        [Header("Ko'rinish")]
        [SerializeField, Tooltip("Ushlanganda buyum shu nisbatda kattalashadi")]
        private float grabScale = 1.05f;
        [SerializeField, Min(0.01f)] private float scaleSpeed = 12f;

        private Vector3 baseScale;
        private Vector3 targetScale;

        private void Awake()
        {
            baseScale = transform.localScale;
            targetScale = baseScale;
        }

        private void Update()
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
        }

        /// <summary>XRGrabInteractable > Select Entered shuni chaqiradi.</summary>
        public void OnGrabbed()
        {
            targetScale = baseScale * grabScale;
            Play(grabClip);
        }

        /// <summary>XRGrabInteractable > Select Exited shuni chaqiradi.</summary>
        public void OnReleased()
        {
            targetScale = baseScale;
            Play(releaseClip);
        }

        private void Play(AudioClip clip)
        {
            if (audioSource != null && clip != null) audioSource.PlayOneShot(clip);
        }
    }
}
