// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EndlessClient.Old;
using EndlessClient.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using XNAControls.Old;

namespace EndlessClient.Input
{
    public class OldInputKeyListenerBase : GameComponent
    {
        //input will be rate-limited to once every {x} MS
        private const int INPUT_RATE_LIMIT_MS = 200;

        protected KeyboardState PreviousKeyState { get; private set; }
        private DateTime? _lastInputTime;

        protected OldCharacter Character => OldWorld.Instance.MainPlayer.ActiveCharacter;
        protected OldCharacterRenderer Renderer => OldWorld.Instance.ActiveCharacterRenderer;

        public event Action<DateTime> InputTimeUpdated;

        /// <summary>
        /// Returns true if input handling for a key listener should be ignored
        /// </summary>
        protected bool IgnoreInput => !Game.IsActive ||
                                      (_lastInputTime != null && (DateTime.Now - _lastInputTime.Value).TotalMilliseconds < INPUT_RATE_LIMIT_MS) ||
                                      XNAControl.Dialogs.Count > 0 ||
                                      Character == null || Renderer == null;

        protected OldInputKeyListenerBase() : base(EOGame.Instance)
        {
            PreviousKeyState = Keyboard.GetState();
        }

        protected void UpdateInputTime()
        {
            _lastInputTime = DateTime.Now;
            if (InputTimeUpdated != null)
                InputTimeUpdated(_lastInputTime ?? DateTime.Now);
        }

        public override void Update(GameTime gameTime)
        {
            PreviousKeyState = Keyboard.GetState();
            base.Update(gameTime);
        }
    }
}
