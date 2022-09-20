using System;
using System.Diagnostics;

namespace EndlessClient.Rendering
{
    public class RenderFrameActionTime
    {
        private readonly Action _sfxCallback;

        public int UniqueID { get; private set; }

        public Stopwatch ActionTimer { get; private set; }

        public bool Replay { get; set; }

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

        public void SoundEffect() => _sfxCallback?.Invoke();
    }
}