using AutomaticTypeMapper;
using EndlessClient.HUD.Chat;
using EndlessClient.Rendering.Character;
using EndlessClient.Services;
using EOLib;
using EOLib.Config;
using EOLib.Domain.Chat;
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
        private readonly IConfigurationProvider _configurationProvider;
        private readonly IChatProcessor _chatProcessor;

        public OtherCharacterEventSubscriber(IChatBubbleActions chatBubbleActions,
                                             ICharacterRendererProvider characterRendererProvider,
                                             IFriendIgnoreListService friendIgnoreListService,
                                             IConfigurationProvider configurationProvider,
                                             IChatProcessor chatProcessor)
        {
            _chatBubbleActions = chatBubbleActions;
            _characterRendererProvider = characterRendererProvider;
            _friendIgnoreListService = friendIgnoreListService;
            _configurationProvider = configurationProvider;
            _chatProcessor = chatProcessor;
        }

        public void OtherCharacterTakeDamage(int characterID, int playerPercentHealth, int damageTaken, bool isHeal)
        {
            if (!_characterRendererProvider.CharacterRenderers.ContainsKey(characterID) ||
                (isHeal && damageTaken == 0))
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
            if (_characterRendererProvider.CharacterRenderers.TryGetValue(characterID, out var characterRenderer) ||
                _characterRendererProvider.MainCharacterRenderer.HasValue)
            {
                _characterRendererProvider.MainCharacterRenderer.MatchSome(x => characterRenderer = x);

                var name = characterRenderer.Character.Name;

                var ignoreList = _friendIgnoreListService.LoadList(Constants.IgnoreListFile);
                if (ignoreList.Any(x => x.Equals(name, StringComparison.InvariantCultureIgnoreCase)) ||
                    (_configurationProvider.StrictFilterEnabled && !_chatProcessor.FilterCurses(message).ShowChat))
                    return;

                characterRenderer.ShowChatBubble(message, isGroupChat);
            }
        }
    }
}
