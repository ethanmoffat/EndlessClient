using AutomaticTypeMapper;
using Optional;

namespace EndlessClient.Rendering
{
    public interface IContextMenuProvider
    {
        Option<IContextMenuRenderer> ContextMenu { get; }
    }

    public interface IContextMenuRepository
    {
        Option<IContextMenuRenderer> ContextMenu { get; set;  }
    }

    [AutoMappedType(IsSingleton = true)]
    public class ContextMenuRepository : IContextMenuProvider, IContextMenuRepository
    {
        public Option<IContextMenuRenderer> ContextMenu { get; set; }

        public ContextMenuRepository()
        {
            ContextMenu = Option.None<IContextMenuRenderer>();
        }
    }
}
