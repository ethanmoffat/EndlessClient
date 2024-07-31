﻿using System;

namespace EndlessClient.Rendering
{
    public class RenderFrameActionTime
    {
        private Action _sfxCallback;

        public int UniqueID { get; private set; }

        public ulong ActionTick { get; private set; }

        public bool Replay { get; private set; }

        public RenderFrameActionTime(int uniqueID, ulong ticks, Action sfxCallback = null)
        {
            UniqueID = uniqueID;
            _sfxCallback = sfxCallback;
            UpdateActionStartTime(ticks);
        }

        public void UpdateActionStartTime(ulong ticks)
        {
            ActionTick = ticks;
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