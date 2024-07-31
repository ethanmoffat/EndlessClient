using AutomaticTypeMapper;
using EndlessClient.Dialogs.Services;
using EOLib.Domain.Character;
using EOLib.Domain.Interact.Quest;
using EOLib.Graphics;
using EOLib.Localization;

namespace EndlessClient.Dialogs.Factories
{
    [AutoMappedType]
    public class QuestStatusDialogFactory : IQuestStatusDialogFactory
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IEODialogButtonService _dialogButtonService;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly IQuestDataProvider _questDataProvider;
        private readonly ICharacterProvider _characterProvider;

        public QuestStatusDialogFactory(INativeGraphicsManager nativeGraphicsManager,
                                 IEODialogButtonService dialogButtonService,
                                 ILocalizedStringFinder localizedStringFinder,
                                 IQuestDataProvider questDataProvider,
                                 ICharacterProvider characterProvider)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _dialogButtonService = dialogButtonService;
            _localizedStringFinder = localizedStringFinder;
            _questDataProvider = questDataProvider;
            _characterProvider = characterProvider;
        }

        public QuestStatusDialog Create()
        {
            return new QuestStatusDialog(_nativeGraphicsManager,
                                         _dialogButtonService,
                                         _localizedStringFinder,
                                         _questDataProvider,
                                         _characterProvider);
        }
    }

    public interface IQuestStatusDialogFactory
    {
        QuestStatusDialog Create();
    }
}