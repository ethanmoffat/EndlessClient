using EndlessClient.Audio;
using EndlessClient.Dialogs.Factories;
using EndlessClient.Dialogs.Services;
using EOLib.Domain.Interact.Jukebox;
using EOLib.Domain.Map;
using EOLib.Graphics;
using EOLib.Localization;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Input.InputListeners;
using Optional;
using System;
using System.Collections.Generic;
using XNAControls;

namespace EndlessClient.Dialogs
{
    public class JukeboxDialog : ScrollingListDialog
    {
        private readonly IEODialogIconService _dialogIconService;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly IEOMessageBoxFactory _messageBoxFactory;
        private readonly IJukeboxActions _jukeboxActions;
        private readonly IJukeboxRepository _jukeboxRepository;
        private readonly ISfxPlayer _sfxPlayer;

        private readonly MapCoordinate _jukeboxCoordinate;

        private readonly IEDFFile _songNames;

        private ListDialogItem _changeSongItem, _playSongItem;

        private DateTime _openedTime;

        private Option<string> _lastRequestedName;
        private int _songIndex;

        public JukeboxDialog(INativeGraphicsManager nativeGraphicsManager,
                             IEODialogButtonService dialogButtonService,
                             IEODialogIconService dialogIconService,
                             ILocalizedStringFinder localizedStringFinder,
                             IDataFileProvider dataFileProvider,
                             IEOMessageBoxFactory messageBoxFactory,
                             IJukeboxActions jukeboxActions,
                             IJukeboxRepository jukeboxRepository,
                             ISfxPlayer sfxPlayer,
                             MapCoordinate jukeboxCoordinate)
            : base(nativeGraphicsManager, dialogButtonService, DialogType.Jukebox)
        {
            _dialogIconService = dialogIconService;
            _localizedStringFinder = localizedStringFinder;
            _messageBoxFactory = messageBoxFactory;
            _jukeboxActions = jukeboxActions;
            _jukeboxRepository = jukeboxRepository;
            _sfxPlayer = sfxPlayer;
            _jukeboxCoordinate = jukeboxCoordinate;

            ListItemType = ListDialogItem.ListItemStyle.Large;
            Buttons = ScrollingListDialogButtons.Cancel;

            _songNames = dataFileProvider.DataFiles[DataFiles.JukeBoxSongs];
            _openedTime = DateTime.Now;

            Title = _localizedStringFinder.GetString(EOResourceID.JUKEBOX_IS_READY);
        }

        public override void Initialize()
        {
            _changeSongItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 0)
            {
                IconGraphic = _dialogIconService.IconSheet,
                IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.JukeboxBrowse),
                PrimaryText = _localizedStringFinder.GetString(EOResourceID.JUKEBOX_BROWSE_THROUGH_SONGS),
                SubText = FormatSubtitle(_songNames.Data[_songIndex]),
                ShowIconBackGround = false,
                OffsetY = 60,
            };
            _changeSongItem.LeftClick += ChangeSongItem_Click;

            _playSongItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 1)
            {
                IconGraphic = _dialogIconService.IconSheet,
                IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.JukeboxPlay),
                PrimaryText = _localizedStringFinder.GetString(EOResourceID.JUKEBOX_PLAY_SONG),
                SubText = FormatSubtitle("25 gold"),
                ShowIconBackGround = false,
                OffsetY = 60,
            };
            _playSongItem.LeftClick += PlaySongItem_Click;

            SetItemList(new List<ListDialogItem> { _changeSongItem, _playSongItem });

            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            if ((DateTime.Now - _openedTime).TotalSeconds >= 95)
            {
                _jukeboxRepository.PlayingRequestName = Option.None<string>();
                _openedTime = DateTime.Now.AddMinutes(100);
            }

            _jukeboxRepository.PlayingRequestName.Match(
                requestedName =>
                {
                    if (_lastRequestedName.Map(x => !x.Equals(requestedName)).ValueOr(true))
                    {
                        _lastRequestedName = Option.Some(requestedName);

                        var titleString = _localizedStringFinder.GetString(EOResourceID.JUKEBOX_PLAYING_REQUEST);
                        if (!string.IsNullOrWhiteSpace(requestedName))
                            titleString += $" ({requestedName})";

                        Title = titleString;
                    }
                },
                () =>
                {
                    if (_lastRequestedName.HasValue)
                    {
                        _lastRequestedName = Option.None<string>();
                        Title = _localizedStringFinder.GetString(EOResourceID.JUKEBOX_IS_READY);
                    }
                });

            base.Update(gameTime);
        }

        private void ChangeSongItem_Click(object sender, MouseEventArgs e)
        {
            _songIndex = (_songIndex + 1) % _songNames.Data.Count;
            _changeSongItem.SubText = FormatSubtitle(_songNames.Data[_songIndex]);
        }

        private void PlaySongItem_Click(object sender, MouseEventArgs e)
        {
            if (_jukeboxRepository.PlayingRequestName.HasValue)
            {
                var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.JUKEBOX_REQUESTED_RECENTLY);
                dlg.ShowDialog();
                return;
            }

            var confirmDlg = _messageBoxFactory.CreateMessageBox(
                $"{_localizedStringFinder.GetString(EOResourceID.JUKEBOX_REQUEST_SONG_FOR)} 25 gold?",
                _localizedStringFinder.GetString(EOResourceID.JUKEBOX_REQUEST_SONG),
                EODialogButtons.OkCancel);

            confirmDlg.DialogClosing += (_, e) =>
            {
                if (e.Result == XNADialogResult.OK)
                {
                    _jukeboxActions.RequestSong(_jukeboxCoordinate, _songIndex);
                    _sfxPlayer.PlaySfx(SoundEffectID.BuySell);

                    Close(XNADialogResult.NO_BUTTON_PRESSED);
                }
            };

            confirmDlg.ShowDialog();
        }

        private string FormatSubtitle(string additionalText)
        {
            return _localizedStringFinder.GetString(EOResourceID.DIALOG_WORD_CURRENT) + " : " + additionalText;
        }
    }
}
