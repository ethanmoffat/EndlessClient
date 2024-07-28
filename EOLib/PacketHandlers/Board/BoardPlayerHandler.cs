using AutomaticTypeMapper;
using EOLib.Domain.Interact.Board;
using EOLib.Domain.Login;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using Optional;

namespace EOLib.PacketHandlers.Board
{
    /// <summary>
    /// Sent by the server to read a post on a board
    /// </summary>
    [AutoMappedType]
    public class BoardPlayerHandler : InGameOnlyPacketHandler<BoardPlayerServerPacket>
    {
        private readonly IBoardRepository _boardRepository;

        public override PacketFamily Family => PacketFamily.Board;

        public override PacketAction Action => PacketAction.Player;

        public BoardPlayerHandler(IPlayerInfoProvider playerInfoProvider,
                                  IBoardRepository boardRepository)
            : base(playerInfoProvider)
        {
            _boardRepository = boardRepository;
        }

        public override bool HandlePacket(BoardPlayerServerPacket packet)
        {
            _boardRepository.ActivePost.MatchSome(post =>
            {
                if (post.PostId == packet.PostId)
                {
                    // EndlessClient uses \n as line split, vanilla client uses \r
                    _boardRepository.ActivePostMessage = Option.Some(packet.PostBody.Replace('\r', '\n'));
                }
            });

            return true;
        }
    }
}