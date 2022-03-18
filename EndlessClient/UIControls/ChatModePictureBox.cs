using System;
using EndlessClient.ControlSets;
using EndlessClient.HUD.Chat;
using EndlessClient.HUD.Controls;
using EOLib;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Optional;
using XNAControls;

namespace EndlessClient.UIControls
{
    public class ChatModePictureBox : XNAPictureBox
    {
        private readonly IChatModeCalculator _chatModeCalculator;
        private readonly IHudControlProvider _hudControlProvider;

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
        private Option<DateTime> _endMuteTime;

        public ChatModePictureBox(IChatModeCalculator chatModeCalculator,
                                  IHudControlProvider hudControlProvider,
                                  Texture2D displayPicture)
        {
            Texture = displayPicture;

            _chatModeCalculator = chatModeCalculator;
            _hudControlProvider = hudControlProvider;

            _lastChat = "";
            _endMuteTime = Option.None<DateTime>();
        }

        public void SetMuted(DateTime endMuteTime)
        {
            _lastChat = "";
            _endMuteTime = Option.Some(endMuteTime);
            UpdateSourceRectangleForMode(ChatMode.Muted);
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            _endMuteTime.Match(
                some: endTime =>
                {
                    if (DateTime.Now >= endTime)
                    {
                        _endMuteTime = Option.None<DateTime>();
                        UpdateSourceRectangleForMode(ChatMode.NoText);
                    }
                },
                none: () =>
                {
                    if (SingleCharTypedOrDeleted())
                    {
                        UpdateSourceRectangleForMode(_chatModeCalculator.CalculateMode(ChatTextBox.Text));
                        _lastChat = ChatTextBox.Text;
                    }
                });

            base.OnUpdateControl(gameTime);
        }

        private bool SingleCharTypedOrDeleted()
        {
            return _hudControlProvider.IsInGame &&
                   ((_lastChat.Length == 0 && ChatTextBox.Text.Length == 1) ||
                    (_lastChat.Length == 1 && ChatTextBox.Text.Length == 0));
        }

        private void UpdateSourceRectangleForMode(ChatMode mode)
        {
            if (!SourceRectangle.HasValue)
                throw new InvalidOperationException("SourceRectangle is expected to have a value.");

            var source = SourceRectangle.Value;
            SourceRectangle = source.WithPosition(new Vector2(0, (float)mode * source.Height));
        }

        private ChatTextBox ChatTextBox => _hudControlProvider.GetComponent<ChatTextBox>(HudControlIdentifier.ChatTextBox);
    }
}
