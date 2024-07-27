using EndlessClient.Dialogs.Factories;
using EndlessClient.Dialogs.Services;
using EOLib.Domain.Character;
using EOLib.Domain.Interact.Guild;
using EOLib.Graphics;
using EOLib.Localization;
using Microsoft.Xna.Framework;
using XNAControls;

namespace EndlessClient.Dialogs
{
    public class GuildDialog : ScrollingListDialog
    {
        private enum GuildDialogState
        {
            // initial menu
            Initial,
            Management,
            Modify,
            ManageRankings,
            AssignRank,
            RemoveMember,
            Disband,

        }

        private readonly IEODialogIconService _dialogIconService;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly ICharacterProvider _characterProvider;
        private readonly IEOMessageBoxFactory _messageBoxFactory;
        private readonly IGuildSessionProvider _guildSessionProvider;
        private readonly IGuildActions _guildActions;
        private readonly ITextInputDialogFactory _textInputDialogFactory;

        private string _guildDescription { get; set; }

        private GuildDialogState _state;

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
            _guildDescription = guildSessionProvider.GuildDescription;
            _guildActions = guildActions;
            _textInputDialogFactory = textInputDialogFactory;


            SetState(GuildDialogState.Initial);

            BackAction += (_, _) =>
            {
                switch (_state)
                {
                    case GuildDialogState.Management:
                        SetState(GuildDialogState.Initial);
                        break;
                    case GuildDialogState.Modify:
                        SetState(GuildDialogState.Management);
                        break;
                    default:
                        // no-op
                        break;
                }
            };

            Title = _localizedStringFinder.GetString(EOResourceID.GUILD_GUILD_MASTER);
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            if (_guildDescription != _guildSessionProvider.GuildDescription && _guildSessionProvider.GuildDescription != "")
            {
                _guildDescription = _guildSessionProvider.GuildDescription;
                SetState(GuildDialogState.Modify);
            }

            base.OnUpdateControl(gameTime);
        }

        private void SetState(GuildDialogState state)
        {
            _state = state;

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
                            OffsetY = 45,
                        };
                        //registrationItem.LeftClick += (_, _) => SetState(GuildDialogState.Registration);
                        //registrationItem.RightClick += (_, _) => SetState(GuildDialogState.Registration);

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
                        //registrationItem.LeftClick += (_, _) => SetState(GuildDialogState.Registration);
                        //registrationItem.RightClick += (_, _) => SetState(GuildDialogState.Registration);

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
                        managementItem.LeftClick += (_, _) => SetState(GuildDialogState.Management);
                        managementItem.RightClick += (_, _) => SetState(GuildDialogState.Management);

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
                        //registrationItem.LeftClick += (_, _) => SetState(GuildDialogState.Registration);
                        //registrationItem.RightClick += (_, _) => SetState(GuildDialogState.Registration);

                        AddItemToList(bankAccountItem, sortList: false);
                    }
                    break;

                case GuildDialogState.Management:
                    {
                        ListItemType = ListDialogItem.ListItemStyle.Large;
                        Buttons = ScrollingListDialogButtons.BackCancel;

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
                            _guildActions.GetGuildDescription(_characterProvider.MainCharacter.GuildTag);
                            SetStateIfGuildMember(GuildDialogState.Modify);
                        };
                        modifyGuildItem.RightClick += (_, _) =>
                        {
                            _guildActions.GetGuildDescription(_characterProvider.MainCharacter.GuildTag);
                            SetStateIfGuildMember(GuildDialogState.Modify);
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
                        manageRankingItem.LeftClick += (_, _) => SetStateIfGuildMember(GuildDialogState.ManageRankings);
                        manageRankingItem.RightClick += (_, _) => SetStateIfGuildMember(GuildDialogState.ManageRankings);

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
                        assignRankItem.LeftClick += (_, _) => SetStateIfGuildMember(GuildDialogState.AssignRank);
                        assignRankItem.RightClick += (_, _) => SetStateIfGuildMember(GuildDialogState.AssignRank);

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
                        removeMemberItem.LeftClick += (_, _) => SetStateIfGuildMember(GuildDialogState.RemoveMember);
                        removeMemberItem.RightClick += (_, _) => SetStateIfGuildMember(GuildDialogState.RemoveMember);

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
                        disbandItem.LeftClick += (_, _) => SetStateIfGuildMember(GuildDialogState.Disband);
                        disbandItem.RightClick += (_, _) => SetStateIfGuildMember(GuildDialogState.Disband);

                        AddItemToList(disbandItem, sortList: false);
                    }
                    break;
                case GuildDialogState.Modify:
                    {
                        ListItemType = ListDialogItem.ListItemStyle.Large;
                        Buttons = ScrollingListDialogButtons.BackCancel;

                        AddItemToList(new ListDialogItem(this, ListDialogItem.ListItemStyle.Small, 0)
                        {
                            PrimaryText = _localizedStringFinder.GetString(EOResourceID.GUILD_CURRENT_DESCRIPTION),
                        }, sortList: false);

                        AddItemToList(new ListDialogItem(this, ListDialogItem.ListItemStyle.Small, 0)
                        {
                            PrimaryText = _guildDescription,
                        }, sortList: false);

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
                    break;
            }
        }

        private void SetStateIfGuildMember(GuildDialogState state)
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
            // TODO: Move max description length to config?
            var dlg = _textInputDialogFactory.Create(_localizedStringFinder.GetString(EOResourceID.GUILD_DESCRIPTION), 240);
            dlg.DialogClosing += (_, e) =>
            {
                if (e.Result == XNADialogResult.OK)
                {
                    _guildActions.SetGuildDescription(dlg.ResponseText);
                    SetState(GuildDialogState.Management);
                }
            };
            dlg.Show();
        }
    }
}
