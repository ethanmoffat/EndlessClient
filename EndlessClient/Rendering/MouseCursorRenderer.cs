using EndlessClient.Controllers;
using EndlessClient.Dialogs;
using EndlessClient.HUD;
using EndlessClient.Input;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Item;
using EOLib.Domain.Map;
using EOLib.Graphics;
using EOLib.IO;
using EOLib.IO.Map;
using EOLib.IO.Pub;
using EOLib.IO.Repositories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Optional;
using System;
using System.Linq;
using System.Threading.Tasks;
using XNAControls;

namespace EndlessClient.Rendering
{
    public class MouseCursorRenderer : XNAControl, IMouseCursorRenderer
    {
        private enum CursorIndex
        {
            Standard = 0,
            HoverNormal = 1,
            HoverItem = 2,
            ClickFirstFrame = 3,
            ClickSecondFrame = 4,
            NumberOfFramesInSheet = 5
        }

        private readonly Rectangle SingleCursorFrameArea;

        private readonly Texture2D _mouseCursorTexture;
        private readonly ICharacterProvider _characterProvider;
        private readonly IRenderOffsetCalculator _renderOffsetCalculator;
        private readonly IMapCellStateProvider _mapCellStateProvider;
        private readonly IItemStringService _itemStringService;
        private readonly IItemNameColorService _itemNameColorService;
        private readonly IEIFFileProvider _eifFileProvider;
        private readonly ICurrentMapProvider _currentMapProvider;
        private readonly IMapInteractionController _mapInteractionController;
        private readonly IUserInputProvider _userInputProvider;
        private readonly IActiveDialogProvider _activeDialogProvider;
        private readonly IContextMenuProvider _contextMenuProvider;
        private readonly XNALabel _mapItemText;

        private int _gridX, _gridY;
        private CursorIndex _cursorIndex;
        private bool _shouldDrawCursor;

        private Option<DateTime> _startClickTime;
        private CursorIndex _clickFrame;
        private int _clickAlpha;
        private Option<MapCoordinate> _clickCoordinate;

        public MapCoordinate GridCoordinates => new MapCoordinate(_gridX, _gridY);

        public MouseCursorRenderer(INativeGraphicsManager nativeGraphicsManager,
                                   ICharacterProvider characterProvider,
                                   IRenderOffsetCalculator renderOffsetCalculator,
                                   IMapCellStateProvider mapCellStateProvider,
                                   IItemStringService itemStringService,
                                   IItemNameColorService itemNameColorService,
                                   IEIFFileProvider eifFileProvider,
                                   ICurrentMapProvider currentMapProvider,
                                   IMapInteractionController mapInteractionController,
                                   IUserInputProvider userInputProvider,
                                   IActiveDialogProvider activeDialogProvider,
                                   IContextMenuProvider contextMenuProvider)
        {
            _mouseCursorTexture = nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 24, true);
            _characterProvider = characterProvider;
            _renderOffsetCalculator = renderOffsetCalculator;
            _mapCellStateProvider = mapCellStateProvider;
            _itemStringService = itemStringService;
            _itemNameColorService = itemNameColorService;
            _eifFileProvider = eifFileProvider;
            _currentMapProvider = currentMapProvider;
            _mapInteractionController = mapInteractionController;
            _userInputProvider = userInputProvider;
            _activeDialogProvider = activeDialogProvider;
            _contextMenuProvider = contextMenuProvider;
            SingleCursorFrameArea = new Rectangle(0, 0,
                                                  _mouseCursorTexture.Width/(int) CursorIndex.NumberOfFramesInSheet,
                                                  _mouseCursorTexture.Height);
            DrawArea = SingleCursorFrameArea;

            _mapItemText = new XNALabel(Constants.FontSize09)
            {
                Visible = false,
                Text = string.Empty,
                ForeColor = Color.White,
                AutoSize = false,
                DrawOrder = 10 //todo: make a better provider for draw orders (see also HudControlsFactory)
            };

            _clickCoordinate = Option.None<MapCoordinate>();
        }

        public override void Initialize()
        {
            _mapItemText.AddControlToDefaultGame();
        }

        #region Update and Helpers

        public override void Update(GameTime gameTime)
        {
            // prevents updates if there is a dialog
            if (!ShouldUpdate() || _activeDialogProvider.ActiveDialogs.Any(x => x.HasValue) ||
                _contextMenuProvider.ContextMenu.HasValue)
                return;

            // todo: don't do anything if there is a context menu and mouse is over context menu

            var offsetX = MainCharacterOffsetX();
            var offsetY = MainCharacterOffsetY();

            SetGridCoordsBasedOnMousePosition(offsetX, offsetY);
            UpdateDrawPostionBasedOnGridPosition(offsetX, offsetY);

            var cellState = _mapCellStateProvider.GetCellStateAt(_gridX, _gridY);
            UpdateCursorSourceRectangle(cellState);

            CheckForClicks(cellState);
        }

        private void SetGridCoordsBasedOnMousePosition(int offsetX, int offsetY)
        {
            //need to solve this system of equations to get x, y on the grid
            //(x * 32) - (y * 32) + 288 - c.OffsetX, => pixX = 32x - 32y + 288 - c.OffsetX
            //(y * 16) + (x * 16) + 144 - c.OffsetY  => 2pixY = 32y + 32x + 288 - 2c.OffsetY
            //                                         => 2pixY + pixX = 64x + 576 - c.OffsetX - 2c.OffsetY
            //                                         => 2pixY + pixX - 576 + c.OffsetX + 2c.OffsetY = 64x
            //                                         => _gridX = (pixX + 2pixY - 576 + c.OffsetX + 2c.OffsetY) / 64; <=
            //pixY = (_gridX * 16) + (_gridY * 16) + 144 - c.OffsetY =>
            //(pixY - (_gridX * 16) - 144 + c.OffsetY) / 16 = _gridY

            var mouseState = _userInputProvider.CurrentMouseState;

            var msX = mouseState.X - SingleCursorFrameArea.Width / 2;
            var msY = mouseState.Y - SingleCursorFrameArea.Height / 2;

            _gridX = (int)Math.Round((msX + 2 * msY - 576 + offsetX + 2 * offsetY) / 64.0);
            _gridY = (int)Math.Round((msY - _gridX * 16 - 144 + offsetY) / 16.0);
        }

        private void UpdateDrawPostionBasedOnGridPosition(int offsetX, int offsetY)
        {
            var drawPosition = GetDrawCoordinatesFromGridUnits(_gridX, _gridY, offsetX, offsetY);
            DrawArea = new Rectangle((int)drawPosition.X,
                                      (int)drawPosition.Y,
                                      DrawArea.Width,
                                      DrawArea.Height);
        }

        private void UpdateCursorSourceRectangle(IMapCellState cellState)
        {
            _shouldDrawCursor = true;
            _cursorIndex = CursorIndex.Standard;
            if (cellState.Character.HasValue || cellState.NPC.HasValue)
                _cursorIndex = CursorIndex.HoverNormal;
            else if (cellState.Sign.HasValue)
                _shouldDrawCursor = false;
            else if (cellState.Items.Any())
            {
                _cursorIndex = CursorIndex.HoverItem;
                UpdateMapItemLabel(Option.Some(cellState.Items.Last()));
            }
            else if (cellState.TileSpec != TileSpec.None)
                UpdateCursorIndexForTileSpec(cellState.TileSpec);

            if (!cellState.Items.Any())
                UpdateMapItemLabel(Option.None<IItem>());

            _startClickTime.MatchSome(st =>
                {
                    if ((DateTime.Now - st).TotalMilliseconds > 350)
                    {
                        _startClickTime = Option.Some(DateTime.Now);
                        _clickFrame = _clickFrame + 1;

                        if (_clickFrame != CursorIndex.ClickFirstFrame && _clickFrame != CursorIndex.ClickSecondFrame)
                        {
                            _clickFrame = CursorIndex.Standard;
                            _startClickTime = Option.None<DateTime>();
                        }
                    }
                });
        }

        private int MainCharacterOffsetX()
        {
            return _renderOffsetCalculator.CalculateOffsetX(_characterProvider.MainCharacter.RenderProperties);
        }

        private int MainCharacterOffsetY()
        {
            return _renderOffsetCalculator.CalculateOffsetY(_characterProvider.MainCharacter.RenderProperties);
        }

        //todo: this same logic is in a base map entity renderer. Maybe extract a service out.
        private static Vector2 GetDrawCoordinatesFromGridUnits(int x, int y, int cOffX, int cOffY)
        {
            return new Vector2(x*32 - y*32 + 288 - cOffX, y*16 + x*16 + 144 - cOffY);
        }

        private void UpdateMapItemLabel(Option<IItem> item)
        {
            item.Match(
                some: i =>
                {
                    var data = _eifFileProvider.EIFFile[i.ItemID];
                    var text = _itemStringService.GetStringForMapDisplay(data, i.Amount);

                    if (!_mapItemText.Visible || _mapItemText.Text != text)
                    {
                        _mapItemText.Visible = true;
                        _mapItemText.Text = text;
                        _mapItemText.ResizeBasedOnText();
                        _mapItemText.ForeColor = _itemNameColorService.GetColorForMapDisplay(data);

                        //relative to cursor DrawPosition, since this control is a parent of MapItemText
                        _mapItemText.DrawPosition = new Vector2(DrawArea.X + 32 - _mapItemText.ActualWidth / 2f,
                                                                DrawArea.Y + -_mapItemText.ActualHeight - 4);
                    }
                },
                none: () =>
                {
                    _mapItemText.Visible = false;
                    _mapItemText.Text = string.Empty;
                });
        }

        private void UpdateCursorIndexForTileSpec(TileSpec tileSpec)
        {
            switch (tileSpec)
            {
                case TileSpec.Wall:
                case TileSpec.JammedDoor:
                case TileSpec.MapEdge:
                case TileSpec.FakeWall:
                case TileSpec.VultTypo:
                    _shouldDrawCursor = false;
                    break;
                case TileSpec.Chest:
                case TileSpec.BankVault:
                case TileSpec.ChairDown:
                case TileSpec.ChairLeft:
                case TileSpec.ChairRight:
                case TileSpec.ChairUp:
                case TileSpec.ChairDownRight:
                case TileSpec.ChairUpLeft:
                case TileSpec.ChairAll:
                case TileSpec.Board1:
                case TileSpec.Board2:
                case TileSpec.Board3:
                case TileSpec.Board4:
                case TileSpec.Board5:
                case TileSpec.Board6:
                case TileSpec.Board7:
                case TileSpec.Board8:
                case TileSpec.Jukebox:
                    _cursorIndex = CursorIndex.HoverNormal;
                    break;
                case TileSpec.NPCBoundary:
                case TileSpec.Jump:
                case TileSpec.Water:
                case TileSpec.Arena:
                case TileSpec.AmbientSource:
                case TileSpec.SpikesStatic:
                case TileSpec.SpikesTrap:
                case TileSpec.SpikesTimed:
                case TileSpec.None:
                    _cursorIndex = CursorIndex.Standard;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(tileSpec), tileSpec, null);
            }
        }

        private void CheckForClicks(IMapCellState cellState)
        {
            var currentMouseState = _userInputProvider.CurrentMouseState;
            var previousMouseState = _userInputProvider.PreviousMouseState;

            // todo: some left clicks should be on the graphic itself instead of based on the grid where the cursor is (map sign, board, etc.)
            if (currentMouseState.LeftButton == ButtonState.Released &&
                previousMouseState.LeftButton == ButtonState.Pressed)
            {
                _mapInteractionController.LeftClick(cellState, Option.Some<IMouseCursorRenderer>(this));
            }
        }

        #endregion

        public void Draw(SpriteBatch spriteBatch, Vector2 additionalOffset)
        {
            if (_contextMenuProvider.ContextMenu.HasValue)
                return;

            if (_shouldDrawCursor && _gridX >= 0 && _gridY >= 0 &&
                _gridX <= _currentMapProvider.CurrentMap.Properties.Width &&
                _gridY <= _currentMapProvider.CurrentMap.Properties.Height)
            {
                spriteBatch.Draw(_mouseCursorTexture,
                                 DrawArea.Location.ToVector2() + additionalOffset,
                                 new Rectangle(SingleCursorFrameArea.Width*(int) _cursorIndex,
                                               0,
                                               SingleCursorFrameArea.Width,
                                               SingleCursorFrameArea.Height),
                                 Color.White);

                if (_startClickTime.HasValue)
                {
                    _clickCoordinate.MatchSome(c =>
                    {
                        var position = GetDrawCoordinatesFromGridUnits(c.X, c.Y, MainCharacterOffsetX(), MainCharacterOffsetY());
                        spriteBatch.Draw(_mouseCursorTexture,
                                         position + additionalOffset,
                                         SingleCursorFrameArea.WithPosition(new Vector2(SingleCursorFrameArea.Width * (int)_clickFrame, 0)),
                                         Color.FromNonPremultiplied(255, 255, 255, _clickAlpha -= 5));
                    });
                }
            }
        }

        public void AnimateClick()
        {
            if (_startClickTime.HasValue)
                return;

            _startClickTime = Option.Some(DateTime.Now);
            _clickFrame = CursorIndex.ClickFirstFrame;
            _clickAlpha = 200;
            _clickCoordinate = Option.Some(new MapCoordinate(_gridX, _gridY));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _spriteBatch.Dispose();
                _mapItemText.Dispose();
            }
        }
    }

    public interface IMouseCursorRenderer : IXNAControl, IDisposable
    {
        MapCoordinate GridCoordinates { get; }

        void Draw(SpriteBatch spriteBatch, Vector2 additionalOffset);

        void AnimateClick();
    }
}
