using EndlessClient.Dialogs.Factories;
using EndlessClient.Dialogs.Services;
using EOLib.Domain.Character;
using EOLib.Domain.Interact.Guild;
using EOLib.Graphics;
using EOLib.Localization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Optional;
using System.Collections.Generic;
using XNAControls;

namespace EndlessClient.Dialogs
{
    public class GuildDialog : ScrollingListDialog
    {
        private enum GuildDialogState
        {
            Initial,
            Management,
            Modify,
            ManageRankings,
            AssignRank,
            RemoveMember,
            Disband,
        }

        private class State
        {
            public GuildDialogState DialogState { get; }
            public ListDialogItem.ListItemStyle ListItemStyle { get; }
            public ScrollingListDialogButtons Buttons { get; }

            public State(GuildDialogState dialogState)
            {
                DialogState = dialogState;
                switch (dialogState)
                {
                    case GuildDialogState.Initial:
                        ListItemStyle = ListDialogItem.ListItemStyle.Large;
                        Buttons = ScrollingListDialogButtons.Cancel;
                        break;
                    case GuildDialogState.Management:
                        ListItemStyle = ListDialogItem.ListItemStyle.Large;
                        Buttons = ScrollingListDialogButtons.BackCancel;
                        break;
                    case GuildDialogState.Modify:
                        ListItemStyle = ListDialogItem.ListItemStyle.Small;
                        Buttons = ScrollingListDialogButtons.BackCancel;
                        break;
                }
            }
        }

        private readonly IEODialogIconService _dialogIconService;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly ICharacterProvider _characterProvider;
        private readonly IEOMessageBoxFactory _messageBoxFactory;
        private readonly IGuildSessionProvider _guildSessionProvider;
        private readonly IGuildActions _guildActions;
        private readonly ITextInputDialogFactory _textInputDialogFactory;

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
                         ITextInputDialogFactory textInputDialogFactory)
            : base(nativeGraphicsManager, dialogButtonService, DialogType.Guild)
        {
            _dialogIconService = dialogIconService;
            _localizedStringFinder = localizedStringFinder;
            _characterProvider = characterProvider;
            _messageBoxFactory = messageBoxFactory;
            _guildSessionProvider = guildSessionProvider;
            _guildActions = guildActions;
            _textInputDialogFactory = textInputDialogFactory;
            _stateStack = new Stack<State>();
            _modifyGuildDescriptionListItem = Option.None<ListDialogItem>();


            SetState(new State(GuildDialogState.Initial));

            BackAction += (_, _) =>
            {
                _modifyGuildDescriptionListItem = Option.None<ListDialogItem>();

                if (_stateStack.Count > 0)
                {
                    var previousState = _stateStack.Pop();
                    SetState(previousState);
                }
                else
                {
                    SetState(new State(GuildDialogState.Initial));
                }
            };

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

        private void SetState(State newState)
        {
            if (_state != newState)
            {
                ClearItemList();
                _stateStack.Push(_state);
            }

            _state = newState;
            ClearItemList();

            ListItemType = _state.ListItemStyle;
            Buttons = _state.Buttons;

            switch (_state.DialogState)
            {
                case GuildDialogState.Initial:
                    SetupInitialState();
                    break;

                case GuildDialogState.Management:
                    SetupManagementState();
                    break;
                case GuildDialogState.Modify:
                    SetupModifyState();
                    break;
            }
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

            AddItemToList(informationItem, sortList: false);

            var administrationItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 1)
            {
                ShowIconBackGround = false,
                IconGraphic = _dialogIconService.IconSheet,
                IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.GuildAdministration),
                PrimaryText = _localizedStringFinder.GetString(EOResourceID.GUILD_ADMINISTRATION),
                SubText = _localizedStringFinder.GetString(EOResourceID.GUILD_JOIN_LEAVE_REGISTER),
                OffsetY = 45,
            };

            AddItemToList(administrationItem, sortList: false);

            var managementItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 2)
            {
                ShowIconBackGround = false,
                IconGraphic = _dialogIconService.IconSheet,
                IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.GuildManagement),
                PrimaryText = _localizedStringFinder.GetString(EOResourceID.GUILD_MANAGEMENT),
                SubText = _localizedStringFinder.GetString(EOResourceID.GUILD_MODIFY_RANKING_DISBAND),
                OffsetY = 45,
            };
            managementItem.LeftClick += (_, _) => SetState(new State(GuildDialogState.Management));
            managementItem.RightClick += (_, _) => SetState(new State(GuildDialogState.Management));

            AddItemToList(managementItem, sortList: false);

            var bankAccountItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 3)
            {
                ShowIconBackGround = false,
                IconGraphic = _dialogIconService.IconSheet,
                IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.GuildBankAccount),
                PrimaryText = _localizedStringFinder.GetString(EOResourceID.GUILD_BANK_ACCOUNT),
                SubText = _localizedStringFinder.GetString(EOResourceID.GUILD_DEPOSIT_TO_GUILD_ACCOUNT),
                OffsetY = 45,
            };

            AddItemToList(bankAccountItem, sortList: false);
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

            modifyGuildItem.LeftClick += (_, _) =>
            {
                SetStateIfGuildMember(new State(GuildDialogState.Modify));
            };

            modifyGuildItem.RightClick += (_, _) =>
            {
                SetStateIfGuildMember(new State(GuildDialogState.Modify));
            };

            AddItemToList(modifyGuildItem, sortList: false);

            var manageRankingItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 0)
            {
                ShowIconBackGround = false,
                IconGraphic = _dialogIconService.IconSheet,
                IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.GuildRanking),
                PrimaryText = _localizedStringFinder.GetString(EOResourceID.GUILD_RANKING),
                SubText = _localizedStringFinder.GetString(EOResourceID.GUILD_MANAGE_MEMBER_RANKINGS),
                OffsetY = 45,
            };

            manageRankingItem.LeftClick += (_, _) => SetStateIfGuildMember(new State(GuildDialogState.ManageRankings));
            manageRankingItem.RightClick += (_, _) => SetStateIfGuildMember(new State(GuildDialogState.ManageRankings));

            AddItemToList(manageRankingItem, sortList: false);

            var assignRankItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 0)
            {
                ShowIconBackGround = false,
                IconGraphic = _dialogIconService.IconSheet,
                IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.GuildRanking),
                PrimaryText = _localizedStringFinder.GetString(EOResourceID.GUILD_RANKING),
                SubText = _localizedStringFinder.GetString(EOResourceID.GUILD_ASSIGN_RANK_TO_MEMBER),
                OffsetY = 45,
            };

            assignRankItem.LeftClick += (_, _) => SetStateIfGuildMember(new State(GuildDialogState.AssignRank));
            assignRankItem.RightClick += (_, _) => SetStateIfGuildMember(new State(GuildDialogState.AssignRank));

            AddItemToList(assignRankItem, sortList: false);

            var removeMemberItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 0)
            {
                ShowIconBackGround = false,
                IconGraphic = _dialogIconService.IconSheet,
                IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.GuildRemoveMember),
                PrimaryText = _localizedStringFinder.GetString(EOResourceID.GUILD_REMOVE_MEMBER),
                SubText = _localizedStringFinder.GetString(EOResourceID.GUILD_REMOVE_A_MEMBER_FROM_GUILD),
                OffsetY = 45,
            };

            removeMemberItem.LeftClick += (_, _) => SetStateIfGuildMember(new State(GuildDialogState.RemoveMember));
            removeMemberItem.RightClick += (_, _) => SetStateIfGuildMember(new State(GuildDialogState.RemoveMember));

            AddItemToList(removeMemberItem, sortList: false);

            var disbandItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 0)
            {
                ShowIconBackGround = false,
                IconGraphic = _dialogIconService.IconSheet,
                IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.GuildDisband),
                PrimaryText = _localizedStringFinder.GetString(EOResourceID.GUILD_DISBAND),
                SubText = _localizedStringFinder.GetString(EOResourceID.GUILD_DISBAND_YOUR_GUILD),
                OffsetY = 45,
            };

            disbandItem.LeftClick += (_, _) => SetStateIfGuildMember(new State(GuildDialogState.Disband));
            disbandItem.RightClick += (_, _) => SetStateIfGuildMember(new State(GuildDialogState.Disband));

            AddItemToList(disbandItem, sortList: false);
        }

        private void SetupModifyState()
        {
            _guildActions.GetGuildDescription(_characterProvider.MainCharacter.GuildTag);

            AddItemToList(new ListDialogItem(this, ListDialogItem.ListItemStyle.Small, 0)
            {
                PrimaryText = _localizedStringFinder.GetString(EOResourceID.GUILD_CURRENT_DESCRIPTION),
            }, sortList: false);

            var descriptionListItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Small, 0)
            {
                PrimaryText = _guildSessionProvider.GuildDescription
            };

            _modifyGuildDescriptionListItem = Option.Some(descriptionListItem);
            AddItemToList(descriptionListItem, sortList: false);

            AddItemToList(new ListDialogItem(this, ListDialogItem.ListItemStyle.Small, 0), sortList: false);

            var changeItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Small, 0)
            {
                PrimaryText = _localizedStringFinder.GetString(EOResourceID.GUILD_CLICK_HERE_TO_CHANGE_THE_DESCRIPTION)[1..],
                UnderlineLinks = true,
            };

            changeItem.SetPrimaryClickAction((_, _) => ShowChangeDescriptionMessageBox());
            changeItem.RightClick += (_, _) => ShowChangeDescriptionMessageBox();

            AddItemToList(changeItem, sortList: false);
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

        private void ShowChangeDescriptionMessageBox()
        {
            var dlg = _textInputDialogFactory.Create(_localizedStringFinder.GetString(EOResourceID.GUILD_DESCRIPTION), 240);
            dlg.DialogClosing += (_, e) =>
            {
                if (e.Result == XNADialogResult.OK)
                {
                    _guildActions.SetGuildDescription(dlg.ResponseText);
                    SetState(new State(GuildDialogState.Management));
                }
            };
            dlg.Show();
        }
    }
}
