using EndlessClient.Content;
using EndlessClient.Dialogs.Factories;
using EndlessClient.Dialogs.Services;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Interact.Guild;
using EOLib.Domain.Map;
using EOLib.Graphics;
using EOLib.IO;
using EOLib.IO.Repositories;
using EOLib.Localization;
using Optional.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using XNAControls;
using EndlessClient.Audio;
using System.Diagnostics;

namespace EndlessClient.Dialogs
{
    public class GuildDialog : ScrollingListDialog
    {
        // Clickable region for the list dialog
        private const int AdjustedDrawAreaOffset = 10;
        // Max allowed text look up for guild
        private const int MaxGuildTag = 3;

        private enum GuildDialogState
        {
            // initial menu
            Initial,
            InformationMenu,
            Registration,
            Adminstration,
            GuildBank,
        }


        private enum DialogAction
        {
            GuildLookUp,
            MemberList
        }

        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IEODialogButtonService _dialogButtonService;
        private readonly IEODialogIconService _dialogIconService;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly ITextInputDialogFactory _textInputDialogFactory;
        private readonly IGuildActions _GuildActions;
        private readonly IContentProvider _contentProvider;
        private readonly ICurrentMapStateProvider _currentMapStateProvider;
        private readonly IENFFileProvider _enfFileProvider;
        private readonly IEOMessageBoxFactory _messageBoxFactory;
        private readonly ICharacterRepository _characterRepository;
        private readonly IEOMessageBoxFactory _eoMessageBoxFactory;
        private readonly ISfxPlayer _sfxPlayer;
        // Keep track of states
        private readonly Stack<GuildDialogState> _stateStack = new Stack<GuildDialogState>();

        private GuildDialogState _state;
        private DialogAction _currentDialogAction;

        public GuildDialog(INativeGraphicsManager nativeGraphicsManager,
                         IEODialogButtonService dialogButtonService,
                         IEODialogIconService dialogIconService,
                         ILocalizedStringFinder localizedStringFinder,
                         ITextInputDialogFactory textInputDialogFactory,
                         IGuildActions guildActions,
                         IContentProvider contentProvider,
                         ICurrentMapStateProvider currentMapStateProvider,
                         IENFFileProvider enfFileProvider,
                         IEOMessageBoxFactory messageBoxFactory,
                         ICharacterRepository characterLvRepository,
                         IEOMessageBoxFactory eoMessageBoxFactory,
                         ISfxPlayer sfxPlayer)
            : base(nativeGraphicsManager, dialogButtonService, DialogType.Guild)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _dialogButtonService = dialogButtonService;
            _dialogIconService = dialogIconService;
            _localizedStringFinder = localizedStringFinder;
            _textInputDialogFactory = textInputDialogFactory;
            _GuildActions = guildActions;
            _contentProvider = contentProvider;
            _currentMapStateProvider = currentMapStateProvider;
            _enfFileProvider = enfFileProvider;
            _messageBoxFactory = messageBoxFactory;
            _characterRepository = characterLvRepository;
            _eoMessageBoxFactory = eoMessageBoxFactory;
            _sfxPlayer = sfxPlayer;

            SetState(GuildDialogState.Initial);

            BackAction += (_, _) =>
            {
                if (_stateStack.Count > 0)
                {
                    var previousState = _stateStack.Pop();
                    SetState(previousState);
                }
                else
                {
                    SetState(GuildDialogState.Initial);
                }
            };

            Title = _localizedStringFinder.GetString(EOResourceID.GUILD_GUILD_MASTER);
        }

        private void SetState(GuildDialogState newState)
        {
            if (_state != newState && _stateStack.Any())
            {
                ClearItemList();
                _stateStack.Push(_state); 
            }

            _state = newState;

            ClearItemList(); 

            switch (_state)
            {
                case GuildDialogState.Initial:
                    {
                        ListItemType = ListDialogItem.ListItemStyle.Large;
                        Buttons = ScrollingListDialogButtons.Cancel;

                        var informationItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 0)
                        {
                            ShowIconBackGround = false,
                            IconGraphic = _dialogIconService.IconSheet,
                            IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.GuildInformation),
                            PrimaryText = _localizedStringFinder.GetString(EOResourceID.GUILD_INFORMATION),
                            SubText = _localizedStringFinder.GetString(EOResourceID.GUILD_LEARN_MORE),
                            OffsetY = 48,
                            OffsetX = -8,
                        };
                        informationItem.DrawArea = informationItem.DrawArea.WithSize(informationItem.DrawArea.Width + AdjustedDrawAreaOffset, informationItem.DrawArea.Height);
                        informationItem.LeftClick += (_, _) => SetState(GuildDialogState.InformationMenu);
                        informationItem.RightClick += (_, _) => SetState(GuildDialogState.InformationMenu);

                        AddItemToList(informationItem, sortList: false);

                        var administrationItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 1)
                        {
                            ShowIconBackGround = false,
                            IconGraphic = _dialogIconService.IconSheet,
                            IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.GuildAdministration),
                            PrimaryText = _localizedStringFinder.GetString(EOResourceID.GUILD_ADMINISTRATION),
                            SubText = _localizedStringFinder.GetString(EOResourceID.GUILD_JOIN_LEAVE_REGISTER),
                            OffsetY = 48,
                            OffsetX = -8,
                        };
                        administrationItem.DrawArea = administrationItem.DrawArea.WithSize(administrationItem.DrawArea.Width + AdjustedDrawAreaOffset, administrationItem.DrawArea.Height);
                        administrationItem.LeftClick += (_, _) => SetState(GuildDialogState.Adminstration);
                        administrationItem.RightClick += (_, _) => SetState(GuildDialogState.Adminstration);

                        AddItemToList(administrationItem, sortList: false);

                        var managementItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 2)
                        {
                            ShowIconBackGround = false,
                            IconGraphic = _dialogIconService.IconSheet,
                            IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.GuildManagement),
                            PrimaryText = _localizedStringFinder.GetString(EOResourceID.GUILD_MANAGEMENT),
                            SubText = _localizedStringFinder.GetString(EOResourceID.GUILD_MODIFY_RANKING_DISBAND),
                            OffsetY = 48,
                            OffsetX = -8,
                        };
                        managementItem.DrawArea = managementItem.DrawArea.WithSize(managementItem.DrawArea.Width + AdjustedDrawAreaOffset, managementItem.DrawArea.Height);
                        //registrationItem.LeftClick += (_, _) => SetState(GuildDialogState.Registration);
                        //registrationItem.RightClick += (_, _) => SetState(GuildDialogState.Registration);

                        AddItemToList(managementItem, sortList: false);

                        var bankAccountItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 3)
                        {
                            ShowIconBackGround = false,
                            IconGraphic = _dialogIconService.IconSheet,
                            IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.GuildBankAccount),
                            PrimaryText = _localizedStringFinder.GetString(EOResourceID.GUILD_BANK_ACCOUNT),
                            SubText = _localizedStringFinder.GetString(EOResourceID.GUILD_DEPOSIT_TO_GUILD_ACCOUNT),
                            OffsetY = 48,
                            OffsetX = -8,
                        };
                        bankAccountItem.DrawArea = bankAccountItem.DrawArea.WithSize(bankAccountItem.DrawArea.Width + AdjustedDrawAreaOffset, bankAccountItem.DrawArea.Height);
                        bankAccountItem.LeftClick += (_, _) => SetState(GuildDialogState.GuildBank);
                        bankAccountItem.RightClick += (_, _) => SetState(GuildDialogState.GuildBank);

                        AddItemToList(bankAccountItem, sortList: false);
                    }
                    break;

                case GuildDialogState.InformationMenu:
                    {
                        Buttons = ScrollingListDialogButtons.BackCancel;

                        var guildLookUp = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 0)
                        {
                            ShowIconBackGround = false,
                            IconGraphic = _dialogIconService.IconSheet,
                            IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.GuildLookup),
                            PrimaryText = _localizedStringFinder.GetString(EOResourceID.GUILD_JOIN_GUILD),
                            SubText = _localizedStringFinder.GetString(EOResourceID.GUILD_JOIN_AN_EXISTING_GUILD),
                            OffsetY = 48,
                            OffsetX = -8,
                        };
                        guildLookUp.DrawArea = guildLookUp.DrawArea.WithSize(guildLookUp.DrawArea.Width + AdjustedDrawAreaOffset, guildLookUp.DrawArea.Height);
                        guildLookUp.LeftClick += (_, _) => ShowGuildDialog(DialogAction.GuildLookUp);
                        guildLookUp.RightClick += (_, _) => ShowGuildDialog(DialogAction.GuildLookUp);
                        AddItemToList(guildLookUp, sortList: false);

                        var memberList = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 1)
                        {
                            ShowIconBackGround = false,
                            IconGraphic = _dialogIconService.IconSheet,
                            IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.GuildMemberlist),
                            PrimaryText = _localizedStringFinder.GetString(EOResourceID.MEMBERLIST),
                            SubText = _localizedStringFinder.GetString(EOResourceID.VIEW_GUILD_MEMBERS),
                            OffsetY = 48,
                            OffsetX = -8,
                        };
                        memberList.DrawArea = memberList.DrawArea.WithSize(memberList.DrawArea.Width + AdjustedDrawAreaOffset, memberList.DrawArea.Height);
                        memberList.LeftClick += (_, _) => ShowGuildDialog(DialogAction.MemberList);
                        memberList.RightClick += (_, _) => ShowGuildDialog(DialogAction.MemberList);
                        AddItemToList(memberList, sortList: false);
                    }
                    break;

                case GuildDialogState.Adminstration:
                    {
                        Buttons = ScrollingListDialogButtons.BackCancel;
                        var guildJoin = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 0)
                        {
                            ShowIconBackGround = false,
                            IconGraphic = _dialogIconService.IconSheet,
                            IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.GuildJoin),
                            PrimaryText = _localizedStringFinder.GetString(EOResourceID.GUILD_JOIN_GUILD),
                            SubText = _localizedStringFinder.GetString(EOResourceID.GUILD_JOIN_AN_EXISTING_GUILD),
                            OffsetY = 48,
                            OffsetX = -8,
                        };
                        guildJoin.DrawArea = guildJoin.DrawArea.WithSize(guildJoin.DrawArea.Width + AdjustedDrawAreaOffset, guildJoin.DrawArea.Height);
                        guildJoin.LeftClick += (_, _) => ShowGuildDialog(DialogAction.GuildLookUp);
                        guildJoin.RightClick += (_, _) => ShowGuildDialog(DialogAction.GuildLookUp);
                        AddItemToList(guildJoin, sortList: false);

                        var leaveGuild = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 1)
                        {
                            ShowIconBackGround = false,
                            IconGraphic = _dialogIconService.IconSheet,
                            IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.GuildLeave),
                            PrimaryText = _localizedStringFinder.GetString(EOResourceID.GUILD_LEAVE_GUILD),
                            SubText = _localizedStringFinder.GetString(EOResourceID.GUILD_LEAVE_YOUR_CURRENT_GUILD),
                            OffsetY = 48,
                            OffsetX = -8,
                        };
                        leaveGuild.DrawArea = leaveGuild.DrawArea.WithSize(leaveGuild.DrawArea.Width + AdjustedDrawAreaOffset, leaveGuild.DrawArea.Height);
                        leaveGuild.LeftClick += (_, _) => LeaveGuildDialog();
                        leaveGuild.RightClick += (_, _) => LeaveGuildDialog();
                        AddItemToList(leaveGuild, sortList: false);

                        var registerGuild = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 2)
                        {
                            ShowIconBackGround = false,
                            IconGraphic = _dialogIconService.IconSheet,
                            IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.GuildRegister),
                            PrimaryText = _localizedStringFinder.GetString(EOResourceID.GUILD_REGISTER_GUILD),
                            SubText = _localizedStringFinder.GetString(EOResourceID.GUILD_CREATE_YOUR_OWN_GUILD),
                            OffsetY = 48,
                            OffsetX = -8,
                        };
                        registerGuild.DrawArea = registerGuild.DrawArea.WithSize(registerGuild.DrawArea.Width + AdjustedDrawAreaOffset, registerGuild.DrawArea.Height);
                        //registerGuild.LeftClick += (_, _) => ShowGuildLookUpDialog();
                        //registerGuild.RightClick += (_, _) => ShowGuildLookUpDialog();
                        AddItemToList(registerGuild, sortList: false);
                    }
                    break;

                case GuildDialogState.GuildBank:
                {
                    if (string.IsNullOrEmpty(_characterRepository.MainCharacter.GuildTag))
                    {
                        var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.GUILD_NOT_IN_GUILD);
                        dlg.ShowDialog();
                        SetState(GuildDialogState.Initial);
                    }
                }
                    break;
            }
        }

        private void HandleDialogAction(string responseText)
        {
            switch (_currentDialogAction)
            {
                case DialogAction.GuildLookUp:
                    Debug.WriteLine("Call Guild look up");
                    break;
                case DialogAction.MemberList:
                    Debug.WriteLine("Call Memberlist");
                    break;
            }

            var dlg2 = _eoMessageBoxFactory.CreateMessageBox(DialogResourceID.GUILD_YOU_HAVE_BEEN_ACCEPTED);
            dlg2.ShowDialog();
        }

        private void ShowGuildDialog(DialogAction action)
        {
            _currentDialogAction = action;

            var promptMessageId = action == DialogAction.GuildLookUp ? EOResourceID.GUILD_VIEW_INFO_PROMPT : EOResourceID.GUILD_VIEW_INFO_PROMPT;
            var dlg = _textInputDialogFactory.Create(_localizedStringFinder.GetString(promptMessageId), maxInputChars: MaxGuildTag);

            dlg.DialogClosing += (_, e) =>
            {
                if (e.Result == XNADialogResult.OK && dlg.ResponseText.Length > 0)
                {
                    HandleDialogAction(dlg.ResponseText);
                }
            };
            dlg.ShowDialog();
        }

        // idk the sound effect number
        private void LeaveGuildDialog()
        {
            ClearItemList(); 
            ListItemType = ListDialogItem.ListItemStyle.Small;
            Buttons = ScrollingListDialogButtons.BackCancel;

            var actions = new List<Action> {
                () =>
                {
                    var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.GUILD_PROMPT_LEAVE_GUILD);
                    _GuildActions.LeaveGuild();
                    _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithGuildTag(string.Empty);
                    dlg.ShowDialog();
                }
            };
            AddTextAsListItems(
                _contentProvider.Fonts[Constants.FontSize09],
                true,
                actions,
                _localizedStringFinder.GetString(EOResourceID.GUILD_LEAVE_GUILD),
                _localizedStringFinder.GetString(EOResourceID.GUILD_YOU_ARE_ABOUT_TO_LEAVE_THE_GUILD),
                _localizedStringFinder.GetString(EOResourceID.GUILD_AFTER_YOU_HAVE_LEFT),
                _localizedStringFinder.GetString(EOResourceID.GUILD_CLICK_HERE_TO_LEAVE_YOUR_GUILD));
        }
    }
}