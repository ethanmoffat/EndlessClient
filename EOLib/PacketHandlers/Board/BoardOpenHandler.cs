using AutomaticTypeMapper;
using EOLib.Domain.Interact.Board;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using Optional;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Board
{
    /// <summary>
    /// Sent by the server when a board should be opened
    /// </summary>
    [AutoMappedType]
    public class BoardOpenHandler : InGameOnlyPacketHandler<BoardOpenServerPacket>
    {
        private readonly IBoardRepository _boardRepository;
        private readonly IEnumerable<IUserInterfaceNotifier> _userInterfaceNotifiers;

        public override PacketFamily Family => PacketFamily.Board;

        public override PacketAction Action => PacketAction.Open;

        public BoardOpenHandler(IPlayerInfoProvider playerInfoProvider,
                                IBoardRepository boardRepository,
                                IEnumerable<IUserInterfaceNotifier> userInterfaceNotifiers)
            : base(playerInfoProvider)
        {
            _boardRepository = boardRepository;
            _userInterfaceNotifiers = userInterfaceNotifiers;
        }

        public override bool HandlePacket(BoardOpenServerPacket packet)
        {
            _boardRepository.BoardId = Option.Some(packet.BoardId);

            var numPosts = packet.Posts;
            _boardRepository.Posts = new HashSet<BoardPostInfo>();
            foreach (var post in packet.Posts)
            {
                _boardRepository.Posts.Add(new BoardPostInfo(post.PostId, post.Author, post.Subject));
            }

            _boardRepository.ActivePost = Option.None<BoardPostInfo>();
            _boardRepository.ActivePostMessage = Option.None<string>();

            foreach (var notifier in _userInterfaceNotifiers)
                notifier.NotifyPacketDialog(PacketFamily.Board);

            return true;
        }
    }
}