// Original Work Copyright (c) Ethan Moffat 2014-2017
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Linq;
using EndlessClient.HUD.Chat;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.Chat;
using EOLib.Domain.Notifiers;

namespace EndlessClient.Subscribers
{
    public class OtherCharacterEventSubscriber : IOtherCharacterEventNotifier
    {
        private readonly IChatBubbleActions _chatBubbleActions;
        private readonly IChatBubbleRepository _chatBubbleRepository;
        private readonly IChatBubbleTextureProvider _chatBubbleTextureProvider;
        private readonly ICharacterRendererProvider _characterRendererProvider;

        public OtherCharacterEventSubscriber(IChatBubbleActions chatBubbleActions,
                                             IChatBubbleRepository chatBubbleRepository,
                                             IChatBubbleTextureProvider chatBubbleTextureProvider,
                                             ICharacterRendererProvider characterRendererProvider)
        {
            _chatBubbleActions = chatBubbleActions;
            _chatBubbleRepository = chatBubbleRepository;
            _chatBubbleTextureProvider = chatBubbleTextureProvider;
            _characterRendererProvider = characterRendererProvider;
        }

        public void OtherCharacterTakeDamage(int characterID,
                                             int playerPercentHealth,
                                             int damageTaken)
        {
            //todo: show health bar
        }

        public void OtherCharacterSaySomething(int characterID, string message)
        {
            SaySomethingShared(isGroupChat: false, characterID: characterID, message: message);
        }

        public void OtherCharacterSaySomethingToGroup(int characterID, string message)
        {
            SaySomethingShared(isGroupChat: true, characterID: characterID, message: message);
        }

        public void AdminAnnounce(string message)
        {
            _chatBubbleActions.ShowChatBubbleForMainCharacter(message);
        }

        private void SaySomethingShared(int characterID, string message, bool isGroupChat)
        {
            //todo: use group chat color

            IChatBubble chatBubble;
            if (_chatBubbleRepository.OtherCharacterChatBubbles.TryGetValue(characterID, out chatBubble))
                chatBubble.SetMessage(message);
            else
            {
                chatBubble = new ChatBubble(message,
                                            _characterRendererProvider.CharacterRenderers[characterID],
                                            _chatBubbleTextureProvider);

                _chatBubbleRepository.OtherCharacterChatBubbles.Add(characterID, chatBubble);
            }
        }
    }
}
