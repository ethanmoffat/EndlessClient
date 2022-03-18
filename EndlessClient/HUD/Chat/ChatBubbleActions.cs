using AutomaticTypeMapper;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.Chat;
using EndlessClient.Rendering.NPC;
using EOLib.Domain.Chat;
using Optional;

namespace EndlessClient.HUD.Chat
{
    [AutoMappedType]
    public class ChatBubbleActions : IChatBubbleActions
    {
        private readonly IChatProcessor _chatProcessor;
        private readonly IChatTypeCalculator _chatTypeCalculator;
        private readonly ICharacterRendererProvider _characterRendererProvider;
        private readonly INPCRendererProvider _npcRendererProvider;
        private readonly IChatBubbleFactory _chatBubbleFactory;

        public ChatBubbleActions(IChatProcessor chatProcessor,
                                 IChatTypeCalculator chatTypeCalculator,
                                 ICharacterRendererProvider characterRendererProvider,
                                 INPCRendererProvider npcRendererProvider,
                                 IChatBubbleFactory chatBubbleFactory)
        {
            _chatProcessor = chatProcessor;
            _chatTypeCalculator = chatTypeCalculator;
            _characterRendererProvider = characterRendererProvider;
            _npcRendererProvider = npcRendererProvider;
            _chatBubbleFactory = chatBubbleFactory;
        }

        public void ShowChatBubbleForMainCharacter(string input)
        {
            //todo: don't show chat bubble if group chat and character is not in a group (party)
            _characterRendererProvider.MainCharacterRenderer.MatchSome(r =>
            {
                _chatTypeCalculator.CalculateChatType(input)
                    .SomeWhen(x => x == ChatType.Local || x == ChatType.Party || x == ChatType.Announce)
                    .MatchSome(chatType =>
                    {
                        var text = _chatProcessor.RemoveFirstCharacterIfNeeded(input, chatType, string.Empty);
                        r.ShowChatBubble(text, chatType == ChatType.Party);
                    });
            });
        }

        public void ShowChatBubbleForNPC(int index, string input)
        {
            if (!_npcRendererProvider.NPCRenderers.ContainsKey(index))
                return;

            _npcRendererProvider.NPCRenderers[index].ShowChatBubble(input);
        }
    }

    public interface IChatBubbleActions
    {
        void ShowChatBubbleForMainCharacter(string input);

        void ShowChatBubbleForNPC(int index, string input);
    }
}