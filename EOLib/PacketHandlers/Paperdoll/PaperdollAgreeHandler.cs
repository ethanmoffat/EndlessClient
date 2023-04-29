using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.IO.Repositories;
using EOLib.Net;

namespace EOLib.PacketHandlers.Paperdoll
{
    /// <summary>
    /// Handler for equipping an item
    /// </summary>
    [AutoMappedType]
    public class PaperdollAgreeHandler : ItemEquipHandler
    {
        public override PacketFamily Family => PacketFamily.PaperDoll;

        public override PacketAction Action => PacketAction.Agree;

        public PaperdollAgreeHandler(IPlayerInfoProvider playerInfoProvider,
                                      ICurrentMapStateRepository currentMapStateRepository,
                                      ICharacterRepository characterRepository,
                                      IEIFFileProvider eifFileProvider,
                                      IPaperdollRepository paperdollRepository,
                                      ICharacterInventoryRepository characterInventoryRepository)
            : base(playerInfoProvider, currentMapStateRepository, characterRepository, eifFileProvider, paperdollRepository, characterInventoryRepository)
        {
        }

        public override bool HandlePacket(IPacket packet)
        {
            return HandlePaperdollPacket(packet, itemUnequipped: false);
        }
    }
}
