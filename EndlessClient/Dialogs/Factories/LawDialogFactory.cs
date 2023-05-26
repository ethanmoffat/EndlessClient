using AutomaticTypeMapper;
using EndlessClient.Content;
using EndlessClient.Dialogs.Services;
using EOLib.Domain.Interact.Law;
using EOLib.Domain.Map;
using EOLib.Graphics;
using EOLib.IO.Repositories;
using EOLib.Localization;

namespace EndlessClient.Dialogs.Factories
{
    [AutoMappedType]
    public class LawDialogFactory : ILawDialogFactory
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IEODialogButtonService _dialogButtonService;
        private readonly IEODialogIconService _dialogIconService;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly ITextInputDialogFactory _textInputDialogFactory;
        private readonly ILawActions _lawActions;
        private readonly IContentProvider _contentProvider;
        private readonly ICurrentMapStateProvider _currentMapStateProvider;
        private readonly IENFFileProvider _enfFileProvider;

        public LawDialogFactory(INativeGraphicsManager nativeGraphicsManager,
                                IEODialogButtonService dialogButtonService,
                                IEODialogIconService dialogIconService,
                                ILocalizedStringFinder localizedStringFinder,
                                ITextInputDialogFactory textInputDialogFactory,
                                ILawActions lawActions,
                                IContentProvider contentProvider,
                                ICurrentMapStateProvider currentMapStateProvider,
                                IENFFileProvider enfFileProvider)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _dialogButtonService = dialogButtonService;
            _dialogIconService = dialogIconService;
            _localizedStringFinder = localizedStringFinder;
            _textInputDialogFactory = textInputDialogFactory;
            _lawActions = lawActions;
            _contentProvider = contentProvider;
            _currentMapStateProvider = currentMapStateProvider;
            _enfFileProvider = enfFileProvider;
        }

        public LawDialog Create()
        {
            return new LawDialog(_nativeGraphicsManager,
                                 _dialogButtonService,
                                 _dialogIconService,
                                 _localizedStringFinder,
                                 _textInputDialogFactory,
                                 _lawActions,
                                 _contentProvider,
                                 _currentMapStateProvider,
                                 _enfFileProvider);
        }
    }

    public interface ILawDialogFactory
    {
        LawDialog Create();
    }
}
