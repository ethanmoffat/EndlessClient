using System;
using System.Collections.Generic;
using System.Linq;
using EndlessClient.Content;
using EndlessClient.Dialogs.Factories;
using EndlessClient.Dialogs.Services;
using EOLib.Domain.Interact.Law;
using EOLib.Domain.Map;
using EOLib.Graphics;
using EOLib.IO;
using EOLib.IO.Repositories;
using EOLib.Localization;
using EOLib.Shared;
using Optional.Collections;
using XNAControls;

namespace EndlessClient.Dialogs
{
    public class LawDialog : ScrollingListDialog
    {
        private enum LawDialogState
        {
            // initial menu
            Initial,
            // registration menu: marriage/divorce items
            Registration,
            Marriage,
            Divorce,
        }

        private readonly IEODialogIconService _dialogIconService;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly ITextInputDialogFactory _textInputDialogFactory;
        private readonly ILawActions _lawActions;
        private readonly IContentProvider _contentProvider;
        private readonly ICurrentMapStateProvider _currentMapStateProvider;
        private readonly IENFFileProvider _enfFileProvider;

        private LawDialogState _state;

        public LawDialog(INativeGraphicsManager nativeGraphicsManager,
                         IEODialogButtonService dialogButtonService,
                         IEODialogIconService dialogIconService,
                         ILocalizedStringFinder localizedStringFinder,
                         ITextInputDialogFactory textInputDialogFactory,
                         ILawActions lawActions,
                         IContentProvider contentProvider,
                         ICurrentMapStateProvider currentMapStateProvider,
                         IENFFileProvider enfFileProvider)
            : base(nativeGraphicsManager, dialogButtonService, DialogType.Law)
        {
            _dialogIconService = dialogIconService;
            _localizedStringFinder = localizedStringFinder;
            _textInputDialogFactory = textInputDialogFactory;
            _lawActions = lawActions;
            _contentProvider = contentProvider;
            _currentMapStateProvider = currentMapStateProvider;
            _enfFileProvider = enfFileProvider;

            SetState(LawDialogState.Initial);

            BackAction += (_, _) =>
            {
                if (_state == LawDialogState.Marriage || _state == LawDialogState.Divorce)
                    SetState(LawDialogState.Registration);
                else if (_state == LawDialogState.Registration)
                    SetState(LawDialogState.Initial);
            };

            _currentMapStateProvider.NPCs
                .Select(x => _enfFileProvider.ENFFile[x.ID])
                .SingleOrNone(x => x.Type == NPCType.Law)
                .MatchSome(x => Title = x.Name);
        }

        private void SetState(LawDialogState state)
        {
            if (state != LawDialogState.Initial && _state == state)
                return;

            _state = state;

            ClearItemList();

            switch (_state)
            {
                case LawDialogState.Initial:
                    {
                        ListItemType = ListDialogItem.ListItemStyle.Large;
                        Buttons = ScrollingListDialogButtons.Cancel;

                        var registrationItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 0)
                        {
                            ShowIconBackGround = false,
                            IconGraphic = _dialogIconService.IconSheet,
                            IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.Registration),
                            PrimaryText = _localizedStringFinder.GetString(EOResourceID.WEDDING_REGISTRATION_SERVICE),
                            SubText = _localizedStringFinder.GetString(EOResourceID.WEDDING_REQUEST_MARRIAGE_OR_DIVORCE),
                            OffsetY = 45,
                        };
                        registrationItem.LeftClick += (_, _) => SetState(LawDialogState.Registration);
                        registrationItem.RightClick += (_, _) => SetState(LawDialogState.Registration);

                        AddItemToList(registrationItem, sortList: false);
                    }
                    break;
                case LawDialogState.Registration:
                    {
                        ListItemType = ListDialogItem.ListItemStyle.Large;
                        Buttons = ScrollingListDialogButtons.BackCancel;

                        var marriageItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 0)
                        {
                            ShowIconBackGround = false,
                            IconGraphic = _dialogIconService.IconSheet,
                            IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.SignUp),
                            PrimaryText = _localizedStringFinder.GetString(EOResourceID.WEDDING_MARRIAGE),
                            SubText = _localizedStringFinder.GetString(EOResourceID.WEDDING_REQUEST_WEDDING_APPROVAL),
                            OffsetY = 45,
                        };
                        marriageItem.LeftClick += (_, _) => SetState(LawDialogState.Marriage);
                        marriageItem.RightClick += (_, _) => SetState(LawDialogState.Marriage);

                        var divorceItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 1)
                        {
                            ShowIconBackGround = false,
                            IconGraphic = _dialogIconService.IconSheet,
                            IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.Unsubscribe),
                            PrimaryText = _localizedStringFinder.GetString(EOResourceID.WEDDING_DIVORCE),
                            SubText = _localizedStringFinder.GetString(EOResourceID.WEDDING_BREAK_UP),
                            OffsetY = 45,
                        };
                        divorceItem.LeftClick += (_, _) => SetState(LawDialogState.Divorce);
                        divorceItem.RightClick += (_, _) => SetState(LawDialogState.Divorce);

                        SetItemList(new List<ListDialogItem> { marriageItem, divorceItem });
                    }
                    break;
                case LawDialogState.Marriage:
                    {
                        ListItemType = ListDialogItem.ListItemStyle.Small;
                        Buttons = ScrollingListDialogButtons.BackCancel;

                        AddTextAsListItems(_contentProvider.Fonts[Constants.FontSize09],
                            insertLineBreaks: true,
                            new List<Action>
                            {
                                () =>
                                {
                                    var dlg = _textInputDialogFactory.Create(_localizedStringFinder.GetString(EOResourceID.WEDDING_PROMPT_ENTER_NAME_MARRY));
                                    dlg.DialogClosing += (_, e) =>
                                    {
                                        if (e.Result != XNADialogResult.OK)
                                            return;

                                        _lawActions.RequestMarriage(dlg.ResponseText);
                                    };
                                    dlg.ShowDialog();
                                },
                            },
                            _localizedStringFinder.GetString(EOResourceID.WEDDING_MARRIAGE),
                            _localizedStringFinder.GetString(EOResourceID.WEDDING_REQUEST_TEXT_1),
                            _localizedStringFinder.GetString(EOResourceID.WEDDING_REQUEST_TEXT_2),
                            _localizedStringFinder.GetString(EOResourceID.WEDDING_REQUEST_TEXT_LINK));
                    }
                    break;
                case LawDialogState.Divorce:
                    {
                        ListItemType = ListDialogItem.ListItemStyle.Small;
                        Buttons = ScrollingListDialogButtons.BackCancel;

                        AddTextAsListItems(_contentProvider.Fonts[Constants.FontSize09],
                            insertLineBreaks: true,
                            new List<Action>
                            {
                                () =>
                                {
                                    var dlg = _textInputDialogFactory.Create(_localizedStringFinder.GetString(EOResourceID.WEDDING_PROMPT_ENTER_NAME_DIVORCE));
                                    dlg.DialogClosing += (_, e) =>
                                    {
                                        if (e.Result != XNADialogResult.OK)
                                            return;

                                        _lawActions.RequestDivorce(dlg.ResponseText);
                                    };
                                    dlg.ShowDialog();
                                },
                            },
                            _localizedStringFinder.GetString(EOResourceID.WEDDING_DIVORCE),
                            _localizedStringFinder.GetString(EOResourceID.WEDDING_DIVORCE_TEXT_1),
                            _localizedStringFinder.GetString(EOResourceID.WEDDING_DIVORCE_TEXT_2),
                            _localizedStringFinder.GetString(EOResourceID.WEDDING_DIVORCE_TEXT_LINK));
                    }
                    break;
            }
        }
    }
}
