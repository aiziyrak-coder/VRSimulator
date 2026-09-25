using UnityEngine;

namespace VRSimulator.Interaction
{
    /// <summary>
    /// Mashqda ishtirok etadigan buyumga identifikator beradi.
    /// <see cref="PlacementZone"/> shu identifikator bo'yicha to'g'ri buyumni aniqlaydi.
    /// </summary>
    public class SimObject : MonoBehaviour
    {
        [Tooltip("Buyum identifikatori, masalan: 'kalit', 'filtr', 'qalpoq'")]
        public string objectId = "buyum";

        [Tooltip("Foydalanuvchiga ko'rinadigan nom")]
        public string displayName = "";

        public string DisplayName => string.IsNullOrEmpty(displayName) ? objectId : displayName;

        /// <summary>Collider joylashgan bola obyektdan ham asosiy buyumni topadi.</summary>
        public static SimObject Find(Component source)
        {
            if (source == null) return null;
            return source.GetComponentInParent<SimObject>();
        }
    }
}
