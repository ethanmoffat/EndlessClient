using System;
using System.Collections.Generic;
using EndlessClient.Content;
using EndlessClient.Dialogs.Factories;
using EndlessClient.Dialogs.Services;
using EOLib.Domain.Interact.Citizen;
using EOLib.Graphics;
using EOLib.IO;
using EOLib.IO.Repositories;
using EOLib.Localization;
using EOLib.Shared;
using Microsoft.Xna.Framework;
using Optional.Collections;
using XNAControls;

namespace EndlessClient.Dialogs
{
    public class InnkeeperDialog : ScrollingListDialog
    {
        private enum InnkeeperDialogState
        {
            // initial menu: Registration and Sleep items
            Initial,
            // registration menu: sign up and unsubscribe items
            Registration,
            // sign up text + link
            SignUp,
            // unsubscribe text + link
            Unsubscribe
        }

        private readonly IEODialogIconService _dialogIconService;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly IEOMessageBoxFactory _messageBoxFactory;
        private readonly ITextInputDialogFactory _textInputDialogFactory;
        private readonly ICitizenActions _citizenActions;
        private readonly IContentProvider _contentProvider;
        private readonly IENFFileProvider _enfFileProvider;
        private readonly ICitizenDataProvider _citizenDataProvider;
        private InnkeeperDialogState _state;
        private int _lastVendorId;

        public InnkeeperDialog(INativeGraphicsManager nativeGraphicsManager,
                               IEODialogButtonService dialogButtonService,
                               IEODialogIconService dialogIconService,
                               ILocalizedStringFinder localizedStringFinder,
                               IEOMessageBoxFactory messageBoxFactory,
                               ITextInputDialogFactory textInputDialogFactory,
                               ICitizenActions citizenActions,
                               IContentProvider contentProvider,
                               IENFFileProvider enfFileProvider,
                               ICitizenDataProvider citizenDataProvider)
            : base(nativeGraphicsManager, dialogButtonService, DialogType.Inn)
        {
            _dialogIconService = dialogIconService;
            _localizedStringFinder = localizedStringFinder;
            _messageBoxFactory = messageBoxFactory;
            _textInputDialogFactory = textInputDialogFactory;
            _citizenActions = citizenActions;
            _contentProvider = contentProvider;
            _enfFileProvider = enfFileProvider;
            _citizenDataProvider = citizenDataProvider;

            SetState(InnkeeperDialogState.Initial);

            BackAction += (_, _) =>
            {
                if (_state == InnkeeperDialogState.SignUp || _state == InnkeeperDialogState.Unsubscribe)
                    SetState(InnkeeperDialogState.Registration);
                else if (_state == InnkeeperDialogState.Registration)
                    SetState(InnkeeperDialogState.Initial);
            };
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            if (_citizenDataProvider.BehaviorID.Map(x => x != _lastVendorId).ValueOr(false))
            {
                _lastVendorId = _citizenDataProvider.BehaviorID.ValueOr(0);

                _enfFileProvider.ENFFile.SingleOrNone(x => x.Type == NPCType.Inn && x.VendorID == _lastVendorId)
                    .MatchSome(innkeeperData => Title = innkeeperData.Name);
            }

            base.OnUpdateControl(gameTime);
        }

        private void SetState(InnkeeperDialogState state)
        {
            if (state != InnkeeperDialogState.Initial && _state == state)
                return;

            _state = state;

            ClearItemList();

            switch (_state)
            {
                case InnkeeperDialogState.Initial:
                    {
                        ListItemType = ListDialogItem.ListItemStyle.Large;
                        Buttons = ScrollingListDialogButtons.Cancel;

                        var registrationItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 0)
                        {
                            ShowIconBackGround = false,
                            IconGraphic = _dialogIconService.IconSheet,
                            IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.Registration),
                            PrimaryText = _localizedStringFinder.GetString(EOResourceID.INN_REGISTRATION_SERVICE),
                            SubText = _localizedStringFinder.GetString(EOResourceID.INN_CITIZEN_REGISTRATION_SERVICE),
                            OffsetY = 45,
                        };
                        registrationItem.LeftClick += (_, _) => SetState(InnkeeperDialogState.Registration);
                        registrationItem.RightClick += (_, _) => SetState(InnkeeperDialogState.Registration);

                        var sleepItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 1)
                        {
                            ShowIconBackGround = false,
                            IconGraphic = _dialogIconService.IconSheet,
                            IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.InnSleep),
                            PrimaryText = _localizedStringFinder.GetString(EOResourceID.INN_SLEEP),
                            SubText = _localizedStringFinder.GetString(EOResourceID.INN_FULL_HP_RECOVERY),
                            OffsetY = 45,
                        };
                        sleepItem.LeftClick += (_, _) => _citizenActions.RequestSleep();
                        sleepItem.RightClick += (_, _) => _citizenActions.RequestSleep();

                        SetItemList(new List<ListDialogItem> { registrationItem, sleepItem });
                    }
                    break;
                case InnkeeperDialogState.Registration:
                    {
                        ListItemType = ListDialogItem.ListItemStyle.Large;
                        Buttons = ScrollingListDialogButtons.BackCancel;

                        var signUpItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 0)
                        {
                            ShowIconBackGround = false,
                            IconGraphic = _dialogIconService.IconSheet,
                            IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.SignUp),
                            PrimaryText = _localizedStringFinder.GetString(EOResourceID.INN_SIGN_UP),
                            SubText = _localizedStringFinder.GetString(EOResourceID.INN_BECOME_A_CITIZEN),
                            OffsetY = 45,
                        };
                        signUpItem.LeftClick += (_, _) => SetState(InnkeeperDialogState.SignUp);
                        signUpItem.RightClick += (_, _) => SetState(InnkeeperDialogState.SignUp);

                        var unsubscribeItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 1)
                        {
                            ShowIconBackGround = false,
                            IconGraphic = _dialogIconService.IconSheet,
                            IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.Unsubscribe),
                            PrimaryText = _localizedStringFinder.GetString(EOResourceID.INN_UNSUBSCRIBE),
                            SubText = _localizedStringFinder.GetString(EOResourceID.INN_GIVE_UP_CITIZENSHIP),
                            OffsetY = 45,
                        };
                        unsubscribeItem.LeftClick += (_, _) => SetState(InnkeeperDialogState.Unsubscribe);
                        unsubscribeItem.RightClick += (_, _) => SetState(InnkeeperDialogState.Unsubscribe);

                        SetItemList(new List<ListDialogItem> { signUpItem, unsubscribeItem });
                    }
                    break;
                case InnkeeperDialogState.SignUp:
                    {
                        ListItemType = ListDialogItem.ListItemStyle.Small;
                        Buttons = ScrollingListDialogButtons.BackCancel;

                        AddTextAsListItems(_contentProvider.Fonts[Constants.FontSize09],
                            insertLineBreaks: true,
                            new List<Action>
                            {
                                () =>
                                {
                                    if (_citizenDataProvider.CurrentHomeID.HasValue)
                                    {
                                        var dlg = _messageBoxFactory.CreateMessageBox(EOResourceID.INN_YOU_ARE_ALREADY_A_CITIZEN_OF_A_TOWN, EOResourceID.INN_REGISTRATION_SERVICE);
                                        dlg.ShowDialog();
                                    }
                                    else
                                    {
                                        var answers = new List<string>(3);

                                        Func<int, TextInputDialog> createDlg = i => _textInputDialogFactory.Create($"{i + 1}. {_citizenDataProvider.Questions[i]}");

                                        // we can't suspend the context and await the result of the dialogs because XNAControls isn't that powerful,
                                        //    so we have to get the result in the DialogClosing event handler after we know the user is done entering their input
                                        var dlg1 = createDlg(0);
                                        dlg1.DialogClosing += (_, e1) =>
                                        {
                                            if (e1.Result != XNADialogResult.OK)
                                                return;

                                            var dlg2 = createDlg(1);
                                            dlg2.DialogClosing += (_, e2) =>
                                            {
                                                if (e2.Result != XNADialogResult.OK)
                                                    return;

                                                var dlg3 = createDlg(2);
                                                dlg3.DialogClosing += (_, e3) =>
                                                {
                                                    if (e3.Result != XNADialogResult.OK)
                                                        return;

                                                    var answers = new List<string>
                                                    {
                                                        dlg1.ResponseText,
                                                        dlg2.ResponseText,
                                                        dlg3.ResponseText
                                                    };

                                                    _citizenActions.SignUp(answers);
                                                };

                                                dlg3.ShowDialog();
                                            };

                                            dlg2.ShowDialog();
                                        };

                                        dlg1.ShowDialog();
                                    }
                                },
                            },
                            _localizedStringFinder.GetString(EOResourceID.INN_SIGN_UP),
                            _localizedStringFinder.GetString(EOResourceID.INN_BECOME_CITIZEN_TEXT_1),
                            _localizedStringFinder.GetString(EOResourceID.INN_BECOME_CITIZEN_TEXT_2),
                            _localizedStringFinder.GetString(EOResourceID.INN_BECOME_CITIZEN_TEXT_LINK));
                    }
                    break;
                case InnkeeperDialogState.Unsubscribe:
                    {
                        ListItemType = ListDialogItem.ListItemStyle.Small;
                        Buttons = ScrollingListDialogButtons.BackCancel;

                        AddTextAsListItems(_contentProvider.Fonts[Constants.FontSize09],
                            insertLineBreaks: true,
                            new List<Action>
                            {
                                () =>
                                {
                                    _citizenActions.Unsubscribe();
                                    SetState(InnkeeperDialogState.Registration);
                                },
                            },
                            _localizedStringFinder.GetString(EOResourceID.INN_UNSUBSCRIBE),
                            _localizedStringFinder.GetString(EOResourceID.INN_GIVE_UP_TEXT_1),
                            _localizedStringFinder.GetString(EOResourceID.INN_GIVE_UP_TEXT_LINK));
                    }
                    break;
            }
        }
    }
}
