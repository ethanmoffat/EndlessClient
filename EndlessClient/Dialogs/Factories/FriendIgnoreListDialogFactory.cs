using AutomaticTypeMapper;
using EndlessClient.ControlSets;
using EndlessClient.Dialogs.Services;
using EndlessClient.GameExecution;
using EndlessClient.HUD.Controls;
using EndlessClient.Old;
using EndlessClient.Services;
using EndlessClient.UIControls;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Graphics;
using EOLib.Localization;
using System;
using System.Linq;
using XNAControls;

namespace EndlessClient.Dialogs.Factories
{
    [AutoMappedType]
    public class FriendIgnoreListDialogFactory : IFriendIgnoreListDialogFactory
    {
        private readonly IGameStateProvider _gameStateProvider;
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IEODialogButtonService _dialogButtonService;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly ICharacterProvider _characterProvider;
        private readonly IHudControlProvider _hudControlProvider;
        private readonly ITextInputDialogFactory _textInputDialogFactory;
        private readonly IEOMessageBoxFactory _eoMessageBoxFactory;
        private readonly IFriendIgnoreListService _friendIgnoreListService;

        public FriendIgnoreListDialogFactory(IGameStateProvider gameStateProvider,
                                             INativeGraphicsManager nativeGraphicsManager,
                                             IEODialogButtonService dialogButtonService,
                                             ILocalizedStringFinder localizedStringFinder,
                                             ICharacterProvider characterProvider,
                                             IHudControlProvider hudControlProvider,
                                             ITextInputDialogFactory textInputDialogFactory,
                                             IEOMessageBoxFactory eoMessageBoxFactory,
                                             IFriendIgnoreListService friendIgnoreListService)
        {
            _gameStateProvider = gameStateProvider;
            _nativeGraphicsManager = nativeGraphicsManager;
            _dialogButtonService = dialogButtonService;
            _localizedStringFinder = localizedStringFinder;
            _characterProvider = characterProvider;
            _hudControlProvider = hudControlProvider;
            _textInputDialogFactory = textInputDialogFactory;
            _eoMessageBoxFactory = eoMessageBoxFactory;
            _friendIgnoreListService = friendIgnoreListService;
        }

        public ScrollingListDialog Create(bool isFriendList)
        {
            var textFileLines = _friendIgnoreListService.LoadList(isFriendList ? Constants.FriendListFile : Constants.IgnoreListFile);

            var dialog = new ScrollingListDialog(_gameStateProvider, _nativeGraphicsManager, _dialogButtonService)
            {
                Buttons = ScrollingListDialogButtons.AddCancel,
                ListItemType = ListDialogItem.ListItemStyle.Small,
            };

            var listItems = textFileLines.Select(x => new ListDialogItem(dialog, ListDialogItem.ListItemStyle.Small) { PrimaryText = x }).ToList();
            foreach (var item in listItems)
                SetClickEventHandlers(item, dialog, isFriendList);
            dialog.SetItemList(listItems);

            dialog.Title = GetDialogTitle(dialog, isFriendList);

            dialog.AddAction += (_, _) => InvokeAdd(isFriendList, dialog);
            dialog.DialogClosing += (_, _) =>
            {
                if (isFriendList)
                    _friendIgnoreListService.SaveFriends(Constants.FriendListFile, dialog.NamesList);
                else
                    _friendIgnoreListService.SaveIgnored(Constants.IgnoreListFile, dialog.NamesList);
            };

            return dialog;
        }

        private void InvokeAdd(bool isFriendList, ScrollingListDialog parentDialog)
        {
            string prompt = _localizedStringFinder.GetString(isFriendList ? EOResourceID.DIALOG_WHO_TO_MAKE_FRIEND : EOResourceID.DIALOG_WHO_TO_MAKE_IGNORE);
            var inputDialog = _textInputDialogFactory.Create(prompt);

            inputDialog.DialogClosing += (_, e) =>
            {
                if (e.Result == XNADialogResult.Cancel)
                    return;

                if (inputDialog.ResponseText.Length < 4)
                {
                    e.Cancel = true;
                    var messageBox = _eoMessageBoxFactory.CreateMessageBox(DialogResourceID.CHARACTER_CREATE_NAME_TOO_SHORT);
                    messageBox.ShowDialog();
                    return;
                }

                if (parentDialog.NamesList.Any(name => string.Equals(name, inputDialog.ResponseText, StringComparison.InvariantCultureIgnoreCase)))
                {
                    e.Cancel = true;
                    var messageBox = _eoMessageBoxFactory.CreateMessageBox("You are already friends with that person!", "Invalid entry!", EODialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
                    messageBox.ShowDialog();
                    return;
                }

                var charName = char.ToUpper(inputDialog.ResponseText[0]) + inputDialog.ResponseText.Substring(1);
                var newItem = new ListDialogItem(parentDialog, ListDialogItem.ListItemStyle.Small) { PrimaryText = charName };
                SetClickEventHandlers(newItem, parentDialog, isFriendList);

                parentDialog.AddItemToList(newItem, sortList: true);
                parentDialog.Title = GetDialogTitle(parentDialog, isFriendList);
            };

            inputDialog.ShowDialog();
        }

        private void SetClickEventHandlers(ListDialogItem item, ScrollingListDialog dialog, bool isFriendList)
        {
            item.LeftClick += (o, e) => _hudControlProvider.GetComponent<ChatTextBox>(HudControlIdentifier.ChatTextBox).Text = $"!{item.PrimaryText} ";
            item.RightClick += (o, e) =>
            {
                dialog.RemoveFromList(item);
                dialog.Title = GetDialogTitle(dialog, isFriendList);
            };
        }

        private string GetDialogTitle(ScrollingListDialog dialog, bool isFriendList)
        {
            var friendOrIgnoreStr = _localizedStringFinder.GetString(isFriendList ? EOResourceID.STATUS_LABEL_FRIEND_LIST : EOResourceID.STATUS_LABEL_IGNORE_LIST);
            return $"{_characterProvider.MainCharacter.Name}'s {friendOrIgnoreStr} [{dialog.NamesList.Count}]";
        }
    }

    public interface IFriendIgnoreListDialogFactory
    {
        ScrollingListDialog Create(bool isFriendList);
    }
}
