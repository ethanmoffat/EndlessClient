using AutomaticTypeMapper;
using EOLib.Domain.Interact.Board;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net;
using EOLib.Net.Handlers;
using Optional;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Board
{
    /// <summary>
    /// Sent by the server when a board should be opened
    /// </summary>
    [AutoMappedType]
    public class BoardOpenHandler : InGameOnlyPacketHandler
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

        public override bool HandlePacket(IPacket packet)
        {
            _boardRepository.BoardId = Option.Some(packet.ReadChar());

            var numPosts = packet.ReadChar();
            _boardRepository.Posts = new HashSet<BoardPostInfo>();

            var chunks = new List<IPacket>();
            while (packet.ReadPosition < packet.Length)
            {
                var chunkData = new List<byte> { (byte)PacketFamily.Board, (byte)PacketAction.Open };
                while (packet.ReadPosition < packet.Length && packet.PeekByte() != 255)
                    chunkData.Add(packet.ReadByte());

                if (packet.ReadPosition < packet.Length)
                    packet.ReadByte();

                chunks.Add(new Packet(chunkData));
            }

            if (chunks.Count % 3 != 0 || chunks.Count / 3 != numPosts)
                throw new MalformedPacketException("Unexpected number of elements in BOARD_OPEN packet", packet);

            for (int i = 0; i < chunks.Count; i += 3)
            {
                var postId = chunks[i].ReadShort();
                var author = chunks[i + 1].ReadEndString();
                var subject = chunks[i + 2].ReadEndString();
                _boardRepository.Posts.Add(new BoardPostInfo(postId, author, subject));
            }

            _boardRepository.ActivePost = Option.None<BoardPostInfo>();
            _boardRepository.ActivePostMessage = Option.None<string>();

            foreach (var notifier in _userInterfaceNotifiers)
                notifier.NotifyPacketDialog(PacketFamily.Board);

            return true;
        }
    }
}
