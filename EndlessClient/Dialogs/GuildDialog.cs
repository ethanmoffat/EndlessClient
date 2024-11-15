using System;
using System.Collections.Generic;
using System.Linq;
using EndlessClient.Audio;
using EndlessClient.Content;
using EndlessClient.Dialogs.Factories;
using EndlessClient.Dialogs.Services;
using EOLib.Domain.Character;
using EOLib.Domain.Interact.Guild;
using EOLib.Extensions;
using EOLib.Graphics;
using EOLib.IO.Repositories;
using EOLib.Localization;
using EOLib.Shared;
using Microsoft.Xna.Framework;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using MonoGame.Extended.Input.InputListeners;
using Optional;
using Optional.Collections;
using XNAControls;

namespace EndlessClient.Dialogs
{
    public class GuildDialog : ScrollingListDialog
    {
        private enum GuildDialogState
        {
            Initial,

            // Initial List Items
            Information,
            Administration,
            Management,
            BankAccount,

            // Information List Items
            Lookup,
            ViewMembers,

            // Administration List Items
            JoinGuild,
            LeaveGuild,
            RegisterGuild,
            WaitingForMembers,

            // Management List Items
            Modify,
            AssignRank,
            RemoveMember,
            Disband,
        }

        private class State
        {
            public GuildDialogState DialogState { get; }
            public ListDialogItem.ListItemStyle ListItemStyle { get; } = ListDialogItem.ListItemStyle.Large;
            public ScrollingListDialogButtons Buttons { get; } = ScrollingListDialogButtons.BackCancel;

            public static State Initial => new(GuildDialogState.Initial);

            public static State Information => new(GuildDialogState.Information);

            public static State Administration => new(GuildDialogState.Administration);

            public static State Management => new(GuildDialogState.Management);

            public static State BankAccount => new(GuildDialogState.BankAccount);

            public static State GuildLookup => new(GuildDialogState.Lookup);

            public static State ViewMembers => new(GuildDialogState.ViewMembers);

            public static State JoinGuild => new(GuildDialogState.JoinGuild);

            public static State LeaveGuild => new(GuildDialogState.LeaveGuild);

            public static State RegisterGuild => new(GuildDialogState.RegisterGuild);

            public static State WaitingForMembers => new(GuildDialogState.WaitingForMembers);

            public static State Modify => new(GuildDialogState.Modify);

            public static State AssignRank => new(GuildDialogState.AssignRank);

            public static State RemoveMember => new(GuildDialogState.RemoveMember);

            public static State Disband => new(GuildDialogState.Disband);

            private State(GuildDialogState dialogState)
            {
                DialogState = dialogState;
                switch (dialogState)
                {
                    case GuildDialogState.Initial:
                        Buttons = ScrollingListDialogButtons.Cancel;
                        break;
                    case GuildDialogState.Modify:
                    case GuildDialogState.Lookup:
                    case GuildDialogState.ViewMembers:
                    case GuildDialogState.JoinGuild:
                    case GuildDialogState.LeaveGuild:
                    case GuildDialogState.RegisterGuild:
                    case GuildDialogState.WaitingForMembers:
                    case GuildDialogState.RemoveMember:
                    case GuildDialogState.BankAccount:
                    case GuildDialogState.AssignRank:
                        ListItemStyle = ListDialogItem.ListItemStyle.Small;
                        break;
                }
            }

            public override bool Equals(object obj)
            {
                return obj is State st && st.DialogState == DialogState;
            }

            public override int GetHashCode()
            {
                return DialogState.GetHashCode();
            }

            public override string ToString() => $"{DialogState}";
        }

        private readonly IReadOnlyDictionary<State, Action> _stateTransitions;

        private readonly IEODialogIconService _dialogIconService;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly ICharacterProvider _characterProvider;
        private readonly IEOMessageBoxFactory _messageBoxFactory;
        private readonly IGuildSessionProvider _guildSessionProvider;
        private readonly IGuildActions _guildActions;
        private readonly ITextInputDialogFactory _textInputDialogFactory;
        private readonly ITextMultiInputDialogFactory _textMultiInputDialogFactory;
        private readonly IItemTransferDialogFactory _itemTransferDialogFactory;
        private readonly IContentProvider _contentProvider;
        private readonly ICharacterInventoryProvider _characterInventoryProvider;
        private readonly IEIFFileProvider _eifFileProvider;
        private readonly ISfxPlayer _sfxPlayer;
        private readonly Stack<State> _stateStack;

        private State _state;

        private HashSet<GuildMember> _cachedMembers;
        private HashSet<string> _cachedCreationMembers;
        private Option<GuildInfo> _cachedGuildInfo;
        private Option<ListDialogItem> _modifyGuildDescriptionListItem, _guildBalanceListItem;
        private int _lastGuildBalance;

        public GuildDialog(INativeGraphicsManager nativeGraphicsManager,
                           IEODialogButtonService dialogButtonService,
                           IEODialogIconService dialogIconService,
                           ILocalizedStringFinder localizedStringFinder,
                           ICharacterProvider characterProvider,
                           IEOMessageBoxFactory messageBoxFactory,
                           IGuildSessionProvider guildSessionProvider,
                           IGuildActions guildActions,
                           ITextInputDialogFactory textInputDialogFactory,
                           ITextMultiInputDialogFactory textMultiInputDialogFactory,
                           IItemTransferDialogFactory itemTransferDialogFactory,
                           IContentProvider contentProvider,
                           ICharacterInventoryProvider characterInventoryProvider,
                           IEIFFileProvider eifFileProvider,
                           ISfxPlayer sfxPlayer)
            : base(nativeGraphicsManager, dialogButtonService, DialogType.Guild)
        {
            _dialogIconService = dialogIconService;
            _localizedStringFinder = localizedStringFinder;
            _characterProvider = characterProvider;
            _messageBoxFactory = messageBoxFactory;
            _guildSessionProvider = guildSessionProvider;
            _guildActions = guildActions;
            _textInputDialogFactory = textInputDialogFactory;
            _textMultiInputDialogFactory = textMultiInputDialogFactory;
            _itemTransferDialogFactory = itemTransferDialogFactory;
            _contentProvider = contentProvider;
            _characterInventoryProvider = characterInventoryProvider;
            _eifFileProvider = eifFileProvider;
            _sfxPlayer = sfxPlayer;

            _stateStack = new Stack<State>();
            _cachedMembers = new HashSet<GuildMember>();
            _cachedCreationMembers = new HashSet<string>();

            _stateTransitions = new Dictionary<State, Action>
            {
                { State.Initial, SetupInitialState },

                // top-level state transitions
                { State.Information, SetupInformationState },
                { State.Administration, SetupAdministrationState },
                { State.Management, SetupManagementState },
                { State.BankAccount, SetupBankAccountState },

                // Information state transitions
                // (none: handled elsewhere)
                // TODO: investigate if they can be moved here

                // Administration state transitions
                { State.JoinGuild, SetupJoinGuildState },
                { State.LeaveGuild, SetupLeaveGuildState },
                { State.RegisterGuild, SetupRegisterGuildState },
                { State.WaitingForMembers, SetupWaitingForMembersState },

                // Management state transitions
                { State.Modify, SetupModifyState },
                { State.AssignRank, SetupAssignRankState },
                { State.RemoveMember, SetupRemoveMemberState },
                { State.Disband, SetupDisbandState },
            };

            SetState(State.Initial);

            BackAction += BackButton_Click;

            Title = _localizedStringFinder.GetString(EOResourceID.GUILD_GUILD_MASTER);
        }

        protected override void OnUnconditionalUpdateControl(GameTime gameTime)
        {
            switch (_state.DialogState)
            {
                case GuildDialogState.Modify:
                    _modifyGuildDescriptionListItem.MatchSome(item =>
                    {
                        if (item.PrimaryText != _guildSessionProvider.GuildDescription)
                        {
                            item.PrimaryText = _guildSessionProvider.GuildDescription;
                        }
                    });
                    break;

                case GuildDialogState.Information:
                    _cachedGuildInfo.Match(
                        some: cachedGuildInfo =>
                        {
                            _guildSessionProvider.GuildInfo.MatchSome(
                                some: repoGuildInfo =>
                                {
                                    if (cachedGuildInfo.Equals(repoGuildInfo))
                                        return;
                                    CacheAndSetGuildInfo(repoGuildInfo);
                                }
                            );
                        },
                        none: () => _guildSessionProvider.GuildInfo.MatchSome(CacheAndSetGuildInfo)
                    );

                    if (!_cachedMembers.SetEquals(_guildSessionProvider.GuildMembers))
                    {
                        SetState(State.ViewMembers);
                        ClearItemList();

                        _cachedMembers = _guildSessionProvider.GuildMembers.ToHashSet();
                        AddTextAsKeyValueListItems(
                            _cachedMembers.Select(x => ($"{x.Rank}  {x.Name.Capitalize()}", x.RankName.Capitalize())).ToArray()
                        );
                    }
                    break;

                case GuildDialogState.RegisterGuild:
                    _guildSessionProvider.CreationSession.MatchSome(creationSession =>
                    {
                        if (creationSession.Approved)
                        {
                            SetState(State.WaitingForMembers);
                        }
                    });
                    break;

                case GuildDialogState.WaitingForMembers:
                    _guildSessionProvider.CreationSession.MatchSome(creationSession =>
                    {
                        if (!_cachedCreationMembers.SetEquals(creationSession.Members))
                        {
                            foreach (var member in creationSession.Members.Where(c => !_cachedCreationMembers.Contains(c)))
                            {
                                AddItemToList(new ListDialogItem(this, ListDialogItem.ListItemStyle.Small)
                                {
                                    PrimaryText = member
                                },
                                sortList: false);
                            }
                            _cachedCreationMembers = creationSession.Members.ToHashSet();
                        }
                    });
                    break;

                case GuildDialogState.BankAccount:
                    if (_lastGuildBalance != _guildSessionProvider.GuildBalance)
                    {
                        _guildBalanceListItem.MatchSome(item =>
                        {
                            item.PrimaryText = $"{_localizedStringFinder.GetString(EOResourceID.GUILD_BANK_STATUS)} {_guildSessionProvider.GuildBalance}";
                            _lastGuildBalance = _guildSessionProvider.GuildBalance;
                        });
                    }
                    break;
            }

            base.OnUnconditionalUpdateControl(gameTime);

            void CacheAndSetGuildInfo(GuildInfo guildInfo)
            {
                SetState(State.Information);

                _cachedGuildInfo = Option.Some(guildInfo);

                ClearItemList();

                AddTextAsListItems(
                    _contentProvider.Fonts[Constants.FontSize08pt5],
                    false,
                    new List<Action>(),
                    $"{guildInfo.Name} [{guildInfo.Tag}]",
                    " ",
                    _localizedStringFinder.GetString(EOResourceID.GUILD_SIGNUP_DATE),
                    guildInfo.CreateDate.ToString("yyyy/MM/dd"),
                    " ",
                    _localizedStringFinder.GetString(EOResourceID.GUILD_GUILD_DESCRIPTION),
                    guildInfo.Description,
                    " ",
                    _localizedStringFinder.GetString(EOResourceID.GUILD_BANK_STATUS),
                    guildInfo.Wealth,
                    " ",
                    _localizedStringFinder.GetString(EOResourceID.GUILD_RANKING_SYSTEM),
                    string.Join("\n", guildInfo.Ranks.Select((x, n) => $"{n + 1}  {x.Capitalize()}")),
                    " ",
                    _localizedStringFinder.GetString(EOResourceID.GUILD_LEADERS),
                    string.Join("\n", guildInfo.Staff.Select(x => $"{x.Name.Capitalize()}{(x.Rank == 0 ? " (founder)" : string.Empty)}")),
                    " "
                );
            }
        }

        private void BackButton_Click(object sender, EventArgs e) => GoBack();

        private void GoBack()
        {
            _guildActions.ClearLocalState();

            _cachedMembers.Clear();
            _cachedGuildInfo = Option.None<GuildInfo>();

            _modifyGuildDescriptionListItem = Option.None<ListDialogItem>();
            _guildBalanceListItem = Option.None<ListDialogItem>();
            _lastGuildBalance = 0;

            SetState(_stateStack.Count > 0 ? _stateStack.Pop() : State.Initial, pushState: false);
        }

        private void SetState(State newState, bool pushState = true)
        {
            ClearItemList();
            if (pushState && _state != newState && _state != null)
            {
                _stateStack.Push(_state);
            }

            _state = newState;

            ListItemType = _state.ListItemStyle;
            Buttons = _state.Buttons;

            if (_stateTransitions.ContainsKey(_state))
                _stateTransitions[_state].Invoke();
        }

        private void SetupInitialState()
        {
            var informationItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 0)
            {
                ShowIconBackGround = false,
                IconGraphic = _dialogIconService.IconSheet,
                IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.GuildInformation),
                PrimaryText = _localizedStringFinder.GetString(EOResourceID.GUILD_INFORMATION),
                SubText = _localizedStringFinder.GetString(EOResourceID.GUILD_LEARN_MORE),
                OffsetY = 45,
            };
            informationItem.LeftClick += (_, _) => SetState(State.Information);
            informationItem.RightClick += (_, _) => SetState(State.Information);

            var administrationItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 1)
            {
                ShowIconBackGround = false,
                IconGraphic = _dialogIconService.IconSheet,
                IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.GuildAdministration),
                PrimaryText = _localizedStringFinder.GetString(EOResourceID.GUILD_ADMINISTRATION),
                SubText = _localizedStringFinder.GetString(EOResourceID.GUILD_JOIN_LEAVE_REGISTER),
                OffsetY = 45,
            };
            administrationItem.LeftClick += (_, _) => SetState(State.Administration);
            administrationItem.RightClick += (_, _) => SetState(State.Administration);

            var managementItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 2)
            {
                ShowIconBackGround = false,
                IconGraphic = _dialogIconService.IconSheet,
                IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.GuildManagement),
                PrimaryText = _localizedStringFinder.GetString(EOResourceID.GUILD_MANAGEMENT),
                SubText = _localizedStringFinder.GetString(EOResourceID.GUILD_MODIFY_RANKING_DISBAND),
                OffsetY = 45,
            };
            managementItem.LeftClick += (_, _) => SetState(State.Management);
            managementItem.RightClick += (_, _) => SetState(State.Management);

            var bankAccountItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 3)
            {
                ShowIconBackGround = false,
                IconGraphic = _dialogIconService.IconSheet,
                IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.GuildBankAccount),
                PrimaryText = _localizedStringFinder.GetString(EOResourceID.GUILD_BANK_ACCOUNT),
                SubText = _localizedStringFinder.GetString(EOResourceID.GUILD_DEPOSIT_TO_GUILD_ACCOUNT),
                OffsetY = 45,
            };
            bankAccountItem.LeftClick += (_, _) => SetStateIfInGuild(State.BankAccount);
            bankAccountItem.RightClick += (_, _) => SetStateIfInGuild(State.BankAccount);

            SetItemList(new List<ListDialogItem> { informationItem, administrationItem, managementItem, bankAccountItem });
        }

        private void SetupInformationState()
        {
            var guildLookup = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 0)
            {
                ShowIconBackGround = false,
                IconGraphic = _dialogIconService.IconSheet,
                IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.GuildLookup),
                PrimaryText = _localizedStringFinder.GetString(EOResourceID.GUILD_LOOK_UP),
                SubText = _localizedStringFinder.GetString(EOResourceID.GUILD_VIEW_DETAILS),
                OffsetY = 45,
            };
            guildLookup.LeftClick += GuildLookup_Click;
            guildLookup.RightClick += GuildLookup_Click;

            var viewMembers = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 0)
            {
                ShowIconBackGround = false,
                IconGraphic = _dialogIconService.IconSheet,
                IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.GuildLookup),
                PrimaryText = _localizedStringFinder.GetString(EOResourceID.GUILD_MEMBERLIST),
                SubText = _localizedStringFinder.GetString(EOResourceID.GUILD_VIEW_MEMBERS),
                OffsetY = 45,
            };
            viewMembers.LeftClick += ViewMembers_Click;
            viewMembers.RightClick += ViewMembers_Click;

            SetItemList(new List<ListDialogItem> { guildLookup, viewMembers });

            void GuildLookup_Click(object sender, MouseEventArgs e)
            {
                var showOnce = false;
                var dlg = _textInputDialogFactory.Create(_localizedStringFinder.GetString(EOResourceID.GUILD_TO_VIEW_INFORMATION_ABOUT_A_GUILD_ENTER_ITS_TAG), maxInputChars: 3, upperCase: true);
                dlg.DialogClosing += (_, e) =>
                {
                    if (e.Result != XNADialogResult.OK)
                        return;

                    if (dlg.ResponseText.Length < 2 && !showOnce)
                    {
                        var invalidGuildTag = _messageBoxFactory.CreateMessageBox(DialogResourceID.GUILD_CREATE_TAG_TOO_SHORT);
                        invalidGuildTag.ShowDialog();
                        showOnce = true;
                    }
                    else
                    {
                        _guildActions.Lookup(dlg.ResponseText);
                    }
                };
                dlg.ShowDialog();
            }

            void ViewMembers_Click(object sender, MouseEventArgs e)
            {
                var showOnce = false;
                var dlg = _textInputDialogFactory.Create(_localizedStringFinder.GetString(EOResourceID.GUILD_TO_VIEW_INFORMATION_ABOUT_A_GUILD_ENTER_ITS_TAG), maxInputChars: 3, upperCase: true);
                dlg.DialogClosing += (_, e) =>
                {
                    if (e.Result != XNADialogResult.OK)
                        return;

                    if (dlg.ResponseText.Length < 2 && !showOnce)
                    {
                        var invalidGuildTag = _messageBoxFactory.CreateMessageBox(DialogResourceID.GUILD_CREATE_TAG_TOO_SHORT);
                        invalidGuildTag.ShowDialog();
                        showOnce = true;
                    }
                    else
                    {
                        _guildActions.ViewMembers(dlg.ResponseText);
                    }
                };
                dlg.ShowDialog();
            }
        }

        private void SetupAdministrationState()
        {
            var joinGuildItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 0)
            {
                ShowIconBackGround = false,
                IconGraphic = _dialogIconService.IconSheet,
                IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.GuildJoin),
                PrimaryText = _localizedStringFinder.GetString(EOResourceID.GUILD_JOIN_GUILD),
                SubText = _localizedStringFinder.GetString(EOResourceID.GUILD_JOIN_AN_EXISTING_GUILD),
                OffsetY = 45,
            };
            joinGuildItem.LeftClick += (_, _) => SetStateIfNotInGuild(State.JoinGuild);
            joinGuildItem.RightClick += (_, _) => SetStateIfNotInGuild(State.JoinGuild);

            var leaveGuildItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 1)
            {
                ShowIconBackGround = false,
                IconGraphic = _dialogIconService.IconSheet,
                IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.GuildLeave),
                PrimaryText = _localizedStringFinder.GetString(EOResourceID.GUILD_LEAVE_GUILD),
                SubText = _localizedStringFinder.GetString(EOResourceID.GUILD_LEAVE_YOUR_CURRENT_GUILD),
                OffsetY = 45,
            };
            leaveGuildItem.LeftClick += (_, _) => SetStateIfInGuild(State.LeaveGuild);
            leaveGuildItem.RightClick += (_, _) => SetStateIfInGuild(State.LeaveGuild);

            var registerGuildItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 2)
            {
                ShowIconBackGround = false,
                IconGraphic = _dialogIconService.IconSheet,
                IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.GuildRegister),
                PrimaryText = _localizedStringFinder.GetString(EOResourceID.GUILD_REGISTER_GUILD),
                SubText = _localizedStringFinder.GetString(EOResourceID.GUILD_CREATE_YOUR_OWN_GUILD),
                OffsetY = 45,
            };
            registerGuildItem.LeftClick += (_, _) => SetStateIfNotInGuild(State.RegisterGuild);
            registerGuildItem.RightClick += (_, _) => SetStateIfNotInGuild(State.RegisterGuild);

            SetItemList(new List<ListDialogItem> { joinGuildItem, leaveGuildItem, registerGuildItem });
        }

        private void SetupManagementState()
        {
            var modifyGuildItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 0)
            {
                ShowIconBackGround = false,
                IconGraphic = _dialogIconService.IconSheet,
                IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.GuildModify),
                PrimaryText = _localizedStringFinder.GetString(EOResourceID.GUILD_MODIFY_GUILD),
                SubText = _localizedStringFinder.GetString(EOResourceID.GUILD_CHANGE_YOUR_GUILD_DETAILS),
                OffsetY = 45,
            };
            modifyGuildItem.LeftClick += (_, _) => SetStateIfLeaderRank(State.Modify);
            modifyGuildItem.RightClick += (_, _) => SetStateIfLeaderRank(State.Modify);

            var manageRankingItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 0)
            {
                ShowIconBackGround = false,
                IconGraphic = _dialogIconService.IconSheet,
                IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.GuildRanking),
                PrimaryText = _localizedStringFinder.GetString(EOResourceID.GUILD_RANKING),
                SubText = _localizedStringFinder.GetString(EOResourceID.GUILD_MANAGE_MEMBER_RANKINGS),
                OffsetY = 45,
            };
            manageRankingItem.LeftClick += (_, _) => ShowManageRankingsDialog();
            manageRankingItem.RightClick += (_, _) => ShowManageRankingsDialog();

            var assignRankItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 0)
            {
                ShowIconBackGround = false,
                IconGraphic = _dialogIconService.IconSheet,
                IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.GuildRanking),
                PrimaryText = _localizedStringFinder.GetString(EOResourceID.GUILD_RANKING),
                SubText = _localizedStringFinder.GetString(EOResourceID.GUILD_ASSIGN_RANK_TO_MEMBER),
                OffsetY = 45,
            };
            assignRankItem.LeftClick += (_, _) => SetStateIfLeaderRank(State.AssignRank);
            assignRankItem.RightClick += (_, _) => SetStateIfLeaderRank(State.AssignRank);

            var removeMemberItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 0)
            {
                ShowIconBackGround = false,
                IconGraphic = _dialogIconService.IconSheet,
                IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.GuildRemoveMember),
                PrimaryText = _localizedStringFinder.GetString(EOResourceID.GUILD_REMOVE_MEMBER),
                SubText = _localizedStringFinder.GetString(EOResourceID.GUILD_REMOVE_A_MEMBER_FROM_GUILD),
                OffsetY = 45,
            };
            removeMemberItem.LeftClick += (_, _) => SetStateIfLeaderRank(State.RemoveMember);
            removeMemberItem.RightClick += (_, _) => SetStateIfLeaderRank(State.RemoveMember);

            var disbandItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 0)
            {
                ShowIconBackGround = false,
                IconGraphic = _dialogIconService.IconSheet,
                IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.GuildDisband),
                PrimaryText = _localizedStringFinder.GetString(EOResourceID.GUILD_DISBAND),
                SubText = _localizedStringFinder.GetString(EOResourceID.GUILD_DISBAND_YOUR_GUILD),
                OffsetY = 45,
            };
            disbandItem.LeftClick += (_, _) => SetStateIfLeaderRank(State.Disband);
            disbandItem.RightClick += (_, _) => SetStateIfLeaderRank(State.Disband);

            SetItemList(new List<ListDialogItem> { modifyGuildItem, manageRankingItem, assignRankItem, removeMemberItem, disbandItem });
        }

        private void SetupBankAccountState()
        {
            _guildActions.GetGuildBankBalance(_characterProvider.MainCharacter.GuildTag);

            AddTextAsListItems(
                _contentProvider.Fonts[Constants.FontSize08pt5],
                insertLineBreaks: true,
                new List<Action> { ShowBankDepositMessageBox },
                $"{_localizedStringFinder.GetString(EOResourceID.GUILD_BANK_STATUS)} {_guildSessionProvider.GuildBalance}",
                _localizedStringFinder.GetString(EOResourceID.GUILD_BANK_DESCRIPTION_1),
                _localizedStringFinder.GetString(EOResourceID.GUILD_BANK_DESCRIPTION_2),
                _localizedStringFinder.GetString(EOResourceID.GUILD_BANK_DESCRIPTION_3)
            );

            _guildBalanceListItem = Option.Some(ChildControls.OfType<ListDialogItem>().ToList()[0]);

            void ShowBankDepositMessageBox()
            {
                var hasEnoughMinimumGold = _characterInventoryProvider.ItemInventory
                    .SingleOrNone(x => x.ItemID == 1)
                    .Match(some: x => x.Amount >= 1000, none: () => false);
                if (!hasEnoughMinimumGold)
                {
                    var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.GUILD_MINIMUM_DEPOSIT_IS_1000);
                    dlg.ShowDialog();
                    return;
                }

                var goldName = _eifFileProvider.EIFFile[1].Name;
                var goldInventoryItem = _characterInventoryProvider.ItemInventory.Single(x => x.ItemID == 1);
                var transferDialog = _itemTransferDialogFactory.CreateItemTransferDialog(goldName, ItemTransferDialog.TransferType.DropItems, goldInventoryItem.Amount, EOResourceID.DIALOG_TRANSFER_DROP);
                transferDialog.DialogClosing += (_, e) =>
                {
                    if (e.Result != XNADialogResult.OK)
                        return;

                    if (transferDialog.SelectedAmount < 1000)
                    {
                        var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.GUILD_MINIMUM_DEPOSIT_IS_1000);
                        dlg.ShowDialog();
                        return;
                    }

                    _guildActions.BankDeposit(transferDialog.SelectedAmount);

                    GoBack();
                };
                transferDialog.ShowDialog();
            }
        }

        private void SetupJoinGuildState()
        {
            AddTextAsListItems(
                _contentProvider.Fonts[Constants.FontSize08pt5],
                insertLineBreaks: true,
                new List<Action> { ShowJoinGuildMessageBox },
                _localizedStringFinder.GetString(EOResourceID.GUILD_JOIN_GUILD),
                _localizedStringFinder.GetString(EOResourceID.GUILD_YOU_ARE_ABOUT_TO_JOIN_A_GUILD),
                _localizedStringFinder.GetString(EOResourceID.GUILD_JOINING_A_GUILD_IS_FREE),
                _localizedStringFinder.GetString(EOResourceID.GUILD_PLEASE_CONSIDER_CAREFULLY),
                _localizedStringFinder.GetString(EOResourceID.GUILD_CLICK_HERE_TO_JOIN_A_GUILD)
            );
        }

        private void SetupLeaveGuildState()
        {
            AddTextAsListItems(
                _contentProvider.Fonts[Constants.FontSize08pt5],
                insertLineBreaks: true,
                new List<Action> { ShowLeaveGuildMessageBox },
                _localizedStringFinder.GetString(EOResourceID.GUILD_LEAVE_GUILD),
                _localizedStringFinder.GetString(EOResourceID.GUILD_YOU_ARE_ABOUT_TO_LEAVE_THE_GUILD),
                _localizedStringFinder.GetString(EOResourceID.GUILD_REMEMBER_THAT_AFTER_YOU_HAVE_LEFT_THE_GUILD),
                _localizedStringFinder.GetString(EOResourceID.GUILD_CLICK_HERE_TO_LEAVE_YOUR_GUILD)
            );
        }

        private void SetupRegisterGuildState()
        {
            AddTextAsListItems(
                _contentProvider.Fonts[Constants.FontSize08pt5],
                insertLineBreaks: true,
                new List<Action> { ShowRegisterGuildMessageBox },
                _localizedStringFinder.GetString(EOResourceID.GUILD_REGISTER_GUILD),
                _localizedStringFinder.GetString(EOResourceID.GUILD_YOU_ARE_ABOUT_TO_CREATE_A_GUILD),
                _localizedStringFinder.GetString(EOResourceID.GUILD_YOU_NEED_TO_HAVE_AT_LEAST_TEN_MEMBERS),
                _localizedStringFinder.GetString(EOResourceID.GUILD_THE_GUILD_MASTER_WILL_ASK_A_FEE),
                _localizedStringFinder.GetString(EOResourceID.GUILD_CLICK_HERE_TO_REGISTER_A_GUILD)
            );
        }

        private void SetupWaitingForMembersState()
        {
            AddTextAsListItems(
                _contentProvider.Fonts[Constants.FontSize08pt5],
                insertLineBreaks: false,
                new List<Action> { },
                _localizedStringFinder.GetString(EOResourceID.GUILD_PLEASE_WAIT_FOR_ALL_MEMBERS_TO_JOIN),
                " ",
                _characterProvider.MainCharacter.Name.Capitalize()
            );
        }

        private void SetupModifyState()
        {
            _guildActions.GetGuildDescription(_characterProvider.MainCharacter.GuildTag);

            AddTextAsListItems(
                _contentProvider.Fonts[Constants.FontSize08pt5],
                insertLineBreaks: false,
                new List<Action> { ShowChangeDescriptionMessageBox },
                _localizedStringFinder.GetString(EOResourceID.GUILD_CURRENT_DESCRIPTION),
                string.IsNullOrEmpty(_guildSessionProvider.GuildDescription) ? " " : _guildSessionProvider.GuildDescription,
                " ",
                _localizedStringFinder.GetString(EOResourceID.GUILD_CLICK_HERE_TO_CHANGE_THE_DESCRIPTION)
            );

            _modifyGuildDescriptionListItem = Option.Some(ChildControls.OfType<ListDialogItem>().ToList()[1]);

            void ShowChangeDescriptionMessageBox()
            {
                var dlg = _textInputDialogFactory.Create(_localizedStringFinder.GetString(EOResourceID.GUILD_WORD_DESCRIPTION), 240);
                dlg.DialogClosing += (_, e) =>
                {
                    if (e.Result == XNADialogResult.OK)
                    {
                        _guildActions.SetGuildDescription(dlg.ResponseText);
                        GoBack();
                    }
                };
                dlg.ShowDialog();
            }
        }

        private void ShowManageRankingsDialog()
        {
            if (!_characterProvider.MainCharacter.InGuild)
            {
                var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.GUILD_NOT_IN_GUILD);
                dlg.ShowDialog();
                return;
            }

            if (_characterProvider.MainCharacter.GuildRankID >= 2)
            {
                var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.GUILD_RANK_TOO_LOW);
                dlg.ShowDialog();
                return;
            }

            // guild ranks dialog pops up on response from server (see GuildEventSubscriber)
            _guildActions.GetGuildRanks(_characterProvider.MainCharacter.GuildTag);
        }

        private void SetupAssignRankState()
        {
            AddTextAsListItems(
                _contentProvider.Fonts[Constants.FontSize08pt5],
                insertLineBreaks: true,
                new List<Action> { ShowAssignRankInputBox },
                _localizedStringFinder.GetString(EOResourceID.GUILD_RANKING),
                _localizedStringFinder.GetString(EOResourceID.GUILD_RANK_DESCRIPTION_1),
                _localizedStringFinder.GetString(EOResourceID.GUILD_RANK_DESCRIPTION_2),
                _localizedStringFinder.GetString(EOResourceID.GUILD_RANK_DESCRIPTION_3)
            );

            void ShowAssignRankInputBox()
            {
                var dlg = _textMultiInputDialogFactory.Create(
                    _localizedStringFinder.GetString(EOResourceID.GUILD_RANKING),
                    _localizedStringFinder.GetString(EOResourceID.GUILD_ASSIGN_RANK_TO_MEMBER),
                    TextMultiInputDialog.DialogSize.Two,
                    new TextMultiInputDialog.InputInfo(_localizedStringFinder.GetString(EOResourceID.GUILD_RANK_ASSIGN_NAME)),
                    new TextMultiInputDialog.InputInfo(
                        _localizedStringFinder.GetString(EOResourceID.GUILD_RANK_ASSIGN_RANK),
                        MaxChars: 1,
                        InputRestriction: TextMultiInputDialog.InputInfo.InputRestrict.Numeric
                    )
                );

                dlg.DialogClosing += (_, e) =>
                {
                    if (e.Result == XNADialogResult.OK)
                    {
                        // The only input validation the official client does is that none of the fields are empty

                        if (dlg.Responses.Any(string.IsNullOrWhiteSpace))
                        {
                            var errorDlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.ACCOUNT_CREATE_FIELDS_STILL_EMPTY);
                            errorDlg.ShowDialog();

                            e.Cancel = true;
                            return;
                        }

                        _guildActions.AssignRank(dlg.Responses[0], int.Parse(dlg.Responses[1]));
                    }
                };
                dlg.ShowDialog();
            }
        }

        private void SetupRemoveMemberState()
        {
            AddTextAsListItems(
                _contentProvider.Fonts[Constants.FontSize08pt5],
                insertLineBreaks: true,
                new List<Action> { ShowRemoveMemberInputBox },
                _localizedStringFinder.GetString(EOResourceID.GUILD_REMOVE_MEMBER),
                _localizedStringFinder.GetString(EOResourceID.GUILD_REMOVE_MEMBER_DESCRIPTION_1),
                _localizedStringFinder.GetString(EOResourceID.GUILD_REMOVE_MEMBER_DESCRIPTION_2),
                _localizedStringFinder.GetString(EOResourceID.GUILD_REMOVE_MEMBER_DESCRIPTION_3)
            );

            void ShowRemoveMemberInputBox()
            {
                var removeMemberInput = _textInputDialogFactory.Create(_localizedStringFinder.GetString(EOResourceID.GUILD_WHO_DO_YOU_WANT_TO_REMOVE));
                removeMemberInput.DialogClosing += (_, e) =>
                {
                    if (e.Result == XNADialogResult.OK)
                    {
                        if (removeMemberInput.ResponseText.Length < 4)
                        {
                            e.Cancel = true;
                            var tooShortDlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.CHARACTER_CREATE_NAME_TOO_SHORT);
                            tooShortDlg.ShowDialog();
                            return;
                        }

                        _guildActions.KickMember(removeMemberInput.ResponseText);
                    }
                };
                removeMemberInput.ShowDialog();
            }
        }

        private void SetupDisbandState()
        {
            AddTextAsListItems(
                _contentProvider.Fonts[Constants.FontSize08pt5],
                insertLineBreaks: true,
                new List<Action> { ShowDisbandGuildConfirmation },
                _localizedStringFinder.GetString(EOResourceID.GUILD_DISBAND),
                _localizedStringFinder.GetString(EOResourceID.GUILD_DISBAND_DESCRIPTION_1),
                _localizedStringFinder.GetString(EOResourceID.GUILD_DISBAND_DESCRIPTION_2),
                _localizedStringFinder.GetString(EOResourceID.GUILD_DISBAND_DESCRIPTION_3)
            );

            void ShowDisbandGuildConfirmation()
            {
                var confirmDlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.GUILD_PROMPT_DISBAND_GUILD, EODialogButtons.OkCancel);
                confirmDlg.DialogClosing += (_, e) =>
                {
                    if (e.Result == XNADialogResult.OK)
                    {
                        _guildActions.DisbandGuild();
                        _sfxPlayer.PlaySfx(SoundEffectID.LeaveGuild);
                    }
                };
                confirmDlg.ShowDialog();
            }
        }

        private void ShowJoinGuildMessageBox()
        {
            if (_characterProvider.MainCharacter.InGuild)
            {
                var dlgAlreadyMember = _messageBoxFactory.CreateMessageBox(DialogResourceID.GUILD_ALREADY_A_MEMBER);
                dlgAlreadyMember.ShowDialog();
                return;
            }

            var dlgJoin = _textMultiInputDialogFactory.Create(
                _localizedStringFinder.GetString(DialogResourceID.GUILD_JOIN_GUILD),
                _localizedStringFinder.GetString(DialogResourceID.GUILD_JOIN_GUILD + 1),
                TextMultiInputDialog.DialogSize.Two,
                new TextMultiInputDialog.InputInfo(_localizedStringFinder.GetString(EOResourceID.GUILD_GUILD_TAG), MaxChars: 3, InputRestriction: TextMultiInputDialog.InputInfo.InputRestrict.Uppercase),
                new TextMultiInputDialog.InputInfo(_localizedStringFinder.GetString(EOResourceID.GUILD_RECRUITER), MaxChars: 12)
            );

            dlgJoin.DialogClosing += (sender, e) =>
            {
                if (e.Result == XNADialogResult.OK)
                {
                    var guildTag = dlgJoin.Responses[0];
                    var recruiterName = dlgJoin.Responses[1];

                    if (string.IsNullOrEmpty(guildTag))
                    {
                        e.Cancel = true;
                        var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.GUILD_CREATE_TAG_FIELD_EMPTY);
                        dlg.ShowDialog();
                        return;
                    }

                    if (string.IsNullOrEmpty(recruiterName))
                    {
                        e.Cancel = true;
                        var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.GUILD_RECRUITER_INPUT_MISSING);
                        dlg.ShowDialog();
                        return;
                    }

                    if (guildTag.Length == 1)
                    {
                        e.Cancel = true;
                        var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.GUILD_CREATE_TAG_TOO_SHORT);
                        dlg.ShowDialog();
                        return;
                    }

                    _guildActions.RequestToJoinGuild(guildTag, recruiterName);
                }
            };

            dlgJoin.ShowDialog();
        }

        private void ShowLeaveGuildMessageBox()
        {
            if (!_characterProvider.MainCharacter.InGuild)
            {
                var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.GUILD_NOT_IN_GUILD);
                dlg.ShowDialog();
                return;
            }

            var dlgLeave = _messageBoxFactory.CreateMessageBox(DialogResourceID.GUILD_PROMPT_LEAVE_GUILD, whichButtons: EODialogButtons.OkCancel);
            dlgLeave.DialogClosing += (_, e) =>
            {
                if (e.Result == XNADialogResult.OK)
                {
                    _sfxPlayer.PlaySfx(SoundEffectID.LeaveGuild);
                    _guildActions.LeaveGuild();
                }
            };
            dlgLeave.ShowDialog();
        }

        private void ShowRegisterGuildMessageBox()
        {
            if (_characterProvider.MainCharacter.InGuild)
            {
                var dlgAlreadyMember = _messageBoxFactory.CreateMessageBox(DialogResourceID.GUILD_ALREADY_A_MEMBER);
                dlgAlreadyMember.ShowDialog();
                return;
            }

            var dlgRegister = _textMultiInputDialogFactory.Create(
                _localizedStringFinder.GetString(EOResourceID.GUILD_REGISTER_GUILD),
                _localizedStringFinder.GetString(EOResourceID.GUILD_ENTER_YOUR_GUILD_DETAILS),
                TextMultiInputDialog.DialogSize.Three,
                new TextMultiInputDialog.InputInfo(_localizedStringFinder.GetString(EOResourceID.GUILD_GUILD_TAG), MaxChars: 3, InputRestriction: TextMultiInputDialog.InputInfo.InputRestrict.Uppercase),
                new TextMultiInputDialog.InputInfo(_localizedStringFinder.GetString(EOResourceID.GUILD_GUILD_NAME), MaxChars: 24),
                new TextMultiInputDialog.InputInfo(_localizedStringFinder.GetString(EOResourceID.GUILD_WORD_DESCRIPTION), MaxChars: 240)
            );

            dlgRegister.DialogClosing += (_, e) =>
            {
                if (e.Result == XNADialogResult.OK)
                {
                    var guildTag = dlgRegister.Responses[0];
                    var guildName = dlgRegister.Responses[1];
                    var guildDescription = dlgRegister.Responses[2];

                    if (!_characterInventoryProvider.ItemInventory.Any(x => x.ItemID == 1 && x.Amount >= 50_000))
                    {
                        var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.WARNING_YOU_HAVE_NOT_ENOUGH, $" {_eifFileProvider.EIFFile[1].Name}");
                        dlg.ShowDialog();
                        return;
                    }

                    if (string.IsNullOrEmpty(guildTag))
                    {
                        e.Cancel = true;
                        var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.GUILD_CREATE_TAG_FIELD_EMPTY);
                        dlg.ShowDialog();
                        return;
                    }

                    if (string.IsNullOrEmpty(guildName))
                    {
                        e.Cancel = true;
                        var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.GUILD_CREATE_NAME_FIELD_EMPTY);
                        dlg.ShowDialog();
                        return;
                    }

                    if (guildTag.Length == 1)
                    {
                        e.Cancel = true;
                        var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.GUILD_CREATE_TAG_TOO_SHORT);
                        dlg.ShowDialog();
                        return;
                    }

                    if (guildName.Length <= 3)
                    {
                        e.Cancel = true;
                        var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.GUILD_CREATE_NAME_TOO_SHORT);
                        dlg.ShowDialog();
                        return;
                    }

                    if (char.ToLower(guildTag[0]) != char.ToLower(guildName[0]))
                    {
                        e.Cancel = true;
                        var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.GUILD_TAG_NAME_LETTER_MUST_MATCH);
                        dlg.ShowDialog();
                        return;
                    }

                    _guildActions.RequestToCreateGuild(guildTag, guildName, guildDescription);
                }
            };

            dlgRegister.ShowDialog();
        }

        private void SetStateIfInGuild(State state)
        {
            if (!_characterProvider.MainCharacter.InGuild)
            {
                var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.GUILD_NOT_IN_GUILD);
                dlg.ShowDialog();
                return;
            }

            SetState(state);
        }

        private void SetStateIfLeaderRank(State state)
        {
            if (!_characterProvider.MainCharacter.InGuild)
            {
                var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.GUILD_NOT_IN_GUILD);
                dlg.ShowDialog();
                return;
            }

            if (_characterProvider.MainCharacter.GuildRankID >= 2)
            {
                var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.GUILD_RANK_TOO_LOW);
                dlg.ShowDialog();
                return;
            }

            SetState(state);
        }

        private void SetStateIfNotInGuild(State state)
        {
            if (_characterProvider.MainCharacter.InGuild)
            {
                var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.GUILD_ALREADY_A_MEMBER);
                dlg.ShowDialog();
                return;
            }

            SetState(state);
        }
    }
}
