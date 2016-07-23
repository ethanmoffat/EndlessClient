// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EOLib.Domain.Map;
using EOLib.Domain.NPC;
using EOLib.Graphics;
using EOLib.IO;
using EOLib.IO.Map;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using IMapFile = EOLib.IO.Map.IMapFile;

namespace EndlessClient.Rendering
{
    public class MiniMapRenderer : IDisposable
    {
        /// <summary>
        /// Indices of the mini map gfx in their single texture (for source rectangle offset)
        /// </summary>
        private enum MiniMapGfx
        {
            //for drawing the lines
            UpLine = 0,
            LeftLine = 1,
            //Corner,
            Solid = 3, //indicates wall or obstacle
            Green = 4, //other player
            Red = 5, //attackable npc
            Orange = 6, //you!
            Blue = 7, //tile that you can interact with
            Purple = 8 //npc
        }

        public IMapFile Map { get; set; }

        public bool Visible { get; set; }

        private SpriteBatch _spriteBatch;
        private readonly MapRenderer _parentRenderer;

        public MiniMapRenderer(MapRenderer parentRenderer)
        {
            Map = parentRenderer.MapRef;
            _spriteBatch = new SpriteBatch(EOGame.Instance.GraphicsDevice);
            _parentRenderer = parentRenderer;
        }

        public void Draw()
        {
            if (Visible)
            {
                _drawMiniMap();
            }
        }

        private void _drawMiniMap()
        {
            Texture2D miniMapText = EOGame.Instance.GFXManager.TextureFromResource(GFXTypes.PostLoginUI, 45, true);
            Character c = OldWorld.Instance.MainPlayer.ActiveCharacter;

            _spriteBatch.Begin();
            for (int row = Math.Max(c.Y - 30, 0); row <= Math.Min(c.Y + 30, Map.Properties.Height); ++row)
            {
                for (int col = Math.Max(c.X - 30, 0); col <= Math.Min(c.X + 30, Map.Properties.Width); ++col)
                {
                    Rectangle miniMapRectSrc = new Rectangle(0, 0, miniMapText.Width / 9, miniMapText.Height);
                    bool isEdge = false;
                    Vector2 loc = _getMiniMapDrawCoordinates(col, row, c);
                    if (c.X == col && c.Y == row)
                    {
                        //draw orange thing
                        miniMapRectSrc.Offset((int)MiniMapGfx.Orange * miniMapRectSrc.Width, 0);
                    }
                    else
                    {
                        isEdge = _drawObjectAndActorIcons(col, row, ref miniMapRectSrc);
                    }

                    _drawGridBox(isEdge, miniMapText, loc, miniMapRectSrc);
                }
            }
            _spriteBatch.End();
        }

        private bool _drawObjectAndActorIcons(int col, int row, ref Rectangle miniMapRect)
        {
            bool isEdge = false;

            var info = _parentRenderer.GetTileInfo((byte)col, (byte)row);
            switch (info.ReturnType)
            {
                case TileInfoReturnType.IsTileSpec:
                    switch (info.Spec)
                    {
                        case TileSpec.FakeWall:
                        case TileSpec.Wall:
                            miniMapRect.Offset((int)MiniMapGfx.Solid * miniMapRect.Width, 0);
                            //draw block
                            break;
                        case TileSpec.BankVault:
                        case TileSpec.ChairAll:
                        case TileSpec.ChairDown:
                        case TileSpec.ChairLeft:
                        case TileSpec.ChairRight:
                        case TileSpec.ChairUp:
                        case TileSpec.ChairDownRight:
                        case TileSpec.ChairUpLeft:
                        case TileSpec.Chest:
                            //draw exclamation
                            miniMapRect.Offset((int)MiniMapGfx.Blue * miniMapRect.Width, 0);
                            break;
                        case TileSpec.MapEdge:
                            isEdge = true;
                            break;
                    }
                    break;
                case TileInfoReturnType.IsOtherNPC:
                    //draw NPC - red or purple depending on type
                    var npcInfo = (OldNPC) info.MapElement;
                    if (npcInfo.Data.Type == NPCType.Aggressive || npcInfo.Data.Type == NPCType.Passive)
                    {
                        miniMapRect.Offset((int) MiniMapGfx.Red*miniMapRect.Width, 0);
                    }
                    else
                    {
                        miniMapRect.Offset((int) MiniMapGfx.Purple*miniMapRect.Width, 0);
                    }
                    break;
                case TileInfoReturnType.IsOtherPlayer:
                    miniMapRect.Offset((int)MiniMapGfx.Green * miniMapRect.Width, 0);
                    //draw Green
                    break;
                case TileInfoReturnType.IsWarpSpec:
                    //var warpInfo = (Warp) info.MapElement;
                    //if (warpInfo.DoorType != 0)
                    //    miniMapRect.Offset((int)MiniMapGfx.Blue * miniMapRect.Width, 0);
                    break;
            }

            return isEdge;
        }

        private void _drawGridBox(bool isEdge, Texture2D miniMapText, Vector2 loc, Rectangle miniMapRect)
        {
            if (!isEdge)
            {
                _spriteBatch.Draw(miniMapText, loc,
                    new Rectangle((int)MiniMapGfx.UpLine * miniMapRect.Width, 0, miniMapRect.Width, miniMapRect.Height),
                    Color.FromNonPremultiplied(255, 255, 255, 128));
                _spriteBatch.Draw(miniMapText, loc,
                    new Rectangle((int)MiniMapGfx.LeftLine * miniMapRect.Width, 0, miniMapRect.Width, miniMapRect.Height),
                    Color.FromNonPremultiplied(255, 255, 255, 128));
            }
            _spriteBatch.Draw(miniMapText, loc, miniMapRect, Color.FromNonPremultiplied(255, 255, 255, 128));
        }

        private Vector2 _getMiniMapDrawCoordinates(int x, int y, Character c)
        {
            return new Vector2((x * 13) - (y * 13) + 288 - (c.X * 13 - c.Y * 13), (y * 7) + (x * 7) + 144 - (c.Y * 7 + c.X * 7));
        }

        ~MiniMapRenderer()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_spriteBatch != null)
                    _spriteBatch.Dispose();
                _spriteBatch = null;
            }
        }
    }
}
