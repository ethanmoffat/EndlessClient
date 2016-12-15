// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EndlessClient.Content;
using EOLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNAControls;

namespace EndlessClient.UIControls
{
    /// <summary>
    /// Special instance of an XNATextBox that should ignore input from the number pad (which is used for Emotes)
    /// </summary>
    public class ChatTextBox : XNATextBox
    {
        private bool _ignoreAllInput;
        private Optional<DateTime> _endMuteTime;

        public ChatTextBox(IContentManagerProvider contentManagerProvider)
            : base(new Rectangle(124, 308, 440, 19),
                Constants.FontSize08,
                caretTexture: contentManagerProvider.Content.Load<Texture2D>("cursor"))
        {
            MaxChars = 140;
            _endMuteTime = Optional<DateTime>.Empty;
        }

        public void SetMuted(DateTime endMuteTime)
        {
            _ignoreAllInput = true;
            _endMuteTime = endMuteTime;
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            if (_endMuteTime.HasValue && DateTime.Now > _endMuteTime)
            {
                _endMuteTime = Optional<DateTime>.Empty;
                _ignoreAllInput = false;
            }

            base.OnUpdateControl(gameTime);
        }

        public override void ReceiveTextInput(char inp)
        {
            if (_ignoreAllInput) return;

            if (IsSpecialInput((Keys) inp))
                HandleSpecialInput((Keys)inp);
            else
                base.ReceiveTextInput(inp);
        }

        private void HandleSpecialInput(Keys key)
        {
            if (key == Keys.Escape)
                Text = "";
        }

        private bool IsSpecialInput(Keys k)
        {
            //todo: figure out how to handle num pad
            return k == Keys.Escape;// || k == Keys.Decimal ||
                   //(k >= Keys.NumPad0 && k <= Keys.NumPad9);
        }
    }
}
