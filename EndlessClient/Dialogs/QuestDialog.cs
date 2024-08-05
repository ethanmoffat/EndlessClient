using System;
using System.Collections.Generic;
using System.Linq;
using EndlessClient.Content;
using EndlessClient.Dialogs.Services;
using EOLib;
using EOLib.Domain.Interact.Quest;
using EOLib.Graphics;
using EOLib.IO.Repositories;
using Microsoft.Xna.Framework;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using Optional;
using XNAControls;

namespace EndlessClient.Dialogs
{
    public class QuestDialog : ScrollingListDialog
    {
        private readonly IQuestActions _questActions;
        private readonly IQuestDataProvider _questDataProvider;
        private readonly IENFFileProvider _enfFileProvider;
        private readonly IContentProvider _contentProvider;

        private Option<QuestDialogData> _cachedData;

        private int _pageIndex = 0;

        public event EventHandler LinkClickAction;

        public QuestDialog(INativeGraphicsManager nativeGraphicsManager,
                           IQuestActions questActions,
                           IEODialogButtonService dialogButtonService,
                           IQuestDataProvider questDataProvider,
                           IENFFileProvider enfFileProvider,
                           IContentProvider contentProvider)
            : base(nativeGraphicsManager, dialogButtonService, dialogType: DialogType.NpcQuestDialog)
        {
            _questActions = questActions;
            _questDataProvider = questDataProvider;
            _enfFileProvider = enfFileProvider;
            _contentProvider = contentProvider;

            _cachedData = Option.None<QuestDialogData>();

            ListItemType = ListDialogItem.ListItemStyle.Small;

            NextAction += NextPage;
            BackAction += PreviousPage;
            DialogClosing += (_, e) =>
            {
                if (e.Result == XNADialogResult.OK)
                    _questActions.RespondToQuestDialog(DialogReply.Ok);
            };
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            _questDataProvider.QuestDialogData.MatchSome(data => UpdateCachedDataIfNeeded(_cachedData, data));
            base.OnUpdateControl(gameTime);
        }

        private void UpdateCachedDataIfNeeded(Option<QuestDialogData> cachedData, QuestDialogData repoData)
        {
            cachedData.Match(
                some: cached =>
                {
                    _cachedData = Option.Some(repoData);
                    if (!cached.Equals(repoData))
                        UpdateDialogControls(repoData);
                },
                none: () =>
                {
                    _cachedData = Option.Some(repoData);
                    UpdateDialogControls(repoData);
                });
        }

        private void UpdateDialogControls(QuestDialogData repoData)
        {
            _pageIndex = 0;

            UpdateTitle(repoData);
            UpdateDialogDisplayText(repoData);
            UpdateButtons(repoData);
        }

        private void UpdateTitle(QuestDialogData repoData)
        {
            if (_questDataProvider.RequestedNPC != null)
            {
                var npcName = _enfFileProvider.ENFFile[_questDataProvider.RequestedNPC.ID].Name;
                var titleText = npcName;
                if (!repoData.DialogTitles.ContainsKey(repoData.VendorID) && repoData.DialogTitles.Count == 1)
                    titleText += $" - {repoData.DialogTitles.Single().Value}";
                else if (repoData.DialogTitles.ContainsKey(repoData.VendorID))
                    titleText += $" - {repoData.DialogTitles[repoData.VendorID]}";

                _titleText.Text = titleText;
                _titleText.ResizeBasedOnText();
            }
            else
            {
                _titleText.Text = string.Empty;
            }
        }

        private void UpdateDialogDisplayText(QuestDialogData repoData)
        {
            ClearItemList();

            var rows = new List<string>();

            var ts = new TextSplitter(repoData.PageText[_pageIndex], _contentProvider.Fonts[Constants.FontSize09]);
            if (!ts.NeedsProcessing)
                rows.Add(repoData.PageText[_pageIndex]);
            else
                rows.AddRange(ts.SplitIntoLines());

            int index = 0;
            foreach (var row in rows)
            {
                var rowItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Small, index++)
                {
                    PrimaryText = row,
                };

                AddItemToList(rowItem, sortList: false);
            }

            // The links are only shown on the last page of the quest dialog
            if (_pageIndex < repoData.PageText.Count - 1)
                return;

            var item = new ListDialogItem(this, ListDialogItem.ListItemStyle.Small, index++) { PrimaryText = " " };
            AddItemToList(item, sortList: false);

            foreach (var action in repoData.Actions)
            {
                var actionItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Small, index++)
                {
                    PrimaryText = action.DisplayText
                };

                var linkIndex = action.ActionID;
                actionItem.SetPrimaryClickAction((_, _) =>
                {
                    _questActions.RespondToQuestDialog(DialogReply.Link, linkIndex);
                    LinkClickAction?.Invoke(this, EventArgs.Empty);
                    Close(XNADialogResult.Cancel);
                });

                AddItemToList(actionItem, sortList: false);
            }
        }

        private void UpdateButtons(QuestDialogData repoData)
        {
            bool morePages = _pageIndex < repoData.PageText.Count - 1;
            bool firstPage = _pageIndex == 0;

            if (firstPage && morePages)
                Buttons = ScrollingListDialogButtons.CancelNext;
            else if (!firstPage && morePages)
                Buttons = ScrollingListDialogButtons.BackNext;
            else if (firstPage)
                Buttons = ScrollingListDialogButtons.CancelOk;
            else
                Buttons = ScrollingListDialogButtons.BackOk;
        }

        private void NextPage(object sender, EventArgs e)
        {
            _cachedData.MatchSome(data =>
            {
                _pageIndex++;
                UpdateDialogDisplayText(data);
                UpdateButtons(data);
            });
        }

        private void PreviousPage(object sender, EventArgs e)
        {
            _cachedData.MatchSome(data =>
            {
                _pageIndex--;
                UpdateDialogDisplayText(data);
                UpdateButtons(data);
            });
        }
    }
}