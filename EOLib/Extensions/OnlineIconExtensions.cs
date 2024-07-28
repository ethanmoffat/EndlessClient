using EOLib.Domain.Chat;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.Extensions
{
    public static class OnlineIconExtensions
    {
        public static ChatIcon ToChatIcon(this CharacterIcon icon)
        {
            return icon switch
            {
                CharacterIcon.Player => ChatIcon.Player,
                CharacterIcon.Gm => ChatIcon.GM,
                CharacterIcon.Hgm => ChatIcon.HGM,
                CharacterIcon.Party => ChatIcon.PlayerParty,
                CharacterIcon.GmParty => ChatIcon.GMParty,
                CharacterIcon.HgmParty => ChatIcon.HGMParty,
                (CharacterIcon)20 => ChatIcon.PlayerPartyDark, // SLNBot in eoserv
                _ => ChatIcon.Player,
            };
        }
    }
}