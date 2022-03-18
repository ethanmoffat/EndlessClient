using System;
using EndlessClient.Content;
using EOLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Optional;
using XNAControls;

namespace EndlessClient.UIControls
{
    /// <summary>
    /// Special instance of an XNATextBox that should ignore input from the number pad (which is used for Emotes)
    /// </summary>
    public class ChatTextBox : XNATextBox
    {
        private bool _ignoreAllInput;
        private Option<DateTime> _endMuteTime;

        public ChatTextBox(IContentManagerProvider contentManagerProvider)
            : base(new Rectangle(124, 308, 440, 19),
                Constants.FontSize08,
                caretTexture: contentManagerProvider.Content.Load<Texture2D>("cursor"))
        {
            MaxChars = 140;
            _endMuteTime = Option.None<DateTime>();
        }

        public void SetMuted(DateTime endMuteTime)
        {
            _ignoreAllInput = true;
            _endMuteTime = Option.Some(endMuteTime);
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            _endMuteTime.MatchSome(endTime =>
            {
                if (DateTime.Now > endTime)
                {
                    _endMuteTime = Option.None<DateTime>();
                    _ignoreAllInput = false;
                }
            });

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
