using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Locker
{

    /// <summary>
    /// Handles LOCKER_GET from server for taking an item from locker
    /// </summary>
    [AutoMappedType]
    public class LockerGetHandler : LockerModifyHandler<LockerGetServerPacket>
    {
        public override PacketAction Action => PacketAction.Get;

        public LockerGetHandler(IPlayerInfoProvider playerInfoProvider,
                                ILockerDataRepository lockerDataRepository,
                                ICharacterRepository characterRepository,
                                ICharacterInventoryRepository characterInventoryRepository)
            : base(playerInfoProvider, lockerDataRepository, characterRepository, characterInventoryRepository)
        {
        }

        public override bool HandlePacket(LockerGetServerPacket packet)
        {
            Handle(packet.TakenItem.Id, packet.TakenItem.Amount, packet.Weight, packet.LockerItems);
            return true;
        }
    }
}
