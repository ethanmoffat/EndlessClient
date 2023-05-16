using AutomaticTypeMapper;
using EOLib.Net;
using EOLib.Net.Communication;

namespace EOLib.Domain.Interact.Board
{
    [AutoMappedType]
    public class BoardActions : IBoardActions
    {
        private readonly IPacketSendService _packetSendService;
        private readonly IBoardProvider _boardProvider;

        public BoardActions(IPacketSendService packetSendService,
                            IBoardProvider boardProvider)
        {
            _packetSendService = packetSendService;
            _boardProvider = boardProvider;
        }

        public void AddPost(string subject, string body)
        {
            _boardProvider.BoardId.MatchSome(boardId =>
            {
                var packet = new PacketBuilder(PacketFamily.Board, PacketAction.Create)
                    .AddShort(boardId)
                    .AddByte(255)
                    .AddBreakString(subject.Replace('y', (char)255)) // this is in EOSERV for some reason. Probably due to chunking (see Sanitization here: https://github.com/Cirras/eo-protocol/blob/master/docs/chunks.md)
                    .AddBreakString(body)
                    .Build();

                _packetSendService.SendPacket(packet);
            });
        }

        public void DeletePost(int postId)
        {
            _boardProvider.BoardId.MatchSome(boardId =>
            {
                var packet = new PacketBuilder(PacketFamily.Board, PacketAction.Remove)
                .AddShort(boardId)
                .AddShort(postId)
                .Build();

                _packetSendService.SendPacket(packet);
            });
        }

        public void ViewPost(int postId)
        {
            _boardProvider.BoardId.MatchSome(boardId =>
            {
                var packet = new PacketBuilder(PacketFamily.Board, PacketAction.Take)
                .AddShort(boardId)
                .AddShort(postId)
                .Build();

                _packetSendService.SendPacket(packet);
            });
        }
    }

    public interface IBoardActions
    {
        void AddPost(string subject, string body);

        void ViewPost(int postId);

        void DeletePost(int postId);
    }
}
