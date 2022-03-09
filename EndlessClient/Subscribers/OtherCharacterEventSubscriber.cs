using AutomaticTypeMapper;
using EndlessClient.HUD.Chat;
using EndlessClient.Rendering.Character;
using EOLib.Domain.Notifiers;

namespace EndlessClient.Subscribers
{
    [MappedType(BaseType = typeof(IOtherCharacterEventNotifier))]
    public class OtherCharacterEventSubscriber : IOtherCharacterEventNotifier
    {
        private readonly IChatBubbleActions _chatBubbleActions;
        private readonly ICharacterRendererProvider _characterRendererProvider;

        public OtherCharacterEventSubscriber(IChatBubbleActions chatBubbleActions,
                                             ICharacterRendererProvider characterRendererProvider)
        {
            _chatBubbleActions = chatBubbleActions;
            _characterRendererProvider = characterRendererProvider;
        }

        public void OtherCharacterTakeDamage(int characterID,
                                             int playerPercentHealth,
                                             int damageTaken)
        {
            if (!_characterRendererProvider.CharacterRenderers.ContainsKey(characterID))
                return;

            _characterRendererProvider.CharacterRenderers[characterID].ShowDamageCounter(damageTaken, playerPercentHealth, isHeal: false);
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
            if (!_characterRendererProvider.CharacterRenderers.ContainsKey(characterID))
                return;

            _characterRendererProvider.CharacterRenderers[characterID].ShowChatBubble(message, isGroupChat);
        }
    }
}
