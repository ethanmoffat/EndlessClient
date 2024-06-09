using System.Diagnostics;
using EndlessClient.Content;
using EndlessClient.Dialogs.Factories;
using EndlessClient.Dialogs.Services;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Interact.Guild;
using EOLib.Domain.Map;
using EOLib.Graphics;
using EOLib.IO.Repositories;
using EOLib.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using XNAControls;
using EndlessClient.Audio;

namespace EndlessClient.Dialogs
{
    public class GuildDialog : ScrollingListDialog
    {
        private const int AdjustedDrawAreaOffset = 10;
        private const int MaxGuildTag = 3;

        private enum GuildDialogState
        {
            Initial,
            InformationMenu,
            Registration,
            Administration,
            GuildBank,
        }

        private enum DialogAction
        {
            GuildLookUp,
            MemberList,
            LeaveGuild,
            KickPlayer,
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
        private readonly Stack<GuildDialogState> _stateStack = new Stack<GuildDialogState>();
        private readonly IGuildSessionProvider _guildSessionProvider;

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
                         ISfxPlayer sfxPlayer,
                         IGuildSessionProvider guildSessionProvider)
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
            _guildSessionProvider = guildSessionProvider;

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
            Debug.WriteLine($"Setting state: {newState}");
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
                    ListItemType = ListDialogItem.ListItemStyle.Large;
                    Buttons = ScrollingListDialogButtons.Cancel;

                    var informationItem = CreateListDialogItem(0, DialogIcon.GuildInformation, EOResourceID.GUILD_INFORMATION, EOResourceID.GUILD_LEARN_MORE, () => SetState(GuildDialogState.InformationMenu));
                    AddItemToList(informationItem, sortList: false);

                    var administrationItem = CreateListDialogItem(1, DialogIcon.GuildAdministration, EOResourceID.GUILD_ADMINISTRATION, EOResourceID.GUILD_JOIN_LEAVE_REGISTER, () => SetState(GuildDialogState.Administration));
                    AddItemToList(administrationItem, sortList: false);

                    var managementItem = CreateListDialogItem(2, DialogIcon.GuildManagement, EOResourceID.GUILD_MANAGEMENT, EOResourceID.GUILD_MODIFY_RANKING_DISBAND);
                    AddItemToList(managementItem, sortList: false);

                    var bankAccountItem = CreateListDialogItem(3, DialogIcon.GuildBankAccount, EOResourceID.GUILD_BANK_ACCOUNT, EOResourceID.GUILD_DEPOSIT_TO_GUILD_ACCOUNT, () => SetState(GuildDialogState.GuildBank));
                    AddItemToList(bankAccountItem, sortList: false);
                    break;

                case GuildDialogState.InformationMenu:
                    Buttons = ScrollingListDialogButtons.BackCancel;

                    var guildLookUp = CreateListDialogItem(0, DialogIcon.GuildLookup, EOResourceID.GUILD_JOIN_GUILD, EOResourceID.GUILD_JOIN_AN_EXISTING_GUILD, () => ShowGuildDialog(DialogAction.GuildLookUp));
                    AddItemToList(guildLookUp, sortList: false);

                    var memberList = CreateListDialogItem(1, DialogIcon.GuildMemberlist, EOResourceID.MEMBERLIST, EOResourceID.VIEW_GUILD_MEMBERS, () => ShowGuildDialog(DialogAction.MemberList));
                    AddItemToList(memberList, sortList: false);
                    break;

                case GuildDialogState.Administration:
                    Buttons = ScrollingListDialogButtons.BackCancel;

                    var guildJoin = CreateListDialogItem(0, DialogIcon.GuildJoin, EOResourceID.GUILD_JOIN_GUILD, EOResourceID.GUILD_JOIN_AN_EXISTING_GUILD, () => ShowGuildDialog(DialogAction.GuildLookUp));
                    AddItemToList(guildJoin, sortList: false);

                    var leaveGuild = CreateListDialogItem(1, DialogIcon.GuildLeave, EOResourceID.GUILD_LEAVE_GUILD, EOResourceID.GUILD_LEAVE_YOUR_CURRENT_GUILD, () => LeaveGuildDialog());
                    AddItemToList(leaveGuild, sortList: false);

                    var registerGuild = CreateListDialogItem(2, DialogIcon.GuildRegister, EOResourceID.GUILD_REGISTER_GUILD, EOResourceID.GUILD_CREATE_YOUR_OWN_GUILD);
                    AddItemToList(registerGuild, sortList: false);
                    break;

                case GuildDialogState.GuildBank:
                    if (string.IsNullOrEmpty(_characterRepository.MainCharacter.GuildTag))
                    {
                        var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.GUILD_NOT_IN_GUILD);
                        dlg.ShowDialog();
                        SetState(GuildDialogState.Initial);
                    }
                    break;
            }
        }

        private ListDialogItem CreateListDialogItem(int index, DialogIcon icon, EOResourceID primaryTextResourceID, EOResourceID subTextResourceID, Action leftClickAction = null)
        {
            var item = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, index)
            {
                ShowIconBackGround = false,
                IconGraphic = _dialogIconService.IconSheet,
                IconGraphicSource = _dialogIconService.GetDialogIconSource(icon),
                PrimaryText = _localizedStringFinder.GetString(primaryTextResourceID),
                SubText = _localizedStringFinder.GetString(subTextResourceID),
                OffsetY = 48,
                OffsetX = -8,
            };
            item.DrawArea = item.DrawArea.WithSize(item.DrawArea.Width + AdjustedDrawAreaOffset, item.DrawArea.Height);

            if (leftClickAction != null)
            {
                item.LeftClick += (_, _) => leftClickAction();
                item.RightClick += (_, _) => leftClickAction();
            }

            return item;
        }

        private void HandleDialogAction(string responseText)
        {          
            switch (_currentDialogAction)
            {
                case DialogAction.GuildLookUp:
                    ClearItemList();
                    ListItemType = ListDialogItem.ListItemStyle.Large;
                    Buttons = ScrollingListDialogButtons.BackCancel;
                    break;

                case DialogAction.MemberList:
                    ListItemType = ListDialogItem.ListItemStyle.Small;
                    Buttons = ScrollingListDialogButtons.BackCancel;
                    _GuildActions.ViewMembers(responseText);
                    _guildSessionProvider.MemberListUpdated += OnMemberListUpdated;
                    //To prevent empty dialogs and old results, the event handler will call ClearItemList and PopulateMemberList.
                    break;
            }
        }

        private void PopulateMemberList()
        {
            foreach (var member in _guildSessionProvider.Members)
            {
                var memberName = member.Key;
                var rank = member.Value.Rank;
                var rankName = member.Value.RankName;

                var memberItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Small, 0)
                {
                    ShowIconBackGround = false,
                    PrimaryText = $"{rank} {memberName} {rankName}",
                };

                AddItemToList(memberItem, sortList: false);
            }
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
                    Debug.WriteLine(dlg.ResponseText);
                    HandleDialogAction(dlg.ResponseText);
                }
            };
            dlg.ShowDialog();
        }

        private void LeaveGuildDialog()
        {
            ClearItemList();
            ListItemType = ListDialogItem.ListItemStyle.Small;
            Buttons = ScrollingListDialogButtons.BackCancel;

            var actions = new List<Action>
            {
                () =>
                {
                    var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.GUILD_PROMPT_LEAVE_GUILD);
                    _GuildActions.LeaveGuild();
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
        // Otherwise Initial dialog will always be empty?
        private void OnMemberListUpdated()
        {
            ClearItemList();
            PopulateMemberList();
        }
    }
}
