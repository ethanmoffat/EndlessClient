using EndlessClient.Controllers;
using EndlessClient.Dialogs.Services;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Optional;
using System;
using XNAControls;

namespace EndlessClient.Dialogs
{
    public class BardDialog : BaseEODialog
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IBardController _bardController;
        private readonly Texture2D _noteHighlight;
        private readonly Rectangle _noteRectangleArea;

        private Vector2 _highlightDrawPosition;
        private Option<Rectangle> _highlightSource;

        public BardDialog(INativeGraphicsManager nativeGraphicsManager,
                          IBardController bardController,
                          IEODialogButtonService dialogButtonService)
            : base(isInGame: true)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _bardController = bardController;
            BackgroundTexture = _nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 65);
            _noteHighlight = _nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 66);
            _noteRectangleArea = new Rectangle(15, 15, 240, 60);

            var cancel = new XNAButton(dialogButtonService.SmallButtonSheet, new Vector2(92, 83),
                dialogButtonService.GetSmallDialogButtonOutSource(SmallButton.Cancel),
                dialogButtonService.GetSmallDialogButtonOverSource(SmallButton.Cancel));
            cancel.Initialize();
            cancel.SetParentControl(this);
            cancel.OnClick += (_, _) => Close(XNADialogResult.Cancel);

            CenterInGameView();
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            var relativeMousePosition = CurrentMouseState.Position.ToVector2() - DrawPositionWithParentOffset;
            if (_noteRectangleArea.Contains(relativeMousePosition))
            {
                var relativePosition = relativeMousePosition - _noteRectangleArea.Location.ToVector2();
                var highlightDrawPosition =  new Vector2(
                    relativePosition.X - (relativePosition.X % 20),
                    relativePosition.Y - (relativePosition.Y % 20));
                _highlightDrawPosition = highlightDrawPosition + DrawPositionWithParentOffset + _noteRectangleArea.Location.ToVector2();
                _highlightSource = Option.Some(new Rectangle(highlightDrawPosition.ToPoint(), new Point(20, 20)));

                if (PreviousMouseState.LeftButton == ButtonState.Pressed && CurrentMouseState.LeftButton == ButtonState.Released)
                {
                    var noteIndex = (int)Math.Floor(highlightDrawPosition.X / 20 + (12 * (highlightDrawPosition.Y / 20)));
                    _bardController.PlayInstrumentNote(noteIndex);
                }
            }
            else
            {
                _highlightSource = Option.None<Rectangle>();
            }

            base.OnUpdateControl(gameTime);
        }

        protected override void OnDrawControl(GameTime gameTime)
        {
            base.OnDrawControl(gameTime);

            _highlightSource.MatchSome(src =>
            {
                _spriteBatch.Begin();
                _spriteBatch.Draw(_noteHighlight, _highlightDrawPosition, src, Color.White);
                _spriteBatch.End();
            });
        }
    }
}
