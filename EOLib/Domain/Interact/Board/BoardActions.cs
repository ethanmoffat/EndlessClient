using AutomaticTypeMapper;
using EOLib.Net.Communication;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;

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
                var packet = new BoardCreateClientPacket
                {
                    BoardId = boardId,
                    PostSubject = subject,
                    // original EO client uses \r as newline separator. XNAControls uses \n.
                    PostBody = body.Replace('\n', '\r')
                };
                _packetSendService.SendPacket(packet);
            });
        }

        public void DeletePost(int postId)
        {
            _boardProvider.BoardId.MatchSome(boardId =>
            {
                var packet = new BoardRemoveClientPacket
                {
                    BoardId = boardId,
                    PostId = postId
                };
                _packetSendService.SendPacket(packet);
            });
        }

        public void ViewPost(int postId)
        {
            _boardProvider.BoardId.MatchSome(boardId =>
            {
                var packet = new BoardTakeClientPacket
                {
                    BoardId = boardId,
                    PostId = postId
                };
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