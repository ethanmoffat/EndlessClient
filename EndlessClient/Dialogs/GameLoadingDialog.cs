using System;
using EndlessClient.GameExecution;
using EndlessClient.Rendering;
using EOLib;
using EOLib.Graphics;
using EOLib.Localization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient.Dialogs
{
    public class GameLoadingDialog : BaseEODialog
    {
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly Texture2D _backgroundSprite;
        private readonly IXNALabel _message, _caption;

        private DateTime _lastBackgroundUpdate;
        private int _bgSrcIndex;

        public GameLoadingDialog(INativeGraphicsManager nativeGraphicsManager,
                                 IGameStateProvider gameStateProvider,
                                 IClientWindowSizeProvider clientWindowSizeProvider,
                                 ILocalizedStringFinder localizedStringFinder)
            : base(nativeGraphicsManager, gameStateProvider)
        {
            _localizedStringFinder = localizedStringFinder;
            _backgroundSprite = GraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 33);

            DrawPosition = new Vector2(clientWindowSizeProvider.Width - _backgroundSprite.Width / 4 - 10,
                                       clientWindowSizeProvider.Height - _backgroundSprite.Height - 10);

            SetSize(_backgroundSprite.Width / 4, _backgroundSprite.Height);

            _bgSrcIndex = 0;
            _lastBackgroundUpdate = DateTime.Now;

            _caption = new XNALabel(Constants.FontSize10)
            {
                AutoSize = true,
                Text = _localizedStringFinder.GetString(EOResourceID.LOADING_GAME_PLEASE_WAIT),
                ForeColor = ColorConstants.LightYellowText,
                DrawPosition = new Vector2(12, 9)
            };
            _caption.SetParentControl(this);

            var gen = new Random();
            var messageTextID = (EOResourceID)gen.Next((int)EOResourceID.LOADING_GAME_HINT_FIRST, (int)EOResourceID.LOADING_GAME_HINT_LAST);
            var localizedMessage = _localizedStringFinder.GetString(messageTextID);

            _message = new XNALabel(Constants.FontSize08)
            {
                AutoSize = true,
                TextWidth = 175,
                ForeColor = ColorConstants.MediumGrayText,
                Text = localizedMessage,
                DrawPosition = new Vector2(18, 61)
            };
            _message.SetParentControl(this);
        }

        public override void Initialize()
        {
            _caption.Initialize();
            _message.Initialize();

            base.Initialize();
        }

        protected override void OnUpdateControl(GameTime gt)
        {
            if ((int) (DateTime.Now - _lastBackgroundUpdate).TotalMilliseconds > 500)
            {
                _bgSrcIndex = _bgSrcIndex == 3 ? 0 : _bgSrcIndex + 1;
                _lastBackgroundUpdate = DateTime.Now;
            }

            base.OnUpdateControl(gt);
        }

        protected override void OnDrawControl(GameTime gt)
        {
            _spriteBatch.Begin();
            _spriteBatch.Draw(_backgroundSprite,
                DrawAreaWithParentOffset,
                new Rectangle(_bgSrcIndex * (_backgroundSprite.Width / 4), 0, _backgroundSprite.Width / 4, _backgroundSprite.Height),
                Color.White);
            _spriteBatch.End();

            base.OnDrawControl(gt);
        }

        public void SetState(GameLoadingDialogState whichState)
        {
            switch (whichState)
            {
                case GameLoadingDialogState.Map:
                    _caption.Text = _localizedStringFinder.GetString(EOResourceID.LOADING_GAME_UPDATING_MAP);
                    break;
                case GameLoadingDialogState.Item:
                    _caption.Text = _localizedStringFinder.GetString(EOResourceID.LOADING_GAME_UPDATING_ITEMS);
                    break;
                case GameLoadingDialogState.NPC:
                    _caption.Text = _localizedStringFinder.GetString(EOResourceID.LOADING_GAME_UPDATING_NPCS);
                    break;
                case GameLoadingDialogState.Spell:
                    _caption.Text = _localizedStringFinder.GetString(EOResourceID.LOADING_GAME_UPDATING_SKILLS);
                    break;
                case GameLoadingDialogState.Class:
                    _caption.Text = _localizedStringFinder.GetString(EOResourceID.LOADING_GAME_UPDATING_CLASSES);
                    break;
                case GameLoadingDialogState.LoadingGame:
                    _caption.Text = _localizedStringFinder.GetString(EOResourceID.LOADING_GAME_LOADING_GAME);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(whichState), whichState, null);
            }
        }

        public void CloseDialog()
        {
            Close(XNADialogResult.NO_BUTTON_PRESSED);
        }
    }

    public enum GameLoadingDialogState
    {
        Map,
        Item,
        NPC,
        Spell,
        Class,
        LoadingGame
    }
}
