using AutomaticTypeMapper;
using EndlessClient.UIControls;
using EOLib.Domain.Character;
using Moffat.EndlessOnline.SDK.Protocol;

namespace EndlessClient.HUD.Chat;

[MappedType(BaseType = typeof(IChatModeCalculator))]
public class ChatModeCalculator : IChatModeCalculator
{
    private readonly ICharacterProvider _characterProvider;

    public ChatModeCalculator(ICharacterProvider characterProvider)
    {
        _characterProvider = characterProvider;
    }

    public ChatModePictureBox.ChatMode CalculateMode(string fullTextString)
    {
        if (string.IsNullOrEmpty(fullTextString))
            return ChatModePictureBox.ChatMode.NoText;

        var playerIsAdmin = _characterProvider.MainCharacter.AdminLevel != AdminLevel.Player;
        var playerIsInGuild = !string.IsNullOrEmpty(_characterProvider.MainCharacter.GuildName);

        if (((fullTextString[0] == '@' || fullTextString[0] == '+') && !playerIsAdmin) ||
            (fullTextString[0] == '&' && !playerIsInGuild))
            return ChatModePictureBox.ChatMode.Public;

        switch (fullTextString[0])
        {
            case '!': return ChatModePictureBox.ChatMode.Private;
            case '@':
            case '~': return ChatModePictureBox.ChatMode.Global;
            case '+': return ChatModePictureBox.ChatMode.Admin;
            case '\'': return ChatModePictureBox.ChatMode.Group;
            case '&': return ChatModePictureBox.ChatMode.Guild;
            default: return ChatModePictureBox.ChatMode.Public;
        }
    }
}