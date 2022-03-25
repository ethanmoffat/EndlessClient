using AutomaticTypeMapper;
using EndlessClient.Content;
using EndlessClient.Dialogs.Services;
using EndlessClient.GameExecution;
using EOLib.Graphics;
using XNAControls;

namespace EndlessClient.Dialogs.Factories
{
    [MappedType(BaseType = typeof(ICreateAccountWarningDialogFactory))]
    public class CreateAccountWarningDialogFactory : ICreateAccountWarningDialogFactory
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IContentProvider _contentProvider;
        private readonly IGameStateProvider _gameStateProvider;
        private readonly IEODialogButtonService _eoDialogButtonService;

        public CreateAccountWarningDialogFactory(
            INativeGraphicsManager nativeGraphicsManager,
            IContentProvider contentProvider,
            IGameStateProvider gameStateProvider,
            IEODialogButtonService eoDialogButtonService)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _contentProvider = contentProvider;
            _gameStateProvider = gameStateProvider;
            _eoDialogButtonService = eoDialogButtonService;
        }

        public IXNADialog ShowCreateAccountWarningDialog(string warningMessage)
        {
            return new ScrollingMessageDialog(
                _nativeGraphicsManager,
                _contentProvider,
                _gameStateProvider,
                _eoDialogButtonService)
            {
                MessageText = warningMessage
            };
        }
    }

    public interface ICreateAccountWarningDialogFactory
    {
        IXNADialog ShowCreateAccountWarningDialog(string warningMessage);
    }
}
