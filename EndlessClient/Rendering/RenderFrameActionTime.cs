using System;
using System.Diagnostics;

namespace EndlessClient.Rendering
{
    public class RenderFrameActionTime
    {
        private Action _sfxCallback;

        public int UniqueID { get; private set; }

        public Stopwatch ActionTimer { get; private set; }

        public bool Replay { get; private set; }

        public RenderFrameActionTime(int uniqueID, Action sfxCallback = null)
        {
            UniqueID = uniqueID;
            _sfxCallback = sfxCallback;
            UpdateActionStartTime();
        }

        public void UpdateActionStartTime()
        {
            ActionTimer = Stopwatch.StartNew();
        }

        public void SetReplay(Action sfxCallback = null)
        {
            _sfxCallback = sfxCallback;
            Replay = true;
        }

        public void ClearReplay()
        {
            Replay = false;
        }

        public void SoundEffect() => _sfxCallback?.Invoke();
    }
}