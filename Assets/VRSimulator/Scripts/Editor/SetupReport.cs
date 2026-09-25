using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace VRSimulator.Editor
{
    /// <summary>
    /// Sozlash qadamlarining natijasini yig'adi. Har bir qadam alohida
    /// try/catch ichida bajariladi — bittasi ishlamasa, qolganlari davom etadi.
    /// </summary>
    public class SetupReport
    {
        private enum Status { Ok, Skipped, Failed }

        private readonly List<(Status status, string name, string detail)> entries = new();

        public bool HasFailures { get; private set; }

        /// <summary>Qadamni bajaradi va natijasini yozib qo'yadi.</summary>
        public void Step(string name, Action action)
        {
            try
            {
                action();
                entries.Add((Status.Ok, name, null));
            }
            catch (Exception ex)
            {
                HasFailures = true;
                entries.Add((Status.Failed, name, ex.Message));
            }
        }

        public void Skip(string name, string reason)
        {
            entries.Add((Status.Skipped, name, reason));
        }

        public void Print(string header)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"=== {header} ===");
            foreach (var (status, name, detail) in entries)
            {
                var mark = status switch
                {
                    Status.Ok => "[OK]     ",
                    Status.Skipped => "[O'TDI]  ",
                    _ => "[XATO]   ",
                };
                sb.Append(mark).Append(name);
                if (!string.IsNullOrEmpty(detail)) sb.Append(" — ").Append(detail);
                sb.AppendLine();
            }

            if (HasFailures)
            {
                sb.AppendLine();
                sb.AppendLine("Ba'zi qadamlar bajarilmadi. docs/SETUP.md dagi qo'lda sozlash bo'limiga qarang.");
                Debug.LogWarning(sb.ToString());
            }
            else
            {
                Debug.Log(sb.ToString());
            }
        }
    }
}
