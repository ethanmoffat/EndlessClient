﻿using AutomaticTypeMapper;
using EndlessClient.Audio;
using EndlessClient.Dialogs.Services;
using EOLib.Domain.Interact.Jukebox;
using EOLib.Domain.Map;
using EOLib.Graphics;
using EOLib.Localization;

namespace EndlessClient.Dialogs.Factories
{
    [AutoMappedType]
    public class JukeboxDialogFactory : IJukeboxDialogFactory
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IEODialogButtonService _dialogButtonService;
        private readonly IEODialogIconService _dialogIconService;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly IDataFileProvider _dataFileProvider;
        private readonly IEOMessageBoxFactory _messageBoxFactory;
        private readonly IJukeboxActions _jukeboxActions;
        private readonly IJukeboxRepository _jukeboxRepository;
        private readonly ISfxPlayer _sfxPlayer;

        public JukeboxDialogFactory(INativeGraphicsManager nativeGraphicsManager,
                                    IEODialogButtonService dialogButtonService,
                                    IEODialogIconService dialogIconService,
                                    ILocalizedStringFinder localizedStringFinder,
                                    IDataFileProvider dataFileProvider,
                                    IEOMessageBoxFactory messageBoxFactory,
                                    IJukeboxActions jukeboxActions,
                                    IJukeboxRepository jukeboxRepository,
                                    ISfxPlayer sfxPlayer)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _dialogButtonService = dialogButtonService;
            _dialogIconService = dialogIconService;
            _localizedStringFinder = localizedStringFinder;
            _dataFileProvider = dataFileProvider;
            _messageBoxFactory = messageBoxFactory;
            _jukeboxActions = jukeboxActions;
            _jukeboxRepository = jukeboxRepository;
            _sfxPlayer = sfxPlayer;
        }

        public JukeboxDialog Create(MapCoordinate mapCoordinate)
        {
            return new JukeboxDialog(_nativeGraphicsManager,
                                     _dialogButtonService,
                                     _dialogIconService,
                                     _localizedStringFinder,
                                     _dataFileProvider,
                                     _messageBoxFactory,
                                     _jukeboxActions,
                                     _jukeboxRepository,
                                     _sfxPlayer,
                                     mapCoordinate);
        }
    }

    public interface IJukeboxDialogFactory
    {
        JukeboxDialog Create(MapCoordinate mapCoordinate);
    }
}
