// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Domain.Chat;
using EOLib.Localization;
using EOLib.Net;

namespace EOLib.PacketHandlers
{
    public class FindCommandPlayerDifferentMapHandler : FindCommandHandlerBase
    {
        public override PacketAction Action
        {
            get { return PacketAction.Net3; }
        }

        protected override EOResourceID ResourceIDForResponse
        {
            get { return EOResourceID.STATUS_LABEL_IS_ONLINE_IN_THIS_WORLD; }
        }

        public FindCommandPlayerDifferentMapHandler(IChatRepository chatRespository,
            ILocalizedStringService localizedStringService)
            : base(chatRespository, localizedStringService)
        {
        }
    }
}