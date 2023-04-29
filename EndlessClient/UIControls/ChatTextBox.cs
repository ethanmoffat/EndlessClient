using EndlessClient.Content;
using EOLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input.InputListeners;
using Optional;
using System;
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

        public ChatTextBox(IContentProvider contentManagerProvider)
            : base(new Rectangle(124, 308, 440, 19),
                Constants.FontSize08,
                caretTexture: contentManagerProvider.Textures[ContentProvider.Cursor])
        {
            MaxChars = 140;
            _endMuteTime = Option.None<DateTime>();
        }

        public override void Initialize()
        {
            base.Initialize();

            // This must be done after Initialize because MaxWidth uses the sprite font.
            // SpriteFont is loaded in LoadContent which is called by Initialize.
            MaxWidth = 440;
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

        protected override bool HandleTextInput(KeyboardEventArgs eventArgs)
        {
            if (_ignoreAllInput)
                return false;

            if (IsSpecialInput(eventArgs.Key))
                HandleSpecialInput(eventArgs.Key);
            else
                base.HandleTextInput(eventArgs);

            return true;
        }

        private void HandleSpecialInput(Keys key)
        {
            if (key == Keys.Escape)
                Text = "";
        }

        private bool IsSpecialInput(Keys k)
        {
            return k == Keys.Escape || k == Keys.Decimal ||
                   (k >= Keys.NumPad0 && k <= Keys.NumPad9);
        }
    }
}
