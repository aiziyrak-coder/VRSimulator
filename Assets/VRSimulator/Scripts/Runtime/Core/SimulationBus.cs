using System;

namespace VRSimulator.Core
{
    /// <summary>
    /// Sahnadagi komponentlarni bir-biriga bog'lamasdan xabar almashish uchun yengil hub.
    /// Masalan <see cref="VRSimulator.Interaction.PlacementZone"/> hodisa chiqaradi,
    /// <see cref="TaskRunner"/> esa uni eshitadi — ular bir-birini bilmaydi.
    /// </summary>
    public static class SimulationBus
    {
        /// <summary>Muvaffaqiyatli bajarilgan amal (hodisa identifikatori).</summary>
        public static event Action<string> Completed;

        /// <summary>Xato amal (hodisa identifikatori va sabab).</summary>
        public static event Action<string, string> Failed;

        public static void RaiseCompleted(string eventId)
        {
            if (!string.IsNullOrEmpty(eventId)) Completed?.Invoke(eventId);
        }

        public static void RaiseFailed(string eventId, string reason = null)
        {
            if (!string.IsNullOrEmpty(eventId)) Failed?.Invoke(eventId, reason);
        }

        /// <summary>
        /// Play rejimidan chiqishda qolgan obunalarni tozalaydi.
        /// Domain reload o'chirilgan bo'lsa (Enter Play Mode Options) bu muhim.
        /// </summary>
        public static void Clear()
        {
            Completed = null;
            Failed = null;
        }
    }
}
