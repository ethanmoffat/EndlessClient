using AutomaticTypeMapper;
using EndlessClient.HUD.Chat;
using EndlessClient.Rendering.Character;
using EndlessClient.Services;
using EOLib;
using EOLib.Domain.Notifiers;
using System;
using System.Linq;

namespace EndlessClient.Subscribers
{
    [MappedType(BaseType = typeof(IOtherCharacterEventNotifier))]
    public class OtherCharacterEventSubscriber : IOtherCharacterEventNotifier
    {
        private readonly IChatBubbleActions _chatBubbleActions;
        private readonly ICharacterRendererProvider _characterRendererProvider;
        private readonly IFriendIgnoreListService _friendIgnoreListService;

        public OtherCharacterEventSubscriber(IChatBubbleActions chatBubbleActions,
                                             ICharacterRendererProvider characterRendererProvider,
                                             IFriendIgnoreListService friendIgnoreListService)
        {
            _chatBubbleActions = chatBubbleActions;
            _characterRendererProvider = characterRendererProvider;
            _friendIgnoreListService = friendIgnoreListService;
        }

        public void OtherCharacterTakeDamage(int characterID,
                                             int playerPercentHealth,
                                             int damageTaken,
                                             bool isHeal)
        {
            if (!_characterRendererProvider.CharacterRenderers.ContainsKey(characterID))
                return;

            _characterRendererProvider.CharacterRenderers[characterID].ShowDamageCounter(damageTaken, playerPercentHealth, isHeal);
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

            var name = _characterRendererProvider.CharacterRenderers[characterID].Character.Name;

            var ignoreList = _friendIgnoreListService.LoadList(Constants.IgnoreListFile);
            if (ignoreList.Any(x => x.Equals(name, StringComparison.InvariantCultureIgnoreCase)))
                return;

            _characterRendererProvider.CharacterRenderers[characterID].ShowChatBubble(message, isGroupChat);
        }
    }
}
