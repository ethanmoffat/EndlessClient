using AutomaticTypeMapper;
using EndlessClient.Dialogs.Services;
using EOLib.Domain.Character;
using EOLib.Graphics;
using EOLib.Localization;

namespace EndlessClient.Dialogs.Factories
{
    [AutoMappedType]
    public class GuildDialogFactory : IGuildDialogFactory
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IEODialogButtonService _dialogButtonService;
        private readonly IEODialogIconService _dialogIconService;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly ICharacterProvider _characterProvider;
        private readonly IEOMessageBoxFactory _messageBoxFactory;
        private readonly IGuildSessionProvider _guildSessionProvider;

        public GuildDialogFactory(INativeGraphicsManager nativeGraphicsManager,
                                IEODialogButtonService dialogButtonService,
                                IEODialogIconService dialogIconService,
                                ILocalizedStringFinder localizedStringFinder,
                                ICharacterProvider characterProvider,
                                IEOMessageBoxFactory messageBoxFactory,
                                IGuildSessionProvider guildSessionProvider)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _dialogButtonService = dialogButtonService;
            _dialogIconService = dialogIconService;
            _localizedStringFinder = localizedStringFinder;
            _characterProvider = characterProvider;
            _messageBoxFactory = messageBoxFactory;
            _guildSessionProvider = guildSessionProvider;
        }

        public GuildDialog Create()
        {
            return new GuildDialog(_nativeGraphicsManager,
                                 _dialogButtonService,
                                 _dialogIconService,
                                 _localizedStringFinder,
                                 _characterProvider,
                                 _messageBoxFactory,
                                 _guildSessionProvider
                                 );
        }
    }

    public interface IGuildDialogFactory
    {
        GuildDialog Create();
    }
}
