using System;
using System.Linq;
using System.Threading;
using EndlessClient.Audio;
using EndlessClient.GameExecution;
using EndlessClient.Old;
using EndlessClient.Rendering.Effects;
using EndlessClient.UIControls;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Graphics;
using EOLib.IO;
using EOLib.IO.Pub;
using EOLib.Net.API;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNAControls;
using XNAControls.Old;
using LabelAlignment = XNAControls.Old.LabelAlignment;
using XNAControl = XNAControls.Old.XNAControl;
using XNALabel = XNAControls.Old.XNALabel;

namespace EndlessClient.Rendering
{
    /// <summary>
    /// HOW THIS WORKS IN-GAME
    /// <para/>
    /// <para>Character rendering in-game is controlled by the map renderer</para>
    /// <para/>
    /// <para>CharacterRenderer is never added to game components,
    /// because of the draw ordering in MapRenderer.</para>
    /// <para>So, Draw is only called from MapRenderer._doMapDrawing
    /// (Update and initialize are also called from MapRenderer)</para>
    /// </summary>
    public class OldCharacterRenderer : XNAControl
    {
        private readonly OldCharacter _char;
        public OldCharacter Character => _char;

        private readonly CharRenderData _data;
        private CharRenderData Data
        {
            get
            {
                CharRenderData data = (_char != null ? _char.RenderData : _data) ?? new CharRenderData();
                return data;
            }
        }

        //setting any of the character data will automatically load the required texture
        private RenderTarget2D _charRenderTarget; //use a render target so that a transparency is applied to entire character image
        private readonly EOSpriteSheet spriteSheet;

        public EODirection Facing
        {
            get { return Data.facing; }
            set {
                int val = (int)value;
                if (val > 3)
                    val = 0;
                Data.SetDirection((EODirection)val);
            }
        }

        /// <summary>
        /// True if walls and map edges should be ignored, false otherwise
        /// </summary>
        public bool NoWall { get; set; }

        /// <summary>
        /// True to signal update loop in map renderer that this character is dead and should be removed
        /// </summary>
        public bool CompleteDeath { get; private set; }

        private readonly object hatHairLock = new object();

        private Rectangle m_skinSourceRect;
        private Texture2D shield, weapon, boots, armor, hat;
        private Texture2D hair, characterSkin;

        private readonly XNALabel levelLabel, nameLabel;
        private Rectangle? adminRect;
        private readonly Texture2D adminGraphic;

        private EIFRecord shieldInfo, weaponInfo/*, bootsInfo, armorInfo*/, hatInfo;

        private Timer _attackTimer, _emoteTimer, _spTimer, _spellCastTimer;
        private readonly bool noLocUpdate;

        private readonly DamageCounter m_damageCounter;

        private DateTime? m_deadTime, m_lastEmoteTime;
        private DateTime m_lastActTime;

        private DateTime? m_drunkTime;
        private int m_drunkOffset;

        private DateTime? _spellInvocationStartTime;

        private CharacterActionState State => Character.State;

        public int TopPixel { get; }

        /// <summary>
        /// Construct a character renderer in-game
        /// </summary>
        /// <param name="charToRender">The character data that should be wrapped by this renderer</param>
        public OldCharacterRenderer(OldCharacter charToRender)
        {
            //this has been happening when shit gets disconnected due to invalid sequence or internal packet id
            if (charToRender == null)
            {
                EOGame.Instance.DoShowLostConnectionDialogAndReturnToMainMenu();
                return;
            }

            spriteSheet = new EOSpriteSheet(((EOGame)Game).GFXManager, charToRender);
            _char = charToRender;
            _data = charToRender.RenderData;
            Texture2D tmpSkin = spriteSheet.GetSkin(false, out m_skinSourceRect);
            if (_char != OldWorld.Instance.MainPlayer.ActiveCharacter)
            {
                drawArea = new Rectangle(
                    _char.OffsetX + 304 - OldWorld.Instance.MainPlayer.ActiveCharacter.OffsetX,
                    _char.OffsetY + 91 - OldWorld.Instance.MainPlayer.ActiveCharacter.OffsetY,
                    m_skinSourceRect.Width, m_skinSourceRect.Height); //set based on size of the sprite and location of charToRender
            }
            else
            {
                drawArea = new Rectangle((618 - m_skinSourceRect.Width) / 2 + 4, (298 - m_skinSourceRect.Height) / 2 - 29, m_skinSourceRect.Width, m_skinSourceRect.Height);
                noLocUpdate = true; //make sure not to update the drawArea rectangle in the update method
            }
            Data.SetUpdate(true);

            //get the top pixel!
            Color[] skinData = new Color[m_skinSourceRect.Width * m_skinSourceRect.Height];
            tmpSkin.GetData(0, m_skinSourceRect, skinData, 0, skinData.Length);
            int i = 0;
            while (i < skinData.Length && skinData[i].A == 0) i++;
            //account for adjustment in drawing the skin in the draw method
            TopPixel = (Data.gender == 0 ? 12 : 13) + (i == skinData.Length - 1 ? 0 : i / m_skinSourceRect.Height);

            m_damageCounter = new DamageCounter(this);
        }

        /// <summary>
        /// Construct a character renderer pre-game (character creation dialog, character list)
        /// </summary>
        /// <param name="drawLocation">Where to draw it</param>
        /// <param name="data">Render data to use for drawing</param>
        public OldCharacterRenderer(Vector2 drawLocation, CharRenderData data)
            : base(drawLocation, null)
        {
            noLocUpdate = true;
            _char = new OldCharacter(null, -1, data);
            spriteSheet = new EOSpriteSheet(((EOGame)Game).GFXManager, _char);
            //when this is a part of a dialog, the drawareaoffset will be set accordingly and is used in the draw method
            //otherwise, it will just draw it at the absolute location specified by drawArea

            drawArea = new Rectangle((int) drawLocation.X, (int) drawLocation.Y, 1, 1);
            Data.SetUpdate(true);

            if (data.name.Length > 0)
            {
                //362, 167 abs loc
                levelLabel = new XNALabel(new Rectangle(-32, 75, 1, 1), Constants.FontSize08pt75)
                {
                    ForeColor = ColorConstants.BeigeText,
                    Text = data.level.ToString()
                };
                levelLabel.SetParent(this);

                //504, 93 abs loc
                nameLabel = new XNALabel(new Rectangle(104, 2, 89, 22), Constants.FontSize08pt5)
                {
                    ForeColor = ColorConstants.BeigeText,
                    Text = ((char) (data.name[0] - 32)) + data.name.Substring(1),
                    TextAlign = LabelAlignment.MiddleCenter,
                    AutoSize = false
                };
                nameLabel.SetParent(this);

                adminGraphic = ((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.PreLoginUI, 22);
                if (data.admin == 1)
                {
                    adminRect = new Rectangle(252, 39, 17, 17);
                }
                else if (data.admin > 1)
                {
                    adminRect = new Rectangle(233, 39, 17, 17);
                }
                else
                {
                    adminRect = null;
                }
            }
        }

        public override void Initialize()
        {
            base.Initialize();
            _charRenderTarget = new RenderTarget2D(GraphicsDevice,
                GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight,
                false,
                SurfaceFormat.Color,
                DepthFormat.None);

            _attackTimer = new Timer(_attackTimerCallback);
            _emoteTimer = new Timer(_emoteTimerCallback);
            if (Character == OldWorld.Instance.MainPlayer.ActiveCharacter)
            {
                _spTimer = new Timer(o =>
                {
                    if (Character != null && Character.Stats != null)
                    {
                        if (Character.Stats.SP < Character.Stats.MaxSP)
                            Character.Stats.SP += 2;
                        if (Character.Stats.SP > Character.Stats.MaxSP)
                            Character.Stats.SP = Character.Stats.MaxSP;
                    }

                }, null, 0, 1000);
                
                _spellCastTimer = new Timer(_endSpellCast, null, Timeout.Infinite, Timeout.Infinite);
            }

            m_lastActTime = DateTime.Now;
        }

        protected override void UnloadContent()
        {
            if (_charRenderTarget != null)
                _charRenderTarget.Dispose();
            base.UnloadContent();
        }

        public override void Update(GameTime gameTime)
        {
            _checkMouseClickState();

            base.Update(gameTime);
            
            _checkUpdateDrawArea();
            _checkUpdateOnClickDrag();
            if (Data.update)
                _updateDisplayDataSprites();
            _checkBringBackFromDead();
            _checkResetCharacterStateAfterSpell();

            if (EOGame.Instance.State == GameStates.PlayingTheGame && this == OldWorld.Instance.ActiveCharacterRenderer)
            {
                _adjustSP(gameTime);
                _checkAFKCharacter();
                _checkHandleDrunkCharacter();
            }
        }

        private void _checkUpdateDrawArea()
        {
            //update the draw location when the player isn't the MainPlayer (so, if they walked)
            if (!noLocUpdate && characterSkin != null && _char != null && OldWorld.Instance.MainPlayer.ActiveCharacter != null)
                drawArea = new Rectangle(
                    _char.OffsetX + 304 - OldWorld.Instance.MainPlayer.ActiveCharacter.OffsetX,
                    _char.OffsetY + 91 - OldWorld.Instance.MainPlayer.ActiveCharacter.OffsetY,
                    m_skinSourceRect.Width, m_skinSourceRect.Height);
        }

        private void _checkUpdateOnClickDrag()
        {
            //update when the control is being dragged (when not in-game)
            if (EOGame.Instance.State != GameStates.PlayingTheGame &&
                PreviousMouseState.LeftButton == ButtonState.Pressed &&
                Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                Data.SetUpdate(true);
            }
        }

        private void _updateDisplayDataSprites()
        {
            if (Data.shield != 0)
            {
                if (OldWorld.Instance.EIF != null)
                {
                    shieldInfo = OldWorld.Instance.EIF.Data.SingleOrDefault(x => x.Type == ItemType.Shield && x.DollGraphic == Data.shield);
                    if(shieldInfo != null)
                        shield = spriteSheet.GetShield(shieldInfo.Name == "Bag" || shieldInfo.SubType == ItemSubType.Arrows || shieldInfo.SubType == ItemSubType.Wings);
                }
            }
            else
            {
                shield = null;
                shieldInfo = null;
            }

            if (Data.weapon != 0)
            {
                if (OldWorld.Instance.EIF != null)
                {
                    weaponInfo = OldWorld.Instance.EIF.Data.SingleOrDefault(x => x.Type == ItemType.Weapon && x.DollGraphic == Data.weapon);
                    if(weaponInfo != null)
                        weapon = spriteSheet.GetWeapon(weaponInfo.SubType == ItemSubType.Ranged);
                }
            }
            else
            {
                weapon = null;
                weaponInfo = null;
            }

            bool isBow = weaponInfo != null && weaponInfo.SubType == ItemSubType.Ranged;
            characterSkin = spriteSheet.GetSkin(isBow, out m_skinSourceRect);
            boots = Data.boots != 0 ? spriteSheet.GetBoots(isBow) : null;
            armor = Data.armor != 0 ? spriteSheet.GetArmor(isBow) : null;
            lock (hatHairLock)
                hair = Data.hairstyle != 0 ? spriteSheet.GetHair(Data.hairNeedRefresh) : null;
            if (Data.hat != 0)
            {
                lock (hatHairLock)
                    hat = spriteSheet.GetHat();
                if (OldWorld.Instance.EIF != null)
                    hatInfo = OldWorld.Instance.EIF.Data.SingleOrDefault(x => x.Type == ItemType.Hat && x.DollGraphic == Data.hat);
            }
            else
            {
                lock (hatHairLock)
                {
                    hat = null;
                    hatInfo = null;
                }
            }

            maskTheHair(); //this will set the combined hat/hair texture with proper data.

            _drawCharToRenderTarget();

            Data.SetUpdate(false);
            Data.SetHairNeedRefresh(false);
        }

        private void _checkBringBackFromDead()
        {
            if (m_deadTime != null && Character.RenderData.dead && (DateTime.Now - m_deadTime.Value).TotalSeconds > 2)
            {
                m_deadTime = null;
                Character.RenderData.SetDead(false);
                CompleteDeath = true;
            }
        }

        private void _checkResetCharacterStateAfterSpell()
        {
            if (_spellInvocationStartTime != null && (DateTime.Now - _spellInvocationStartTime.Value).TotalMilliseconds > 280)
            {
                Character.SetSpellCastComplete();
                _spellInvocationStartTime = null;
            }
        }

        private void _adjustSP(GameTime gameTime)
        {
            //adjust SP
            if (Character.Stats != null && Character.Stats.SP < Character.Stats.MaxSP &&
                State != CharacterActionState.Attacking && (int)gameTime.TotalGameTime.TotalMilliseconds % 1000 == 0)
                Character.Stats.SP = (short)(Character.Stats.SP + 1);
        }

        private void _checkAFKCharacter()
        {
            //5-minute timeout: start sending emotes every minute
            if ((DateTime.Now - m_lastActTime).TotalMinutes > 5 &&
                (m_lastEmoteTime == null || (DateTime.Now - m_lastEmoteTime.Value).TotalMinutes > 1))
            {
                m_lastEmoteTime = DateTime.Now;
                Character.Emote(Emote.Moon);
                PlayerEmote();
            }
        }

        private void _checkHandleDrunkCharacter()
        {
            if (m_drunkTime.HasValue && Character.IsDrunk)
            {
                //note: these timer values (between 1-6 seconds and 30 seconds) are completely arbitrary
                if (!m_lastEmoteTime.HasValue || (DateTime.Now - m_lastEmoteTime.Value).TotalMilliseconds > m_drunkOffset)
                {
                    m_lastEmoteTime = DateTime.Now;
                    Character.Emote(Emote.Drunk);
                    PlayerEmote();
                    m_drunkOffset = (new Random()).Next(1000, 6000); //between 1-6 seconds 
                }

                if ((DateTime.Now - m_drunkTime.Value).TotalMilliseconds >= 30000)
                {
                    m_drunkTime = null;
                    Character.IsDrunk = false;
                }
            }
        }

        private bool _getMouseOverActual()
        {
            var skinDrawLoc = _getSkinDrawLoc();
            var actualDrawAreaRect = new Rectangle((int)skinDrawLoc.X, (int)skinDrawLoc.Y, m_skinSourceRect.Width,
                m_skinSourceRect.Height);
            bool mouseOverActual = actualDrawAreaRect.ContainsPoint(Mouse.GetState().X, Mouse.GetState().Y);
            return mouseOverActual;
        }

        private void _checkMouseClickState()
        {
            if (!_getMouseOverActual()) return; //ignore clicks when mouse isn't over

            var currentState = Mouse.GetState();

            bool leftClicked = PreviousMouseState.LeftButton == ButtonState.Pressed &&
                               currentState.LeftButton == ButtonState.Released;
            bool rightClicked = PreviousMouseState.RightButton == ButtonState.Pressed &&
                                currentState.RightButton == ButtonState.Released;

            if (leftClicked)
            {
                if (OldWorld.Instance.MainPlayer.ActiveCharacter.NeedsSpellTarget)
                {
                    var data = OldWorld.Instance.ESF[OldWorld.Instance.MainPlayer.ActiveCharacter.SelectedSpell];
                    if (data.TargetRestrict == SpellTargetRestrict.NPCOnly || 
                        data.TargetRestrict == SpellTargetRestrict.Opponent && !OldWorld.Instance.ActiveMapRenderer.MapRef.Properties.PKAvailable)
                    {
                        //todo: status label message "(something something cannot attack player)"
                        OldWorld.Instance.MainPlayer.ActiveCharacter.SelectSpell(-1);
                    }
                    else
                    {
                        OldWorld.Instance.ActiveCharacterRenderer.SetSpellTarget(this);
                        OldWorld.Instance.ActiveCharacterRenderer._prepareSpell();
                    }
                }
            }
            //handle right-clicking a player. menu when not ActiveCharacter, paperdoll when ActiveCharacter
            else if (rightClicked)
            {
                if (this == OldWorld.Instance.ActiveCharacterRenderer)
                    ((EOGame) Game).API.RequestPaperdoll((short) Character.ID);
                else
                    OldWorld.Instance.ActiveMapRenderer.ShowContextMenu(this);
            }
        }

        public void PlayerWalk(bool isWaterTile, bool isSpikeTrap)
        {
            //if (!string.IsNullOrEmpty(_shoutName))
            //    _cancelSpell(false);

            Data.SetUpdate(true);

            if (OldWorld.Instance.SoundEnabled)
            {
                if (NoWall)
                    EOGame.Instance.SoundManager.GetSoundEffectRef(SoundEffectID.NoWallWalk).Play();
                if (isSpikeTrap)
                    EOGame.Instance.SoundManager.GetSoundEffectRef(SoundEffectID.Spikes).Play();
            }

            if (isWaterTile)
                ShowWaterSplashieAnimation();
        }

        public void PlayerAttack(bool isWaterTile)
        {
            //if (!string.IsNullOrEmpty(_shoutName))
            //    _cancelSpell(false);

            const int attackTimer = 285;
            Data.SetUpdate(true);

            if (OldWorld.Instance.SoundEnabled)
            {
                if (weaponInfo != null)
                {
                    if (weaponInfo.SubType == ItemSubType.Ranged)
                    {
                        if (weaponInfo.Name.ToLower().Contains("gun"))
                            EOGame.Instance.SoundManager.GetSoundEffectRef(SoundEffectID.Gun).Play();
                        else
                            EOGame.Instance.SoundManager.GetSoundEffectRef(SoundEffectID.AttackBow).Play();
                    }
                    else if (weaponInfo.Name.ToLower().Contains("harp"))
                    {
                        EOGame.Instance.SoundManager.GetSoundEffectRef(SoundEffectID.Harp1 + (new Random()).Next(0, 3)).Play();
                    }
                    else if (weaponInfo.Name.ToLower().Contains("guitar"))
                    {
                        EOGame.Instance.SoundManager.GetSoundEffectRef(SoundEffectID.Guitar1 + (new Random()).Next(0, 3)).Play();
                    }
                    else
                        EOGame.Instance.SoundManager.GetSoundEffectRef(SoundEffectID.MeleeWeaponAttack).Play();
                }
                else
                    EOGame.Instance.SoundManager.GetSoundEffectRef(SoundEffectID.PunchAttack).Play();
            }

            if (isWaterTile)
                ShowWaterSplashieAnimation();

            try
            {
                _attackTimer.Change(0, attackTimer);
            }
            catch (ObjectDisposedException) { }
        }

        public void PlayerEmote()
        {
            if (OldWorld.Instance.SoundEnabled && Character.RenderData.emote == Emote.LevelUp)
                EOGame.Instance.SoundManager.GetSoundEffectRef(SoundEffectID.LevelUp).Play();
            //else if (!string.IsNullOrEmpty(_shoutName))
            //    _cancelSpell(false);

            const int EmoteTimeBetweenFrames = 250;
            Data.SetUpdate(true);
            try
            {
                _emoteTimer.Change(0, EmoteTimeBetweenFrames);
            }
            catch (ObjectDisposedException) { }
        }

        public void UpdateInputTime(DateTime lastInputTime)
        {
            m_lastActTime = lastInputTime;
        }

        public void Die()
        {
            //if (!string.IsNullOrEmpty(_shoutName))
            //    _cancelSpell(false);

            if(OldWorld.Instance.SoundEnabled)
                EOGame.Instance.SoundManager.GetSoundEffectRef(SoundEffectID.Dead).Play();
            Character.RenderData.SetDead(true);
            m_deadTime = DateTime.Now;
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            Draw(SpriteBatch); //framework: draw with this instance's spritebatch
        }

        //does the draw call with a particular spritebatch (CharacterRenderer.Draw uses this with instance spritebatch)
        //started indicates that the spritebatch.begin call has already been made
        public void Draw(SpriteBatch sb, bool started = false)
        {
            if (!Visible) return;

            if (adminRect != null)
            {
                if(!started) sb.Begin();
                sb.Draw(adminGraphic, new Rectangle(DrawAreaWithOffset.X + 48, DrawAreaWithOffset.Y + 73, adminRect.Value.Width, adminRect.Value.Height), adminRect, Color.White);
                if(!started) sb.End();
            }

            if (_char == null || _char.RenderData == null) return;

            //note: if character is hidden, only draw if a) they are not active character and b) the active character is admin
            if (_char != OldWorld.Instance.MainPlayer.ActiveCharacter && _char.RenderData.hidden &&
                OldWorld.Instance.MainPlayer.ActiveCharacter.AdminLevel == AdminLevel.Player)
                return;

            if(!started) sb.Begin();
            sb.Draw(_charRenderTarget, new Vector2(0, 0),
                _char.RenderData.hidden || _char.RenderData.dead ? Color.FromNonPremultiplied(255, 255, 255, 128) : Color.White);
            if(!started) sb.End();
        }

        private void _attackTimerCallback(object state)
        {
            if (_char == null || State != CharacterActionState.Attacking) return;

            if (Data.attackFrame == 2)
            {
                _char.DoneAttacking();
                _attackTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }
            else
            {
                Data.SetAttackFrame((byte) (Data.attackFrame + 1));
            }

            Data.SetUpdate(true);
        }

        private void _emoteTimerCallback(object state)
        {
            if (_char == null) return;

            if (Data.emoteFrame == 3)
            {
                _char.DoneEmote();
                _emoteTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }
            else
            {
                Data.SetEmoteFrame(Data.emoteFrame + 1);
            }

            Data.SetUpdate(true);
        }

        //character is drawn in the following order:
        // - shield (if wings/arrows)
        // - weapon (if not melee attack frame 2 in certain directions)
        // - character body sprite
        // - boots
        // - armor
        // - weapon (if not already drawn)
        // - shield (if not already drawn)
        // - hair
        // - hat
        private void _drawCharToRenderTarget()
        {
            bool flipped = (int)Data.facing > 1; //flipped if direction is Up or Right

            try
            {
                GraphicsDevice.SetRenderTarget(_charRenderTarget);
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            if(Data.hidden && (_char == OldWorld.Instance.MainPlayer.ActiveCharacter || _char.AdminLevel != AdminLevel.Player))
                GraphicsDevice.Clear(ClearOptions.Target, Color.FromNonPremultiplied(255,255,255,100), 1, 0); //hidden players should blend nicely
            else
                GraphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 1, 0);

            SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            
            bool shieldDrawn = false, weaponDrawn = false;
            if (shield != null && !_drawShieldLater())
            {
                SpriteBatch.Draw(shield, new Vector2(DrawAreaWithOffset.X - 10, DrawAreaWithOffset.Y - 7), null, Color.White, 0.0f,
                    Vector2.Zero, 1.0f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.0f);
                shieldDrawn = true;
            }

            if (weapon != null && !_drawWeaponLater())
            {
                _drawWeapon(flipped);
                weaponDrawn = true;
            }

            if (characterSkin != null)
            {
                _drawSkin(flipped);
            }

            _drawBoots(flipped);
            _drawArmor(flipped);

            if (weapon != null && !weaponDrawn)
                _drawWeapon(flipped);

            lock (hatHairLock)
            {
                _drawHatHair(flipped, hatInfo == null || hatInfo.SubType != ItemSubType.FaceMask);
            }

            if (shield != null && !shieldDrawn)
                SpriteBatch.Draw(shield, new Vector2(DrawAreaWithOffset.X - 10, DrawAreaWithOffset.Y - 7), null, Color.White, 0.0f,
                    Vector2.Zero, 1.0f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.0f);
            SpriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);
        }

        private void _drawSkin(bool flipped)
        {
            var skinLoc = _getSkinDrawLoc();

            SpriteBatch.Draw(characterSkin, skinLoc, m_skinSourceRect, Color.White, 0f, Vector2.Zero, 1f,
                flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);

            //get face and draw
            if (Character.RenderData.emoteFrame >= 0)
            {
                Rectangle faceRect, emoteRect;
                Texture2D face = spriteSheet.GetFace(out faceRect), 
                    emote = spriteSheet.GetEmote(out emoteRect);

                if (face != null && (Facing == EODirection.Down || Facing == EODirection.Right))
                {
                    Vector2 facePos = new Vector2(skinLoc.X + (Facing == EODirection.Down ? 2 : 3), 
                        skinLoc.Y + (_data != null ? (_data.gender == 0 ? 2 : 0) : 0));
                    SpriteBatch.Draw(face, facePos, faceRect,
                        Color.White, 0f, Vector2.Zero, 1f,
                        flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
                }
                if (emote != null)
                {
                    Vector2 emotePos = new Vector2(skinLoc.X - 15, DrawAreaWithOffset.Y - emote.Height + 10);
                    SpriteBatch.Draw(emote, emotePos, emoteRect, Color.FromNonPremultiplied(0xff, 0xff, 0xff, 128));
                }
            }
        }

        private Vector2 _getSkinDrawLoc()
        {
            int skinXOff = 0, skinYOff = 0;
            Vector2 skinLoc = new Vector2(6 + DrawAreaWithOffset.X, (Data.gender == 0 ? 12 : 13) + DrawAreaWithOffset.Y);
            if (Data != null)
            {
                switch (State)
                {
                    case CharacterActionState.SpellCast:
                        skinLoc = new Vector2(skinLoc.X, skinLoc.Y - 4);
                        break;
                    case CharacterActionState.Walking:
                        if (_data != null && _data.gender == 1)
                        {
                            switch (Facing)
                            {
                                case EODirection.Down:
                                    skinXOff = -1;
                                    break;
                                case EODirection.Right:
                                    skinXOff = 1;
                                    break;
                            }
                        }
                        skinLoc = new Vector2(2 + DrawAreaWithOffset.X + skinXOff, (Data.gender == 0 ? 11 : 12) + DrawAreaWithOffset.Y);
                        break;
                    case CharacterActionState.Attacking:
                        if (weaponInfo == null || (weaponInfo != null && weaponInfo.SubType != ItemSubType.Ranged))
                        {
                            switch (Facing)
                            {
                                case EODirection.Up:
                                case EODirection.Right:
                                    skinXOff = Data.gender == 1 ? -1 : -2;
                                    if (Data.attackFrame == 2)
                                    {
                                        skinXOff += Data.gender == 1 ? 2 : 4;
                                        skinYOff += 1;
                                        if (Facing == EODirection.Up) skinYOff += -2;
                                    }
                                    break;
                                case EODirection.Down:
                                case EODirection.Left:
                                    skinXOff = Data.gender == 1 ? -5 : -4;
                                    if (Data.attackFrame == 2)
                                    {
                                        skinXOff += Data.gender == 1 ? -2 : -4;
                                        skinYOff += -1;
                                        if (Facing == EODirection.Down) skinYOff += 2;
                                    }
                                    break;
                            }
                        }
                        else if (weaponInfo != null && weaponInfo.SubType == ItemSubType.Ranged && Data.attackFrame == 1)
                        {
                            switch (Facing)
                            {
                                case EODirection.Up:
                                    skinXOff += Data.gender == 1 ? 2 : 1;
                                    break;
                                case EODirection.Right:
                                    skinXOff += Data.gender == 1 ? 4 : 3;
                                    skinYOff += 1; //Data.gender == 1 ? 1 : 1;
                                    break;
                                case EODirection.Left:
                                    skinXOff += Data.gender == 1 ? -9 : -8;
                                    break;
                                case EODirection.Down:
                                    skinXOff += Data.gender == 1 ? -11 : -10;
                                    skinYOff += 1; //Data.gender == 1 ? 1 : 1;
                                    break;
                            }
                        }
                        //else if (weaponInfo == null)
                        //{
                        //    switch (Facing)
                        //    {
                        //        case EODirection.Up:
                        //        case EODirection.Right:
                        //            skinXOff += Data.gender == 1 ? -1 : 0;
                        //            break;
                        //    }
                        //}
                        skinLoc = new Vector2(skinLoc.X + skinXOff, skinLoc.Y + skinYOff);
                        break;
                }
            }
            return skinLoc;
        }

        private bool _drawWeaponLater()
        {
            // weapon will be drawn later if:
            //  - attack frame is 2 (or data is NULL in which case ignore)
            //  - Direction is right or down
            //  - Weapon subtype is not Ranged (or weapon info is NULL in which case ignore)
            bool pass1 = Data == null || Data.attackFrame == 2;
            bool pass2 = Facing == EODirection.Down || Facing == EODirection.Right;
            bool pass3 = weaponInfo == null || weaponInfo.SubType != ItemSubType.Ranged;

            return pass1 && pass2 && pass3;
        }

        private bool _drawShieldLater()
        {
            //draw the shield last if:
            //  - Direction is up or left and shield is wings or arrows
            //  - OR shield is not wings or errors

            bool upLeft = Facing == EODirection.Left || Facing == EODirection.Up;
            //bool downRight = Facing == EODirection.Down || Facing == EODirection.Right;
            bool arrowsWings = shieldInfo != null && (shieldInfo.SubType == ItemSubType.Wings || shieldInfo.SubType == ItemSubType.Arrows);

            return (upLeft && arrowsWings) || !arrowsWings;
        }

        private void _drawWeapon(bool flipped)
        {
            if (State == CharacterActionState.Sitting || State == CharacterActionState.SpellCast)
                return;

            int xOffLoc = 0;
            if (Data != null && Data.attackFrame == 2 && weaponInfo.SubType != ItemSubType.Ranged)
            {
                if (Facing == EODirection.Up || Facing == EODirection.Right)
                    xOffLoc = Data.gender == 0 ? 2 : 4;
                if (Facing == EODirection.Down || Facing == EODirection.Left)
                    xOffLoc = Data.gender == 0 ? -2 : -4;
            }
            Vector2 loc = flipped
                ? new Vector2(DrawAreaWithOffset.X - 10 + xOffLoc, DrawAreaWithOffset.Y - 7)
                : new Vector2(DrawAreaWithOffset.X - 28 + xOffLoc, DrawAreaWithOffset.Y - 7);

            SpriteBatch.Draw(weapon, loc, null, Color.White, 0.0f,
                Vector2.Zero, 1.0f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.0f);
        }

        private void _drawArmor(bool flipped)
        {
            if (armor == null) return;

            int xAdjust = 0;
            int yAdjust = State == CharacterActionState.Walking ? -1 : 0;
            bool weaponIsMelee = weaponInfo == null || (weaponInfo != null && weaponInfo.SubType != ItemSubType.Ranged);
            if (weaponIsMelee && Data != null && Data.attackFrame == 2)
            {
                switch (Facing)
                {
                    case EODirection.Up:
                        xAdjust = Data.gender == 1 ? 6 : 7;
                        yAdjust += -1;
                        break;
                    case EODirection.Right:
                        xAdjust = Data.gender == 1 ? 6 : 7;
                        yAdjust += 1;
                        break;
                    case EODirection.Down:
                        xAdjust = Data.gender == 1 ? -6 : -7;
                        yAdjust += 1;
                        break;
                    case EODirection.Left:
                        xAdjust = Data.gender == 1 ? -6 : -7;
                        yAdjust += -1;
                        break;
                }
            }
            else if(!weaponIsMelee && Data != null && Data.attackFrame == 1)
            {
                switch (Facing)
                {
                    case EODirection.Up:
                        xAdjust = 4;
                        break;
                    case EODirection.Right:
                        xAdjust = 6;
                        yAdjust += 1;
                        break;
                    case EODirection.Down:
                        xAdjust = -6;
                        yAdjust += 1;
                        break;
                    case EODirection.Left:
                        xAdjust = -4;
                        break;
                }
            }
            SpriteBatch.Draw(armor, new Vector2(DrawAreaWithOffset.X - 2 + xAdjust, DrawAreaWithOffset.Y + yAdjust), null, Color.White, 0.0f,
                Vector2.Zero, 1.0f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.0f);
        }

        private void _drawHatHair(bool flipped, bool drawHairFirst)
        {
            if (hair == null && hat == null) return;

            bool weaponIsMelee = weaponInfo == null || (weaponInfo != null && weaponInfo.SubType != ItemSubType.Ranged);
            int hairOffX = 0, hairOffY = 0;
            Vector2 hairLoc = new Vector2(DrawAreaWithOffset.X + (flipped ? 2 : 0), DrawAreaWithOffset.Y);
            Vector2 hatLoc = new Vector2(DrawAreaWithOffset.X, DrawAreaWithOffset.Y - 3);
            if(weaponIsMelee && Data != null && Data.attackFrame == 2)
            {
                switch (Facing)
                {
                    case EODirection.Down:
                        hairOffX = -8;
                        hairOffY = Data.gender == 1 ? 5 : 6;
                        break;
                    case EODirection.Right:
                        hairOffX = 8;
                        hairOffY = Data.gender == 1 ? 5 : 6;
                        break;
                    case EODirection.Up:
                        hairOffX = Data.gender == 1 ? 6 : 8;
                        break;
                    case EODirection.Left:
                        hairOffX = Data.gender == 1 ? -6 : -8;
                        break;
                }
            }
            else if (!weaponIsMelee && Data != null && Data.attackFrame == 1)
            {
                switch (Facing)
                {
                    case EODirection.Down:
                        hairOffX = Data.gender == 1 ? -3 : -1;
                        hairOffY = Data.gender == 1 ? 1 : 0;
                        break;
                    case EODirection.Right:
                        hairOffX = Data.gender == 1 ? 3 : 1;
                        hairOffY = Data.gender == 1 ? 1 : 0;
                        break;
                    case EODirection.Up:
                        hairOffX = Data.gender == 1 ? 3 : 1;
                        break;
                    case EODirection.Left:
                        hairOffX = Data.gender == 1 ? -3 : -1;
                        break;
                }
            }

            SpriteBatch.End();

            lock (hatHairLock)
            {
                SpriteBatch.Begin();
                if (drawHairFirst)
                {
                    if (hair != null)
                        SpriteBatch.Draw(hair, new Vector2(hairLoc.X + hairOffX, hairLoc.Y + hairOffY), null, Color.White, 0.0f,
                            Vector2.Zero, 1.0f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.0f);
                    if (hat != null)
                        SpriteBatch.Draw(hat, new Vector2(hatLoc.X + hairOffX, hatLoc.Y + hairOffY), null, Color.White, 0.0f, Vector2.Zero,
                            1.0f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.0f);
                }
                else
                {
                    if (hat != null)
                        SpriteBatch.Draw(hat, new Vector2(hatLoc.X + hairOffX, hatLoc.Y + hairOffY), null, Color.White, 0.0f, Vector2.Zero,
                            1.0f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.0f);
                    if (hair != null)
                        SpriteBatch.Draw(hair, new Vector2(hairLoc.X + hairOffX, hairLoc.Y + hairOffY), null, Color.White, 0.0f,
                            Vector2.Zero, 1.0f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.0f);
                }
                SpriteBatch.End();
            }

            SpriteBatch.Begin();
        }

        private void _drawBoots(bool flipped)
        {
            if (boots == null) return;

            bool weaponIsMelee = weaponInfo == null || (weaponInfo != null && weaponInfo.SubType != ItemSubType.Ranged);
            int bootsOffX = 0, bootsOffY = 0;
            Vector2 drawLoc = new Vector2(DrawAreaWithOffset.X - 2, DrawAreaWithOffset.Y + 49);
            if (weaponIsMelee && Data != null && Data.attackFrame == 2)
            {
                switch (Facing)
                {
                    case EODirection.Down:
                        bootsOffX = -6;
                        break;
                    case EODirection.Right:
                        bootsOffX = 6;
                        break;
                    case EODirection.Up:
                        bootsOffX = 6;
                        bootsOffY = Data.gender == 1 ? -1 : 0;
                        break;
                    case EODirection.Left:
                        bootsOffX = -6;
                        bootsOffY = Data.gender == 1 ? -1 : 0;
                        break;
                }
            }
            else if(!weaponIsMelee && Data != null && Data.attackFrame == 1)
            {
                switch (Facing)
                {
                    case EODirection.Down:
                        bootsOffX = -6;
                        bootsOffY = 1;
                        break;
                    case EODirection.Right:
                        bootsOffX = 6;
                        bootsOffY = 1;
                        break;
                    case EODirection.Up:
                        bootsOffX = Data.gender == 1 ? 7 : 3;
                        break;
                    case EODirection.Left:
                        bootsOffX = Data.gender == 1 ? -7 : -3;
                        break;
                }
            }

            SpriteBatch.Draw(boots, new Vector2(drawLoc.X + bootsOffX, drawLoc.Y + bootsOffY), null, Color.White, 0.0f, Vector2.Zero, 1.0f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.0f);
        }

        private void maskTheHair()
        {
            if (Data.hat == 0)
                return;

            Color[] hatPixels;
            //if (OldWorld.Instance.EIF != null && OldWorld.Instance.EIF.Version > 0)
            {
                switch (hatInfo.SubType)
                {
                    case ItemSubType.HideHair: //anything matching ^[A-Za-z ]*[Hh]elm[A-Za-z ]*$ or ^[A-Za-z ]*[Hh]ood[A-Za-z ]*$, or id=314 (pirate hat)
                        hair = null;
                        if (hat != null)
                        {
                            hatPixels = new Color[hat.Width * hat.Height];
                            hat.GetData(hatPixels);
                            for (int i = 0; i < hatPixels.Length; ++i)
                                if (hatPixels[i] == Color.Black) hatPixels[i] = Color.Transparent;
                            hat.SetData(hatPixels);
                        }
                        return;
                    case ItemSubType.FaceMask: //Frog Head, glasses, and Dragon Mask (anything with mask in the name, really)
                        if (hatInfo.Name.ToLower() == "glasses")
                        { //special case for glasses, hooray hard-coding :/
                            Color[] glassesData = new Color[hat.Width * hat.Height];
                            hat.GetData(glassesData);
                            for (int i = 0; i < glassesData.Length; ++i)
                                if (glassesData[i] == Color.Black) glassesData[i] = Color.Transparent;
                            hat.SetData(glassesData);
                        }
                        return;
                }
            }
            
            if (Data.facing == EODirection.Left || Data.facing == EODirection.Up || hair == null)
            {
                lock (hatHairLock)
                {
                    hatPixels = new Color[hat.Width*hat.Height];
                    hat.GetData(hatPixels);
                    for (int i = 0; i < hatPixels.Length; ++i)
                        if (hatPixels[i] == Color.Black) hatPixels[i] = Color.Transparent;
                    hat.SetData(hatPixels);
                }
                return; //don't clip if left or up - this game is so screwy. Make the black color transparent.
            }

            hatPixels = new Color[hat.Width*hat.Height];
            Color[] hairPixels = new Color[hair.Width * hair.Height];
            hat.GetData(hatPixels, 0, hatPixels.Length);
            hair.GetData(hairPixels, 0, hairPixels.Length);
            
            for (int i = 0; i < hat.Height; ++i)
            {
                for (int j = 0; j < hat.Width; ++j)
                {
                    int _1d = i*hat.Width + j;
                    if (hatPixels[_1d].R == 0 && hatPixels[_1d].G == 0 && hatPixels[_1d].B == 0 && hatPixels[_1d].A != 0)
                    {
                        hatPixels[_1d] = new Color(0, 0, 0, 0);
                        _1d = i*hair.Width + j;
                        if (_1d < hairPixels.Length)
                            hairPixels[_1d] = new Color(0, 0, 0, 0);
                    }
                }
            }

            try
            {
                lock (hatHairLock)
                {
                    hat.SetData(hatPixels);
                    hair.SetData(hairPixels);
                }
            }
            catch (InvalidOperationException) { }
        }

        public void HideChatBubble()
        {
        }

        public void SetDamageCounterValue(int value, int pctHealth, bool isHeal = false)
        {
            m_damageCounter.SetValue(value, pctHealth, isHeal);
        }

        public void MakeDrunk()
        {
            m_drunkTime = DateTime.Now;
            Character.IsDrunk = true;
        }

        #region Spell Casting

        //Workflow for spells (main player):
        // - F-key pressed, calls SelectSpell
        // - Goes into target mode - on target select, calls _prepareSpell
        // - Prepare spell starts shout and timer callback (_beginSpellCast)
        // - end of timer callback does actual spell cast
        // - attacking, moving, emoting, or dying cancel the spell cast (_cancelSpell)
        /*Workflow for other players:
           - Spell request packet - show shout name
           - Spell cast packet - show animation
           - other logic for different spell types, etc.
        */
        public void SelectSpell(int spellIndex)
        {
            var toCast = ((EOGame)Game).Hud.GetSpellFromIndex(spellIndex);
            if (toCast == null) return;

            Character.SelectSpell(toCast.ID);
            if (toCast.Target == SpellTarget.Self ||
                (toCast.Target == SpellTarget.Group && ((EOGame) Game).Hud.MainPlayerIsInParty()))
                _prepareSpell();
        }

        public void SetSpellTarget(DrawableGameComponent target)
        {
            Character.SetSpellTarget(target);
            _prepareSpell();
        }

        private void _cancelSpell(bool completedPrep)
        {
            _spellCastTimer.Change(Timeout.Infinite, Timeout.Infinite);
            //if completedPrep is true make it darker briefly and then hide it
            StopShouting(completedPrep);

            Character.SelectSpell(-1);
            Character.SetSpellTarget(null);

            if (!completedPrep)
                Character.CancelSpellPrepare();
        }

        private void _prepareSpell()
        {
            if (Character.SelectedSpell <= 0)
                throw new InvalidOperationException("You must call SelectSpell before calling _prepareSpell");

            var toCast = OldWorld.Instance.ESF[Character.SelectedSpell];

            Character.PrepareSpell(toCast.ID);
            _beginSpellCast(toCast);

            //SetSpellShout(toCast.Shout);
        }

        private void _beginSpellCast(ESFRecord spell)
        {
            if (Character.SelectedSpell <= 0)
                throw new InvalidOperationException("You must have a selected spell before casting (race condition?");

            var castTime = (int)(Math.Round(spell.CastTime / 2.0 * 950)); //this probably needs different math
            _spellCastTimer.Change(castTime, Timeout.Infinite);
        }

        private void _endSpellCast(object state)
        {
            if (Character.SelectedSpell <= 0)
                return;

            Character.CastSpell(Character.SelectedSpell);
            _cancelSpell(true);
            _spellInvocationStartTime = DateTime.Now;
        }

        public void StopShouting(bool isSpellBeingCast)
        {
            //if (!isSpellBeingCast || OldWorld.Instance.ESF[Character.SelectedSpell].Target == SpellTarget.Self || Character.SpellTarget == this)
            //{
            //    _mouseoverName.BlinkRate = null;
            //    _mouseoverName.Text = Character.Name;
            //    _mouseoverName.ForeColor = Color.White;
            //    _mouseoverName.Visible = false;
            //    _shoutName = null;
            //    return;
            //}

            //_mouseoverName.Visible = true;
            //_mouseoverName.BlinkRate = null;
            //_mouseoverName.ForeColor = Color.FromNonPremultiplied(0xf5, 0xc8, 0x9c, 0xff);
            //_mouseoverName.SetCallback(600, () => StopShouting(false));
        }

        public void StartCastingSpell()
        {
            _spellInvocationStartTime = DateTime.Now;
            Character.SetSpellCastStart();
        }

        #endregion

        #region Effect Rendering

        public void ShowWarpArrive()
        {
            //ResetEffectRenderer();
            //RenderEffect(EffectType.WarpDestination);
        }

        public void ShowWarpLeave()
        {
            //ResetEffectRenderer();
            //RenderEffect(EffectType.WarpOriginal, 0, Close);
        }

        public void ShowPotionAnimation(int potionID)
        {
            //var hud = ((EOGame)Game).Hud;
            //hud.DisableEffectPotionUse();

            //ResetEffectRenderer();
            //RenderEffect(EffectType.Potion, potionID, hud.EnableEffectPotionUse);
        }

        public void ShowSpellAnimation(int spellGraphicID)
        {
            //ResetEffectRenderer();
            //RenderEffect(EffectType.Spell, spellGraphicID);
        }

        //only used in CharacterRenderer
        private void ShowWaterSplashieAnimation()
        {
            //if (HasExistingWaterEffect())
            //{
            //    _effectRenderer.Restart();
            //    return;
            //}

            //ResetEffectRenderer();
            //RenderEffect(EffectType.WaterSplashies);
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (levelLabel != null)
                    levelLabel.Dispose();
                if (nameLabel != null)
                    nameLabel.Dispose();
                if (_attackTimer != null)
                    _attackTimer.Dispose();
                if (_emoteTimer != null)
                    _emoteTimer.Dispose();
                if (_spTimer != null)
                    _spTimer.Dispose();
                if (_charRenderTarget != null)
                    _charRenderTarget.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
