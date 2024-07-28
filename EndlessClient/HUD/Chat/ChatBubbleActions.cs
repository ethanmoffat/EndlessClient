using AutomaticTypeMapper;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.Chat;
using EndlessClient.Rendering.NPC;
using EOLib.Domain.Chat;
using EOLib.Domain.Party;
using Optional;
using System.Linq;

namespace EndlessClient.HUD.Chat;

[AutoMappedType]
public class ChatBubbleActions : IChatBubbleActions
{
    private readonly ICharacterRendererProvider _characterRendererProvider;
    private readonly INPCRendererProvider _npcRendererProvider;
    private readonly IPartyDataProvider _partyDataProvider;

    public ChatBubbleActions(ICharacterRendererProvider characterRendererProvider,
                             INPCRendererProvider npcRendererProvider,
                             IPartyDataProvider partyDataProvider)
    {
        _characterRendererProvider = characterRendererProvider;
        _npcRendererProvider = npcRendererProvider;
        _partyDataProvider = partyDataProvider;
    }

    public void ShowChatBubbleForMainCharacter(string text, bool isPartyChat = false)
    {
        _characterRendererProvider.MainCharacterRenderer.MatchSome(r =>
        {
            if (!_partyDataProvider.Members.Any() && isPartyChat)
                return;

            r.ShowChatBubble(text, isPartyChat);
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
    void ShowChatBubbleForMainCharacter(string input, bool isPartyChat = false);

    void ShowChatBubbleForNPC(int index, string input);
}