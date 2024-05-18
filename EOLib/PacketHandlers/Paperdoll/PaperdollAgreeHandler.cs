using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.IO.Repositories;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Paperdoll
{
    /// <summary>
    /// Handler for equipping an item
    /// </summary>
    [AutoMappedType]
    public class PaperdollAgreeHandler : ItemEquipHandler<PaperdollAgreeServerPacket>
    {
        public override PacketFamily Family => PacketFamily.Paperdoll;

        public override PacketAction Action => PacketAction.Agree;

        public PaperdollAgreeHandler(IPlayerInfoProvider playerInfoProvider,
                                      ICurrentMapStateRepository currentMapStateRepository,
                                      ICharacterRepository characterRepository,
                                      IPaperdollRepository paperdollRepository,
                                      ICharacterInventoryRepository characterInventoryRepository,
                                      IEIFFileProvider eifFileProvider)
            : base(playerInfoProvider, currentMapStateRepository, characterRepository, paperdollRepository, characterInventoryRepository, eifFileProvider)
        {
        }

        public override bool HandlePacket(PaperdollAgreeServerPacket packet)
        {
            return HandlePaperdollPacket(packet.Change, packet.ItemId, packet.RemainingAmount, packet.SubLoc, packet.Stats);
        }
    }
}
