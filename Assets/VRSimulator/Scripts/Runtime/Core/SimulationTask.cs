using System.Collections.Generic;
using UnityEngine;

namespace VRSimulator.Core
{
    /// <summary>
    /// Mashq topshirig'i: ketma-ket bajariladigan qadamlar to'plami.
    /// Assets > Create > VRSimulator > Mashq topshirig'i orqali yaratiladi.
    /// </summary>
    [CreateAssetMenu(menuName = "VRSimulator/Mashq topshirig'i", fileName = "YangiTopshiriq")]
    public class SimulationTask : ScriptableObject
    {
        [Tooltip("Topshiriq nomi (foydalanuvchiga ko'rinadi)")]
        public string title = "Yangi topshiriq";

        [Tooltip("Topshiriq haqida qisqa tavsif")]
        [TextArea(2, 5)]
        public string description = "";

        [Tooltip("Qadamlar — ro'yxatdagi tartibda bajariladi")]
        public List<TaskStep> steps = new List<TaskStep>();

        public int StepCount => steps?.Count ?? 0;

        public TaskStep GetStep(int index)
        {
            if (steps == null || index < 0 || index >= steps.Count) return null;
            return steps[index];
        }

        /// <summary>Qadam identifikatorlari takrorlanmasligini tekshiradi.</summary>
        public bool TryFindDuplicateId(out string duplicateId)
        {
            duplicateId = null;
            if (steps == null) return false;

            var seen = new HashSet<string>();
            foreach (var step in steps)
            {
                if (step == null || string.IsNullOrEmpty(step.id)) continue;
                if (!seen.Add(step.id))
                {
                    duplicateId = step.id;
                    return true;
                }
            }
            return false;
        }
    }
}
