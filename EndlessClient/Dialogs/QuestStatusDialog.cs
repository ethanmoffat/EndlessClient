using EndlessClient.Dialogs.Services;
using EOLib.Domain.Character;
using EOLib.Domain.Interact.Quest;
using EOLib.Graphics;
using EOLib.Localization;
using Microsoft.Xna.Framework;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using System.Collections.Generic;

namespace EndlessClient.Dialogs;

public class QuestStatusDialog : ScrollingListDialog
{
    private readonly ILocalizedStringFinder _localizedStringFinder;
    private readonly IQuestDataProvider _questDataProvider;
    private readonly ICharacterProvider _characterProvider;


    private IReadOnlyList<QuestProgressData> _cachedProgress = new List<QuestProgressData>();
    private IReadOnlyList<string> _cachedHistory = new List<string>();

    private QuestPage _page;

    public QuestStatusDialog(INativeGraphicsManager nativeGraphicsManager,
                             IEODialogButtonService dialogButtonService,
                             ILocalizedStringFinder localizedStringFinder,
                             IQuestDataProvider questDataProvider,
                             ICharacterProvider characterProvider)
        : base(nativeGraphicsManager, dialogButtonService, DialogType.QuestProgressHistory)
    {
        ListItemType = ListDialogItem.ListItemStyle.Small;
        Buttons = ScrollingListDialogButtons.HistoryOk;
        _localizedStringFinder = localizedStringFinder;
        _questDataProvider = questDataProvider;
        _characterProvider = characterProvider;
        HistoryAction += (_, _) => ShowHistory();
        ProgressAction += (_, _) => ShowProgress();

        _page = QuestPage.Progress;
    }

    protected override void OnUpdateControl(GameTime gameTime)
    {
        if (_questDataProvider.QuestHistory != _cachedHistory)
        {
            _cachedHistory = _questDataProvider.QuestHistory;
            if (_page == QuestPage.History)
                ShowHistory();
        }
        else if (_questDataProvider.QuestProgress != _cachedProgress)
        {
            _cachedProgress = _questDataProvider.QuestProgress;
            if (_page == QuestPage.Progress)
                ShowProgress();
        }

        base.OnUpdateControl(gameTime);
    }

    private void ShowHistory()
    {
        ClearItemList();
        SetTitle(QuestPage.History);

        if (_cachedHistory.Count == 0)
        {
            var nextItem = new QuestStatusListDialogItem(this, QuestPage.History)
            {
                QuestName = _localizedStringFinder.GetString(EOResourceID.QUEST_DID_NOT_FINISH_ANY),
                ShowIcons = false
            };
            nextItem.SetScrollWheelHandler(this);
            AddItemToList(nextItem, sortList: false);
        }

        foreach (var questName in _cachedHistory)
        {
            var nextItem = new QuestStatusListDialogItem(this, QuestPage.History)
            {
                QuestName = questName,
                QuestProgress = _localizedStringFinder.GetString(EOResourceID.QUEST_COMPLETED),
            };
            nextItem.SetScrollWheelHandler(this);
            AddItemToList(nextItem, sortList: false);
        }

        _scrollBar.ScrollToTop();
        Buttons = ScrollingListDialogButtons.ProgressOk;
    }

    private void ShowProgress()
    {
        ClearItemList();
        SetTitle(QuestPage.Progress);

        if (_cachedProgress.Count == 0)
        {
            var nextItem = new QuestStatusListDialogItem(this, QuestPage.Progress)
            {
                QuestName = _localizedStringFinder.GetString(EOResourceID.QUEST_DID_NOT_START_ANY),
                ShowIcons = false,
            };
            nextItem.SetScrollWheelHandler(this);
            AddItemToList(nextItem, sortList: false);
        }

        foreach (var quest in _cachedProgress)
        {
            var nextItem = new QuestStatusListDialogItem(this, QuestPage.Progress)
            {
                QuestName = quest.Name,
                QuestStep = quest.Description,
                Icon = (QuestStatusListDialogItem.QuestStatusIcon)quest.IconIndex,
                QuestProgress = quest.Target > 0 ? $"{quest.Progress} / {quest.Target}" : "n / a"
            };
            nextItem.SetScrollWheelHandler(this);
            AddItemToList(nextItem, sortList: false);
        }

        _scrollBar.ScrollToTop();
        Buttons = ScrollingListDialogButtons.HistoryOk;
    }

    private void SetTitle(QuestPage page)
    {
        var resource = page == QuestPage.Progress
            ? EOResourceID.QUEST_PROGRESS
            : EOResourceID.QUEST_HISTORY;
        var description = _localizedStringFinder.GetString(resource);

        Title = $"{_characterProvider.MainCharacter.Name}'s {description}";
    }
}