using AutomaticTypeMapper;
using EndlessClient.Content;
using EndlessClient.Dialogs.Services;
using EOLib.Domain.Interact.Quest;
using EOLib.Graphics;
using EOLib.IO.Repositories;
using EOLib.Localization;

namespace EndlessClient.Dialogs.Factories
{
    [AutoMappedType]
    public class QuestDialogFactory : IQuestDialogFactory
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IQuestActions _questActions;
        private readonly IEODialogButtonService _dialogButtonService;
        private readonly IQuestDataProvider _questDataProvider;
        private readonly IENFFileProvider _enfFileProvider;
        private readonly IContentProvider _contentProvider;
        private readonly ILocalizedStringFinder _localizedStringFinder;

        public QuestDialogFactory(INativeGraphicsManager nativeGraphicsManager,
                                  IQuestActions questActions,
                                  IEODialogButtonService dialogButtonService,
                                  IQuestDataProvider questDataProvider,
                                  IENFFileProvider enfFileProvider,
                                  IContentProvider contentProvider,
                                  ILocalizedStringFinder localizedStringFinder)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _questActions = questActions;
            _dialogButtonService = dialogButtonService;
            _questDataProvider = questDataProvider;
            _enfFileProvider = enfFileProvider;
            _contentProvider = contentProvider;
            _localizedStringFinder = localizedStringFinder;
        }

        public QuestDialog Create()
        {
            return new QuestDialog(_nativeGraphicsManager,
                                   _questActions,
                                   _dialogButtonService,
                                   _questDataProvider,
                                   _enfFileProvider,
                                   _contentProvider,
                                   _localizedStringFinder);
        }
    }

    public interface IQuestDialogFactory
    {
        QuestDialog Create();
    }
}
