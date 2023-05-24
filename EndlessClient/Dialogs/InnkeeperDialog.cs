using EndlessClient.Content;
using EndlessClient.Dialogs.Factories;
using EndlessClient.Dialogs.Services;
using EOLib;
using EOLib.Domain.Interact.Citizen;
using EOLib.Graphics;
using EOLib.IO;
using EOLib.IO.Repositories;
using EOLib.Localization;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Input.InputListeners;
using Optional.Collections;
using System.Collections.Generic;

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
            Unsubscribe,
            // sleep at inn (todo)
            Sleep
        }

        private readonly IEODialogIconService _dialogIconService;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly IEOMessageBoxFactory _messageBoxFactory;
        private readonly ITextInputDialogFactory _textInputDialogFactory;
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
                               IContentProvider contentProvider,
                               IENFFileProvider enfFileProvider,
                               ICitizenDataProvider citizenDataProvider)
            : base(nativeGraphicsManager, dialogButtonService, DialogType.Inn)
        {
            _dialogIconService = dialogIconService;
            _localizedStringFinder = localizedStringFinder;
            _messageBoxFactory = messageBoxFactory;
            _textInputDialogFactory = textInputDialogFactory;
            _contentProvider = contentProvider;
            _enfFileProvider = enfFileProvider;
            _citizenDataProvider = citizenDataProvider;

            SetState(InnkeeperDialogState.Initial);

            BackAction += (_, _) =>
            {
                if (_state == InnkeeperDialogState.SignUp || _state == InnkeeperDialogState.Unsubscribe)
                    SetState(InnkeeperDialogState.Registration);
                else if (_state == InnkeeperDialogState.Registration || _state == InnkeeperDialogState.Sleep)
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
                            IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.InnRegistration),
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
                        sleepItem.LeftClick += Sleep_Click;
                        sleepItem.RightClick += Sleep_Click;

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
                            () =>
                            {
                            },
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
                            () =>
                            {
                            },
                            _localizedStringFinder.GetString(EOResourceID.INN_GIVE_UP_TEXT_1),
                            _localizedStringFinder.GetString(EOResourceID.INN_GIVE_UP_TEXT_LINK));
                    }
                    break;
                case InnkeeperDialogState.Sleep: break;
            }
        }

        private void Sleep_Click(object sender, MouseEventArgs e)
        {
            // todo: inn sleeping
        }
    }
}
