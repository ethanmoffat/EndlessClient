using AutomaticTypeMapper;
using EndlessClient.Dialogs.Services;
using EOLib.Domain.Character;
using EOLib.Graphics;
using EOLib.Localization;

namespace EndlessClient.Dialogs.Factories
{
    [AutoMappedType]
    public class SessionExpDialogFactory : ISessionExpDialogFactory
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IEODialogButtonService _dialogButtonService;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly ICharacterProvider _characterProvider;
        private readonly IExperienceTableProvider _expTableProvider;
        private readonly ICharacterSessionProvider _characterSessionProvider;

        public SessionExpDialogFactory(INativeGraphicsManager nativeGraphicsManager,
                                       IEODialogButtonService dialogButtonService,
                                       ILocalizedStringFinder localizedStringFinder,
                                       ICharacterProvider characterProvider,
                                       IExperienceTableProvider expTableProvider,
                                       ICharacterSessionProvider characterSessionProvider)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _dialogButtonService = dialogButtonService;
            _localizedStringFinder = localizedStringFinder;
            _characterProvider = characterProvider;
            _expTableProvider = expTableProvider;
            _characterSessionProvider = characterSessionProvider;
        }

        public SessionExpDialog Create()
        {
            return new SessionExpDialog(_nativeGraphicsManager,
                                        _dialogButtonService,
                                        _localizedStringFinder,
                                        _characterProvider,
                                        _expTableProvider,
                                        _characterSessionProvider);
        }
    }

    public interface ISessionExpDialogFactory
    {
        SessionExpDialog Create();
    }
}
