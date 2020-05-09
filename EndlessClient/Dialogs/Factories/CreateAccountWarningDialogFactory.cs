using AutomaticTypeMapper;
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
        private readonly IGameStateProvider _gameStateProvider;
        private readonly IEODialogButtonService _eoDialogButtonService;

        public CreateAccountWarningDialogFactory(
            INativeGraphicsManager nativeGraphicsManager,
            IGameStateProvider gameStateProvider,
            IEODialogButtonService eoDialogButtonService)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _gameStateProvider = gameStateProvider;
            _eoDialogButtonService = eoDialogButtonService;
        }

        public IXNADialog ShowCreateAccountWarningDialog(string warningMessage)
        {
            return new ScrollingMessageDialog(
                _nativeGraphicsManager,
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
