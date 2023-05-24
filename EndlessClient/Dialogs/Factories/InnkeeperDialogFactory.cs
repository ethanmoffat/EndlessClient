using AutomaticTypeMapper;
using EndlessClient.Content;
using EndlessClient.Dialogs.Services;
using EOLib.Domain.Interact.Citizen;
using EOLib.Graphics;
using EOLib.IO.Repositories;
using EOLib.Localization;

namespace EndlessClient.Dialogs.Factories
{
    [AutoMappedType]
    public class InnkeeperDialogFactory : IInnkeeperDialogFactory
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IEODialogButtonService _dialogButtonService;
        private readonly IEODialogIconService _dialogIconService;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly IEOMessageBoxFactory _messageBoxFactory;
        private readonly ITextInputDialogFactory _textInputDialogFactory;
        private readonly IContentProvider _contentProvider;
        private readonly IENFFileProvider _enfFileProvider;
        private readonly ICitizenDataProvider _citizenDataProvider;

        public InnkeeperDialogFactory(INativeGraphicsManager nativeGraphicsManager,
                               IEODialogButtonService dialogButtonService,
                               IEODialogIconService dialogIconService,
                               ILocalizedStringFinder localizedStringFinder,
                               IEOMessageBoxFactory messageBoxFactory,
                               ITextInputDialogFactory textInputDialogFactory,
                               IContentProvider contentProvider,
                               IENFFileProvider enfFileProvider,
                               ICitizenDataProvider citizenDataProvider)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _dialogButtonService = dialogButtonService;
            _dialogIconService = dialogIconService;
            _localizedStringFinder = localizedStringFinder;
            _messageBoxFactory = messageBoxFactory;
            _textInputDialogFactory = textInputDialogFactory;
            _contentProvider = contentProvider;
            _enfFileProvider = enfFileProvider;
            _citizenDataProvider = citizenDataProvider;
        }

        public InnkeeperDialog Create()
        {
            return new InnkeeperDialog(_nativeGraphicsManager,
                _dialogButtonService,
                _dialogIconService,
                _localizedStringFinder,
                _messageBoxFactory,
                _textInputDialogFactory,
                _contentProvider,
                _enfFileProvider,
                _citizenDataProvider);
        }
    }

    public interface IInnkeeperDialogFactory
    {
        InnkeeperDialog Create();
    }
}
