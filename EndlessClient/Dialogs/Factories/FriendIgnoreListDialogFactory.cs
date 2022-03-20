using AutomaticTypeMapper;
using EndlessClient.ControlSets;
using EndlessClient.Dialogs.Services;
using EndlessClient.GameExecution;
using EndlessClient.HUD.Controls;
using EndlessClient.Old;
using EndlessClient.UIControls;
using EOLib.Domain.Character;
using EOLib.Graphics;
using EOLib.Localization;
using System;
using System.Linq;

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

        public FriendIgnoreListDialogFactory(IGameStateProvider gameStateProvider,
                                             INativeGraphicsManager nativeGraphicsManager,
                                             IEODialogButtonService dialogButtonService,
                                             ILocalizedStringFinder localizedStringFinder,
                                             ICharacterProvider characterProvider,
                                             IHudControlProvider hudControlProvider)
        {
            _gameStateProvider = gameStateProvider;
            _nativeGraphicsManager = nativeGraphicsManager;
            _dialogButtonService = dialogButtonService;
            _localizedStringFinder = localizedStringFinder;
            _characterProvider = characterProvider;
            _hudControlProvider = hudControlProvider;
        }

        public ScrollingListDialog Create(bool isFriendList)
        {
            // todo: refactor InteractList
            var textFileLines = isFriendList ? InteractList.LoadAllFriend() : InteractList.LoadAllIgnore();
            var friendOrIgnoreStr = _localizedStringFinder.GetString(isFriendList ? EOResourceID.STATUS_LABEL_FRIEND_LIST : EOResourceID.STATUS_LABEL_IGNORE_LIST);

            var dialog = new ScrollingListDialog(_gameStateProvider, _nativeGraphicsManager, _dialogButtonService)
            {
                Title = $"{_characterProvider.MainCharacter.Name}'s {friendOrIgnoreStr} [{textFileLines.Count}]",
                Buttons = ScrollingListDialogButtons.AddCancel,
                ListItemType = ListDialogItem.ListItemStyle.Small,
            };

            var listItems = textFileLines.Select(x => new ListDialogItem(dialog, ListDialogItem.ListItemStyle.Small) { PrimaryText = x }).ToList();
            foreach (var item in listItems)
            {
                item.OnLeftClick += (o, e) => _hudControlProvider.GetComponent<ChatTextBox>(HudControlIdentifier.ChatTextBox).Text = $"!{item.PrimaryText} ";
                item.OnRightClick += (o, e) =>
                {
                    dialog.RemoveFromList(item);
                    dialog.Title = $"{_characterProvider.MainCharacter.Name}'s {friendOrIgnoreStr} [{dialog.NamesList.Count}]";
                };
            }
            dialog.SetItemList(listItems);

            dialog.AddAction += InvokeAdd;

            dialog.CloseAction += (_, _) =>
            {
                if (isFriendList)
                    InteractList.WriteFriendList(dialog.NamesList);
                else
                    InteractList.WriteIgnoreList(dialog.NamesList);
            };

            return dialog;
        }

        private void InvokeAdd(object sender, EventArgs e)
        {
            //e.CancelClose = true;
            //string prompt = OldWorld.GetString(isIgnoreList ? EOResourceID.DIALOG_WHO_TO_MAKE_IGNORE : EOResourceID.DIALOG_WHO_TO_MAKE_FRIEND);
            //TextInputDialog dlgInput = new TextInputDialog(prompt);
            //dlgInput.DialogClosing += (_o, _e) =>
            //{
            //    if (_e.Result == XNADialogResult.Cancel) return;

            //    if (dlgInput.ResponseText.Length < 4)
            //    {
            //        _e.CancelClose = true;
            //        EOMessageBox.Show(DialogResourceID.CHARACTER_CREATE_NAME_TOO_SHORT);
            //        dlgInput.SetAsKeyboardSubscriber();
            //        return;
            //    }

            //    if (dlg.NamesList.FindIndex(name => name.ToLower() == dlgInput.ResponseText.ToLower()) >= 0)
            //    {
            //        _e.CancelClose = true;
            //        EOMessageBox.Show("You are already friends with that person!", "Invalid entry!", EODialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
            //        dlgInput.SetAsKeyboardSubscriber();
            //        return;
            //    }

            //    ListDialogItem newItem = new ListDialogItem(dlg, ListDialogItem.ListItemStyle.Small)
            //    {
            //        PrimaryText = dlgInput.ResponseText
            //    };
            //    newItem.OnLeftClick += (oo, ee) => EOGame.Instance.Hud.SetChatText("!" + newItem.PrimaryText + " ");
            //    newItem.OnRightClick += (oo, ee) =>
            //    {
            //        dlg.RemoveFromList(newItem);
            //        dlg.Title = string.Format("{0}'s {2} [{1}]",
            //            charName,
            //            dlg.NamesList.Count,
            //            OldWorld.GetString(isIgnoreList ? EOResourceID.STATUS_LABEL_IGNORE_LIST : EOResourceID.STATUS_LABEL_FRIEND_LIST));
            //    };
            //    dlg.AddItemToList(newItem, true);
            //    dlg.Title = string.Format("{0}'s {2} [{1}]", charName, dlg.NamesList.Count,
            //        OldWorld.GetString(isIgnoreList ? EOResourceID.STATUS_LABEL_IGNORE_LIST : EOResourceID.STATUS_LABEL_FRIEND_LIST));
            //};
        }
    }

    public interface IFriendIgnoreListDialogFactory
    {
        ScrollingListDialog Create(bool isFriendList);
    }
}
