using System;
using System.Collections.Generic;
using System.Linq;
using EndlessClient.Content;
using EndlessClient.Dialogs.Factories;
using EndlessClient.Dialogs.Services;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Interact.Guild;
using EOLib.Graphics;
using EOLib.Localization;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Input.InputListeners;
using Optional;
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

            // Management
            Modify,
            ManageRankings,
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

            public static State Management => new(GuildDialogState.Management);

            public static State Modify => new(GuildDialogState.Modify);

            public static State ManageRankings => new(GuildDialogState.ManageRankings);

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
        private readonly IContentProvider _contentProvider;
        private readonly Stack<State> _stateStack;

        private State _state;
        private Option<ListDialogItem> _modifyGuildDescriptionListItem;

        public GuildDialog(INativeGraphicsManager nativeGraphicsManager,
                           IEODialogButtonService dialogButtonService,
                           IEODialogIconService dialogIconService,
                           ILocalizedStringFinder localizedStringFinder,
                           ICharacterProvider characterProvider,
                           IEOMessageBoxFactory messageBoxFactory,
                           IGuildSessionProvider guildSessionProvider,
                           IGuildActions guildActions,
                           ITextInputDialogFactory textInputDialogFactory,
                           IContentProvider contentProvider)
            : base(nativeGraphicsManager, dialogButtonService, DialogType.Guild)
        {
            _dialogIconService = dialogIconService;
            _localizedStringFinder = localizedStringFinder;
            _characterProvider = characterProvider;
            _messageBoxFactory = messageBoxFactory;
            _guildSessionProvider = guildSessionProvider;
            _guildActions = guildActions;
            _textInputDialogFactory = textInputDialogFactory;
            _contentProvider = contentProvider;

            _stateStack = new Stack<State>();
            _modifyGuildDescriptionListItem = Option.None<ListDialogItem>();

            _stateTransitions = new Dictionary<State, Action>
            {
                { State.Initial, SetupInitialState },
                { State.Information, SetupInformationState },
                { State.Management, SetupManagementState },
                { State.Modify, SetupModifyState },
                { State.ManageRankings, () => { } },
                { State.AssignRank, () => { } },
                { State.RemoveMember, () => { } },
                { State.Disband, () => { } },
            };

            SetState(State.Initial);

            BackAction += BackButton_Click;

            Title = _localizedStringFinder.GetString(EOResourceID.GUILD_GUILD_MASTER);
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            if (_state.DialogState == GuildDialogState.Modify)
            {
                _modifyGuildDescriptionListItem.MatchSome(item =>
                {
                    if (item.PrimaryText != _guildSessionProvider.GuildDescription)
                    {
                        item.PrimaryText = _guildSessionProvider.GuildDescription;
                    }
                });
            }

            base.OnUpdateControl(gameTime);
        }

        private void BackButton_Click(object sender, EventArgs e) => GoBack();

        private void GoBack()
        {
            _modifyGuildDescriptionListItem = Option.None<ListDialogItem>();
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

            var administrationItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 1)
            {
                ShowIconBackGround = false,
                IconGraphic = _dialogIconService.IconSheet,
                IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.GuildAdministration),
                PrimaryText = _localizedStringFinder.GetString(EOResourceID.GUILD_ADMINISTRATION),
                SubText = _localizedStringFinder.GetString(EOResourceID.GUILD_JOIN_LEAVE_REGISTER),
                OffsetY = 45,
            };

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
                var dlg = _textInputDialogFactory.Create(_localizedStringFinder.GetString(EOResourceID.GUILD_TO_VIEW_INFORMATION_ABOUT_A_GUILD_ENTER_ITS_TAG), 3);
                dlg.DialogClosing += (_, e) =>
                {
                    if (dlg.ResponseText.Length < 2 && !showOnce && e.Result != XNADialogResult.Cancel)
                    {
                        var invalidGuildTag = _messageBoxFactory.CreateMessageBox(_localizedStringFinder.GetString(DialogResourceID.GUILD_CREATE_TAG_TOO_SHORT), _localizedStringFinder.GetString(DialogResourceID.GUILD_WRONG_INPUT), EODialogButtons.OkCancel);
                        invalidGuildTag.ShowDialog();
                        showOnce = true;
                    }
                };
                dlg.ShowDialog();
            }

            void ViewMembers_Click(object sender, MouseEventArgs e)
            {
                var showOnce = false;
                var dlg = _textInputDialogFactory.Create(_localizedStringFinder.GetString(EOResourceID.GUILD_TO_VIEW_INFORMATION_ABOUT_A_GUILD_ENTER_ITS_TAG), 3);
                dlg.DialogClosing += (_, e) =>
                {
                    if (dlg.ResponseText.Length < 2 && !showOnce && e.Result != XNADialogResult.Cancel)
                    {
                        var invalidGuildTag = _messageBoxFactory.CreateMessageBox(_localizedStringFinder.GetString(DialogResourceID.GUILD_CREATE_TAG_TOO_SHORT), _localizedStringFinder.GetString(DialogResourceID.GUILD_WRONG_INPUT), EODialogButtons.OkCancel);
                        invalidGuildTag.ShowDialog();
                        showOnce = true;
                    }
                };
                dlg.ShowDialog();
            }
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
            modifyGuildItem.LeftClick += (_, _) => SetStateIfGuildMember(State.Modify);
            modifyGuildItem.RightClick += (_, _) => SetStateIfGuildMember(State.Modify);

            var manageRankingItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 0)
            {
                ShowIconBackGround = false,
                IconGraphic = _dialogIconService.IconSheet,
                IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.GuildRanking),
                PrimaryText = _localizedStringFinder.GetString(EOResourceID.GUILD_RANKING),
                SubText = _localizedStringFinder.GetString(EOResourceID.GUILD_MANAGE_MEMBER_RANKINGS),
                OffsetY = 45,
            };
            manageRankingItem.LeftClick += (_, _) => SetStateIfGuildMember(State.ManageRankings);
            manageRankingItem.RightClick += (_, _) => SetStateIfGuildMember(State.ManageRankings);

            var assignRankItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 0)
            {
                ShowIconBackGround = false,
                IconGraphic = _dialogIconService.IconSheet,
                IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.GuildRanking),
                PrimaryText = _localizedStringFinder.GetString(EOResourceID.GUILD_RANKING),
                SubText = _localizedStringFinder.GetString(EOResourceID.GUILD_ASSIGN_RANK_TO_MEMBER),
                OffsetY = 45,
            };
            assignRankItem.LeftClick += (_, _) => SetStateIfGuildMember(State.AssignRank);
            assignRankItem.RightClick += (_, _) => SetStateIfGuildMember(State.AssignRank);

            var removeMemberItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 0)
            {
                ShowIconBackGround = false,
                IconGraphic = _dialogIconService.IconSheet,
                IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.GuildRemoveMember),
                PrimaryText = _localizedStringFinder.GetString(EOResourceID.GUILD_REMOVE_MEMBER),
                SubText = _localizedStringFinder.GetString(EOResourceID.GUILD_REMOVE_A_MEMBER_FROM_GUILD),
                OffsetY = 45,
            };
            removeMemberItem.LeftClick += (_, _) => SetStateIfGuildMember(State.RemoveMember);
            removeMemberItem.RightClick += (_, _) => SetStateIfGuildMember(State.RemoveMember);

            var disbandItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 0)
            {
                ShowIconBackGround = false,
                IconGraphic = _dialogIconService.IconSheet,
                IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.GuildDisband),
                PrimaryText = _localizedStringFinder.GetString(EOResourceID.GUILD_DISBAND),
                SubText = _localizedStringFinder.GetString(EOResourceID.GUILD_DISBAND_YOUR_GUILD),
                OffsetY = 45,
            };
            disbandItem.LeftClick += (_, _) => SetStateIfGuildMember(State.Disband);
            disbandItem.RightClick += (_, _) => SetStateIfGuildMember(State.Disband);

            SetItemList(new List<ListDialogItem> { modifyGuildItem, manageRankingItem, assignRankItem, removeMemberItem, disbandItem });
        }

        private void SetupModifyState()
        {
            _guildActions.GetGuildDescription(_characterProvider.MainCharacter.GuildTag);

            AddTextAsListItems(
                _contentProvider.Fonts[Constants.FontSize08pt5],
                insertLineBreaks: false,
                new List<Action> { ShowChangeDescriptionMessageBox },
                _localizedStringFinder.GetString(EOResourceID.GUILD_CURRENT_DESCRIPTION),
                _guildSessionProvider.GuildDescription,
                " ",
                _localizedStringFinder.GetString(EOResourceID.GUILD_CLICK_HERE_TO_CHANGE_THE_DESCRIPTION)
            );

            _modifyGuildDescriptionListItem = Option.Some(ChildControls.OfType<ListDialogItem>().ToList()[1]);

            void ShowChangeDescriptionMessageBox()
            {
                var dlg = _textInputDialogFactory.Create(_localizedStringFinder.GetString(EOResourceID.GUILD_DESCRIPTION), 240);
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

        private void SetStateIfGuildMember(State state)
        {
            if (!_characterProvider.MainCharacter.InGuild)
            {
                var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.GUILD_NOT_IN_GUILD);
                dlg.ShowDialog();
                return;
            }

            SetState(state);
        }
    }
}
