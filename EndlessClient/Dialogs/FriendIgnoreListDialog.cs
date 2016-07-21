// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using System.Linq;
using EOLib;
using EOLib.Localization;
using EOLib.Net.API;
using XNAControls;

namespace EndlessClient.Dialogs
{
    public static class FriendIgnoreListDialog
    {
        private static ScrollingListDialog Instance;

        public static void Show(PacketAPI apiHandle, bool isIgnoreList)
        {
            if (Instance != null)
                return;

            List<string> allLines = isIgnoreList ? InteractList.LoadAllIgnore() : InteractList.LoadAllFriend();

            string charName = OldWorld.Instance.MainPlayer.ActiveCharacter.Name;
            charName = char.ToUpper(charName[0]) + charName.Substring(1);
            string titleText = string.Format("{0}'s {2} [{1}]", charName, allLines.Count,
                OldWorld.GetString(isIgnoreList ? EOResourceID.STATUS_LABEL_IGNORE_LIST : EOResourceID.STATUS_LABEL_FRIEND_LIST));

            ScrollingListDialog dlg = new ScrollingListDialog
            {
                Title = titleText,
                Buttons = ScrollingListDialogButtons.AddCancel,
                ListItemType = ListDialogItem.ListItemStyle.Small
            };

            List<ListDialogItem> characters = allLines.Select(character => new ListDialogItem(dlg, ListDialogItem.ListItemStyle.Small) { Text = character }).ToList();
            characters.ForEach(character =>
            {
                character.OnLeftClick += (o, e) => EOGame.Instance.Hud.SetChatText("!" + character.Text + " ");
                character.OnRightClick += (o, e) =>
                {
                    dlg.RemoveFromList(character);
                    dlg.Title = string.Format("{0}'s {2} [{1}]", charName, dlg.NamesList.Count,
                        OldWorld.GetString(isIgnoreList ? EOResourceID.STATUS_LABEL_IGNORE_LIST : EOResourceID.STATUS_LABEL_FRIEND_LIST));
                };
            });
            dlg.SetItemList(characters);

            dlg.DialogClosing += (o, e) =>
            {
                if (e.Result == XNADialogResult.Cancel)
                {
                    Instance = null;
                    if (isIgnoreList)
                        InteractList.WriteIgnoreList(dlg.NamesList);
                    else
                        InteractList.WriteFriendList(dlg.NamesList);
                }
                else if (e.Result == XNADialogResult.Add)
                {
                    e.CancelClose = true;
                    string prompt = OldWorld.GetString(isIgnoreList ? EOResourceID.DIALOG_WHO_TO_MAKE_IGNORE : EOResourceID.DIALOG_WHO_TO_MAKE_FRIEND);
                    TextInputDialog dlgInput = new TextInputDialog(prompt);
                    dlgInput.DialogClosing += (_o, _e) =>
                    {
                        if (_e.Result == XNADialogResult.Cancel) return;

                        if (dlgInput.ResponseText.Length < 4)
                        {
                            _e.CancelClose = true;
                            EOMessageBox.Show(DialogResourceID.CHARACTER_CREATE_NAME_TOO_SHORT);
                            dlgInput.SetAsKeyboardSubscriber();
                            return;
                        }

                        if (dlg.NamesList.FindIndex(name => name.ToLower() == dlgInput.ResponseText.ToLower()) >= 0)
                        {
                            _e.CancelClose = true;
                            EOMessageBox.Show("You are already friends with that person!", "Invalid entry!", XNADialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
                            dlgInput.SetAsKeyboardSubscriber();
                            return;
                        }

                        ListDialogItem newItem = new ListDialogItem(dlg, ListDialogItem.ListItemStyle.Small)
                        {
                            Text = dlgInput.ResponseText
                        };
                        newItem.OnLeftClick += (oo, ee) => EOGame.Instance.Hud.SetChatText("!" + newItem.Text + " ");
                        newItem.OnRightClick += (oo, ee) =>
                        {
                            dlg.RemoveFromList(newItem);
                            dlg.Title = string.Format("{0}'s {2} [{1}]",
                                charName,
                                dlg.NamesList.Count,
                                OldWorld.GetString(isIgnoreList ? EOResourceID.STATUS_LABEL_IGNORE_LIST : EOResourceID.STATUS_LABEL_FRIEND_LIST));
                        };
                        dlg.AddItemToList(newItem, true);
                        dlg.Title = string.Format("{0}'s {2} [{1}]", charName, dlg.NamesList.Count,
                            OldWorld.GetString(isIgnoreList ? EOResourceID.STATUS_LABEL_IGNORE_LIST : EOResourceID.STATUS_LABEL_FRIEND_LIST));
                    };
                }
            };

            Instance = dlg;

            List<OnlineEntry> onlineList;
            apiHandle.RequestOnlinePlayers(false, out onlineList);
            Instance.SetActiveItemList(onlineList.Select(_oe => _oe.Name).ToList());

            EOGame.Instance.Hud.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_ACTION, isIgnoreList ? EOResourceID.STATUS_LABEL_IGNORE_LIST : EOResourceID.STATUS_LABEL_FRIEND_LIST,
                OldWorld.GetString(EOResourceID.STATUS_LABEL_USE_RIGHT_MOUSE_CLICK_DELETE));
            //show the dialog
        }
    }
}
