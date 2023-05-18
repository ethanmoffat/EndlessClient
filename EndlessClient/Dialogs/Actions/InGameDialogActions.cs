﻿using AutomaticTypeMapper;
using EndlessClient.Audio;
using EndlessClient.Dialogs.Factories;
using EndlessClient.HUD;
using EOLib.Domain.Character;
using EOLib.Domain.Interact.Quest;
using EOLib.Domain.Interact.Shop;
using EOLib.Domain.Interact.Skill;
using EOLib.Localization;
using Optional;
using System;
using System.Collections.Generic;
using System.Linq;
using XNAControls;

namespace EndlessClient.Dialogs.Actions
{
    [AutoMappedType]
    public class InGameDialogActions : IInGameDialogActions
    {
        private readonly IFriendIgnoreListDialogFactory _friendIgnoreListDialogFactory;
        private readonly IPaperdollDialogFactory _paperdollDialogFactory;
        private readonly ISessionExpDialogFactory _sessionExpDialogFactory;
        private readonly IQuestStatusDialogFactory _questStatusDialogFactory;
        private readonly IActiveDialogRepository _activeDialogRepository;
        private readonly IShopDataRepository _shopDataRepository;
        private readonly IQuestDataRepository _questDataRepository;
        private readonly ISkillDataRepository _skillDataRepository;
        private readonly IChestDialogFactory _chestDialogFactory;
        private readonly ILockerDialogFactory _lockerDialogFactory;
        private readonly IBankAccountDialogFactory _bankAccountDialogFactory;
        private readonly ISkillmasterDialogFactory _skillmasterDialogFactory;
        private readonly IBardDialogFactory _bardDialogFactory;
        private readonly IScrollingListDialogFactory _scrollingListDialogFactory;
        private readonly ITradeDialogFactory _tradeDialogFactory;
        private readonly IBoardDialogFactory _boardDialogFactory;
        private readonly ISfxPlayer _sfxPlayer;
        private readonly IStatusLabelSetter _statusLabelSetter;
        private readonly IShopDialogFactory _shopDialogFactory;
        private readonly IQuestDialogFactory _questDialogFactory;

        public InGameDialogActions(IFriendIgnoreListDialogFactory friendIgnoreListDialogFactory,
                                   IPaperdollDialogFactory paperdollDialogFactory,
                                   ISessionExpDialogFactory sessionExpDialogFactory,
                                   IQuestStatusDialogFactory questStatusDialogFactory,
                                   IShopDialogFactory shopDialogFactory,
                                   IQuestDialogFactory questDialogFactory,
                                   IActiveDialogRepository activeDialogRepository,
                                   IShopDataRepository shopDataRepository,
                                   IQuestDataRepository questDataRepository,
                                   ISkillDataRepository skillDataRepository,
                                   IChestDialogFactory chestDialogFactory,
                                   ILockerDialogFactory lockerDialogFactory,
                                   IBankAccountDialogFactory bankAccountDialogFactory,
                                   ISkillmasterDialogFactory skillmasterDialogFactory,
                                   IBardDialogFactory bardDialogFactory,
                                   IScrollingListDialogFactory scrollingListDialogFactory,
                                   ITradeDialogFactory tradeDialogFactory,
                                   IBoardDialogFactory boardDialogFactory,
                                   ISfxPlayer sfxPlayer,
                                   IStatusLabelSetter statusLabelSetter)
        {
            _friendIgnoreListDialogFactory = friendIgnoreListDialogFactory;
            _paperdollDialogFactory = paperdollDialogFactory;
            _sessionExpDialogFactory = sessionExpDialogFactory;
            _questStatusDialogFactory = questStatusDialogFactory;
            _activeDialogRepository = activeDialogRepository;
            _shopDataRepository = shopDataRepository;
            _questDataRepository = questDataRepository;
            _skillDataRepository = skillDataRepository;
            _chestDialogFactory = chestDialogFactory;
            _lockerDialogFactory = lockerDialogFactory;
            _bankAccountDialogFactory = bankAccountDialogFactory;
            _skillmasterDialogFactory = skillmasterDialogFactory;
            _bardDialogFactory = bardDialogFactory;
            _scrollingListDialogFactory = scrollingListDialogFactory;
            _tradeDialogFactory = tradeDialogFactory;
            _boardDialogFactory = boardDialogFactory;
            _sfxPlayer = sfxPlayer;
            _statusLabelSetter = statusLabelSetter;
            _shopDialogFactory = shopDialogFactory;
            _questDialogFactory = questDialogFactory;
        }

        public void ShowFriendListDialog()
        {
            _activeDialogRepository.FriendIgnoreDialog.MatchNone(() =>
            {
                var dlg = _friendIgnoreListDialogFactory.Create(isFriendList: true);
                dlg.DialogClosed += (_, _) => _activeDialogRepository.FriendIgnoreDialog = Option.None<FriendIgnoreListDialog>();
                _activeDialogRepository.FriendIgnoreDialog = Option.Some(dlg);

                UseDefaultDialogSounds(dlg);

                dlg.Show();
            });
        }

        public void ShowIgnoreListDialog()
        {
            _activeDialogRepository.FriendIgnoreDialog.MatchNone(() =>
            {
                var dlg = _friendIgnoreListDialogFactory.Create(isFriendList: false);
                dlg.DialogClosed += (_, _) => _activeDialogRepository.FriendIgnoreDialog = Option.None<FriendIgnoreListDialog>();
                _activeDialogRepository.FriendIgnoreDialog = Option.Some(dlg);

                UseDefaultDialogSounds(dlg);

                dlg.Show();
            });
        }

        public void ShowSessionExpDialog()
        {
            _activeDialogRepository.SessionExpDialog.MatchNone(() =>
            {
                var dlg = _sessionExpDialogFactory.Create();
                dlg.DialogClosed += (_, _) => _activeDialogRepository.SessionExpDialog = Option.None<SessionExpDialog>();
                _activeDialogRepository.SessionExpDialog = Option.Some(dlg);

                UseDefaultDialogSounds(dlg);

                dlg.Show();
            });
        }

        public void ShowQuestStatusDialog()
        {
            _activeDialogRepository.QuestStatusDialog.MatchNone(() =>
            {
                var dlg = _questStatusDialogFactory.Create();
                dlg.DialogClosed += (_, _) => _activeDialogRepository.QuestStatusDialog = Option.None<QuestStatusDialog>();
                _activeDialogRepository.QuestStatusDialog = Option.Some(dlg);

                UseDefaultDialogSounds(dlg);

                dlg.Show();
            });
        }

        public void ShowPaperdollDialog(Character character, bool isMainCharacter)
        {
            _activeDialogRepository.PaperdollDialog.MatchNone(() =>
            {
                var dlg = _paperdollDialogFactory.Create(character, isMainCharacter);
                dlg.DialogClosed += (_, _) => _activeDialogRepository.PaperdollDialog = Option.None<PaperdollDialog>();
                _activeDialogRepository.PaperdollDialog = Option.Some(dlg);

                UseDefaultDialogSounds(dlg);

                dlg.Show();
            });
        }

        public void ShowShopDialog()
        {
            _activeDialogRepository.ShopDialog.MatchNone(() =>
            {
                var dlg = _shopDialogFactory.Create();
                dlg.DialogClosed += (_, _) =>
                {
                    _activeDialogRepository.ShopDialog = Option.None<ShopDialog>();
                    _shopDataRepository.ResetState();
                };
                _activeDialogRepository.ShopDialog = Option.Some(dlg);

                UseDefaultDialogSounds(dlg);

                dlg.Show();
            });
        }

        public void ShowQuestDialog()
        {
            _activeDialogRepository.QuestDialog.MatchNone(() =>
            {
                var dlg = _questDialogFactory.Create();
                dlg.DialogClosed += (_, _) =>
                {
                    _activeDialogRepository.QuestDialog = Option.None<QuestDialog>();
                    _questDataRepository.ResetState();
                };
                _activeDialogRepository.QuestDialog = Option.Some(dlg);

                UseQuestDialogSounds(dlg);

                dlg.Show();
            });
        }

        public void ShowChestDialog()
        {
            _activeDialogRepository.ChestDialog.MatchNone(() =>
            {
                var dlg = _chestDialogFactory.Create();
                dlg.DialogClosed += (_, _) => _activeDialogRepository.ChestDialog = Option.None<ChestDialog>();
                _activeDialogRepository.ChestDialog = Option.Some(dlg);

                UseDefaultDialogSounds(dlg);
                _sfxPlayer.PlaySfx(SoundEffectID.ChestOpen);

                dlg.Show();
            });
        }

        public void ShowLockerDialog()
        {
            _activeDialogRepository.LockerDialog.MatchNone(() =>
            {
                var dlg = _lockerDialogFactory.Create();
                dlg.DialogClosed += (_, _) => _activeDialogRepository.LockerDialog = Option.None<LockerDialog>();
                _activeDialogRepository.LockerDialog = Option.Some(dlg);

                UseDefaultDialogSounds(dlg);
                _sfxPlayer.PlaySfx(SoundEffectID.ChestOpen);

                dlg.Show();
            });
        }

        public void ShowBankAccountDialog()
        {
            _activeDialogRepository.BankAccountDialog.MatchNone(() =>
            {
                var dlg = _bankAccountDialogFactory.Create();
                dlg.DialogClosed += (_, _) => _activeDialogRepository.BankAccountDialog = Option.None<BankAccountDialog>();
                _activeDialogRepository.BankAccountDialog = Option.Some(dlg);

                UseDefaultDialogSounds(dlg);

                dlg.Show();
            });
        }

        public void ShowSkillmasterDialog()
        {
            var dlg = _skillmasterDialogFactory.Create();
            dlg.DialogClosed += (_, _) =>
            {
                _activeDialogRepository.SkillmasterDialog = Option.None<SkillmasterDialog>();
                _skillDataRepository.ResetState();
            };
            _activeDialogRepository.SkillmasterDialog = Option.Some(dlg);

            UseDefaultDialogSounds(dlg);

            dlg.Show();
        }

        public void ShowBardDialog()
        {
            _activeDialogRepository.BardDialog.MatchNone(() =>
            {
                var dlg = _bardDialogFactory.Create();
                dlg.DialogClosed += (_, _) =>
                {
                    _activeDialogRepository.BardDialog = Option.None<BardDialog>();
                };
                _activeDialogRepository.BardDialog = Option.Some(dlg);

                UseDefaultDialogSounds(dlg);

                dlg.Show();
            });
        }

        public void ShowMessageDialog(string title, IReadOnlyList<string> messages)
        {
            _activeDialogRepository.MessageDialog.MatchNone(() =>
            {
                var dlg = _scrollingListDialogFactory.Create(ScrollingListDialogSize.Large);
                dlg.DialogClosed += (_, _) => _activeDialogRepository.MessageDialog = Option.None<ScrollingListDialog>();

                dlg.ListItemType = ListDialogItem.ListItemStyle.Small;
                dlg.Buttons = ScrollingListDialogButtons.Cancel;
                dlg.Title = title;

                var _68spaces = new string(Enumerable.Repeat(' ', 68).ToArray());
                var items = messages
                    // BU hack - assume that 68 spaces or more indicates an extra line break
                    .Select(x => x.Replace(_68spaces, "   \n"))
                    // BU hack - assume that 3 spaces or more indicates extra padding and should split the message into multiple lines
                    .SelectMany(x => x.Split("   ", StringSplitOptions.RemoveEmptyEntries))
                    .Select(x => new ListDialogItem(dlg, ListDialogItem.ListItemStyle.Small) { PrimaryText = x == "\n" ? string.Empty : x.Trim() }).ToList();

                dlg.SetItemList(items);

                _activeDialogRepository.MessageDialog = Option.Some(dlg);

                UseDefaultDialogSounds(dlg);

                dlg.Show();
            });
        }

        public void ShowTradeDialog()
        {
            _activeDialogRepository.TradeDialog.MatchNone(() =>
            {
                var dlg = _tradeDialogFactory.Create();
                dlg.DialogClosed += (_, _) => _activeDialogRepository.TradeDialog = Option.None<TradeDialog>();
                _activeDialogRepository.TradeDialog = Option.Some(dlg);

                UseDefaultDialogSounds(dlg);

                dlg.Show();
            });
        }

        public void CloseTradeDialog()
        {
            _activeDialogRepository.TradeDialog.MatchSome(dlg => dlg.Close());
        }

        public void ShowBoardDialog()
        {
            _activeDialogRepository.BoardDialog.MatchNone(() =>
            {
                var dlg = _boardDialogFactory.Create();
                dlg.DialogClosed += (_, _) => _activeDialogRepository.BoardDialog = Option.None<BoardDialog>();
                _activeDialogRepository.BoardDialog = Option.Some(dlg);

                dlg.Show();

                UseDefaultDialogSounds(dlg);
            });

            // the vanilla client shows the status label any time the server sends the BOARD_OPEN packet
            _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_ACTION, EOResourceID.BOARD_TOWN_BOARD_NOW_VIEWED);
        }

        private void UseDefaultDialogSounds(ScrollingListDialog dialog)
        {
            UseDefaultDialogSounds((BaseEODialog)dialog);

            foreach (var button in dialog.ChildControls.OfType<IXNAButton>())
                button.OnClick += Handler;

            void Handler(object sender, EventArgs e) => _sfxPlayer.PlaySfx(SoundEffectID.DialogButtonClick);
        }

        private void UseDefaultDialogSounds(BaseEODialog dialog)
        {
            dialog.DialogClosing += (_, _) => _sfxPlayer.PlaySfx(SoundEffectID.DialogButtonClick);

            foreach (var textbox in dialog.ChildControls.OfType<IXNATextBox>())
                textbox.OnGotFocus += (_, _) => _sfxPlayer.PlaySfx(SoundEffectID.TextBoxFocus);
        }

        private void UseQuestDialogSounds(QuestDialog dialog)
        {
            dialog.DialogClosing += (_, _) => _sfxPlayer.PlaySfx(SoundEffectID.DialogButtonClick);
            dialog.BackAction += (_, _) => _sfxPlayer.PlaySfx(SoundEffectID.TextBoxFocus);
            dialog.NextAction += (_, _) => _sfxPlayer.PlaySfx(SoundEffectID.TextBoxFocus);
            dialog.LinkClickAction += (_, _) => _sfxPlayer.PlaySfx(SoundEffectID.ButtonClick);
        }
    }

    public interface IInGameDialogActions
    {
        void ShowFriendListDialog();

        void ShowIgnoreListDialog();

        void ShowSessionExpDialog();

        void ShowQuestStatusDialog();

        void ShowPaperdollDialog(Character character, bool isMainCharacter);

        void ShowShopDialog();

        void ShowQuestDialog();

        void ShowChestDialog();

        void ShowLockerDialog();

        void ShowBankAccountDialog();

        void ShowSkillmasterDialog();

        void ShowBardDialog();

        void ShowMessageDialog(string title, IReadOnlyList<string> messages);

        void ShowTradeDialog();

        void CloseTradeDialog();

        void ShowBoardDialog();
    }
}
