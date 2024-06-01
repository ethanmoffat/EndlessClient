using AutomaticTypeMapper;
using EndlessClient.Controllers;
using EndlessClient.Dialogs.Services;
using EndlessClient.Rendering.Character;
using EOLib.Graphics;
using EndlessClient.Content;
using EndlessClient.Rendering.Factories;
using EOLib.Domain.Character;
using EOLib.Localization;
using EOLib.Domain.Interact.Barber;
using EOLib.IO.Repositories;
using EOLib.Domain.Notifiers;
using System.Collections.Generic;

namespace EndlessClient.Dialogs.Factories
{
    [AutoMappedType]
    public class BarberDialogFactory : IBarberDialogFactory
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly ICharacterRendererFactory _characterRendererFactory;
        private readonly IEODialogButtonService _dialogButtonService;
        private readonly ICharacterRepository _characterRepository;
        private readonly IEODialogIconService _dialogIconService;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly IBarberActions _barberActions;
        private readonly ICharacterInventoryProvider _characterInventoryProvider;
        private readonly IEOMessageBoxFactory _messageBoxFactory;
        private readonly IEIFFileProvider _eifFileProvider;
        private readonly IEnumerable<ISoundNotifier> _soundNotifiers;

        public BarberDialogFactory(INativeGraphicsManager nativeGraphicsManager,
                                   ICharacterRendererFactory characterRendererFactory,
                                   IEODialogButtonService dialogButtonService,
                                   ICharacterRepository characterRepository,
                                   IEODialogIconService dialogIconService,
                                   ILocalizedStringFinder localizedStringFinder,
                                   IBarberActions barberActions,
                                   ICharacterInventoryProvider characterInventoryProvider,
                                   IEOMessageBoxFactory messageBoxFactory,
                                   IEIFFileProvider eifFileProvider,
                                   IEnumerable<ISoundNotifier> soundNotifiers)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _characterRendererFactory = characterRendererFactory;
            _dialogButtonService = dialogButtonService;
            _characterRepository = characterRepository;
            _dialogIconService = dialogIconService;
            _localizedStringFinder = localizedStringFinder;
            _barberActions = barberActions;
            _characterInventoryProvider = characterInventoryProvider;
            _messageBoxFactory = messageBoxFactory;
            _eifFileProvider = eifFileProvider;
            _soundNotifiers = soundNotifiers;
        }

        public BarberDialog Create()
        {
            return new BarberDialog(_nativeGraphicsManager,
                                    _characterRendererFactory,
                                    _dialogButtonService,
                                    _characterRepository,
                                    _dialogIconService,
                                    _localizedStringFinder,
                                    _barberActions,
                                    _characterInventoryProvider,
                                    _messageBoxFactory,
                                    _eifFileProvider,
                                    _soundNotifiers);
        }
    }

    public interface IBarberDialogFactory
    {
        BarberDialog Create();
    }
}