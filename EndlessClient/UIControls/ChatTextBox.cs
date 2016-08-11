// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Content;
using EOLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient.UIControls
{
    /// <summary>
    /// Special instance of an XNATextBox that should ignore input from the number pad (which is used for Emotes)
    /// </summary>
    public class ChatTextBox : XNATextBox
    {
        private bool _ignoreNextInput;
        private bool _ignoreAllInput;

        private string _lastTextInput;

        public bool SingleCharFromNone
        {
            get
            {
                return _lastTextInput.Length == 0 && Text.Length == 1 ||
                       Text.Length == 0 && _lastTextInput.Length == 1;
            }
        }

        public ChatTextBox(IContentManagerProvider contentManagerProvider)
            : base(new Rectangle(124, 308, 440, 19),
                contentManagerProvider.Content.Load<Texture2D>("cursor"),
                Constants.FontSize08)
        {
            MaxChars = 140;
            _lastTextInput = "";
        }

        public void ToggleTextInputIgnore()
        {
            _ignoreAllInput = !_ignoreAllInput;
        }

        public override void ReceiveTextInput(char inp)
        {
            if (_ignoreAllInput) return;

            _lastTextInput = Text;

            if (!_ignoreNextInput)
                base.ReceiveTextInput(inp);
            else
                _ignoreNextInput = false;
        }

        public override void ReceiveTextInput(string inp)
        {
            if (_ignoreAllInput) return;

            _lastTextInput = Text;

            if (!_ignoreNextInput)
                base.ReceiveTextInput(inp);
            else
                _ignoreNextInput = false;
        }

        //public override void ReceiveSpecialInput(Keys key)
        //{
        //    if (_ignoreAllInput) return;

        //    //ignore the emote input keys!
        //    if (key >= Keys.NumPad0 && key <= Keys.NumPad9 || key == Keys.Decimal)
        //    {
        //        _ignoreNextInput = true;
        //    }
        //}
    }
}
