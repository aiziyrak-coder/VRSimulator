using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace VRSimulator.Core
{
    /// <summary>
    /// Bitta mashq seansining natijasini yig'adi: vaqt, xatolar, qadamlar tarixi.
    /// <see cref="TaskRunner"/> bilan bir GameObject'da turadi.
    /// </summary>
    public class SimulationSession : MonoBehaviour
    {
        /// <summary>Bajarilgan qadam yozuvi.</summary>
        public readonly struct StepRecord
        {
            public readonly string StepId;
            public readonly float Seconds;

            public StepRecord(string stepId, float seconds)
            {
                StepId = stepId;
                Seconds = seconds;
            }
        }

        private readonly List<StepRecord> records = new List<StepRecord>();
        private readonly List<string> mistakes = new List<string>();

        private SimulationTask task;
        private float startedAt;
        private float finishedAt = -1f;

        public IReadOnlyList<StepRecord> Records => records;
        public IReadOnlyList<string> Mistakes => mistakes;
        public int MistakeCount => mistakes.Count;

        /// <summary>Seans davomiyligi (sekund).</summary>
        public float Duration => finishedAt >= 0f ? finishedAt - startedAt : Time.time - startedAt;

        /// <summary>0..100 oraliqdagi baho: har bir xato uchun 10 ball ayiriladi.</summary>
        public int Score => Mathf.Clamp(100 - mistakes.Count * 10, 0, 100);

        public void Begin(SimulationTask newTask)
        {
            task = newTask;
            records.Clear();
            mistakes.Clear();
            startedAt = Time.time;
            finishedAt = -1f;
        }

        public void RegisterStepCompleted(TaskStep step, float seconds)
        {
            if (step != null) records.Add(new StepRecord(step.id, seconds));
        }

        public void RegisterMistake(string reason)
        {
            mistakes.Add(reason);
        }

        public void Finish()
        {
            finishedAt = Time.time;
            Debug.Log(BuildReport());
        }

        /// <summary>Seans natijasini o'qishga qulay matn sifatida qaytaradi.</summary>
        public string BuildReport()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"=== Mashq natijasi: {(task != null ? task.title : "—")} ===");
            sb.AppendLine($"Vaqt: {Duration:F1} s");
            sb.AppendLine($"Bajarilgan qadamlar: {records.Count}");
            sb.AppendLine($"Xatolar: {mistakes.Count}");
            sb.AppendLine($"Baho: {Score}/100");

            if (records.Count > 0)
            {
                sb.AppendLine("Qadamlar bo'yicha vaqt:");
                foreach (var r in records) sb.AppendLine($"  - {r.StepId}: {r.Seconds:F1} s");
            }
            if (mistakes.Count > 0)
            {
                sb.AppendLine("Xatolar ro'yxati:");
                foreach (var m in mistakes) sb.AppendLine($"  - {m}");
            }
            return sb.ToString();
        }
    }
}
