using System;
using System.Collections;
using UnityEngine;

namespace Vestige.Systems.MiniGames
{
    public abstract class MiniGameBase : MonoBehaviour, IMiniGame
    {
        [Header("Phase Timers")]
        [SerializeField] private float prepareDuration = 1.5f;
        [SerializeField] private float playerPredictsDuration = 6f;
        [SerializeField] private float resolveDuration = 1.5f;

        public MiniGamePhase CurrentPhase { get; private set; } = MiniGamePhase.Prepare;
        public bool IsRunning { get; private set; }
        public float PhaseTimeRemaining { get; private set; }

        public event Action<MiniGamePhase> PhaseChanged;
        public event Action<float> TimerTick;
        public event Action Rewarded;
        public event Action Failed;
        public event Action Retried;

        private Coroutine activeRoutine;
        private bool predictionSubmitted;
        private bool predictionSucceeded;
        private Func<bool> retryHook;

        public void StartMiniGame()
        {
            if (IsRunning)
            {
                return;
            }

            IsRunning = true;
            predictionSubmitted = false;
            predictionSucceeded = false;
            activeRoutine = StartCoroutine(RunMiniGame());
        }

        public void StopMiniGame()
        {
            if (!IsRunning)
            {
                return;
            }

            if (activeRoutine != null)
            {
                StopCoroutine(activeRoutine);
                activeRoutine = null;
            }

            IsRunning = false;
            PhaseTimeRemaining = 0f;
        }

        public void Retry()
        {
            bool canRetry = retryHook == null || retryHook.Invoke();
            if (!canRetry)
            {
                return;
            }

            StopMiniGame();
            Retried?.Invoke();
            StartMiniGame();
        }

        public void SetRetryHook(Func<bool> retryHook)
        {
            this.retryHook = retryHook;
        }

        public void RegisterRewardCallback(Action rewardCallback)
        {
            if (rewardCallback == null)
            {
                return;
            }

            Rewarded += rewardCallback;
        }

        public void RegisterFailCallback(Action failCallback)
        {
            if (failCallback == null)
            {
                return;
            }

            Failed += failCallback;
        }

        public void SubmitPrediction(bool predictionSucceeded)
        {
            if (!IsRunning || CurrentPhase != MiniGamePhase.PlayerPredicts)
            {
                return;
            }

            predictionSubmitted = true;
            this.predictionSucceeded = predictionSucceeded;
        }

        protected virtual void OnPrepare() { }
        protected virtual void OnPlayerPredicts() { }
        protected virtual void OnResolve(bool predictionSucceeded) { }
        protected virtual void OnAward() { }
        protected virtual void OnFail() { }

        private IEnumerator RunMiniGame()
        {
            yield return RunTimedPhase(MiniGamePhase.Prepare, prepareDuration, OnPrepare);
            yield return RunPredictPhase();
            yield return RunTimedPhase(MiniGamePhase.Resolve, resolveDuration, () => OnResolve(predictionSucceeded));

            if (predictionSucceeded)
            {
                EnterPhase(MiniGamePhase.Award);
                OnAward();
                Rewarded?.Invoke();
            }
            else
            {
                EnterPhase(MiniGamePhase.Fail);
                OnFail();
                Failed?.Invoke();
            }

            IsRunning = false;
            activeRoutine = null;
        }

        private IEnumerator RunPredictPhase()
        {
            predictionSubmitted = false;
            predictionSucceeded = false;
            EnterPhase(MiniGamePhase.PlayerPredicts);
            OnPlayerPredicts();

            float remaining = Mathf.Max(0f, playerPredictsDuration);
            while (remaining > 0f)
            {
                if (predictionSubmitted)
                {
                    break;
                }

                remaining -= Time.deltaTime;
                PhaseTimeRemaining = remaining;
                TimerTick?.Invoke(PhaseTimeRemaining);
                yield return null;
            }

            PhaseTimeRemaining = 0f;
            TimerTick?.Invoke(PhaseTimeRemaining);
        }

        private IEnumerator RunTimedPhase(MiniGamePhase phase, float duration, Action phaseAction)
        {
            EnterPhase(phase);
            phaseAction?.Invoke();

            float remaining = Mathf.Max(0f, duration);
            while (remaining > 0f)
            {
                remaining -= Time.deltaTime;
                PhaseTimeRemaining = remaining;
                TimerTick?.Invoke(PhaseTimeRemaining);
                yield return null;
            }

            PhaseTimeRemaining = 0f;
            TimerTick?.Invoke(PhaseTimeRemaining);
        }

        private void EnterPhase(MiniGamePhase phase)
        {
            CurrentPhase = phase;
            PhaseChanged?.Invoke(phase);
        }
    }
}
