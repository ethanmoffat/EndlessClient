using AutomaticTypeMapper;
using EndlessClient.Dialogs.Services;
using EOLib.Graphics;

namespace EndlessClient.Dialogs.Factories
{
    [AutoMappedType]
    public class BoardDialogFactory : IBoardDialogFactory
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IEODialogButtonService _eoDialogButtonService;

        public BoardDialogFactory(INativeGraphicsManager nativeGraphicsManager,
                                  IEODialogButtonService eoDialogButtonService)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _eoDialogButtonService = eoDialogButtonService;
        }

        public BoardDialog Create()
        {
            return new BoardDialog(_nativeGraphicsManager, _eoDialogButtonService);
        }
    }

    public interface IBoardDialogFactory
    {
        BoardDialog Create();
    }
}
