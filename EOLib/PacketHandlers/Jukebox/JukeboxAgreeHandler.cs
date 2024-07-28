using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using Optional.Collections;

namespace EOLib.PacketHandlers.Jukebox
{
    /// <summary>
    /// Sent to update character's remaining gold after requesting a song
    /// </summary>
    [AutoMappedType]
    public class JukeboxAgreeHandler : InGameOnlyPacketHandler<JukeboxAgreeServerPacket>
    {
        private readonly ICharacterInventoryRepository _characterInventoryRepository;

        public override PacketFamily Family => PacketFamily.Jukebox;

        public override PacketAction Action => PacketAction.Agree;

        public JukeboxAgreeHandler(IPlayerInfoProvider playerInfoProvider,
                                   ICharacterInventoryRepository characterInventoryRepository)
            : base(playerInfoProvider)
        {
            _characterInventoryRepository = characterInventoryRepository;
        }

        public override bool HandlePacket(JukeboxAgreeServerPacket packet)
        {
            _characterInventoryRepository.ItemInventory.SingleOrNone(x => x.ItemID == 1).MatchSome(x => _characterInventoryRepository.ItemInventory.Remove(x));
            _characterInventoryRepository.ItemInventory.Add(new InventoryItem(1, packet.GoldAmount));

            return true;
        }
    }
}