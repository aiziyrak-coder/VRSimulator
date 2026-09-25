using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace VRSimulator.Editor
{
    /// <summary>
    /// XR Interaction Toolkit komponentlarini nomi bo'yicha qo'shish uchun yordamchi.
    /// XRI 2.x va 3.x da nom maydonlari (namespace) farq qiladi, shuning uchun
    /// to'g'ridan-to'g'ri havola o'rniga turni ish vaqtida topamiz —
    /// natijada bu skript paket versiyasi o'zgarganda ham kompilyatsiya bo'ladi.
    /// </summary>
    internal static class EditorTypeUtility
    {
        /// <summary>Berilgan to'liq nomlardan birinchi topilgan turni qaytaradi.</summary>
        public static Type FindType(params string[] fullNames)
        {
            foreach (var name in fullNames)
            {
                var type = AppDomain.CurrentDomain.GetAssemblies()
                    .Select(assembly => assembly.GetType(name, throwOnError: false))
                    .FirstOrDefault(t => t != null);
                if (type != null) return type;
            }
            return null;
        }

        /// <summary>
        /// Komponentni nomi bo'yicha qo'shadi. Tur topilmasa ogohlantiradi va null qaytaradi —
        /// sahna qurish to'xtamaydi.
        /// </summary>
        public static Component AddComponent(GameObject target, string humanName, params string[] fullNames)
        {
            var type = FindType(fullNames);
            if (type == null)
            {
                Debug.LogWarning($"[VRSimulator] '{humanName}' turi topilmadi ({string.Join(", ", fullNames)}). " +
                                 "Kerakli paket o'rnatilganini tekshiring.");
                return null;
            }
            return target.GetComponent(type) ?? target.AddComponent(type);
        }

        /// <summary>private [SerializeField] maydonga qiymat yozadi.</summary>
        public static void SetObjectField(UnityEngine.Object target, string fieldName, UnityEngine.Object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(fieldName);
            if (prop == null)
            {
                Debug.LogWarning($"[VRSimulator] '{target.GetType().Name}.{fieldName}' maydoni topilmadi.");
                return;
            }
            prop.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
