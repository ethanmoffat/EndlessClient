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

        public BarberDialogFactory(INativeGraphicsManager nativeGraphicsManager,
                                   ICharacterRendererFactory characterRendererFactory,
                                   IEODialogButtonService dialogButtonService,
                                   ICharacterRepository characterRepository,  
                                   IEODialogIconService dialogIconService,
                                   ILocalizedStringFinder localizedStringFinder,
                                   IBarberActions barberActions)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _characterRendererFactory = characterRendererFactory;
            _dialogButtonService = dialogButtonService;
            _dialogIconService = dialogIconService;
            _characterRepository = characterRepository;
            _localizedStringFinder = localizedStringFinder;
            _barberActions = barberActions;
        }


        public BarberDialog Create()
        {
        return new BarberDialog(_nativeGraphicsManager, _characterRendererFactory, _dialogButtonService, _characterRepository, _dialogIconService,_localizedStringFinder,_barberActions);
        }
    }

    public interface IBarberDialogFactory
    {
        BarberDialog Create();
    }
}
