using EOLib.Domain.Chat;
using EOLib.Domain.Online;
using System;

namespace EOLib.Extensions
{
    public static class OnlineIconExtensions
    {
        public static ChatIcon ToChatIcon(this OnlineIcon icon)
        {
            switch (icon)
            {
                case OnlineIcon.Normal: return ChatIcon.Player;
                case OnlineIcon.GM: return ChatIcon.GM;
                case OnlineIcon.HGM: return ChatIcon.HGM;
                case OnlineIcon.Party: return ChatIcon.PlayerParty;
                case OnlineIcon.GMParty: return ChatIcon.GMParty;
                case OnlineIcon.HGMParty: return ChatIcon.HGMParty;
                case OnlineIcon.SLNBot: return ChatIcon.PlayerPartyDark;
                default: throw new ArgumentOutOfRangeException(nameof(icon), "Invalid Icon type specified.");
            }
        }
    }
}
