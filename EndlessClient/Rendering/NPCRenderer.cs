// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Linq;
using EndlessClient.Dialogs;
using EndlessClient.Rendering.Effects;
using EndlessClient.Rendering.Sprites;
using EOLib;
using EOLib.Domain.NPC;
using EOLib.IO;
using EOLib.Net.API;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNAControls;

namespace EndlessClient.Rendering
{
    //Map NPCs are coupled with draw/update operations
    //Note: this is like character, in that it isn't actually drawn or updated by the framework
    //        Map Renderer handles all draw/update operations
    public class NPCRenderer : DrawableGameComponent
    {
        /* Default NPC speed table for eoserv, corresponding to the speed stored in the NPCRenderer spawn */
        /* Not sure if this is ever transferred to the client at all, but it is a config value... */
        //private static readonly  int[] SPEED_TABLE = {900, 600, 1300, 1900, 3700, 7500, 15000, 0};

        public OldNPC NPC { get; private set; }

        /// <summary>
        /// The actual target draw rectangle of the NPC
        /// </summary>
        public Rectangle DrawArea { get; private set; }

        /// <summary>
        /// Represents the current map location (scaled to one human character, not NPC size).
        /// <para>Since most NPCs are graphically larger/smaller than one "slot" based on character size,</para>
        ///    <para>    this helps determine what the normal size would be if it were a character sprite.</para>
        /// </summary>
        public Rectangle MapProjectedDrawArea { get; private set; }

        /// <summary>
        /// The Y coordinate of the first non-transparent pixel on the NPC's default sprite
        /// </summary>
        public int TopPixel { get; private set; }

        /// <summary>
        /// The current state of the NPC; determines which sprite to draw
        /// </summary>
        public NPCFrame Frame { get; private set; }

        //drawing related members
        private readonly NPCSpriteSheet _npcSheet;
        private SpriteBatch _sb;
        private Rectangle _npcTextureFrameRectangle;
        private bool hasStandFrame1;
        private int _fadeAwayAlpha;
        
        private EffectRenderer _effectRenderer;

        private int DrawOffsetX { get { return NPC.X * 32 - NPC.Y * 32 + walkingAdjustedX; } }
        private int DrawOffsetY { get { return NPC.X * 16 + NPC.Y * 16 + walkingAdjustedY; } }

        //"child" controls added to this NPC
        private readonly EOChatBubble _chatBubble;
        private readonly DamageCounter m_damageCounter;
        private XNALabel _mouseoverName;

        //update related members
        private int walkingAdjustedX, walkingAdjustedY;
        private DateTime _actionStartTime;
        private DateTime _lastAnimUpdateTime;
        private MouseState _prevMouseState, _currMouseState;

        public NPCRenderer(OldNPC npc)
            : base(EOGame.Instance)
        {
            NPC = npc;
            _fadeAwayAlpha = 255;
            _actionStartTime = DateTime.Now;
            _lastAnimUpdateTime = DateTime.Now;

            _npcSheet = new NPCSpriteSheet(((EOGame)Game).GFXManager, this);

            _chatBubble = new EOChatBubble(this);
            m_damageCounter = new DamageCounter(this);
            CreateMouseoverName();
        }

        #region Game Component Overrides

        public override void Initialize()
        {
            base.Initialize(); //required to use the GraphicsDevice property

            InitializeTopPixel();
            InitializeStandingFrame1();

            _sb = new SpriteBatch(GraphicsDevice);

            Frame = NPCFrame.Standing;

            var baseFrame = _npcSheet.GetNPCTexture();
            _npcTextureFrameRectangle = new Rectangle(0, 0, baseFrame.Width, baseFrame.Height);
            UpdateDrawArea();

            _chatBubble.Initialize();
            _chatBubble.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            if (!Visible) return;

            UpdateDrawArea();

            UpdateStandingFrameIfNeeded();
            UpdateWalkFrameIfNeeded();
            UpdateAttackFrameIfNeeded();
            UpdateEffectAnimation();

            if (Game.IsActive)
            {
                _currMouseState = Mouse.GetState();
                UpdateMouseoverName();
                HandleLeftClick();
                _prevMouseState = _currMouseState;
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (!Visible) return;

            DrawToSpriteBatch(_sb);
            base.Draw(gameTime);
        }

        public void DrawToSpriteBatch(SpriteBatch batch, bool started = false)
        {
            if (_effectRenderer != null)
                _effectRenderer.DrawBehindTarget(batch, started);

            if (!started)
                batch.Begin();

            SpriteEffects effects = NPC.Direction == EODirection.Left || NPC.Direction == EODirection.Down
                ? SpriteEffects.None
                : SpriteEffects.FlipHorizontally;

            Color col = NPC.Dying ? Color.FromNonPremultiplied(255, 255, 255, _fadeAwayAlpha -= 3) : Color.White;

            batch.Draw(_npcSheet.GetNPCTexture(),
                DrawArea,
                null,
                col,
                0f,
                Vector2.Zero,
                effects,
                1f);

            if (NPC.Dying && _fadeAwayAlpha <= 0)
                NPC.EndDying();

            if (!started)
                batch.End();

            if (_effectRenderer != null)
                _effectRenderer.DrawInFrontOfTarget(batch, started);
        }

        #endregion

        #region Public Interface

        public void Walk(byte x, byte y, EODirection dir)
        {
            if (NPC.Walking) return;

            _actionStartTime = DateTime.Now;
            NPC.BeginWalking(dir, x, y);
        }

        public void Attack(EODirection dir)
        {
            if (NPC.Attacking) return;

            _actionStartTime = DateTime.Now;
            NPC.BeginAttacking(dir);
        }

        public void SetChatBubbleText(string message, bool isGroupChat)
        {
            _chatBubble.SetMessage(message, isGroupChat);
        }

        public void HideChatBubble()
        {
            _chatBubble.HideBubble();
        }

        public void TakeDamageFrom(OldCharacter opponent, int damage, int pctHealth)
        {
            m_damageCounter.SetValue(damage, pctHealth); //NPCs don't know heal spells
            NPC.SetOpponent(opponent);
        }

        public void Kill()
        {
            NPC.BeginDying();

            if (_mouseoverName != null)
            {
                _mouseoverName.Close();
                _mouseoverName = null;
            }
        }

        public void ShowSpellAnimation(int spellGraphicID)
        {
            ResetEffectRenderer();
            RenderEffect(EffectType.Spell, spellGraphicID);
        }

        private void RenderEffect(EffectType effectType, int effectID)
        {
            var gfxManager = ((EOGame)Game).GFXManager;
            _effectRenderer = new EffectRenderer(gfxManager, this, () => _effectRenderer = null);
            _effectRenderer.SetEffectInfoTypeAndID(effectType, effectID);
            _effectRenderer.ShowEffect();
        }

        private void ResetEffectRenderer()
        {
            if (_effectRenderer != null)
                _effectRenderer.Dispose();
        }

        #endregion

        #region Helpers

        private void InitializeTopPixel()
        {
            int tries;
            for (tries = 0; tries < 3; ++tries)
            {
                try
                {
                    //get the first non-transparent pixel to determine offsets for name labels and damage counters
                    Frame = NPCFrame.Standing;

                    var frameTexture = _npcSheet.GetNPCTexture();
                    var frameTextureData = new Color[frameTexture.Width * frameTexture.Height];
                    frameTexture.GetData(frameTextureData);

                    if (frameTextureData.All(x => x.A == 0))
                        TopPixel = 0;
                    else
                    {
                        var firstVisiblePixelIndex = frameTextureData.Select((color, index) => new { color, index })
                                                                     .Where(x => x.color.R != 0)
                                                                     .Select(x => x.index)
                                                                     .First();
                        TopPixel = firstVisiblePixelIndex/frameTexture.Height;
                    }
                } //this block throws errors sometimes..no idea why. It usually doesn't fail 3 times.
                catch (InvalidOperationException) { continue; }

                break;
            }

            if (tries >= 3)
                throw new InvalidOperationException("Something weird happened initializing this NPC.");
        }

        private void InitializeStandingFrame1()
        {
            //attempt to get standing frame 1. It will have non-black pixels if it exists.
            Frame = NPCFrame.StandingFrame1;

            Texture2D frameTexture = _npcSheet.GetNPCTexture();
            Color[] textureData = new Color[frameTexture.Width * frameTexture.Height];
            frameTexture.GetData(textureData);

            hasStandFrame1 = textureData.Any(_c => _c.R != 0 || _c.G != 0 || _c.B != 0);
        }

        private void CreateMouseoverName()
        {
            _mouseoverName = new XNALabel(new Rectangle(1, 1, 1, 1), Constants.FontSize08pt75)
            {
                Visible = false,
                Text = NPC.Data.Name,
                ForeColor = Color.White,
                AutoSize = false,
                DrawOrder = (int)ControlDrawLayer.BaseLayer + 3
            };
            _mouseoverName.DrawLocation = new Vector2(
                DrawArea.X + (DrawArea.Width - _mouseoverName.ActualWidth) / 2f,
                DrawArea.Y + TopPixel - _mouseoverName.ActualHeight - 4);
            _mouseoverName.ResizeBasedOnText();
        }

        private void UpdateDrawArea()
        {
            DrawArea = new Rectangle(
                DrawOffsetX + 320 - OldWorld.Instance.MainPlayer.ActiveCharacter.OffsetX - (int)(_npcTextureFrameRectangle.Width / 6.4 * 3.2),
                DrawOffsetY + 168 - OldWorld.Instance.MainPlayer.ActiveCharacter.OffsetY - _npcTextureFrameRectangle.Height,
                _npcTextureFrameRectangle.Width, _npcTextureFrameRectangle.Height);
            
            var oneGridSize = new Vector2(OldWorld.Instance.ActiveCharacterRenderer.DrawArea.Width,
                                          OldWorld.Instance.ActiveCharacterRenderer.DrawArea.Height);
            MapProjectedDrawArea = new Rectangle(
                DrawArea.X + (int) (Math.Abs(oneGridSize.X - DrawArea.Width)/2),
                DrawArea.Bottom - (int) oneGridSize.Y,
                (int)oneGridSize.X,
                (int)oneGridSize.Y);
        }

        private void UpdateStandingFrameIfNeeded()
        {
            if (NPC.Walking || NPC.Attacking || !hasStandFrame1)
                return;

            //switch the standing animation for NPCs every 500ms, if they're standing still
            if ((DateTime.Now - _lastAnimUpdateTime).TotalMilliseconds > 250)
            {
                if (Frame == NPCFrame.Standing)
                {
                    Frame = NPCFrame.StandingFrame1;
                }
                else if (Frame == NPCFrame.StandingFrame1)
                {
                    Frame = NPCFrame.Standing;
                }
                _lastAnimUpdateTime = DateTime.Now;
            }
        }

        private void UpdateMouseoverName()
        {
            if (_mouseoverName == null) return;

            _mouseoverName.Visible = DrawArea.ContainsPoint(_currMouseState.X, _currMouseState.Y);
            _mouseoverName.DrawLocation = new Vector2(
                DrawArea.X + (DrawArea.Width - _mouseoverName.ActualWidth) / 2f,
                DrawArea.Y + TopPixel - _mouseoverName.ActualHeight - 4);
        }

        private void HandleLeftClick()
        {
            bool mouseClicked = _currMouseState.LeftButton == ButtonState.Released &&
                                _prevMouseState.LeftButton == ButtonState.Pressed;

            if (mouseClicked && DrawArea.ContainsPoint(_currMouseState.X, _currMouseState.Y))
            {
                if (OldWorld.Instance.MainPlayer.ActiveCharacter.NeedsSpellTarget)
                {
                    var data = OldWorld.Instance.ESF[OldWorld.Instance.MainPlayer.ActiveCharacter.SelectedSpell];
                    if (data.TargetRestrict != SpellTargetRestrict.Friendly)
                    {
                        OldWorld.Instance.ActiveCharacterRenderer.SetSpellTarget(this);
                    }
                    else
                    {
                        //todo status label message "you cannot attack this NPC"
                        OldWorld.Instance.MainPlayer.ActiveCharacter.SelectSpell(-1);
                    }

                    return; //don't process regular click on NPC while targeting a spell
                }

                PacketAPI api = ((EOGame)Game).API;
                switch (NPC.Data.Type)
                {
                    case NPCType.Shop: ShopDialog.Show(api, this); break;
                    case NPCType.Inn: break;
                    case NPCType.Bank: BankAccountDialog.Show(api, NPC.Index); break;
                    case NPCType.Barber: break;
                    case NPCType.Guild: break;
                    case NPCType.Priest: break;
                    case NPCType.Law: break;
                    case NPCType.Skills: SkillmasterDialog.Show(api, NPC.Index); break;
                    case NPCType.Quest: QuestDialog.Show(api, NPC.Index, NPC.Data.VendorID, NPC.Data.Name); break;
                }
            }
        }

        private void UpdateWalkFrameIfNeeded()
        {
            if (!NPC.Walking || (DateTime.Now - _actionStartTime).TotalMilliseconds < 100)
                return;
            _actionStartTime = DateTime.Now;

            switch (NPC.Direction)
            {
                case EODirection.Down: walkingAdjustedX += -8; walkingAdjustedY += 4; break;
                case EODirection.Left: walkingAdjustedX += -8; walkingAdjustedY += -4; break;
                case EODirection.Up: walkingAdjustedX += 8; walkingAdjustedY += -4; break;
                case EODirection.Right: walkingAdjustedX += 8; walkingAdjustedY += 4; break;
            }

            switch (Frame)
            {
                case NPCFrame.Standing:
                case NPCFrame.StandingFrame1:
                    Frame = NPCFrame.WalkFrame1;
                    break;
                case NPCFrame.WalkFrame1:
                    Frame = NPCFrame.WalkFrame2;
                    break;
                case NPCFrame.WalkFrame2:
                    Frame = NPCFrame.WalkFrame3;
                    break;
                case NPCFrame.WalkFrame3:
                    Frame = NPCFrame.WalkFrame4;
                    break;
                case NPCFrame.WalkFrame4:
                    Frame = NPCFrame.Standing;
                    NPC.EndWalking();
                    walkingAdjustedX = walkingAdjustedY = 0;
                    break;
            }
        }

        private void UpdateAttackFrameIfNeeded()
        {
            if (!NPC.Attacking || (DateTime.Now - _actionStartTime).TotalMilliseconds < 100)
                return;
            _actionStartTime = DateTime.Now;

            switch (Frame)
            {
                case NPCFrame.Standing:
                case NPCFrame.StandingFrame1:
                    Frame = NPCFrame.Attack1;
                    break;
                case NPCFrame.Attack1:
                    Frame = NPCFrame.Attack2;
                    break;
                case NPCFrame.Attack2:
                    Frame = NPCFrame.Standing;
                    NPC.EndAttacking();
                    break;
            }
        }

        private void UpdateEffectAnimation()
        {
            if (_effectRenderer != null)
                _effectRenderer.Update();
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_sb != null)
                    _sb.Dispose();

                if (_chatBubble != null)
                    _chatBubble.Dispose();

                if (_mouseoverName != null)
                    _mouseoverName.Close();
            }

            base.Dispose(disposing);
        }
    }
}
