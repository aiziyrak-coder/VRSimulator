using UnityEngine;
using UnityEngine.InputSystem;

namespace VRSimulator.Core
{
    /// <summary>
    /// Mashqni boshqaruvchi tugmalarni o'qiydi (tasdiqlash, qayta boshlash, HUD).
    /// Input amallari Resources'dagi VRSimulatorControls faylidan yuklanadi —
    /// shuning uchun sahnada hech narsa ulash kerak emas.
    /// </summary>
    public class SimulationInput : MonoBehaviour
    {
        private const string AssetName = "VRSimulatorControls";
        private const string MapName = "Simulation";

        [SerializeField] private TaskRunner taskRunner;
        [SerializeField] private GameObject hudRoot;

        private InputActionAsset asset;
        private InputAction confirmStep;
        private InputAction resetTask;
        private InputAction toggleHud;

        private void Awake()
        {
            if (taskRunner == null) taskRunner = FindAnyObjectByType<TaskRunner>();

            asset = Resources.Load<InputActionAsset>(AssetName);
            if (asset == null)
            {
                Debug.LogWarning($"[{nameof(SimulationInput)}] '{AssetName}' topilmadi " +
                                 "(Assets/VRSimulator/Resources ichida bo'lishi kerak). Tugmalar ishlamaydi.", this);
                return;
            }

            // Asl aktivni o'zgartirmaslik uchun nusxa bilan ishlaymiz.
            asset = Instantiate(asset);
            var map = asset.FindActionMap(MapName, throwIfNotFound: false);
            if (map == null)
            {
                Debug.LogWarning($"[{nameof(SimulationInput)}] '{MapName}' action map topilmadi.", this);
                return;
            }

            confirmStep = map.FindAction("ConfirmStep", throwIfNotFound: false);
            resetTask = map.FindAction("ResetTask", throwIfNotFound: false);
            toggleHud = map.FindAction("ToggleHud", throwIfNotFound: false);
        }

        private void OnEnable()
        {
            if (confirmStep != null) { confirmStep.performed += OnConfirm; confirmStep.Enable(); }
            if (resetTask != null) { resetTask.performed += OnReset; resetTask.Enable(); }
            if (toggleHud != null) { toggleHud.performed += OnToggleHud; toggleHud.Enable(); }
        }

        private void OnDisable()
        {
            if (confirmStep != null) { confirmStep.performed -= OnConfirm; confirmStep.Disable(); }
            if (resetTask != null) { resetTask.performed -= OnReset; resetTask.Disable(); }
            if (toggleHud != null) { toggleHud.performed -= OnToggleHud; toggleHud.Disable(); }
        }

        private void OnDestroy()
        {
            if (asset != null) Destroy(asset);
        }

        private void OnConfirm(InputAction.CallbackContext _)
        {
            if (taskRunner != null) taskRunner.ForceCompleteCurrentStep();
        }

        private void OnReset(InputAction.CallbackContext _)
        {
            if (taskRunner == null) return;

            taskRunner.Restart();
            foreach (var zone in FindObjectsByType<Interaction.PlacementZone>(FindObjectsSortMode.None))
            {
                zone.ResetZone();
            }
        }

        private void OnToggleHud(InputAction.CallbackContext _)
        {
            if (hudRoot != null) hudRoot.SetActive(!hudRoot.activeSelf);
        }
    }
}
