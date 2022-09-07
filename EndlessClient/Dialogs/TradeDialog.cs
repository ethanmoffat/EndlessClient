using EndlessClient.Content;
using EndlessClient.Dialogs.Services;
using EndlessClient.UIControls;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Trade;
using EOLib.Graphics;
using EOLib.Localization;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using XNAControls;

namespace EndlessClient.Dialogs
{
    public class TradeDialog : BaseEODialog
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly ITradeProvider _tradeProvider;
        private readonly ICharacterProvider _characterProvider;

        private readonly IXNALabel _leftPlayerName, _rightPlayerName;
        private readonly IXNALabel _leftPlayerStatus, _rightPlayerStatus;
        private readonly ScrollBar _leftScroll, _rightScroll;

        private readonly IXNAButton _ok, _cancel;

        private readonly List<ListDialogItem> _leftItems, _rightItems;

        // cached values: updated/compared from domain state
        private TradeOffer _leftOffer, _rightOffer;
        private int _leftScrollOffset, _rightScrollOffset;

        // tracks state around the trade partner quickly changing an offer
        // pops up warning for "other player trying to trick you"
        private int _recentPartnerItemChanges;
        private Stopwatch _partnerItemChangeTick;

        public TradeDialog(INativeGraphicsManager nativeGraphicsManager,
                           ILocalizedStringFinder localizedStringFinder,
                           IEODialogButtonService dialogButtonService,
                           ITradeProvider tradeProvider,
                           ICharacterProvider characterProvider)
            : base(isInGame: true)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _localizedStringFinder = localizedStringFinder;
            _tradeProvider = tradeProvider;
            _characterProvider = characterProvider;

            BackgroundTexture = _nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 50);

            _leftPlayerName = new XNALabel(Constants.FontSize08pt5)
            {
                DrawArea = new Rectangle(20, 14, 166, 20),
                AutoSize = false,
                TextAlign = LabelAlignment.MiddleLeft,
                ForeColor = ColorConstants.LightGrayText
            };
            _leftPlayerName.SetParentControl(this);

            _rightPlayerName = new XNALabel(Constants.FontSize08pt5)
            {
                DrawArea = new Rectangle(285, 14, 166, 20),
                AutoSize = false,
                TextAlign = LabelAlignment.MiddleLeft,
                ForeColor = ColorConstants.LightGrayText
            };
            _rightPlayerName.SetParentControl(this);

            _leftPlayerStatus = new XNALabel(Constants.FontSize08pt5)
            {
                DrawArea = new Rectangle(195, 14, 79, 20),
                AutoSize = false,
                TextAlign = LabelAlignment.MiddleLeft,
                ForeColor = ColorConstants.LightGrayText,
                Text = _localizedStringFinder.GetString(EOResourceID.DIALOG_TRADE_WORD_TRADING)
            };
            _leftPlayerStatus.SetParentControl(this);

            _rightPlayerStatus = new XNALabel(Constants.FontSize08pt5)
            {
                DrawArea = new Rectangle(462, 14, 79, 20),
                AutoSize = false,
                TextAlign = LabelAlignment.MiddleLeft,
                ForeColor = ColorConstants.LightGrayText,
                Text = _localizedStringFinder.GetString(EOResourceID.DIALOG_TRADE_WORD_TRADING)
            };
            _rightPlayerStatus.SetParentControl(this);

            _leftScroll = new ScrollBar(new Vector2(252, 44), new Vector2(16, 199), ScrollBarColors.LightOnMed, _nativeGraphicsManager) { LinesToRender = 5 };
            _leftScroll.SetParentControl(this);
            _rightScroll = new ScrollBar(new Vector2(518, 44), new Vector2(16, 199), ScrollBarColors.LightOnMed, _nativeGraphicsManager) { LinesToRender = 5 };
            _rightScroll.SetParentControl(this);

            _ok = new XNAButton(dialogButtonService.SmallButtonSheet, new Vector2(356, 252),
                dialogButtonService.GetSmallDialogButtonOutSource(SmallButton.Ok),
                dialogButtonService.GetSmallDialogButtonOverSource(SmallButton.Ok));
            _ok.OnClick += OkButtonClicked;
            _ok.SetParentControl(this);

            _cancel = new XNAButton(dialogButtonService.SmallButtonSheet, new Vector2(449, 252),
                dialogButtonService.GetSmallDialogButtonOutSource(SmallButton.Cancel),
                dialogButtonService.GetSmallDialogButtonOverSource(SmallButton.Cancel));
            _cancel.OnClick += CancelButtonClicked;
            _cancel.SetParentControl(this);

            _leftItems = new List<ListDialogItem>();
            _rightItems = new List<ListDialogItem>();
            _leftOffer = new TradeOffer.Builder().ToImmutable();
            _rightOffer = new TradeOffer.Builder().ToImmutable();
        }

        public override void Initialize()
        {
            base.Initialize();

            _leftPlayerName.Initialize();
            _rightPlayerName.Initialize();

            _leftPlayerStatus.Initialize();
            _rightPlayerStatus.Initialize();

            _leftScroll.Initialize();
            _rightScroll.Initialize();

            _ok.Initialize();
            _cancel.Initialize();
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            if (_tradeProvider.PlayerOneOffer != null && !_tradeProvider.PlayerOneOffer.Equals(_leftOffer))
            {
                UpdateOffer(_tradeProvider.PlayerOneOffer, _leftOffer, _leftPlayerName, _leftPlayerStatus, _leftItems, -3);
                _leftOffer = _tradeProvider.PlayerOneOffer;
                _leftScroll.UpdateDimensions(_leftOffer.Items.Count);
            }

            if (_tradeProvider.PlayerTwoOffer != null && !_tradeProvider.PlayerTwoOffer.Equals(_rightOffer))
            {
                UpdateOffer(_tradeProvider.PlayerTwoOffer, _rightOffer, _rightPlayerName, _rightPlayerStatus, _rightItems, 263);
                _rightOffer = _tradeProvider.PlayerTwoOffer;
                _rightScroll.UpdateDimensions(_rightOffer.Items.Count);
            }

            if (_leftScrollOffset != _leftScroll.ScrollOffset)
            {
                // todo: update left list item display
                _leftScrollOffset = _leftScroll.ScrollOffset;
            }

            if (_rightScrollOffset != _rightScroll.ScrollOffset)
            {
                // todo: update right list item display
                _rightScrollOffset = _rightScroll.ScrollOffset;
            }

            if (_partnerItemChangeTick?.ElapsedMilliseconds > 1000)
            {
                _recentPartnerItemChanges--;
                _partnerItemChangeTick = Stopwatch.StartNew();
            }

            base.OnUpdateControl(gameTime);
        }

        private void UpdateOffer(TradeOffer actualOffer, TradeOffer cachedOffer,
            IXNALabel playerNameLabel, IXNALabel playerStatusLabel,
            List<ListDialogItem> listItems,
            int listitemOffset)
        {
            if (actualOffer.PlayerName != cachedOffer.PlayerName || actualOffer.Items.Count != cachedOffer.Items.Count)
            {
                playerNameLabel.Text = $"{actualOffer.PlayerName}{(actualOffer.Items.Any() ? $"[{actualOffer.Items.Count}]" : "")}";
            }

            // todo: check if packets properly reset agrees to false when items change
            if (actualOffer.Agrees != cachedOffer.Agrees)
            {
                playerStatusLabel.Text = actualOffer.Agrees
                    ? _localizedStringFinder.GetString(EOResourceID.DIALOG_TRADE_WORD_AGREE)
                    : _localizedStringFinder.GetString(EOResourceID.DIALOG_TRADE_WORD_TRADING);

                //if (agrees && !m_leftAgrees)
                //    ((EOGame)Game).Hud.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_ACTION,
                //        isMain ? EOResourceID.STATUS_LABEL_TRADE_YOU_ACCEPT : EOResourceID.STATUS_LABEL_TRADE_OTHER_ACCEPT);
                //else if (!agrees && m_leftAgrees)
                //    ((EOGame)Game).Hud.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_ACTION,
                //        isMain ? EOResourceID.STATUS_LABEL_TRADE_YOU_CANCEL : EOResourceID.STATUS_LABEL_TRADE_OTHER_CANCEL);
            }

            if (!actualOffer.Items.ToHashSet().SetEquals(cachedOffer.Items))
            {
                var added = actualOffer.Items.Except(cachedOffer.Items);
                var removed = cachedOffer.Items.Where(i => !actualOffer.Items.Contains(i));

                foreach (var addedItem in added)
                {

                }

                foreach (var removedItem in removed)
                {

                }

                if (actualOffer.PlayerID != _characterProvider.MainCharacter.ID)
                {
                    _partnerItemChangeTick = Stopwatch.StartNew();
                    _recentPartnerItemChanges++;

                    if (_recentPartnerItemChanges == 3)
                    {
                        //EOMessageBox.Show(DialogResourceID.TRADE_OTHER_PLAYER_TRICK_YOU, EODialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
                        //m_recentPartnerRemoves = -1000; //this will prevent the message from showing more than once (I'm too lazy to find something more elegant)
                    }
                    else
                    {
                        //EOMessageBox.Show(DialogResourceID.TRADE_ABORTED_OFFER_CHANGED, EODialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
                        //((EOGame)Game).Hud.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING, EOResourceID.STATUS_LABEL_TRADE_OTHER_PLAYER_CHANGED_OFFER);
                    }
                }
            }
        }

        private void OkButtonClicked(object sender, EventArgs e)
        {
        }

        private void CancelButtonClicked(object sender, EventArgs e)
        {
        }
    }
}
