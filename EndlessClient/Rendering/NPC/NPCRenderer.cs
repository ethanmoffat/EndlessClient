// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Linq;
using EndlessClient.GameExecution;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.Sprites;
using EOLib;
using EOLib.Domain.Extensions;
using EOLib.Domain.NPC;
using EOLib.IO.Repositories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.NPC
{
    public class NPCRenderer : DrawableGameComponent, INPCRenderer
    {
        private readonly ICharacterRendererProvider _characterRendererProvider;
        private readonly IENFFileProvider _enfFileProvider;
        private readonly INPCSpriteSheet _npcSpriteSheet;
        private readonly IRenderOffsetCalculator _renderOffsetCalculator;
        private readonly Rectangle _baseTextureFrameRectangle;
        private readonly int _readonlyTopPixel;
        private readonly bool _hasStandingAnimation;

        public int TopPixel { get { return _readonlyTopPixel; } }

        public Rectangle DrawArea { get; private set; }

        public Rectangle MapProjectedDrawArea { get; private set; }

        public INPC NPC { get; set; }

        public NPCRenderer(IEndlessGameProvider endlessGameProvider,
                           ICharacterRendererProvider characterRendererProvider,
                           IENFFileProvider enfFileProvider,
                           INPCSpriteSheet npcSpriteSheet,
                           IRenderOffsetCalculator renderOffsetCalculator)
            : base((Game)endlessGameProvider.Game)
        {
            _characterRendererProvider = characterRendererProvider;
            _enfFileProvider = enfFileProvider;
            _npcSpriteSheet = npcSpriteSheet;
            _renderOffsetCalculator = renderOffsetCalculator;

            _baseTextureFrameRectangle = GetStandingFrameRectangle();
            _readonlyTopPixel = GetTopPixel();
            _hasStandingAnimation = GetHasStandingAnimation();
        }

        public override void Initialize()
        {
            UpdateDrawAreas();

            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            if (!Visible) return;

            base.Update(gameTime);
        }

        public void DrawToSpriteBatch(SpriteBatch spriteBatch)
        {
            if (!Visible) return;

            var data = _enfFileProvider.ENFFile[NPC.ID];

            //todo: fade out when dying
            var color = /*NPC.Dying ? Color.FromNonPremultiplied(255, 255, 255, _fadeAwayAlpha -= 3) :*/ Color.White;
            var effects = NPC.IsFacing(EODirection.Left, EODirection.Down) ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            spriteBatch.Begin();
            spriteBatch.Draw(_npcSpriteSheet.GetNPCTexture(data.Graphic, NPC.Frame, NPC.Direction),
                DrawArea, null, color, 0f, Vector2.Zero, effects, 1f);
            spriteBatch.End();
        }

        private Rectangle GetStandingFrameRectangle()
        {
            var data = _enfFileProvider.ENFFile[NPC.ID];
            var baseFrame = _npcSpriteSheet.GetNPCTexture(data.Graphic, NPCFrame.Standing, EODirection.Down);
            return new Rectangle(0, 0, baseFrame.Width, baseFrame.Height);
        }

        private int GetTopPixel()
        {
            var data = _enfFileProvider.ENFFile[NPC.ID];
            var frameTexture = _npcSpriteSheet.GetNPCTexture(data.Graphic, NPCFrame.Standing, EODirection.Down);
            var frameTextureData = new Color[frameTexture.Width * frameTexture.Height];
            frameTexture.GetData(frameTextureData);

            if (frameTextureData.All(x => x.A == 0))
                return 0;

            var firstVisiblePixelIndex = frameTextureData.Select((color, index) => new { color, index })
                                                            .Where(x => x.color.A != 0)
                                                            .Select(x => x.index)
                                                            .First();
            return firstVisiblePixelIndex/frameTexture.Height;
        }

        private bool GetHasStandingAnimation()
        {
            var data = _enfFileProvider.ENFFile[NPC.ID];

            var frameTexture = _npcSpriteSheet.GetNPCTexture(data.Graphic, NPCFrame.StandingFrame1, NPC.Direction);
            var textureData = new Color[frameTexture.Width * frameTexture.Height];
            frameTexture.GetData(textureData);

            return textureData.Any(color => (color.R > 0 || color.G > 0 || color.B > 0) && color.A > 0);
        }

        private void UpdateDrawAreas()
        {
            var offsetX = _renderOffsetCalculator.CalculateOffsetX(NPC);
            var offsetY = _renderOffsetCalculator.CalculateOffsetY(NPC);

            var mainRenderer = _characterRendererProvider.MainCharacterRenderer;
            var mainOffsetX = _renderOffsetCalculator.CalculateOffsetX(mainRenderer.RenderProperties);
            var mainOffsetY = _renderOffsetCalculator.CalculateOffsetY(mainRenderer.RenderProperties);

            DrawArea = new Rectangle(
                //not sure where magic numbers 6.4 and 3.2 come from. They seem important.
                offsetX + 320 - mainOffsetX - (int)(_baseTextureFrameRectangle.Width / 6.4 * 3.2),
                offsetY + 168 - mainOffsetY - _baseTextureFrameRectangle.Height,
                _baseTextureFrameRectangle.Width, _baseTextureFrameRectangle.Height);

            var oneGridSize = new Vector2(mainRenderer.DrawArea.Width,
                                          mainRenderer.DrawArea.Height);
            MapProjectedDrawArea = new Rectangle(
                DrawArea.X + (int)(Math.Abs(oneGridSize.X - DrawArea.Width) / 2),
                DrawArea.Bottom - (int)oneGridSize.Y,
                (int)oneGridSize.X,
                (int)oneGridSize.Y);
        }
    }
}
