using EndlessClient.Content;
using EndlessClient.Dialogs.Factories;
using EndlessClient.Dialogs.Services;
using EOLib.Domain.Character;
using EOLib.Domain.Interact.Guild;
using EOLib.Domain.Map;
using EOLib.Graphics;
using EOLib.IO.Repositories;
using EOLib.Localization;

namespace EndlessClient.Dialogs
{
    public class GuildDialog : ScrollingListDialog
    {
        private enum GuildDialogState
        {
            // initial menu
            Initial,
            Management,
            Information,
            LookUp,
            ViewMembers,
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
        private readonly TextInputDialogFactory _textInputDialogFactory;

        private GuildDialogState _state;

        public GuildDialog(INativeGraphicsManager nativeGraphicsManager,
                         IEODialogButtonService dialogButtonService,
                         IEODialogIconService dialogIconService,
                         ILocalizedStringFinder localizedStringFinder,
                         ICharacterProvider characterProvider,
                         IEOMessageBoxFactory messageBoxFactory,
                         TextInputDialogFactory textInputDialogFactory)
            : base(nativeGraphicsManager, dialogButtonService, DialogType.Guild)
        {
            _dialogIconService = dialogIconService;
            _localizedStringFinder = localizedStringFinder;
            _characterProvider = characterProvider;
            _messageBoxFactory = messageBoxFactory;
            _textInputDialogFactory = textInputDialogFactory;

            SetState(GuildDialogState.Initial);

            BackAction += (_, _) =>
            {
                switch (_state)
                {
                    case GuildDialogState.Management:
                        SetState(GuildDialogState.Initial);
                        break;
                    case GuildDialogState.Information:
                        SetState(GuildDialogState.Initial); ;
                        break;
                    default:
                        // no-op
                        break;
                }
            };

            Title = _localizedStringFinder.GetString(EOResourceID.GUILD_GUILD_MASTER);
        }

        private void SetState(GuildDialogState state)
        {
            if (state != GuildDialogState.Initial && _state == state)
                return;

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
                        informationItem.LeftClick += (_, _) => SetState(GuildDialogState.Information);
                        informationItem.RightClick += (_, _) => SetState(GuildDialogState.Information);

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

                case GuildDialogState.Information:
                    {
                        ListItemType = ListDialogItem.ListItemStyle.Large;
                        Buttons = ScrollingListDialogButtons.BackCancel;

                        var lookGuildUp = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 0)
                        {
                            ShowIconBackGround = false,
                            IconGraphic = _dialogIconService.IconSheet,
                            IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.GuildLookup),
                            PrimaryText = _localizedStringFinder.GetString(EOResourceID.GUILD_LOOK_UP),
                            SubText = _localizedStringFinder.GetString(EOResourceID.GUILD_VIEW_DETAILS),
                            OffsetY = 45,
                        };
                        lookGuildUp.LeftClick += (_, _) =>
                        {
                            var dlg = _textInputDialogFactory.Create(_localizedStringFinder.GetString(EOResourceID.GUILD_TO_VIEW_INFORMATION_ABOUT_A_GUILD_ENTER_ITS_TAG));
                            dlg.DialogClosing += (_, e) =>
                            {
                                if(e.Result == XNAControls.XNADialogResult.OK)
                                {

                                }
                            };
                            dlg.ShowDialog();
                        };
                        lookGuildUp.RightClick += (_, _) =>
                        {
                            var dlg = _textInputDialogFactory.Create(_localizedStringFinder.GetString(EOResourceID.GUILD_TO_VIEW_INFORMATION_ABOUT_A_GUILD_ENTER_ITS_TAG));
                            dlg.DialogClosing += (_, e) =>
                            {
                                if (e.Result == XNAControls.XNADialogResult.OK)
                                {

                                }
                            };
                            dlg.ShowDialog();
                        };

                        AddItemToList(lookGuildUp, sortList: false);

                        var viewMembers = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 0)
                        {
                            ShowIconBackGround = false,
                            IconGraphic = _dialogIconService.IconSheet,
                            IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.GuildLookup),
                            PrimaryText = _localizedStringFinder.GetString(EOResourceID.GUILD_MEMBERLIST),
                            SubText = _localizedStringFinder.GetString(EOResourceID.GUILD_VIEW_MEMBERS),
                            OffsetY = 45,
                        };
                        viewMembers.LeftClick += (_, _) =>
                        {
                            var dlg = _textInputDialogFactory.Create(_localizedStringFinder.GetString(EOResourceID.GUILD_TO_VIEW_INFORMATION_ABOUT_A_GUILD_ENTER_ITS_TAG));
                            dlg.DialogClosing += (_, e) =>
                            {
                                if (e.Result == XNAControls.XNADialogResult.OK)
                                {

                                }
                            };
                            dlg.ShowDialog();
                        };
                        viewMembers.RightClick += (_, _) =>
                        {
                            var dlg = _textInputDialogFactory.Create(_localizedStringFinder.GetString(EOResourceID.GUILD_TO_VIEW_INFORMATION_ABOUT_A_GUILD_ENTER_ITS_TAG));
                            dlg.DialogClosing += (_, e) =>
                            {
                                if (e.Result == XNAControls.XNADialogResult.OK)
                                {

                                }
                            };
                            dlg.ShowDialog();
                        };

                        AddItemToList(viewMembers, sortList: false);
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
                        modifyGuildItem.LeftClick += (_, _) => SetStateIfGuildMember(GuildDialogState.Modify);
                        modifyGuildItem.RightClick += (_, _) => SetStateIfGuildMember(GuildDialogState.Modify);

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
    }
}
