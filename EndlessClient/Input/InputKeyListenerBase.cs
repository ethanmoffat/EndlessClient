// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EndlessClient.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using XNAControls;

namespace EndlessClient.Input
{
    public class InputKeyListenerBase : GameComponent
    {
        //input will be rate-limited to once every {x} MS
        private const int INPUT_RATE_LIMIT_MS = 200;

        private KeyboardState _prevKeyState;
        private DateTime? _lastInputTime;

        protected Character Character { get { return OldWorld.Instance.MainPlayer.ActiveCharacter; } }
        protected OldCharacterRenderer Renderer { get { return OldWorld.Instance.ActiveCharacterRenderer; } }

        public event Action<DateTime> InputTimeUpdated;

        /// <summary>
        /// Returns true if input handling for a key listener should be ignored
        /// </summary>
        protected bool IgnoreInput
        {
            get
            {
                return
                    !Game.IsActive ||
                    (_lastInputTime != null && (DateTime.Now - _lastInputTime.Value).TotalMilliseconds < INPUT_RATE_LIMIT_MS) ||
                    XNAControl.Dialogs.Count > 0 ||
                    Character == null || Renderer == null;
            }
        }

        protected InputKeyListenerBase() : base(EOGame.Instance)
        {
            _prevKeyState = Keyboard.GetState();
        }

        protected bool IsKeyPressed(Keys key, KeyboardState? currentState = null)
        {
            if (!currentState.HasValue)
                currentState = Keyboard.GetState();

            return _prevKeyState.IsKeyDown(key) && currentState.Value.IsKeyDown(key);
        }

        protected bool IsKeyPressedOnce(Keys key, KeyboardState? currentState = null)
        {
            if (!currentState.HasValue)
                currentState = Keyboard.GetState();

            return _prevKeyState.IsKeyDown(key) && currentState.Value.IsKeyUp(key);
        }

        protected void UpdateInputTime()
        {
            _lastInputTime = DateTime.Now;
            if (InputTimeUpdated != null)
                InputTimeUpdated(_lastInputTime ?? DateTime.Now);
        }

        public override void Update(GameTime gameTime)
        {
            _prevKeyState = Keyboard.GetState();
            base.Update(gameTime);
        }
    }
}
