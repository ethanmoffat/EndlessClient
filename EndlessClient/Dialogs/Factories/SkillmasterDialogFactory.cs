using AutomaticTypeMapper;
using EndlessClient.Dialogs.Services;
using EOLib.Domain.Character;
using EOLib.Domain.Interact.Skill;
using EOLib.Graphics;
using EOLib.Localization;

namespace EndlessClient.Dialogs.Factories
{
    [AutoMappedType]
    public class SkillmasterDialogFactory : ISkillmasterDialogFactory
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IEODialogButtonService _dialogButtonService;
        private readonly IEODialogIconService _dialogIconService;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly IEOMessageBoxFactory _messageBoxFactory;
        private readonly ISkillDataProvider _skillDataProvider;
        private readonly ICharacterInventoryProvider _characterInventoryProvider;

        public SkillmasterDialogFactory(INativeGraphicsManager nativeGraphicsManager,
                                        IEODialogButtonService dialogButtonService,
                                        IEODialogIconService dialogIconService,
                                        ILocalizedStringFinder localizedStringFinder,
                                        IEOMessageBoxFactory messageBoxFactory,
                                        ISkillDataProvider skillDataProvider,
                                        ICharacterInventoryProvider characterInventoryProvider)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _dialogButtonService = dialogButtonService;
            _dialogIconService = dialogIconService;
            _localizedStringFinder = localizedStringFinder;
            _messageBoxFactory = messageBoxFactory;
            _skillDataProvider = skillDataProvider;
            _characterInventoryProvider = characterInventoryProvider;
        }

        public SkillmasterDialog Create()
        {
            return new SkillmasterDialog(_nativeGraphicsManager,
                                         _dialogButtonService,
                                         _dialogIconService,
                                         _localizedStringFinder,
                                         _messageBoxFactory,
                                         _skillDataProvider,
                                         _characterInventoryProvider);
        }
    }

    public interface ISkillmasterDialogFactory
    {
        SkillmasterDialog Create();
    }
}
