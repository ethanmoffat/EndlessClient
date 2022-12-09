﻿using EndlessClient.Controllers;
using EndlessClient.GameExecution;
using EndlessClient.HUD.Spells;
using EndlessClient.Input;
using EndlessClient.Rendering.Chat;
using EndlessClient.Rendering.Effects;
using EndlessClient.Rendering.Factories;
using EndlessClient.Rendering.Sprites;
using EOLib;
using EOLib.Domain.Extensions;
using EOLib.Domain.NPC;
using EOLib.Domain.Spells;
using EOLib.Graphics;
using EOLib.IO.Repositories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Optional;
using System;
using XNAControls;

namespace EndlessClient.Rendering.NPC
{
    public class NPCRenderer : DrawableGameComponent, INPCRenderer
    {
        private readonly IENFFileProvider _enfFileProvider;
        private readonly INPCSpriteSheet _npcSpriteSheet;
        private readonly IGridDrawCoordinateCalculator _gridDrawCoordinateCalculator;
        private readonly IHealthBarRendererFactory _healthBarRendererFactory;
        private readonly IChatBubbleFactory _chatBubbleFactory;
        private readonly IRenderTargetFactory _renderTargetFactory;
        private readonly INPCInteractionController _npcInteractionController;
        private readonly IMapInteractionController _mapInteractionController;
        private readonly IUserInputProvider _userInputProvider;
        private readonly ISpellSlotDataProvider _spellSlotDataProvider;
        private readonly IEffectRenderer _effectRenderer;
        private readonly IHealthBarRenderer _healthBarRenderer;

        private RenderTarget2D _npcRenderTarget;
        private SpriteBatch _spriteBatch;

        private DateTime _lastStandingAnimation;
        private int _fadeAwayAlpha;
        private bool _isDying, _isBlankSprite;

        private XNALabel _nameLabel;
        private IChatBubble _chatBubble;

        public int NameLabelY { get; private set; }

        public int HorizontalCenter { get; private set; }

        public bool IsAlive => !_isDying && !IsDead;

        public Rectangle DrawArea { get; private set; }

        public bool MouseOver => DrawArea.Contains(_userInputProvider.CurrentMouseState.Position);

        public bool MouseOverPreviously => DrawArea.Contains(_userInputProvider.PreviousMouseState.Position);

        public EOLib.Domain.NPC.NPC NPC { get; set; }

        public ISpellTargetable SpellTarget => NPC;

        public bool IsDead { get; private set; }

        public Rectangle EffectTargetArea { get; private set; }

        public NPCRenderer(IEndlessGameProvider endlessGameProvider,
                           IENFFileProvider enfFileProvider,
                           INPCSpriteSheet npcSpriteSheet,
                           IGridDrawCoordinateCalculator gridDrawCoordinateCalculator,
                           IHealthBarRendererFactory healthBarRendererFactory,
                           IChatBubbleFactory chatBubbleFactory,
                           IRenderTargetFactory renderTargetFactory,
                           INPCInteractionController npcInteractionController,
                           IMapInteractionController mapInteractionController,
                           IUserInputProvider userInputProvider,
                           ISpellSlotDataProvider spellSlotDataProvider,
                           IEffectRendererFactory effectRendererFactory,
                           EOLib.Domain.NPC.NPC initialNPC)
            : base((Game)endlessGameProvider.Game)
        {
            NPC = initialNPC;

            _enfFileProvider = enfFileProvider;
            _npcSpriteSheet = npcSpriteSheet;
            _gridDrawCoordinateCalculator = gridDrawCoordinateCalculator;
            _healthBarRendererFactory = healthBarRendererFactory;
            _chatBubbleFactory = chatBubbleFactory;
            _renderTargetFactory = renderTargetFactory;
            _npcInteractionController = npcInteractionController;
            _mapInteractionController = mapInteractionController;
            _userInputProvider = userInputProvider;
            _spellSlotDataProvider = spellSlotDataProvider;
            _effectRenderer = effectRendererFactory.Create();

            DrawArea = GetStandingFrameRectangle();

            _lastStandingAnimation = DateTime.Now;
            _fadeAwayAlpha = 255;

            _healthBarRenderer = _healthBarRendererFactory.CreateHealthBarRenderer(this);
        }

        public override void Initialize()
        {
            UpdateDrawAreas();

            _nameLabel = new XNALabel(Constants.FontSize08pt5)
            {
                Visible = false,
                TextWidth = 89,
                TextAlign = LabelAlignment.MiddleCenter,
                ForeColor = Color.White,
                AutoSize = true,
                Text = _enfFileProvider.ENFFile[NPC.ID].Name,
                DrawOrder = 30,
                KeepInClientWindowBounds = false,
            };
            _nameLabel.Initialize();

            if (!_nameLabel.Game.Components.Contains(_nameLabel))
                _nameLabel.Game.Components.Add(_nameLabel);

            _nameLabel.DrawPosition = GetNameLabelPosition();

            _npcRenderTarget = _renderTargetFactory.CreateRenderTarget(640, 480);
            _spriteBatch = new SpriteBatch(Game.GraphicsDevice);

            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            if (!Visible) return;

            UpdateDrawAreas();
            UpdateStandingFrameAnimation();
            UpdateDeadState();
            DrawToRenderTarget();

            var currentMousePosition = _userInputProvider.CurrentMouseState.Position;
            var currentFrame = _npcSpriteSheet.GetNPCTexture(_enfFileProvider.ENFFile[NPC.ID].Graphic, NPC.Frame, NPC.Direction);

            if (DrawArea.Contains(currentMousePosition))
            {
                var colorData = new Color[] { Color.FromNonPremultiplied(0, 0, 0, 255) };
                if (currentFrame != null && !_isBlankSprite)
                {
                    if (_npcRenderTarget.Bounds.Contains(currentMousePosition))
                        _npcRenderTarget.GetData(0, new Rectangle(currentMousePosition.X, currentMousePosition.Y, 1, 1), colorData, 0, 1);
                }

                var chatBubbleIsVisible = _chatBubble != null && _chatBubble.Visible;
                _nameLabel.Visible = !_healthBarRenderer.Visible && !chatBubbleIsVisible && !_isDying && (_isBlankSprite || colorData[0].A > 0);
                _nameLabel.DrawPosition = GetNameLabelPosition();

                if (!_userInputProvider.ClickHandled &&
                    _userInputProvider.CurrentMouseState.LeftButton == ButtonState.Released &&
                    _userInputProvider.PreviousMouseState.LeftButton == ButtonState.Pressed)
                {
                    if (_spellSlotDataProvider.SpellIsPrepared)
                    {
                        _mapInteractionController.LeftClick(NPC);
                    }
                    else
                    {
                        if (_isBlankSprite || colorData[0].A > 0)
                        {
                            _npcInteractionController.ShowNPCDialog(NPC);
                        }
                    }
                }
            }
            else
            {
                _nameLabel.Visible = false;
            }

            _effectRenderer.Update();
            _healthBarRenderer.Update(gameTime);

            base.Update(gameTime);
        }

        public void DrawToSpriteBatch(SpriteBatch spriteBatch)
        {
            if (!Visible) return;

            _effectRenderer.DrawBehindTarget(spriteBatch);
            if (_npcRenderTarget != null)
                spriteBatch.Draw(_npcRenderTarget, Vector2.Zero, Color.White);
            _effectRenderer.DrawInFrontOfTarget(spriteBatch);

            _healthBarRenderer.DrawToSpriteBatch(spriteBatch);
        }

        public void StartDying()
        {
            _isDying = true;
        }

        public void ShowDamageCounter(int damage, int percentHealth, bool isHeal)
        {
            var optionalDamage = damage.SomeWhen(d => d > 0);
            _healthBarRenderer.SetDamage(optionalDamage, percentHealth, () => _chatBubble?.Show());
            _chatBubble?.Hide();
        }

        public void ShowChatBubble(string message, bool isGroupChat)
        {
            if (_chatBubble == null)
                _chatBubble = _chatBubbleFactory.CreateChatBubble(this);
            _chatBubble.SetMessage(message, isGroupChat: false);
        }

        #region Effects

        public bool EffectIsPlaying()
        {
            return _effectRenderer.State == EffectState.Playing;
        }

        public void PlayEffect(int graphic)
        {
            _effectRenderer.PlayEffect(graphic, this);
        }

        #endregion

        private Rectangle GetStandingFrameRectangle()
        {
            var data = _enfFileProvider.ENFFile[NPC.ID];
            var baseFrame = _npcSpriteSheet.GetNPCTexture(data.Graphic, NPCFrame.Standing, EODirection.Down);
            return new Rectangle(0, 0, baseFrame.Width, baseFrame.Height);
        }

        private void UpdateDrawAreas()
        {
            var data = _enfFileProvider.ENFFile[NPC.ID];
            var frameTexture = _npcSpriteSheet.GetNPCTexture(data.Graphic, NPC.Frame, NPC.Direction);
            var metaData = _npcSpriteSheet.GetNPCMetadata(data.Graphic);

            var isUpOrRight = NPC.IsFacing(EODirection.Up, EODirection.Right) ? -1 : 1;
            var isDownOrRight = NPC.IsFacing(EODirection.Down, EODirection.Right) ? -1 : 1;

            int metaDataOffsetX, metaDataOffsetY;
            if (NPC.Frame == NPCFrame.Attack2)
            {
                metaDataOffsetX = metaData.AttackOffsetX * isUpOrRight + (metaData.OffsetX * isUpOrRight);
                metaDataOffsetY = metaData.AttackOffsetY * isDownOrRight - metaData.OffsetY;
            }
            else
            {
                metaDataOffsetX = metaData.OffsetX * isUpOrRight;
                metaDataOffsetY = -metaData.OffsetY;
            }

            var renderCoordinates = _gridDrawCoordinateCalculator.CalculateDrawCoordinates(NPC) +
                new Vector2(metaDataOffsetX - frameTexture.Width / 2, metaDataOffsetY - (frameTexture.Height - 23));
            DrawArea = frameTexture.Bounds.WithPosition(renderCoordinates);

            var horizontalOffset = _npcSpriteSheet.GetNPCMetadata(data.Graphic).OffsetX * (NPC.IsFacing(EODirection.Down, EODirection.Left) ? -1 : 1);
            HorizontalCenter = DrawArea.X + (DrawArea.Width / 2) + horizontalOffset;

            NameLabelY = DrawArea.Y - (int)(_nameLabel?.ActualHeight + 4 ?? 0);

            EffectTargetArea = DrawArea.WithSize(DrawArea.Width + horizontalOffset * 2, DrawArea.Height);
        }

        private void UpdateStandingFrameAnimation()
        {
            var now = DateTime.Now;

            var data = _enfFileProvider.ENFFile[NPC.ID];
            var metaData = _npcSpriteSheet.GetNPCMetadata(data.Graphic);

            if (!metaData.HasStandingFrameAnimation
                || !NPC.IsActing(NPCActionState.Standing)
                || (now - _lastStandingAnimation).TotalMilliseconds < 250)
                return;

            _lastStandingAnimation = now;
            NPC = NPC.WithFrame(NPC.Frame == NPCFrame.Standing ? NPCFrame.StandingFrame1 : NPCFrame.Standing);
        }

        private void UpdateDeadState()
        {
            if (!_isDying) return;

            if (_fadeAwayAlpha >= 3)
                _fadeAwayAlpha -= 3;
            IsDead = _fadeAwayAlpha <= 0 && !EffectIsPlaying();
        }

        private void DrawToRenderTarget()
        {
            if (_npcRenderTarget == null)
                return;

            var data = _enfFileProvider.ENFFile[NPC.ID];
            var texture = _npcSpriteSheet.GetNPCTexture(data.Graphic, NPC.Frame, NPC.Direction);

            var color = Color.FromNonPremultiplied(255, 255, 255, _fadeAwayAlpha);
            var effects = NPC.IsFacing(EODirection.Left, EODirection.Down) ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            GraphicsDevice.SetRenderTarget(_npcRenderTarget);
            GraphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 1, 0);

            _spriteBatch.Begin();
            _spriteBatch.Draw(texture, DrawArea, null, color, 0, Vector2.Zero, effects, 1);
            _spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);
        }

        private Vector2 GetNameLabelPosition()
        {
            return new Vector2(HorizontalCenter - (_nameLabel.ActualWidth / 2f), NameLabelY);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _nameLabel.Dispose();
                _chatBubble?.Dispose();
                _spriteBatch?.Dispose();
                _npcRenderTarget?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
