using UnityEngine;
using UnityEngine.Events;

namespace VRSimulator.Core
{
    /// <summary>
    /// Topshiriqni qadam-baqadam olib boruvchi komponent.
    /// Hodisalarni <see cref="SimulationBus"/> orqali eshitadi, natijani
    /// <see cref="SimulationSession"/> ga yozadi va UI uchun UnityEvent chiqaradi.
    /// </summary>
    public class TaskRunner : MonoBehaviour
    {
        [Header("Topshiriq")]
        [SerializeField] private SimulationTask task;
        [SerializeField, Tooltip("Sahna ochilishi bilan avtomatik boshlansinmi")]
        private bool autoStart = true;

        [Header("Hodisalar")]
        [Tooltip("Yangi qadam boshlanganda: ko'rsatma matni")]
        public UnityEvent<string> OnInstructionChanged = new UnityEvent<string>();
        [Tooltip("Bajarilish darajasi 0..1")]
        public UnityEvent<float> OnProgressChanged = new UnityEvent<float>();
        [Tooltip("Qadam muvaffaqiyatli yopilganda")]
        public UnityEvent<string> OnStepCompleted = new UnityEvent<string>();
        [Tooltip("Xato harakat qilinganda: sabab matni")]
        public UnityEvent<string> OnMistake = new UnityEvent<string>();
        [Tooltip("Barcha qadamlar tugaganda")]
        public UnityEvent OnTaskCompleted = new UnityEvent();

        private SimulationSession session;
        private int currentIndex = -1;
        private float stepStartedAt;
        private bool running;

        public SimulationTask Task => task;
        public bool IsRunning => running;
        public int CurrentIndex => currentIndex;
        public TaskStep CurrentStep => task != null ? task.GetStep(currentIndex) : null;

        private void Awake()
        {
            session = GetComponent<SimulationSession>();
        }

        private void OnEnable()
        {
            SimulationBus.Completed += HandleCompleted;
            SimulationBus.Failed += HandleFailed;
        }

        private void OnDisable()
        {
            SimulationBus.Completed -= HandleCompleted;
            SimulationBus.Failed -= HandleFailed;
        }

        private void Start()
        {
            if (autoStart) Begin();
        }

        /// <summary>Topshiriqni boshidan boshlaydi.</summary>
        public void Begin()
        {
            if (task == null)
            {
                Debug.LogWarning($"[{nameof(TaskRunner)}] Topshiriq (SimulationTask) biriktirilmagan.", this);
                return;
            }
            if (task.StepCount == 0)
            {
                Debug.LogWarning($"[{nameof(TaskRunner)}] '{task.title}' topshirig'ida qadam yo'q.", this);
                return;
            }
            if (task.TryFindDuplicateId(out var duplicate))
            {
                Debug.LogWarning($"[{nameof(TaskRunner)}] '{task.title}' da qadam id takrorlangan: '{duplicate}'.", this);
            }

            session?.Begin(task);
            running = true;
            currentIndex = -1;
            Advance();
        }

        /// <summary>Topshiriqni qayta boshlaydi (ResetTask tugmasi shuni chaqiradi).</summary>
        public void Restart() => Begin();

        /// <summary>Qadamni majburan yopadi — masalan o'qituvchi rejimi yoki tugma orqali.</summary>
        public void ForceCompleteCurrentStep()
        {
            var step = CurrentStep;
            if (running && step != null) CompleteStep(step);
        }

        private void Update()
        {
            if (!running) return;

            var step = CurrentStep;
            if (step == null || !step.HasTimeLimit) return;

            if (Time.time - stepStartedAt >= step.timeLimit)
            {
                RegisterMistake($"'{step.instruction}' qadami uchun vaqt tugadi.");
                stepStartedAt = Time.time; // vaqtni qayta sanaydi, qadam ochiq qoladi
            }
        }

        private void HandleCompleted(string eventId)
        {
            if (!running) return;

            var step = CurrentStep;
            if (step == null) return;

            // Qadam hodisa kutmaydi (qo'lda yopiladi) — e'tibor bermaymiz.
            if (string.IsNullOrEmpty(step.completionEventId)) return;

            if (step.completionEventId == eventId)
            {
                CompleteStep(step);
            }
            else
            {
                RegisterMistake($"Hozir '{step.instruction}' bajarilishi kerak.");
            }
        }

        private void HandleFailed(string eventId, string reason)
        {
            if (!running) return;
            RegisterMistake(string.IsNullOrEmpty(reason) ? $"Xato amal: {eventId}" : reason);
        }

        private void CompleteStep(TaskStep step)
        {
            session?.RegisterStepCompleted(step, Time.time - stepStartedAt);
            OnStepCompleted.Invoke(step.id);
            Advance();
        }

        private void RegisterMistake(string reason)
        {
            session?.RegisterMistake(reason);
            OnMistake.Invoke(reason);
        }

        private void Advance()
        {
            currentIndex++;
            OnProgressChanged.Invoke(task.StepCount == 0 ? 1f : Mathf.Clamp01((float)currentIndex / task.StepCount));

            var step = CurrentStep;
            if (step == null)
            {
                running = false;
                session?.Finish();
                OnInstructionChanged.Invoke($"Topshiriq bajarildi: {task.title}");
                OnTaskCompleted.Invoke();
                return;
            }

            stepStartedAt = Time.time;
            OnInstructionChanged.Invoke($"{currentIndex + 1}/{task.StepCount} — {step.instruction}");
        }
    }
}
