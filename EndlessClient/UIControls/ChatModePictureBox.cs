// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EndlessClient.HUD.Chat;
using EOLib;
using EOLib.Domain.Chat;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient.UIControls
{
    public class ChatModePictureBox : XNAPictureBox
    {
        private readonly IChatModeCalculator _chatModeCalculator;
        private readonly IChatProvider _chatProvider;

        public enum ChatMode
        {
            NoText,
            Public,
            Private,
            Global,
            Group,
            Admin,
            Muted,
            Guild
        }

        private string _lastChat;
        private readonly bool _constructed;
        private Optional<DateTime> _endMuteTime;

        public ChatModePictureBox(IChatModeCalculator chatModeCalculator,
                                  IChatProvider chatProvider,
                                  Texture2D displayPicture)
        {
            Texture = displayPicture;

            _chatModeCalculator = chatModeCalculator;
            _chatProvider = chatProvider;

            _lastChat = "";
            _constructed = true;
            _endMuteTime = Optional<DateTime>.Empty;
        }

        public void SetMuted(DateTime endMuteTime)
        {
            _lastChat = "";
            _endMuteTime = endMuteTime;
            UpdateSourceRectangleForMode(ChatMode.Muted);
        }

        public override void Update(GameTime gameTime)
        {
            if (!_constructed || !ShouldUpdate())
                return;

            if (_endMuteTime.HasValue)
            {
                if (DateTime.Now > _endMuteTime)
                {
                    _endMuteTime = Optional<DateTime>.Empty;
                    UpdateSourceRectangleForMode(ChatMode.NoText);
                }
            }
            else if (SingleCharTypedOrDeleted())
            {
                UpdateSourceRectangleForMode(_chatModeCalculator.CalculateMode(_chatProvider.LocalTypedText));
                _lastChat = _chatProvider.LocalTypedText;
            }

            base.Update(gameTime);
        }

        private bool SingleCharTypedOrDeleted()
        {
            return (_lastChat.Length == 0 && _chatProvider.LocalTypedText.Length == 1) ||
                   (_lastChat.Length == 1 && _chatProvider.LocalTypedText.Length == 0);
        }

        private void UpdateSourceRectangleForMode(ChatMode mode)
        {
            if (!SourceRectangle.HasValue)
                throw new InvalidOperationException("SourceRectangle is expected to have a value.");

            var source = SourceRectangle.Value;
            SourceRectangle = source.WithPosition(new Vector2(0, (float)mode * source.Height));
        }
    }
}
