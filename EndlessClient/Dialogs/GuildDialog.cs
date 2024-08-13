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
using EOLib.IO.Repositories;
using EOLib.Localization;
using Microsoft.Xna.Framework;
using XNAControls;

namespace EndlessClient.Dialogs
{
    public class GuildDialog : ScrollingListDialog
    {
        private enum GuildDialogState
        {
            Initial,
            Administration,
            JoinGuild,
            LeaveGuild,
            RegisterGuild,
            WaitingForMembers,
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
                    case GuildDialogState.Administration:
                        ListItemStyle = ListDialogItem.ListItemStyle.Large;
                        Buttons = ScrollingListDialogButtons.BackCancel;
                        break;
                    case GuildDialogState.JoinGuild:
                        ListItemStyle = ListDialogItem.ListItemStyle.Small;
                        Buttons = ScrollingListDialogButtons.BackCancel;
                        break;
                    case GuildDialogState.LeaveGuild:
                        ListItemStyle = ListDialogItem.ListItemStyle.Small;
                        Buttons = ScrollingListDialogButtons.BackCancel;
                        break;
                    case GuildDialogState.RegisterGuild:
                        ListItemStyle = ListDialogItem.ListItemStyle.Small;
                        Buttons = ScrollingListDialogButtons.BackCancel;
                        break;
                    case GuildDialogState.WaitingForMembers:
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
        private readonly ITextMultiInputDialogFactory _textMultiInputDialogFactory;
        private readonly IContentProvider _contentProvider;
        private readonly IGuildActions _guildActions;
        private readonly IGuildSessionProvider _guildSessionProvider;
        private readonly ICharacterInventoryProvider _characterInventoryProvider;
        private readonly IEIFFileProvider _eifFileProvider;

        private readonly Stack<State> _stateStack;
        private int _creationMemberCount;

        private State _state;
        public GuildDialog(INativeGraphicsManager nativeGraphicsManager,
                         IEODialogButtonService dialogButtonService,
                         IEODialogIconService dialogIconService,
                         ILocalizedStringFinder localizedStringFinder,
                         ICharacterProvider characterProvider,
                         IEOMessageBoxFactory messageBoxFactory,
                         ITextMultiInputDialogFactory textMultiInputDialogFactory,
                         IContentProvider contentProvider,
                         IGuildActions guildActions,
                         IGuildSessionProvider guildSessionProvider,
                         ICharacterInventoryProvider characterInventoryProvider,
                         IEIFFileProvider eifFileProvider)
            : base(nativeGraphicsManager, dialogButtonService, DialogType.Guild)
        {
            _dialogIconService = dialogIconService;
            _localizedStringFinder = localizedStringFinder;
            _characterProvider = characterProvider;
            _messageBoxFactory = messageBoxFactory;
            _textMultiInputDialogFactory = textMultiInputDialogFactory;
            _contentProvider = contentProvider;
            _guildActions = guildActions;
            _guildSessionProvider = guildSessionProvider;
            _characterInventoryProvider = characterInventoryProvider;
            _eifFileProvider = eifFileProvider;
            _stateStack = new Stack<State>();
            _creationMemberCount = 0;

            SetState(new State(GuildDialogState.Initial));

            BackAction += (_, _) =>
            {
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
            if (_state.DialogState == GuildDialogState.RegisterGuild)
            {
                _guildSessionProvider.CreationSession.MatchSome(creationSession =>
                {
                    if (creationSession.Approved)
                    {
                        SetState(new State(GuildDialogState.WaitingForMembers), false);
                    }
                });
            }

            if (_state.DialogState == GuildDialogState.WaitingForMembers)
            {
                _guildSessionProvider.CreationSession.MatchSome(creationSession =>
                {
                    if (creationSession.Members.Count != _creationMemberCount)
                    {
                        AddItemToList(new ListDialogItem(this, ListDialogItem.ListItemStyle.Small, 0)
                        {
                            PrimaryText = Capitalize(creationSession.Members.Last())
                        }, false);
                        _creationMemberCount += 1;
                    }
                });
            }

            base.OnUpdateControl(gameTime);
        }

        private void SetState(State newState, bool pushState = true)
        {
            if (_state != newState && pushState)
            {
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
                case GuildDialogState.Administration:
                    SetupAdministrationState();
                    break;
                case GuildDialogState.JoinGuild:
                    SetupJoinGuildState();
                    break;
                case GuildDialogState.LeaveGuild:
                    SetupLeaveGuildState();
                    break;
                case GuildDialogState.RegisterGuild:
                    SetupRegisterGuildState();
                    break;
                case GuildDialogState.WaitingForMembers:
                    SetupWaitingForMembersState();
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
            administrationItem.LeftClick += (_, _) => SetState(new State(GuildDialogState.Administration));
            administrationItem.RightClick += (_, _) => SetState(new State(GuildDialogState.Administration));

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
            joinGuildItem.LeftClick += (_, _) => SetStateIfNotInGuild(new State(GuildDialogState.JoinGuild));
            joinGuildItem.RightClick += (_, _) => SetStateIfNotInGuild(new State(GuildDialogState.JoinGuild));

            AddItemToList(joinGuildItem, sortList: false);

            var leaveGuildItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 1)
            {
                ShowIconBackGround = false,
                IconGraphic = _dialogIconService.IconSheet,
                IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.GuildLeave),
                PrimaryText = _localizedStringFinder.GetString(EOResourceID.GUILD_LEAVE_GUILD),
                SubText = _localizedStringFinder.GetString(EOResourceID.GUILD_LEAVE_YOUR_CURRENT_GUILD),
                OffsetY = 45,
            };
            leaveGuildItem.LeftClick += (_, _) => SetStateIfInGuild(new State(GuildDialogState.LeaveGuild));
            leaveGuildItem.RightClick += (_, _) => SetStateIfInGuild(new State(GuildDialogState.LeaveGuild));

            AddItemToList(leaveGuildItem, sortList: false);

            var registerGuildItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 2)
            {
                ShowIconBackGround = false,
                IconGraphic = _dialogIconService.IconSheet,
                IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.GuildRegister),
                PrimaryText = _localizedStringFinder.GetString(EOResourceID.GUILD_REGISTER_GUILD),
                SubText = _localizedStringFinder.GetString(EOResourceID.GUILD_CREATE_YOUR_OWN_GUILD),
                OffsetY = 45,
            };
            registerGuildItem.LeftClick += (_, _) => SetStateIfNotInGuild(new State(GuildDialogState.RegisterGuild));
            registerGuildItem.RightClick += (_, _) => SetStateIfNotInGuild(new State(GuildDialogState.RegisterGuild));

            AddItemToList(registerGuildItem, sortList: false);
        }

        private void SetupJoinGuildState()
        {
            AddTextAsListItems(_contentProvider.Fonts[Constants.FontSize09],
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
            AddTextAsListItems(_contentProvider.Fonts[Constants.FontSize09],
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
            AddTextAsListItems(_contentProvider.Fonts[Constants.FontSize09],
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
            AddItemToList(new ListDialogItem(this, ListDialogItem.ListItemStyle.Small, 0)
            {
                PrimaryText = _localizedStringFinder.GetString(EOResourceID.GUILD_PLEASE_WAIT_FOR_ALL_MEMBERS_TO_JOIN)
            }, false);

            AddItemToList(new ListDialogItem(this, ListDialogItem.ListItemStyle.Small, 1), false);

            AddItemToList(new ListDialogItem(this, ListDialogItem.ListItemStyle.Small, 0)
            {
                PrimaryText = Capitalize(_characterProvider.MainCharacter.Name)
            }, false);
        }

        private void SetStateIfInGuild(State state)
        {
            if (!_characterProvider.MainCharacter.InGuild)
            {
                var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.GUILD_ALREADY_A_MEMBER);
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
                new TextMultiInputDialog.InputInfo { Label = _localizedStringFinder.GetString(EOResourceID.GUILD_GUILD_TAG), MaxChars = 3 },
                new TextMultiInputDialog.InputInfo { Label = _localizedStringFinder.GetString(EOResourceID.GUILD_RECRUITER), MaxChars = 12 }
            );

            dlgJoin.DialogClosing += (sender, e) =>
            {
                if (e.Result == XNADialogResult.OK)
                {
                    var guildTag = dlgJoin.Responses[0];
                    var recruiterName = dlgJoin.Responses[1];

                    if (guildTag.Length == 0)
                    {
                        e.Cancel = true;
                        var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.GUILD_CREATE_TAG_FIELD_EMPTY);
                        dlg.Show();
                        return;
                    }

                    if (recruiterName.Length == 0)
                    {
                        e.Cancel = true;
                        var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.GUILD_RECRUITER_INPUT_MISSING);
                        dlg.Show();
                        return;
                    }

                    if (guildTag.Length == 1)
                    {
                        e.Cancel = true;
                        var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.GUILD_CREATE_TAG_TOO_SHORT);
                        dlg.Show();
                        return;
                    }

                    _guildActions.RequestToJoinGuild(guildTag, recruiterName);
                }
            };

            dlgJoin.Show();
        }

        private void ShowLeaveGuildMessageBox()
        {
            var dlgLeave = _messageBoxFactory.CreateMessageBox(DialogResourceID.GUILD_PROMPT_LEAVE_GUILD, whichButtons: EODialogButtons.OkCancel);
            dlgLeave.DialogClosing += (_, e) =>
            {
                if (e.Result == XNADialogResult.OK)
                {
                    _guildActions.LeaveGuild();
                }
            };

            dlgLeave.Show();

        }

        private void ShowRegisterGuildMessageBox()
        {
            var dlgRegister = _textMultiInputDialogFactory.Create(
                _localizedStringFinder.GetString(EOResourceID.GUILD_REGISTER_GUILD),
                _localizedStringFinder.GetString(EOResourceID.GUILD_ENTER_YOUR_GUILD_DETAILS),
                TextMultiInputDialog.DialogSize.Three,
                new TextMultiInputDialog.InputInfo { Label = _localizedStringFinder.GetString(EOResourceID.GUILD_GUILD_TAG), MaxChars = 3 },
                new TextMultiInputDialog.InputInfo { Label = _localizedStringFinder.GetString(EOResourceID.GUILD_GUILD_NAME), MaxChars = 24 },
                new TextMultiInputDialog.InputInfo { Label = _localizedStringFinder.GetString(EOResourceID.GUILD_GUILD_DESCRIPTION), MaxChars = 240 }
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
                        dlg.Show();
                        return;
                    }

                    if (guildTag.Length == 0)
                    {
                        e.Cancel = true;
                        var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.GUILD_CREATE_TAG_FIELD_EMPTY);
                        dlg.Show();
                        return;
                    }

                    if (guildName.Length == 0)
                    {
                        e.Cancel = true;
                        var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.GUILD_CREATE_NAME_FIELD_EMPTY);
                        dlg.Show();
                        return;
                    }

                    if (guildTag.Length == 1)
                    {
                        e.Cancel = true;
                        var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.GUILD_CREATE_TAG_TOO_SHORT);
                        dlg.Show();
                        return;
                    }

                    if (guildName.Length <= 3)
                    {
                        e.Cancel = true;
                        var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.GUILD_CREATE_NAME_TOO_SHORT);
                        dlg.Show();
                        return;
                    }

                    if (char.ToLower(guildTag[0]) != char.ToLower(guildName[0]))
                    {
                        e.Cancel = true;
                        var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.GUILD_CREATE_NAME_FIELD_EMPTY);
                        dlg.Show();
                        return;
                    }

                    _guildActions.RequestToCreateGuild(guildTag, guildName, guildDescription);
                }
            };

            dlgRegister.Show();
        }
        private static string Capitalize(string input) =>
            string.IsNullOrEmpty(input) ? string.Empty : char.ToUpper(input[0]) + input[1..].ToLower();
    }
}
