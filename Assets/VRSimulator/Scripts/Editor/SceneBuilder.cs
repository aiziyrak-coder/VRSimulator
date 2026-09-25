using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VRSimulator.Core;
using VRSimulator.Interaction;
using VRSimulator.Platform;
using VRSimulator.UI;

namespace VRSimulator.Editor
{
    /// <summary>
    /// Namunaviy mashq sahnasini kod orqali yig'adi: xona, stol, ushlanadigan
    /// asboblar, ularni qo'yish joylari, VR ichidagi ko'rsatma paneli va topshiriq mantiqi.
    /// Sahna Unity ichida generatsiya qilinganligi uchun barcha havolalar to'g'ri bo'ladi.
    /// </summary>
    public static class SceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/VRSimulator.unity";
        private const string TaskPath = "Assets/VRSimulator/Settings/NamunaviyTopshiriq.asset";
        private const string MaterialFolder = "Assets/VRSimulator/Settings/Materials";

        /// <summary>Asbob: identifikator, ko'rinadigan nom, rang.</summary>
        private readonly struct ToolSpec
        {
            public readonly string Id;
            public readonly string Name;
            public readonly Color Color;

            public ToolSpec(string id, string name, Color color)
            {
                Id = id;
                Name = name;
                Color = color;
            }
        }

        private static readonly ToolSpec[] Tools =
        {
            new("kalit",  "Gayka kaliti", new Color(0.85f, 0.25f, 0.25f)),
            new("filtr",  "Havo filtri",  new Color(0.25f, 0.65f, 0.95f)),
            new("qalpoq", "Himoya qalpog'i", new Color(0.95f, 0.80f, 0.25f)),
        };

        [MenuItem("Tools/VRSimulator/2. Namunaviy sahna yaratish", priority = 2)]
        public static void Build()
        {
            if (!EditorUtility.DisplayDialog(
                    "Namunaviy sahna yaratish",
                    $"Yangi sahna yaratiladi va '{ScenePath}' ga saqlanadi.\n" +
                    "Ochiq sahnadagi saqlanmagan o'zgarishlar yo'qoladi. Davom etamizmi?",
                    "Ha, yaratish", "Bekor qilish"))
            {
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            var report = new SetupReport();
            ProjectSetup.EnsureFolder("Assets/Scenes");
            ProjectSetup.EnsureFolder(MaterialFolder);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            report.Step("Yorug'lik va muhit", BuildEnvironmentLighting);
            report.Step("Xona va stol", BuildRoom);

            GameObject hudRoot = null;
            TaskHud hud = null;
            report.Step("Ko'rsatma paneli (HUD)", () => hudRoot = BuildHud(out hud));

            report.Step("Asboblar va qo'yish joylari", BuildToolsAndZones);

            SimulationTask task = null;
            report.Step("Topshiriq aktivi", () => task = CreateTask());

            report.Step("Boshqaruv obyekti (TaskRunner)", () => BuildManager(task, hud, hudRoot));
            report.Step("XR Origin (shlem va pultlar)", BuildXrOrigin);
            report.Step("XR Device Simulator (shlemsiz sinash)", BuildDeviceSimulator);

            report.Step("Sahna saqlandi va Build Settings ga qo'shildi", () =>
            {
                EditorSceneManager.SaveScene(scene, ScenePath);
                AddSceneToBuildSettings(ScenePath);
            });

            AssetDatabase.SaveAssets();
            report.Print("VRSimulator — namunaviy sahna");

            Debug.Log($"Sahna tayyor: {ScenePath}. Play tugmasini bosib sinab ko'ring " +
                      $"(shlem ulanmagan bo'lsa XR Device Simulator ishlaydi: WASD — yurish, " +
                      $"sichqoncha — qarash, Enter — qadamni tasdiqlash).");
        }

        // ---------- Muhit ----------

        private static void BuildEnvironmentLighting()
        {
            var lightObject = new GameObject("Directional Light");
            lightObject.transform.SetPositionAndRotation(new Vector3(0f, 3f, 0f), Quaternion.Euler(50f, -30f, 0f));

            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            light.shadows = LightShadows.Hard;

            // Quest'da real vaqtdagi GI qimmat — oddiy gradient fon yetarli.
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.55f, 0.60f, 0.68f);
            RenderSettings.ambientEquatorColor = new Color(0.42f, 0.44f, 0.48f);
            RenderSettings.ambientGroundColor = new Color(0.22f, 0.22f, 0.24f);
        }

        private static void BuildRoom()
        {
            var root = new GameObject("Xona");

            var floor = CreatePrimitive(PrimitiveType.Plane, "Pol", root.transform,
                Vector3.zero, new Vector3(1.2f, 1f, 1.2f), new Color(0.35f, 0.37f, 0.40f));
            floor.isStatic = true;

            // To'rt devor — foydalanuvchi o'zini yopiq xonada his qilishi uchun.
            var wallColor = new Color(0.62f, 0.63f, 0.66f);
            CreateWall(root.transform, "Devor_Shimol", new Vector3(0f, 1.5f, 6f), new Vector3(12f, 3f, 0.2f), wallColor);
            CreateWall(root.transform, "Devor_Janub", new Vector3(0f, 1.5f, -6f), new Vector3(12f, 3f, 0.2f), wallColor);
            CreateWall(root.transform, "Devor_Sharq", new Vector3(6f, 1.5f, 0f), new Vector3(0.2f, 3f, 12f), wallColor);
            CreateWall(root.transform, "Devor_Garb", new Vector3(-6f, 1.5f, 0f), new Vector3(0.2f, 3f, 12f), wallColor);

            // Ish stoli — asboblar shu ustida turadi (VR uchun qulay balandlik 0.75 m).
            var table = CreatePrimitive(PrimitiveType.Cube, "Ish stoli", root.transform,
                new Vector3(0f, 0.375f, 1.2f), new Vector3(1.6f, 0.75f, 0.7f), new Color(0.45f, 0.32f, 0.22f));
            table.isStatic = true;
        }

        private static void CreateWall(Transform parent, string name, Vector3 position, Vector3 scale, Color color)
        {
            var wall = CreatePrimitive(PrimitiveType.Cube, name, parent, position, scale, color);
            wall.isStatic = true;
        }

        // ---------- Asboblar va zonalar ----------

        private static void BuildToolsAndZones()
        {
            var toolsRoot = new GameObject("Asboblar");
            var zonesRoot = new GameObject("Qo'yish joylari");

            for (int i = 0; i < Tools.Length; i++)
            {
                var spec = Tools[i];
                var x = -0.5f + i * 0.5f;

                // Ushlanadigan asbob.
                var tool = CreatePrimitive(PrimitiveType.Cube, spec.Name, toolsRoot.transform,
                    new Vector3(x, 0.83f, 1.2f), Vector3.one * 0.12f, spec.Color);

                var body = tool.AddComponent<Rigidbody>();
                body.mass = 0.5f;
                body.interpolation = RigidbodyInterpolation.Interpolate;
                body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

                var simObject = tool.AddComponent<SimObject>();
                simObject.objectId = spec.Id;
                simObject.displayName = spec.Name;

                var feedback = tool.AddComponent<InteractionFeedback>();
                var audioSource = tool.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 1f;
                EditorTypeUtility.SetObjectField(feedback, "audioSource", audioSource);

                // XRI versiyasidan qat'i nazar ushlanadigan qilish.
                var grab = EditorTypeUtility.AddComponent(tool, "XRGrabInteractable",
                    "UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable",
                    "UnityEngine.XR.Interaction.Toolkit.XRGrabInteractable");
                if (grab != null) WireGrabFeedback(grab, feedback);

                // Shu asbob qo'yilishi kerak bo'lgan joy.
                var zone = CreatePrimitive(PrimitiveType.Cube, $"Joy_{spec.Name}", zonesRoot.transform,
                    new Vector3(x, 0.79f, 0.55f), new Vector3(0.22f, 0.02f, 0.22f),
                    new Color(spec.Color.r, spec.Color.g, spec.Color.b, 0.45f));

                var trigger = zone.AddComponent<BoxCollider>();
                trigger.isTrigger = true;
                trigger.size = new Vector3(1f, 8f, 1f); // qo'yilganda aniqroq ushlash uchun balandroq

                var placement = zone.AddComponent<PlacementZone>();
                placement.expectedObjectId = spec.Id;
                placement.completionEventId = $"joyla_{spec.Id}";
            }
        }

        private static void WireGrabFeedback(Component grabInteractable, InteractionFeedback feedback)
        {
            // XRGrabInteractable'dagi selectEntered/selectExited maydonlari UnityEvent'dan meros oladi,
            // shuning uchun ularni SerializedObject orqali emas, reflection bilan olamiz.
            var type = grabInteractable.GetType();
            TryAddVoidListener(type, grabInteractable, "m_SelectEntered", "selectEntered", feedback.OnGrabbed);
            TryAddVoidListener(type, grabInteractable, "m_SelectExited", "selectExited", feedback.OnReleased);
        }

        private static void TryAddVoidListener(System.Type type, Component instance,
            string fieldName, string propertyName, UnityAction callback)
        {
            const System.Reflection.BindingFlags flags =
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic;

            object target = type.GetField(fieldName, flags)?.GetValue(instance)
                            ?? type.GetProperty(propertyName, flags)?.GetValue(instance);

            if (target is UnityEventBase eventBase)
            {
                UnityEventTools.AddVoidPersistentListener(eventBase, callback);
            }
            else
            {
                Debug.LogWarning($"[VRSimulator] '{propertyName}' hodisasi topilmadi — " +
                                 "ushlash effektini Inspector orqali qo'lda ulash kerak.");
            }
        }

        // ---------- HUD ----------

        private static GameObject BuildHud(out TaskHud hud)
        {
            var canvasObject = new GameObject("Ko'rsatma paneli");
            canvasObject.transform.SetPositionAndRotation(new Vector3(0f, 1.6f, 2.2f), Quaternion.identity);

            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;      // VR'da panel dunyo ichida turadi
            canvasObject.AddComponent<CanvasScaler>();

            var rect = canvas.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(900f, 400f);
            rect.localScale = Vector3.one * 0.0016f;        // ~1.4 m kenglik

            CreateImage(canvasObject.transform, "Fon", new Color(0.08f, 0.09f, 0.11f, 0.88f),
                Vector2.zero, new Vector2(900f, 400f));

            var instruction = CreateText(canvasObject.transform, "Ko'rsatma",
                "Mashq boshlanmoqda...", 40, TextAnchor.UpperLeft,
                new Vector2(0f, 40f), new Vector2(820f, 230f));

            var status = CreateText(canvasObject.transform, "Holat",
                "", 32, TextAnchor.MiddleLeft,
                new Vector2(0f, -110f), new Vector2(820f, 70f));
            status.color = new Color(1f, 0.72f, 0.3f);

            CreateImage(canvasObject.transform, "Progress_Fon", new Color(1f, 1f, 1f, 0.15f),
                new Vector2(0f, -170f), new Vector2(820f, 22f));
            var progressFill = CreateImage(canvasObject.transform, "Progress_Chiziq",
                new Color(0.3f, 0.85f, 0.45f), new Vector2(0f, -170f), new Vector2(820f, 22f));
            progressFill.type = Image.Type.Filled;
            progressFill.fillMethod = Image.FillMethod.Horizontal;
            progressFill.fillAmount = 0f;

            hud = canvasObject.AddComponent<TaskHud>();
            EditorTypeUtility.SetObjectField(hud, "instructionLabel", instruction);
            EditorTypeUtility.SetObjectField(hud, "statusLabel", status);
            EditorTypeUtility.SetObjectField(hud, "progressFill", progressFill);

            return canvasObject;
        }

        // ---------- Boshqaruv ----------

        private static SimulationTask CreateTask()
        {
            var task = AssetDatabase.LoadAssetAtPath<SimulationTask>(TaskPath);
            if (task == null)
            {
                task = ScriptableObject.CreateInstance<SimulationTask>();
                AssetDatabase.CreateAsset(task, TaskPath);
            }

            task.title = "Asboblarni joyiga qo'yish";
            task.description = "Stol ustidagi asboblarni belgilangan joylarga ketma-ket qo'ying.";
            task.steps = new List<TaskStep>();

            foreach (var spec in Tools)
            {
                task.steps.Add(new TaskStep
                {
                    id = $"qadam_{spec.Id}",
                    instruction = $"{spec.Name}ni o'z joyiga qo'ying.",
                    hint = $"{spec.Name} stol ustida turadi — uni ushlab, bir xil rangdagi maydonga olib boring.",
                    completionEventId = $"joyla_{spec.Id}",
                    timeLimit = 0f,
                });
            }

            EditorUtility.SetDirty(task);
            return task;
        }

        private static void BuildManager(SimulationTask task, TaskHud hud, GameObject hudRoot)
        {
            var manager = new GameObject("Mashq boshqaruvi");

            manager.AddComponent<SimulationSession>();
            var runner = manager.AddComponent<TaskRunner>();
            var input = manager.AddComponent<SimulationInput>();
            manager.AddComponent<QuestRuntimeTuner>();

            EditorTypeUtility.SetObjectField(runner, "task", task);
            EditorTypeUtility.SetObjectField(input, "taskRunner", runner);
            EditorTypeUtility.SetObjectField(input, "hudRoot", hudRoot);

            if (hud == null) return;

            UnityEventTools.AddStringPersistentListener(runner.OnInstructionChanged, hud.SetInstruction, string.Empty);
            UnityEventTools.AddFloatPersistentListener(runner.OnProgressChanged, hud.SetProgress, 0f);
            UnityEventTools.AddStringPersistentListener(runner.OnMistake, hud.ShowMistake, string.Empty);
            UnityEventTools.AddVoidPersistentListener(runner.OnTaskCompleted, hud.ShowCompleted);
        }

        private static void BuildXrOrigin()
        {
            // XRI o'zining menyu buyrug'i bilan rig yaratadi — bu joriy versiyaga
            // to'g'ri keladigan eng ishonchli usul.
            if (EditorApplication.ExecuteMenuItem("GameObject/XR/XR Origin (VR)")) return;
            if (EditorApplication.ExecuteMenuItem("GameObject/XR/XR Origin (Action-based)")) return;

            Debug.LogWarning("[VRSimulator] XR Origin avtomatik yaratilmadi. " +
                             "Sahnada o'ng tugma > XR > XR Origin (VR) ni qo'lda qo'shing.");
        }

        private static void BuildDeviceSimulator()
        {
            var simulatorObject = new GameObject("XR Device Simulator");
            var component = EditorTypeUtility.AddComponent(simulatorObject, "XRDeviceSimulator",
                "UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.XRDeviceSimulator");

            if (component == null)
            {
                Object.DestroyImmediate(simulatorObject);
                Debug.LogWarning("[VRSimulator] XR Device Simulator topilmadi. " +
                                 "XR Interaction Toolkit > Samples > 'XR Device Simulator' namunasini import qiling.");
            }
        }

        // ---------- Umumiy yordamchilar ----------

        private static GameObject CreatePrimitive(PrimitiveType type, string name, Transform parent,
            Vector3 position, Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;

            var renderer = go.GetComponent<MeshRenderer>();
            if (renderer != null) renderer.sharedMaterial = GetMaterial(color);
            return go;
        }

        /// <summary>Rang bo'yicha URP materialini yaratadi (bir marta) va qayta ishlatadi.</summary>
        private static Material GetMaterial(Color color)
        {
            var transparent = color.a < 0.99f;
            var path = $"{MaterialFolder}/Mat_{ColorUtility.ToHtmlStringRGBA(color)}.mat";

            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) return existing;

            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (shader == null)
            {
                Debug.LogWarning("[VRSimulator] URP Lit shader topilmadi — materiallar standart holda qoladi.");
                return null;
            }

            var material = new Material(shader) { color = color };
            if (transparent)
            {
                // URP'da shaffoflik uchun Surface Type = Transparent.
                material.SetFloat("_Surface", 1f);
                material.SetFloat("_Blend", 0f);
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                material.SetShaderPassEnabled("ShadowCaster", false);
            }

            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static Text CreateText(Transform parent, string name, string content, int fontSize,
            TextAnchor anchor, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            var text = go.AddComponent<Text>();
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = anchor;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.font = GetBuiltinFont();
            return text;
        }

        private static Image CreateImage(Transform parent, string name, Color color, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            var image = go.AddComponent<Image>();
            image.color = color;
            return image;
        }

        /// <summary>Unity 6 da o'rnatilgan shrift nomi o'zgargan — ikkalasini ham sinaymiz.</summary>
        private static Font GetBuiltinFont()
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                       ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (font == null) Debug.LogWarning("[VRSimulator] O'rnatilgan shrift topilmadi — matn ko'rinmasligi mumkin.");
            return font;
        }

        private static void AddSceneToBuildSettings(string path)
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (scenes.Exists(s => s.path == path)) return;

            scenes.Insert(0, new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
