using System;
using EndlessClient.Controllers;
using EndlessClient.Dialogs.Services;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Input;
using MonoGame.Extended.Input.InputListeners;
using Optional;
using XNAControls;

namespace EndlessClient.Dialogs
{
    public class BardDialog : BaseEODialog
    {
        private readonly IBardController _bardController;
        private readonly Texture2D _noteHighlight;
        private readonly Rectangle _noteRectangleArea;

        private Vector2 _highlightDrawPosition;
        private Option<Rectangle> _highlightSource;

        private ulong _currentTick;

        public BardDialog(INativeGraphicsManager nativeGraphicsManager,
                          IBardController bardController,
                          IEODialogButtonService dialogButtonService)
            : base(nativeGraphicsManager, isInGame: true)
        {
            _bardController = bardController;
            BackgroundTexture = GraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 65);
            _noteHighlight = GraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 66);
            _noteRectangleArea = new Rectangle(15, 15, 240, 60);

            var cancel = new XNAButton(dialogButtonService.SmallButtonSheet, new Vector2(92, 83),
                dialogButtonService.GetSmallDialogButtonOutSource(SmallButton.Cancel),
                dialogButtonService.GetSmallDialogButtonOverSource(SmallButton.Cancel));
            cancel.Initialize();
            cancel.SetParentControl(this);
            cancel.OnClick += (_, _) => Close(XNADialogResult.Cancel);

            CenterInGameView();

            OnMouseOver += HandleMouseOver;
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            _currentTick++;

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

        protected override bool HandleClick(IXNAControl control, MouseEventArgs eventArgs)
        {
            if (eventArgs.Button == MouseButton.Left && _currentTick > 8)
            {
                _highlightSource.MatchSome(highlightSource =>
                {
                    var highlightDrawPosition = highlightSource.Location.ToVector2();
                    var noteIndex = (int)Math.Floor(highlightDrawPosition.X / 20 + (12 * (highlightDrawPosition.Y / 20)));
                    _bardController.PlayInstrumentNote(noteIndex);
                    _currentTick = 0;
                });
            }

            return true;
        }

        private void HandleMouseOver(object sender, MouseStateExtended eventArgs)
        {
            var relativeMousePosition = eventArgs.Position - DrawAreaWithParentOffset.Location;

            if (_noteRectangleArea.Contains(relativeMousePosition))
            {
                var relativePosition = relativeMousePosition - _noteRectangleArea.Location;
                var highlightDrawPosition = new Vector2(
                    relativePosition.X - (relativePosition.X % 20),
                    relativePosition.Y - (relativePosition.Y % 20));
                _highlightDrawPosition = highlightDrawPosition + DrawPositionWithParentOffset + _noteRectangleArea.Location.ToVector2();
                _highlightSource = Option.Some(new Rectangle(highlightDrawPosition.ToPoint(), new Point(20, 20)));
            }
            else
            {
                _highlightSource = Option.None<Rectangle>();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                OnMouseOver -= HandleMouseOver;
            }

            base.Dispose(disposing);
        }
    }
}