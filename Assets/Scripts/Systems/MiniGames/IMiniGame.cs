using System;

namespace Vestige.Systems.MiniGames
{
    public enum MiniGamePhase
    {
        Prepare,
        PlayerPredicts,
        Resolve,
        Award,
        Fail
    }

    public interface IMiniGame
    {
        MiniGamePhase CurrentPhase { get; }
        bool IsRunning { get; }
        float PhaseTimeRemaining { get; }

        event Action<MiniGamePhase> PhaseChanged;
        event Action<float> TimerTick;
        event Action Rewarded;
        event Action Failed;
        event Action Retried;

        void StartMiniGame();
        void StopMiniGame();
        void Retry();

        void SetRetryHook(Func<bool> retryHook);
        void RegisterRewardCallback(Action rewardCallback);
        void RegisterFailCallback(Action failCallback);
        void SubmitPrediction(bool predictionSucceeded);
    }
}
