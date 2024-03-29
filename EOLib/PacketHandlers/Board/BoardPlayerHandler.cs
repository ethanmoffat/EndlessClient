﻿using AutomaticTypeMapper;
using EOLib.Domain.Interact.Board;
using EOLib.Domain.Login;
using EOLib.Net;
using EOLib.Net.Handlers;
using Optional;

namespace EOLib.PacketHandlers.Board
{
    /// <summary>
    /// Sent by the server to read a post on a board
    /// </summary>
    [AutoMappedType]
    public class BoardPlayerHandler : InGameOnlyPacketHandler
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

        public override bool HandlePacket(IPacket packet)
        {
            var postId = packet.ReadShort();
            var message = packet.ReadEndString();

            _boardRepository.ActivePost.MatchSome(post =>
            {
                if (post.PostId == postId)
                {
                    // EndlessClient uses \n as line split, vanilla client uses \r
                    _boardRepository.ActivePostMessage = Option.Some(message.Replace('\r', '\n'));
                }
            });

            return true;
        }
    }
}
