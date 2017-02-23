// Original Work Copyright (c) Ethan Moffat 2014-2017
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Linq;
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
using XNAControls;

namespace EndlessClient.Rendering
{
    public class MouseCursorRenderer : IMouseCursorRenderer
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
        private readonly IEIFFileProvider _eifFileProvider;
        private readonly ICurrentMapProvider _currentMapProvider;
        private readonly XNALabel _mapItemText;

        private readonly SpriteBatch _spriteBatch;

        private Rectangle _drawArea;
        private int _gridX, _gridY;
        private CursorIndex _cursorIndex;
        private bool _shouldDrawCursor;

        public MouseCursorRenderer(INativeGraphicsManager nativeGraphicsManager,
                                   ICharacterProvider characterProvider,
                                   IRenderOffsetCalculator renderOffsetCalculator,
                                   IMapCellStateProvider mapCellStateProvider,
                                   IItemStringService itemStringService,
                                   IEIFFileProvider eifFileProvider,
                                   ICurrentMapProvider currentMapProvider,
                                   IGraphicsDeviceProvider graphicsDeviceProvider)
        {
            _mouseCursorTexture = nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 24, true);
            _characterProvider = characterProvider;
            _renderOffsetCalculator = renderOffsetCalculator;
            _mapCellStateProvider = mapCellStateProvider;
            _itemStringService = itemStringService;
            _eifFileProvider = eifFileProvider;
            _currentMapProvider = currentMapProvider;

            SingleCursorFrameArea = new Rectangle(0, 0,
                                                  _mouseCursorTexture.Width/(int) CursorIndex.NumberOfFramesInSheet,
                                                  _mouseCursorTexture.Height);
            _drawArea = SingleCursorFrameArea;

            _mapItemText = new XNALabel(Constants.FontSize08pt75)
            {
                Visible = false,
                Text = string.Empty,
                ForeColor = Color.White,
                AutoSize = false,
            };

            _spriteBatch = new SpriteBatch(graphicsDeviceProvider.GraphicsDevice);
        }

        public void Initialize()
        {
            _mapItemText.Initialize();
        }

        #region Update and Helpers

        public void Update(GameTime gameTime)
        {
            //todo: don't do anything if there are dialogs or a context menu and mouse is over context menu

            var offsetX = MainCharacterOffsetX();
            var offsetY = MainCharacterOffsetY();

            SetGridCoordsBasedOnMousePosition(offsetX, offsetY);
            UpdateDrawPostionBasedOnGridPosition(offsetX, offsetY);

            var cellState = _mapCellStateProvider.GetCellStateAt(_gridX, _gridY);
            UpdateCursorSourceRectangle(cellState);

            _mapItemText.Update(gameTime);
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

            var mouseState = Mouse.GetState();

            var msX = mouseState.X - SingleCursorFrameArea.Width / 2;
            var msY = mouseState.Y - SingleCursorFrameArea.Height / 2;

            _gridX = (int)Math.Round((msX + 2 * msY - 576 + offsetX + 2 * offsetY) / 64.0);
            _gridY = (int)Math.Round((msY - _gridX * 16 - 144 + offsetY) / 16.0);
        }

        private void UpdateDrawPostionBasedOnGridPosition(int offsetX, int offsetY)
        {
            var drawPosition = GetDrawCoordinatesFromGridUnits(_gridX, _gridY, offsetX, offsetY);
            _drawArea = new Rectangle((int)drawPosition.X,
                                      (int)drawPosition.Y,
                                      _drawArea.Width,
                                      _drawArea.Height);
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
                UpdateMapItemLabel(new Optional<IItem>(cellState.Items.Last()));
            }
            else if (cellState.TileSpec != TileSpec.None)
                UpdateCursorIndexForTileSpec(cellState.TileSpec);

            if (!cellState.Items.Any())
                UpdateMapItemLabel(Optional<IItem>.Empty);
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

        private void UpdateMapItemLabel(Optional<IItem> item)
        {
            if (!item.HasValue)
            {
                _mapItemText.Visible = false;
                _mapItemText.Text = string.Empty;
            }
            else if (!_mapItemText.Visible)
            {
                _mapItemText.Visible = true;
                _mapItemText.Text = _itemStringService.GetStringForMapDisplay(
                    _eifFileProvider.EIFFile[item.Value.ItemID], item.Value.Amount);
                _mapItemText.ResizeBasedOnText();
                _mapItemText.ForeColor = GetColorForMapDisplay(_eifFileProvider.EIFFile[item.Value.ItemID]);

                //relative to cursor DrawPosition, since this control is a parent of MapItemText
                _mapItemText.DrawPosition = new Vector2(32 - _mapItemText.ActualWidth/2f,
                                                        -_mapItemText.ActualHeight - 4);
            }
        }

        private void UpdateCursorIndexForTileSpec(TileSpec tileSpec)
        {
            switch (tileSpec)
            {
                case TileSpec.Wall:
                case TileSpec.JammedDoor:
                case TileSpec.MapEdge:
                case TileSpec.FakeWall:
                case TileSpec.NPCBoundary:
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

        //todo: extract this into a service (also used by inventory)
        private static Color GetColorForMapDisplay(EIFRecord record)
        {
            switch (record.Special)
            {
                case ItemSpecial.Lore:
                case ItemSpecial.Unique:
                    return Color.FromNonPremultiplied(0xff, 0xf0, 0xa5, 0xff);
                case ItemSpecial.Rare:
                    return Color.FromNonPremultiplied(0xf5, 0xc8, 0x9c, 0xff);
            }

            return Color.White;
        }

        #endregion

        public void Draw(GameTime gameTime)
        {
            //todo: don't draw if context menu is visible and mouse is over the context menu

            if (_shouldDrawCursor && _gridX >= 0 && _gridY >= 0 &&
                _gridX <= _currentMapProvider.CurrentMap.Properties.Width &&
                _gridY <= _currentMapProvider.CurrentMap.Properties.Height)
            {
                _spriteBatch.Begin();
                _spriteBatch.Draw(_mouseCursorTexture,
                                  _drawArea,
                                  new Rectangle(SingleCursorFrameArea.Width*(int) _cursorIndex,
                                                0,
                                                SingleCursorFrameArea.Width,
                                                SingleCursorFrameArea.Height),
                                  Color.White);
                _spriteBatch.End();
            }

            _mapItemText.Draw(gameTime);
        }

        public void Dispose()
        {
            _spriteBatch.Dispose();
            _mapItemText.Dispose();
        }
    }

    public interface IMouseCursorRenderer : IDisposable
    {
        void Initialize();

        void Update(GameTime gameTime);

        void Draw(GameTime gameTime);
    }
}
