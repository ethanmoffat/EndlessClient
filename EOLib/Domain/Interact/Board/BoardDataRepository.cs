using System.Collections.Generic;
using AutomaticTypeMapper;
using Optional;

namespace EOLib.Domain.Interact.Board
{
    public interface IBoardRepository
    {
        Option<int> BoardId { get; set; }

        HashSet<BoardPostInfo> Posts { get; set; }

        Option<BoardPostInfo> ActivePost { get; set; }

        Option<string> ActivePostMessage { get; set; }
    }

    public interface IBoardProvider
    {
        Option<int> BoardId { get; }

        IReadOnlyCollection<BoardPostInfo> Posts { get; }

        Option<BoardPostInfo> ActivePost { get; }

        Option<string> ActivePostMessage { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class BoardDataRepository : IBoardRepository, IBoardProvider, IResettable
    {
        public Option<int> BoardId { get; set; }

        public HashSet<BoardPostInfo> Posts { get; set; }

        IReadOnlyCollection<BoardPostInfo> IBoardProvider.Posts => Posts;

        public Option<BoardPostInfo> ActivePost { get; set; }

        public Option<string> ActivePostMessage { get; set; }

        public BoardDataRepository()
        {
            ResetState();
        }

        public void ResetState()
        {
            BoardId = Option.None<int>();
            Posts = new HashSet<BoardPostInfo>();
            ActivePost = Option.None<BoardPostInfo>();
            ActivePostMessage = Option.None<string>();
        }
    }
}
