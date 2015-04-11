using System;
using System.Threading;
using EndlessClient.Handlers;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using XNAControls;
using EOLib;
using EOLib.Data;

namespace EndlessClient
{
	public enum CharacterActionState
	{
		Standing,
		Walking,
		Attacking,
		Sitting,
		SpellCast,
		Emote
	}

	/// <summary>
	/// HOW THIS WORKS IN-GAME
	/// <para/>
	/// <para>Character rendering in-game is controlled by the map renderer</para>
	/// <para/>
	/// <para>EOCharacterRenderer is never added to game components,
	/// because of the draw ordering in EOMapRenderer.</para>
	/// <para>So, Draw is only called from EOMapRenderer._doMapDrawing
	/// (Update and initialize are also called from EOMapRenderer)</para>
	/// </summary>
	public class EOCharacterRenderer : XNAControl
	{
		private readonly Character _char;
		public Character Character { get { return _char; } }

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

		public byte HairColor
		{
			get { return Data.haircolor; }
			set
			{
				Data.SetHairColor(value);
				if (Data.hairstyle != 0)
				{
					hair = spriteSheet.GetHair(true);
					maskTheHair();
				}
			}
		}
		public byte HairType
		{
			get { return Data.hairstyle; }
			set
			{
				Data.SetHairStyle(value);
				if (Data.hairstyle != 0)
				{
					hair = spriteSheet.GetHair(true);
					maskTheHair();
				}
			}
		}
		public byte Gender
		{
			get { return Data.gender; }
			set
			{
				Data.SetGender(value);
			}
		}
		public byte SkinColor
		{
			get { return Data.race; }
			set
			{
				Data.SetRace(value);
				characterSkin = spriteSheet.GetSkin(weaponInfo != null && weaponInfo.SubType == ItemSubType.Ranged, out m_skinSourceRect);
			}
		}
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

		private readonly object hatHairLock = new object();

		private Rectangle m_skinSourceRect;
		private Texture2D shield, weapon, boots, armor, hat;
		private Texture2D hair, characterSkin;

		private readonly XNALabel levelLabel, nameLabel;
		private Rectangle? adminRect;
		private readonly Texture2D adminGraphic;

		private ItemRecord shieldInfo, weaponInfo/*, bootsInfo, armorInfo*/, hatInfo;

		private KeyboardState _prevKeyState;
		private Timer _walkTimer, _attackTimer, _emoteTimer, _spTimer;
		private readonly bool noLocUpdate;

		private readonly EOChatBubble m_chatBubble;
		private readonly DamageCounter m_damageCounter;

		private GameTime startWalkingThroughPlayerTime;
		private DateTime? m_deadTime, m_lastEmoteTime;
		private DateTime m_lastActTime;

		private DateTime? m_drunkTime;
		private int m_drunkOffset;

		private CharacterActionState State
		{
			get { return Character.State; }
		}

		public int TopPixel { get; private set; }

		/// <summary>
		/// Construct a character renderer in-game
		/// </summary>
		/// <param name="charToRender">The character data that should be wrapped by this renderer</param>
		public EOCharacterRenderer(Character charToRender)
		{
			//this has been happening when shit gets disconnected due to invalid sequence or internal packet id
			if (charToRender == null)
			{
				EOGame.Instance.LostConnectionDialog();
				return;
			}

			spriteSheet = new EOSpriteSheet(charToRender);
			_char = charToRender;
			_data = charToRender.RenderData;
			Texture2D tmpSkin = spriteSheet.GetSkin(false, out m_skinSourceRect);
			if (_char != World.Instance.MainPlayer.ActiveCharacter)
			{
				drawArea = new Rectangle(
					_char.OffsetX + 304 - World.Instance.MainPlayer.ActiveCharacter.OffsetX,
					_char.OffsetY + 91 - World.Instance.MainPlayer.ActiveCharacter.OffsetY,
					m_skinSourceRect.Width, m_skinSourceRect.Height); //set based on size of the sprite and location of charToRender
			}
			else
			{
				drawArea = new Rectangle((618 - m_skinSourceRect.Width) / 2 + 4, (298 - m_skinSourceRect.Height) / 2 - 29, m_skinSourceRect.Width, m_skinSourceRect.Height);
				noLocUpdate = true; //make sure not to update the drawArea rectangle in the update method
			}
			Data.SetUpdate(true);
			_prevKeyState = Keyboard.GetState();

			//get the top pixel!
			Color[] skinData = new Color[m_skinSourceRect.Width * m_skinSourceRect.Height];
			tmpSkin.GetData(0, m_skinSourceRect, skinData, 0, skinData.Length);
			int i = 0;
			while (i < skinData.Length && skinData[i].A == 0) i++;
			//account for adjustment in drawing the skin in the draw method
			TopPixel = (Data.gender == 0 ? 12 : 13) + (i == skinData.Length - 1 ? 0 : i / m_skinSourceRect.Height);

			m_chatBubble = new EOChatBubble(this);
			m_damageCounter = new DamageCounter(this, GetType());
		}

		/// <summary>
		/// Construct a character renderer pre-game (character creation dialog, character list)
		/// </summary>
		/// <param name="drawLocation">Where to draw it</param>
		/// <param name="data">Render data to use for drawing</param>
		public EOCharacterRenderer(Vector2 drawLocation, CharRenderData data)
			: base(drawLocation, null)
		{
			noLocUpdate = true;
			_char = new Character(-1, data);
			spriteSheet = new EOSpriteSheet(_char);
			//when this is a part of a dialog, the drawareaoffset will be set accordingly and is used in the draw method
			//otherwise, it will just draw it at the absolute location specified by drawArea

			drawArea = new Rectangle((int) drawLocation.X, (int) drawLocation.Y, 1, 1);
			Data.SetUpdate(true);

			if (data.name.Length > 0)
			{
				//362, 167 abs loc
				levelLabel = new XNALabel(new Rectangle(-32, 75, 1, 1), "Microsoft Sans Serif", 8.75f)
				{
					ForeColor = System.Drawing.Color.FromArgb(0xFF, 0xb4, 0xa0, 0x8c),
// ReSharper disable SpecifyACultureInStringConversionExplicitly
					Text = data.level.ToString()
// ReSharper restore SpecifyACultureInStringConversionExplicitly
				};
				levelLabel.SetParent(this);

				//504, 93 abs loc
				nameLabel = new XNALabel(new Rectangle(104, 2, 89, 22), "Microsoft Sans Serif", 8.5f)
				{
					ForeColor = System.Drawing.Color.FromArgb(0xFF, 0xb4, 0xa0, 0x8c),
					Text = ((char) (data.name[0] - 32)) + data.name.Substring(1),
					TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
					AutoSize = false
				};
				nameLabel.SetParent(this);

				adminGraphic = GFXLoader.TextureFromResource(GFXTypes.PreLoginUI, 22);
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

			_walkTimer = new Timer(_walkTimerCallback); //wait a minute. I'm the leader. I'll say when it's time to start.
			_attackTimer = new Timer(_attackTimerCallback);
			_emoteTimer = new Timer(_emoteTimerCallback);
			if (Character == World.Instance.MainPlayer.ActiveCharacter)
			{
				_spTimer = new Timer(o =>
				{
					if (Character != null && Character.Stats != null &&
						Character.Stats.sp < Character.Stats.maxsp)
						Character.Stats.sp += 2;
				}, null, 0, 1000);
			}

			if (m_chatBubble != null) //this will be null when constructed during menu time
			{
				m_chatBubble.Initialize();
				m_chatBubble.LoadContent();
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
			base.Update(gameTime);
			
			//update the draw location when the player isn't the MainPlayer (so, if they walked)
			if (!noLocUpdate && characterSkin != null && _char != null && World.Instance.MainPlayer.ActiveCharacter != null)
				drawArea = new Rectangle(
					_char.OffsetX + 304 - World.Instance.MainPlayer.ActiveCharacter.OffsetX,
					_char.OffsetY + 91 - World.Instance.MainPlayer.ActiveCharacter.OffsetY,
					characterSkin.Width, characterSkin.Height);

			//update when the control is being dragged (when not in-game)
			if (EOGame.Instance.State != GameStates.PlayingTheGame &&
			    PreviousMouseState.LeftButton == ButtonState.Pressed &&
			    Mouse.GetState().LeftButton == ButtonState.Pressed)
			{
				Data.SetUpdate(true);
			}

			#region refresh all the textures from the GFX files or image cache
			if (Data.update)
			{
				if (Data.shield != 0)
				{
					if (World.Instance.EIF != null)
					{
						shieldInfo = (ItemRecord)World.Instance.EIF.Data.Find(x => (x as ItemRecord != null) && (x as ItemRecord).DollGraphic == Data.shield && (x as ItemRecord).Type == ItemType.Shield);
						shield = spriteSheet.GetShield(shieldInfo.SubType == ItemSubType.Arrows || shieldInfo.SubType == ItemSubType.Wings);
					}
				}
				else
				{
					shield = null;
					shieldInfo = null;
				}

				if (Data.weapon != 0)
				{
					if (World.Instance.EIF != null)
					{
						weaponInfo =
							(ItemRecord)
								World.Instance.EIF.Data.Find(
									x =>
										(x as ItemRecord != null) && (x as ItemRecord).DollGraphic == Data.weapon &&
										(x as ItemRecord).Type == ItemType.Weapon);
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
					if(World.Instance.EIF != null)
						hatInfo = (ItemRecord)World.Instance.EIF.Data.Find(x => (x as ItemRecord != null) && (x as ItemRecord).DollGraphic == Data.hat && (x as ItemRecord).Type == ItemType.Hat);
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
			#endregion

			//bring back from the dead after 2 seconds
			if (m_deadTime != null && Character.RenderData.dead && (DateTime.Now - m_deadTime.Value).TotalSeconds > 2)
			{
				m_deadTime = null;
				Character.RenderData.SetDead(false);
			}

			if (EOGame.Instance.State == GameStates.PlayingTheGame && this == World.Instance.ActiveCharacterRenderer)
			{
				//adjust SP
				if (Character.Stats != null && Character.Stats.sp < Character.Stats.maxsp &&
				    State != CharacterActionState.Attacking && (int) gameTime.TotalGameTime.TotalMilliseconds%1000 == 0)
					Character.Stats.SetSP((short) (Character.Stats.sp + 1));

				//5-minute timeout: start sending emotes every minute
				if ((DateTime.Now - m_lastActTime).TotalMilliseconds > 300000 &&
				    (m_lastEmoteTime == null || (DateTime.Now - m_lastEmoteTime.Value).TotalMilliseconds > 60000))
				{
					m_lastEmoteTime = DateTime.Now;
					Character.Emote(Emote.Moon);
					PlayerEmote();
				}

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

			#region input handling for keyboard
			//only check for a keypress if not currently acting and if this is the active character renderer
			//also only check every 1/4 of a second
			KeyboardState currentKeyState = Keyboard.GetState();
			if (Game.IsActive && _char != null && _char == World.Instance.MainPlayer.ActiveCharacter && gameTime.TotalGameTime.Milliseconds % 100 <= 25 && Dialogs.Count == 0)
			{
				EODirection direction = (EODirection)255; //first, get the direction we should try to move based on the key presses from the player
				bool attacking = false;
				if (currentKeyState.IsKeyDown(Keys.Up) && _prevKeyState.IsKeyDown(Keys.Up))
					direction = EODirection.Up;
				else if (currentKeyState.IsKeyDown(Keys.Down) && _prevKeyState.IsKeyDown(Keys.Down))
					direction = EODirection.Down;
				else if (currentKeyState.IsKeyDown(Keys.Left) && _prevKeyState.IsKeyDown(Keys.Left))
					direction = EODirection.Left;
				else if (currentKeyState.IsKeyDown(Keys.Right) && _prevKeyState.IsKeyDown(Keys.Right))
					direction = EODirection.Right;
				else if ((currentKeyState.IsKeyDown(Keys.LeftControl) || currentKeyState.IsKeyDown(Keys.RightControl)) &&
				         (_prevKeyState.IsKeyDown(Keys.LeftControl) || _prevKeyState.IsKeyDown(Keys.RightControl)))
				{
					attacking = true;
					direction = _char.RenderData.facing;
					startWalkingThroughPlayerTime = null;
				}
				else
				{
					//on 'else': code path should return without doing anything
					_checkAndHandleEmote(currentKeyState);
					startWalkingThroughPlayerTime = null;
				}
				
				byte destX, destY;
				switch (direction)
				{
					case EODirection.Up:
						destX = (byte) _char.X;
						destY = (byte) (_char.Y - 1);
						break;
					case EODirection.Down:
						destX = (byte) _char.X;
						destY = (byte) (_char.Y + 1);
						break;
					case EODirection.Right:
						destX = (byte) (_char.X + 1);
						destY = (byte) _char.Y;
						break;
					case EODirection.Left:
						destX = (byte) (_char.X - 1);
						destY = (byte) _char.Y;
						break;
					default:
						if (State != CharacterActionState.Walking)
							_prevKeyState = currentKeyState; //only set this when not walking already
						destX = destY = 255;
						break;
				}

				if (destX > World.Instance.ActiveMapRenderer.MapRef.Width || destY > World.Instance.ActiveMapRenderer.MapRef.Height)
				{
					//this will execute when the direction above is invalid.
					//so, if not attacking and not walking we will hit this.
					return;
				}
				
				//reset the sleeping emote time trackers
				m_lastActTime = DateTime.Now;
				m_lastEmoteTime = null;

				if (!attacking)
				{
					//valid direction at this point
					if (_char.RenderData.facing != direction)
					{
						_char.Face(direction);
						return;
					}

					TileInfo info = World.Instance.ActiveMapRenderer.CheckCoordinates(destX, destY);
					switch (info.ReturnValue)
					{
						case TileInfo.ReturnType.IsOtherPlayer:
							if (NoWall) goto case TileInfo.ReturnType.IsTileSpec;

							EOGame.Instance.Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_ACTION, DATCONST2.STATUS_LABEL_KEEP_MOVING_THROUGH_PLAYER);
							if(startWalkingThroughPlayerTime == null)
								startWalkingThroughPlayerTime = gameTime;
							else if ((gameTime.TotalGameTime.TotalSeconds - startWalkingThroughPlayerTime.TotalGameTime.TotalSeconds) > 3)
							{
								startWalkingThroughPlayerTime = null;
								goto case TileInfo.ReturnType.IsTileSpec;
							}
							break;
						case TileInfo.ReturnType.IsOtherNPC:
							if (NoWall) goto case TileInfo.ReturnType.IsTileSpec;
#if DEBUG
							EOGame.Instance.Hud.SetStatusLabel("OTHER NPC IS HERE"); //idk what's supposed to happen here, I think nothing?
#endif
							break;
						case TileInfo.ReturnType.IsWarpSpec:
							if (NoWall) goto case TileInfo.ReturnType.IsTileSpec;
							if (info.Warp.door != 0)
							{
								if (!info.Warp.doorOpened && !info.Warp.backOff)
								{
									Door.DoorOpen(destX, destY); //just do it...no checking yet, really
									info.Warp.backOff = true; //set flag to prevent hella door packets from the client
								}
								else
								{
									//normal walking
									_chkWalk(TileSpec.None, direction, destX, destY);
								}
							}
							else if (info.Warp.levelRequirement != 0 && Character.Stats.level < info.Warp.levelRequirement)
							{
								EOGame.Instance.Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_WARNING, 
									DATCONST2.STATUS_LABEL_NOT_READY_TO_USE_ENTRANCE,
									" - LVL " + info.Warp.levelRequirement);
							}
							else
							{
								//normal walking
								_chkWalk(TileSpec.None, direction, destX, destY);
							}
							break;
						case TileInfo.ReturnType.IsTileSpec:
							_chkWalk(info.Spec, direction, destX, destY);
							break;
					}
				}
				else if(State == CharacterActionState.Standing)
				{
					if (Character.CanAttack)
					{
						Character.Attack(Data.facing, destX, destY); //destX and destY validity check above
						PlayerAttack();
					}
					else if(Character.Weight > Character.MaxWeight)
					{
						EOGame.Instance.Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_WARNING, DATCONST2.STATUS_LABEL_CANNOT_ATTACK_OVERWEIGHT);
					}
				}

				if (State == CharacterActionState.Standing) _prevKeyState = currentKeyState; //only set this when not walking already
			}
#endregion
		}
		
		//convenience wrapper
		private void _chkWalk(TileSpec spec, EODirection dir, byte destX, byte destY)
		{
			bool walkValid = true;
			switch (spec)
			{
				case TileSpec.ChairDown: //todo: make character sit in chairs
				case TileSpec.ChairLeft:
				case TileSpec.ChairRight:
				case TileSpec.ChairUp:
				case TileSpec.ChairDownRight:
				case TileSpec.ChairUpLeft:
				case TileSpec.ChairAll:
					walkValid = NoWall;
					break;
				case TileSpec.Chest:
					walkValid = NoWall;
					if (!walkValid)
					{
						MapChest chest = World.Instance.ActiveMapRenderer.MapRef.Chests.Find(_c => _c.x == destX && _c.y == destY);
						if(chest.x == destX && chest.y == destY)
							EOChestDialog.Show(chest.x, chest.y);
					}
					break;
				case TileSpec.BankVault:
					walkValid = NoWall;
					if (!walkValid)
					{
						EOBankVaultDialog.Show(destX, destY);
					}
					break;
				case TileSpec.Board1: //todo: boards?
				case TileSpec.Board2:
				case TileSpec.Board3:
				case TileSpec.Board4:
				case TileSpec.Board5:
				case TileSpec.Board6:
				case TileSpec.Board7:
				case TileSpec.Board8:
					walkValid = NoWall;
					break;
				case TileSpec.Jukebox: //todo: jukebox?
					walkValid = NoWall;
					break;
				case TileSpec.MapEdge:
				case TileSpec.Wall:
					walkValid = NoWall;
					break;
			}

			if (State != CharacterActionState.Walking && walkValid)
			{
				_char.Walk(dir, destX, destY, NoWall);
				PlayerWalk(spec == TileSpec.Water);
			}
		}

		//convenience wrapper - update block is getting unruly
		private void _checkAndHandleEmote(KeyboardState state)
		{
			if (State != CharacterActionState.Standing)
				return;

			Emote em;
			if (state.IsKeyUp(Keys.NumPad0) && _prevKeyState.IsKeyDown(Keys.NumPad0))
			{
				em = Emote.Playful;
				Character.Emote(em);
				PlayerEmote();
			}
			else if (state.IsKeyUp(Keys.NumPad1) && _prevKeyState.IsKeyDown(Keys.NumPad1))
			{
				em = (Emote) 1;
				Character.Emote(em);
				PlayerEmote();
			}
			else if (state.IsKeyUp(Keys.NumPad2) && _prevKeyState.IsKeyDown(Keys.NumPad2))
			{
				em = (Emote) 2;
				Character.Emote(em);
				PlayerEmote();
			}
			else if (state.IsKeyUp(Keys.NumPad3) && _prevKeyState.IsKeyDown(Keys.NumPad3))
			{
				em = (Emote) 3;
				Character.Emote(em);
				PlayerEmote();
			}
			else if (state.IsKeyUp(Keys.NumPad4) && _prevKeyState.IsKeyDown(Keys.NumPad4))
			{
				em = (Emote) 4;
				Character.Emote(em);
				PlayerEmote();
			}
			else if (state.IsKeyUp(Keys.NumPad5) && _prevKeyState.IsKeyDown(Keys.NumPad5))
			{
				em = (Emote) 5;
				Character.Emote(em);
				PlayerEmote();
			}
			else if (state.IsKeyUp(Keys.NumPad6) && _prevKeyState.IsKeyDown(Keys.NumPad6))
			{
				em = (Emote) 6;
				Character.Emote(em);
				PlayerEmote();
			}
			else if (state.IsKeyUp(Keys.NumPad7) && _prevKeyState.IsKeyDown(Keys.NumPad7))
			{
				em = (Emote) 7;
				Character.Emote(em);
				PlayerEmote();
			}
			else if (state.IsKeyUp(Keys.NumPad8) && _prevKeyState.IsKeyDown(Keys.NumPad8))
			{
				em = (Emote) 8;
				Character.Emote(em);
				PlayerEmote();
			}
			else if (state.IsKeyUp(Keys.NumPad9) && _prevKeyState.IsKeyDown(Keys.NumPad9))
			{
				em = (Emote) 9;
				Character.Emote(em);
				PlayerEmote();
			}
			//The Decimal enumeration is 110, which is the Virtual Key code (VK_XXXX) for the 'del'/'.' key on the numpad
			else if (state.IsKeyUp(Keys.Decimal) && _prevKeyState.IsKeyDown(Keys.Decimal))
			{
				em = Emote.Embarassed;
				Character.Emote(em);
				PlayerEmote();
			}
		}

		public void PlayerWalk(bool isWaterTile)
		{
			const int walkTimer = 100;
			Data.SetUpdate(true);

			if (World.Instance.SoundEnabled && NoWall)
			{
				EOGame.Instance.SoundManager.GetSoundEffectRef(SoundEffectID.NoWallWalk).Play();
			}

			if (World.Instance.SoundEnabled && isWaterTile)
			{
				EOGame.Instance.SoundManager.GetSoundEffectRef(SoundEffectID.Water).Play();
			}
			
			if(isWaterTile)
				World.Instance.ActiveMapRenderer.NewWaterEffect(Character.DestX, Character.DestY);

			_walkTimer.Change(0, walkTimer); //ok, it's time to start
		}

		public void PlayerAttack()
		{
			const int attackTimer = 285;
			Data.SetUpdate(true);

			if (World.Instance.SoundEnabled)
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

			_attackTimer.Change(0, attackTimer);
		}

		public void PlayerEmote()
		{
			if (World.Instance.SoundEnabled && Character.RenderData.emote == Emote.LevelUp)
				EOGame.Instance.SoundManager.GetSoundEffectRef(SoundEffectID.LevelUp).Play();

			const int EmoteTimeBetweenFrames = 250;
			Data.SetUpdate(true);
			_emoteTimer.Change(0, EmoteTimeBetweenFrames);
		}

		public void Die()
		{
			if(World.Instance.SoundEnabled)
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
			if (adminRect != null)
			{
				if(!started) sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
				sb.Draw(adminGraphic, new Rectangle(DrawAreaWithOffset.X + 48, DrawAreaWithOffset.Y + 73, adminRect.Value.Width, adminRect.Value.Height), adminRect, Color.White);
				if(!started) sb.End();
			}

			if (_char == null || _char.RenderData == null) return;

			//note: if character is hidden, only draw if a) they are not active character and b) the active character is admin
			if (_char != World.Instance.MainPlayer.ActiveCharacter && _char.RenderData.hidden &&
			    World.Instance.MainPlayer.ActiveCharacter.AdminLevel == AdminLevel.Player)
				return;

			if(!started) sb.Begin();
			sb.Draw(_charRenderTarget, new Vector2(0, 0),
				_char.RenderData.hidden || _char.RenderData.dead ? Color.FromNonPremultiplied(255, 255, 255, 128) : Color.White);
			if(!started) sb.End();
		}

		//changes the frame for walking - used by the walk timer
		private void _walkTimerCallback(object state)
		{
			if (_char == null || State != CharacterActionState.Walking) return;

			if (Data.walkFrame == 4)
			{
				_char.DoneWalking();
				_walkTimer.Change(Timeout.Infinite, Timeout.Infinite);
			}
			//change character frame
			else
			{
				Data.SetWalkFrame((byte)(Data.walkFrame + 1));
				int xAdjust, yAdjust;
				switch (Data.facing)
				{
					case EODirection.Down: xAdjust = -8; yAdjust = 4; break;
					case EODirection.Left: xAdjust = -8; yAdjust = -4; break;
					case EODirection.Up: xAdjust = 8; yAdjust = -4; break;
					case EODirection.Right: xAdjust = 8; yAdjust = 4; break;
					default:
						return;
				}
				_char.ViewAdjustX += xAdjust;
				_char.ViewAdjustY += yAdjust;
			}
			
			World.Instance.ActiveMapRenderer.UpdateOtherPlayers(); //SetUpdate(true) for all other character's renderdata
			Data.SetUpdate(true); //not concerned about multithreaded implications of this member
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
			if (_char == null || State != CharacterActionState.Emote) return;

			if (Data.emoteFrame == 3)
			{
				_char.DoneEmote();
				_emoteTimer.Change(Timeout.Infinite, Timeout.Infinite);
			}
			else
			{
				Data.SetEmoteFrame((byte) (Data.emoteFrame + 1));
			}

			Data.SetUpdate(true);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (levelLabel != null)
					levelLabel.Dispose();
				if (nameLabel != null)
					nameLabel.Dispose();
				if (_walkTimer != null)
					_walkTimer.Dispose();
				if (_attackTimer != null)
					_attackTimer.Dispose();
				if (_emoteTimer != null)
					_emoteTimer.Dispose();
				if (_spTimer != null)
					_spTimer.Dispose();
				if (_charRenderTarget != null)
					_charRenderTarget.Dispose();
				if (m_chatBubble != null)
					m_chatBubble.Dispose();
			}

			base.Dispose(disposing);
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

			GraphicsDevice.SetRenderTarget(_charRenderTarget);
			if(Data.hidden && (_char == World.Instance.MainPlayer.ActiveCharacter || _char.AdminLevel != AdminLevel.Player))
				GraphicsDevice.Clear(ClearOptions.Target, Color.FromNonPremultiplied(255,255,255,100), 1, 0); //hidden players should blend nicely
			else
				GraphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 1, 0);

			SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
			//if subtype for this item is wings or arrows, draw at bottom, otherwise draw on top of armor
			bool shieldDrawn = false, weaponDrawn = false;
			if (shield != null && World.Instance.EIF != null && shieldInfo != null)
			{
				//draw it now if: shield type is Wings/Arrows/Bag && facing down/right
				//also draw it now if: shield type is NOT Wings/Arrows/Bag && facing up/left (ie normal shields)
				if (((Facing == EODirection.Down || Facing == EODirection.Right) && (shieldInfo.SubType == EOLib.Data.ItemSubType.Wings || shieldInfo.SubType == EOLib.Data.ItemSubType.Arrows))
					|| ((Facing == EODirection.Left || Facing == EODirection.Up) && !(shieldInfo.SubType == EOLib.Data.ItemSubType.Wings || shieldInfo.SubType == EOLib.Data.ItemSubType.Arrows)))
				{
					SpriteBatch.Draw(shield, new Vector2(DrawAreaWithOffset.X - 10, DrawAreaWithOffset.Y - 7), null, Color.White, 0.0f,
						Vector2.Zero, 1.0f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.0f);
					shieldDrawn = true;
				}
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

			if (boots != null)
				SpriteBatch.Draw(boots, new Vector2(DrawAreaWithOffset.X - 2, DrawAreaWithOffset.Y + 49), null, Color.White, 0.0f, Vector2.Zero, 1.0f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.0f);

			_drawArmor(flipped);

			if (weapon != null && !weaponDrawn)
				_drawWeapon(flipped);

			SpriteBatch.End();
			lock (hatHairLock)
			{
				SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
				if (hatInfo != null && hatInfo.SubType == EOLib.Data.ItemSubType.FaceMask)
				{
					if (hat != null)
						SpriteBatch.Draw(hat, new Vector2(DrawAreaWithOffset.X, DrawAreaWithOffset.Y - 3), null, Color.White, 0.0f,
							Vector2.Zero, 1.0f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.0f);
					if (hair != null)
						SpriteBatch.Draw(hair, new Vector2(DrawAreaWithOffset.X + (flipped ? 2 : 0), DrawAreaWithOffset.Y), null,
							Color.White, 0.0f, Vector2.Zero, 1.0f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.0f);
				}
				else
				{
					if (hair != null)
						SpriteBatch.Draw(hair, new Vector2(DrawAreaWithOffset.X + (flipped ? 2 : 0), DrawAreaWithOffset.Y), null,
							Color.White, 0.0f, Vector2.Zero, 1.0f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.0f);
					if (hat != null)
						SpriteBatch.Draw(hat, new Vector2(DrawAreaWithOffset.X, DrawAreaWithOffset.Y - 3), null, Color.White, 0.0f,
							Vector2.Zero, 1.0f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.0f);
				}
				SpriteBatch.End();
			}

			SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
			if (shield != null && !shieldDrawn)
				SpriteBatch.Draw(shield, new Vector2(DrawAreaWithOffset.X - 10, DrawAreaWithOffset.Y - 7), null, Color.White, 0.0f, Vector2.Zero, 1.0f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.0f);
			SpriteBatch.End();

			GraphicsDevice.SetRenderTarget(null);
		}

		private void _drawSkin(bool flipped)
		{
			int skinXOff = 0, skinYOff = 0;
			Vector2 skinLoc = new Vector2(6 + DrawAreaWithOffset.X, (Data.gender == 0 ? 12 : 13) + DrawAreaWithOffset.Y);
			if (Data != null)
			{
				switch (State)
				{
					case CharacterActionState.Walking:
						if (_data != null && _data.gender == 1)
						{
							switch (Facing)
							{
								case EODirection.Down: skinXOff = -1; break;
								case EODirection.Right: skinXOff = 1; break;
							}
						}
						skinLoc = new Vector2(2 + DrawAreaWithOffset.X + skinXOff, (Data.gender == 0 ? 11 : 12) + DrawAreaWithOffset.Y);
						break;
					case CharacterActionState.Attacking:
						if (weaponInfo != null && weaponInfo.SubType != ItemSubType.Ranged)
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
						else if(weaponInfo != null && weaponInfo.SubType == ItemSubType.Ranged && Data.attackFrame == 1)
						{
							switch(Facing)
							{
								case EODirection.Up:
									skinXOff += Data.gender == 1 ? 2 : 1;
									//skinYOff += Data.gender == 1 ? 0 : 0;
									break;
								case EODirection.Right:
									skinXOff += Data.gender == 1 ? 4 : 3;
									skinYOff += 1;//Data.gender == 1 ? 1 : 1;
									break;
								case EODirection.Left:
									skinXOff += Data.gender == 1 ? -9 : -8;
									break;
								case EODirection.Down:
									skinXOff += Data.gender == 1 ? -11 : -10;
									skinYOff += 1;//Data.gender == 1 ? 1 : 1;
									break;
							}
						}
						skinLoc = new Vector2(skinLoc.X + skinXOff, skinLoc.Y + skinYOff);
						break;
				}
			}

			SpriteBatch.Draw(characterSkin, skinLoc, m_skinSourceRect, Color.White, 0f, Vector2.Zero, 1f,
				flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);

			//get face and draw
			if (State == CharacterActionState.Emote)
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
					SpriteBatch.Draw(emote, emotePos, emoteRect, Color.FromNonPremultiplied(0xff,0xff,0xff,128));
				}
			}
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

		private void _drawWeapon(bool flipped)
		{
			int xOffLoc = 0;
			if (Data != null && Data.attackFrame == 2 && weaponInfo.SubType != ItemSubType.Ranged)
			{
				if (Facing == EODirection.Up || Facing == EODirection.Right)
					xOffLoc = Data.gender == 0 ? 2 : 4;
				if (Facing == EODirection.Down || Facing == EODirection.Left)
					xOffLoc = Data.gender == 0 ? -2 : -4;
			}
			Vector2 loc = (int)Facing > 1 ? new Vector2(DrawAreaWithOffset.X - 10 + xOffLoc, DrawAreaWithOffset.Y - 7):
				new Vector2(DrawAreaWithOffset.X - 28 + xOffLoc, DrawAreaWithOffset.Y - 7);

			SpriteBatch.Draw(weapon, loc, null, Color.White, 0.0f,
				Vector2.Zero, 1.0f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.0f);
		}

		private void _drawArmor(bool flipped)
		{
			if (armor != null)
			{
				int xAdjust = 0;
				int yAdjust = State == CharacterActionState.Walking ? -1 : 0;
				if (Data != null && Data.attackFrame == 2)
				{
					switch (Facing)
					{
						case EODirection.Up:
						case EODirection.Right:
							xAdjust = Data.gender == 1 ? 4 : 2;
							yAdjust -= (Data.gender == 1 ? 1 : 0);
							break;
						case EODirection.Left:
						case EODirection.Down:
							xAdjust = Data.gender == 1 ? -4 : 1;
							break;
					}
				}
				SpriteBatch.Draw(armor, new Vector2(DrawAreaWithOffset.X - 2 + xAdjust, DrawAreaWithOffset.Y + yAdjust), null, Color.White, 0.0f,
					Vector2.Zero, 1.0f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.0f);
			}
		}

		private void maskTheHair()
		{
			if (Data.hat == 0)
				return;
			
			if (World.Instance.EIF != null && World.Instance.EIF.Version > 0)
			{
				switch (hatInfo.SubType)
				{
					case EOLib.Data.ItemSubType.HideHair: //anything matching ^[A-Za-z ]*[Hh]elm[A-Za-z ]*$ or ^[A-Za-z ]*[Hh]ood[A-Za-z ]*$, or id=314 (pirate hat)
						hair = null;
						return;
					case EOLib.Data.ItemSubType.FaceMask: //Frog Head and Dragon Mask (anything with mask in the name, really)
						return;
				}
			}
			
			Color[] hatPixels;
			if (Data.facing == EODirection.Left || Data.facing == EODirection.Up || hair == null)
			{
				hatPixels = new Color[hat.Width * hat.Height];
				hat.GetData(hatPixels);
				for(int i = 0; i < hatPixels.Length; ++i)
					if(hatPixels[i] == Color.Black) hatPixels[i] = Color.Transparent;
				hat.SetData(hatPixels);
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

			lock (hatHairLock)
			{
				hat.SetData(hatPixels);
				hair.SetData(hairPixels);
			}
		}

		public void SetChatBubbleText(string msg)
		{
			m_chatBubble.SetMessage(msg);
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
	}
}
