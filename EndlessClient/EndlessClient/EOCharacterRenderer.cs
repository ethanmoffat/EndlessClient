using System;
using System.Threading;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using XNAControls;
using EOLib;
using EOLib.Data;

namespace EndlessClient
{
	/// <summary>
	/// REDESIGN WEIRDNESS WARNING
	/// Never actually adding EOCharacterRenderer (in-game ones) to game components
	/// Because of the weirdness with draw ordering in the map renderer
	/// So, Draw is only called from EOMapRenderer._doMapDrawing :(
	/// Update and initialize are also called from EOMapRenderer
	/// todo: have this make some actual god damn sense
	/// </summary>
	public class EOCharacterRenderer : XNAControl
	{
		private readonly Character _char;
		public Character Character { get { return _char; } }

		private CharRenderData _data;
		private CharRenderData Data
		{
			get
			{
				CharRenderData data = (_char != null ? _char.RenderData : _data) ?? new CharRenderData();
				return data;
			}
			set
			{
				if (_char != null)
					_char.RenderData = value;
				else
					_data = value;
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
					hair = spriteSheet.GetHair();
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
					hair = spriteSheet.GetHair();
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
				if (characterSkin != null)
					characterSkin.Dispose();
				characterSkin = spriteSheet.GetSkin(); //method automatically clips sprite sheet, removing need for source rectangle
			}
		}

		/*
		public short Shield
		{
			get { return Data.shield; }
			set
			{
				Data.SetShield(value);
				if (Data.shield != 0)
				{
					shield = spriteSheet.GetShield(ArmorShieldSpriteType.Standing);
					if(World.Instance.EIF != null)
						shieldInfo = (ItemRecord)World.Instance.EIF.Data.Find(x => (x as ItemRecord != null) && (x as ItemRecord).DollGraphic == Data.shield && (x as ItemRecord).Type == ItemType.Shield);
					updateAll = true;
				}
			}
		}
		public short Weapon
		{
			get { return Data.weapon; }
			set
			{
				Data.SetWeapon(value);
				if (Data.weapon != 0)
				{
					weapon = spriteSheet.GetWeapon(WeaponSpriteType.Standing);
					updateAll = true;
				}
			}
		}
		public short Boots
		{
			get { return Data.boots; }
			set
			{
				Data.SetBoots(value);
				if (Data.boots != 0)
				{
					boots = spriteSheet.GetBoots(BootsSpriteType.Standing);
					updateAll = true;
				}
			}
		}
		public short Armor
		{
			get { return Data.armor; }
			set
			{
				Data.SetArmor(value);
				if (Data.armor != 0)
				{
					armor = spriteSheet.GetArmor(ArmorShieldSpriteType.Standing);
					updateAll = true;
				}
			}
		}
		public short Hat
		{
			get { return Data.hat; }
			set
			{
				Data.SetHat(value);
				if (Data.hat != 0)
				{
					hat = spriteSheet.GetHat();
					if(World.Instance.EIF != null)
						hatInfo = (ItemRecord)World.Instance.EIF.Data.Find(x => (x as ItemRecord != null) && (x as ItemRecord).DollGraphic == Data.hat && (x as ItemRecord).Type == ItemType.Hat);
					updateAll = true;
				}
				maskTheHair();
			}
		}
		*/
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
		private Texture2D shield, weapon, boots, armor, hat;
		private Texture2D hair, characterSkin;

		private readonly XNALabel levelLabel, nameLabel;
		private Rectangle? adminRect;
		private readonly Texture2D adminGraphic;

		private ItemRecord shieldInfo/*, weaponInfo, bootsInfo, armorInfo*/, hatInfo;

		private KeyboardState _prevState;
		private Timer _walkTimer;
		private readonly bool noLocUpdate;

		public EOCharacterRenderer(Game g, Character charToRender)
			: base(g)
		{
			spriteSheet = new EOSpriteSheet(charToRender);
			_char = charToRender;
			_data = charToRender.RenderData;
			Texture2D tmpSkin = spriteSheet.GetSkin();
			if (_char != World.Instance.MainPlayer.ActiveCharacter)
			{
				drawArea = new Rectangle(
					_char.OffsetX + 304 - World.Instance.MainPlayer.ActiveCharacter.OffsetX,
					_char.OffsetY + 91 - World.Instance.MainPlayer.ActiveCharacter.OffsetY,
					tmpSkin.Width, tmpSkin.Height); //set based on size of the sprite and location of charToRender
			}
			else
			{
				drawArea = new Rectangle((618 - tmpSkin.Width) / 2 + 4, (298 - tmpSkin.Height) / 2 - 29, tmpSkin.Width, tmpSkin.Height);
				noLocUpdate = true; //make sure not to update the drawArea rectangle in the update method
			}
			Data.SetUpdate(true);
			_prevState = Keyboard.GetState();
		}

		public EOCharacterRenderer(Game encapsulatingGame, Vector2 drawLocation, CharRenderData data)
			: base(encapsulatingGame, drawLocation, null)
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
				levelLabel = new XNALabel(encapsulatingGame, new Rectangle(-32, 75, 1, 1), "Microsoft Sans Serif", 8.75f)
				{
					ForeColor = System.Drawing.Color.FromArgb(0xFF, 0xb4, 0xa0, 0x8c),
// ReSharper disable SpecifyACultureInStringConversionExplicitly
					Text = data.level.ToString()
// ReSharper restore SpecifyACultureInStringConversionExplicitly
				};
				levelLabel.SetParent(this);

				//504, 93 abs loc
				nameLabel = new XNALabel(encapsulatingGame, new Rectangle(104, 2, 89, 22), "Microsoft Sans Serif", 8.5f)
				{
					ForeColor = System.Drawing.Color.FromArgb(0xFF, 0xb4, 0xa0, 0x8c),
					Text = ((char) (data.name[0] - 32)) + data.name.Substring(1),
					TextAlign = System.Drawing.ContentAlignment.MiddleCenter
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

			//refresh all the textures from the GFX files or image cache
			if (Data.update)
			{
				if (Data.shield != 0)
				{
					shield = spriteSheet.GetShield();
					if(World.Instance.EIF != null)
						shieldInfo = (ItemRecord)World.Instance.EIF.Data.Find(x => (x as ItemRecord != null) && (x as ItemRecord).DollGraphic == Data.shield && (x as ItemRecord).Type == ItemType.Shield);
				}

				if (Data.weapon != 0)
					weapon = spriteSheet.GetWeapon();
				if (characterSkin != null)
					characterSkin.Dispose();
				characterSkin = spriteSheet.GetSkin(); //method automatically clips sprite sheet, removing need for source rectangle
				if (Data.boots != 0)
					boots = spriteSheet.GetBoots();
				if (Data.armor != 0)
					armor = spriteSheet.GetArmor();
				if(Data.hairstyle != 0)
					hair = spriteSheet.GetHair();
				if (Data.hat != 0)
				{
					hat = spriteSheet.GetHat();
					if(World.Instance.EIF != null)
						hatInfo = (ItemRecord)World.Instance.EIF.Data.Find(x => (x as ItemRecord != null) && (x as ItemRecord).DollGraphic == Data.hat && (x as ItemRecord).Type == ItemType.Hat);
				}

				maskTheHair(); //this will set the combined hat/hair texture with proper data.
				
				_drawCharToRenderTarget();
				Data.SetUpdate(false);
			}

			//input handling for arrow keys done here
			//only check for a keypress if not currently walking and the renderer is for the active character
			//also only check every 1/2 of a second
			KeyboardState currentState = Keyboard.GetState();
			if (_char != null && _char == World.Instance.MainPlayer.ActiveCharacter && gameTime.TotalGameTime.Milliseconds % 100 <= 25)
			{
				EODirection direction; //first, get the direction we should try to move based on the key presses from the player
				if (currentState.IsKeyDown(Keys.Up) && _prevState.IsKeyDown(Keys.Up))
					direction = EODirection.Up;
				else if (currentState.IsKeyDown(Keys.Down) && _prevState.IsKeyDown(Keys.Down))
					direction = EODirection.Down;
				else if (currentState.IsKeyDown(Keys.Left) && _prevState.IsKeyDown(Keys.Left))
					direction = EODirection.Left;
				else if (currentState.IsKeyDown(Keys.Right) && _prevState.IsKeyDown(Keys.Right))
					direction = EODirection.Right;
				else
					direction = (EODirection)255;
				
				byte destX, destY;
				switch (direction)
				{
					case EODirection.Up: destX = (byte)_char.X; destY = (byte)(_char.Y - 1); break;
					case EODirection.Down: destX = (byte)_char.X; destY = (byte)(_char.Y + 1); break;
					case EODirection.Right: destX = (byte)(_char.X + 1); destY = (byte)_char.Y; break;
					case EODirection.Left: destX = (byte)(_char.X - 1); destY = (byte)_char.Y; break;
					default:
						if (!_char.Walking)
							_prevState = currentState; //only set this when not walking already
						return;
				}

				//valid direction at this point
				if (_char.RenderData.facing != direction)
				{
					_char.Face(direction);
					return;
				}

				bool playerCollision, npcCollision;
				TileSpec spec = World.Instance.ActiveMapRenderer.CheckCoordinates(destX, destY, out playerCollision, out npcCollision);
				if (playerCollision) EOGame.Instance.Hud.SetStatusLabel("OTHER PLAYER WAHHHHHHHHHH");
					//status bar: keep moving into player to walk through...
				else if (npcCollision) EOGame.Instance.Hud.SetStatusLabel("OTHER NPC WAHHHHHHHHHH");
					//do nothing? idk...
				else
				{
					//see what the action of moving into the next tile should do
					//	bank vaults should open vault
					//	chest should open chest
					//	chairs should sit
					//	etc. etc.
					bool walkValid = false;
					switch (spec) //handle the tile specs differently
					{
						case TileSpec.None:
						case TileSpec.NPCBoundary:
						case TileSpec.FakeWall:
							walkValid = true;
							break;
					}

					if (!_char.Walking && walkValid)
					{
						_char.Walk(direction, destX, destY);
						PlayerWalk();
					}
				}

				if (!_char.Walking) _prevState = currentState; //only set this when not walking already
			}
		}

		public void PlayerWalk()
		{
			const int walkTimer = 150;
			Data.SetUpdate(true);
			_walkTimer.Change(0, walkTimer); //ok, it's time to start
		}

		//character is drawn in the following order:
		// - shield (if wings/arrows)
		// - weapon
		// - character body sprite
		// - boots
		// - armor
		// - shield (if not already drawn)
		// - hair (rendertarget)
		// - hat  (rendertarget)
		public override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);
			
			if (adminRect != null)
			{
				SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
				SpriteBatch.Draw(adminGraphic, new Rectangle(DrawAreaWithOffset.X + 48, DrawAreaWithOffset.Y + 73, adminRect.Value.Width, adminRect.Value.Height), adminRect, Color.White);
				SpriteBatch.End();
			}
			
			Color drawFilter;
			if (EOGame.Instance.State == GameStates.PlayingTheGame)
			{
				drawFilter = World.Instance.ActiveMapRenderer.PlayerBehindSomething(_char)
					? Color.FromNonPremultiplied(255, 255, 255, 60)
					: Color.White;
				if (this != World.Instance.ActiveCharacterRenderer)
				{
					//do something similar for npcs in relation to active character renderer, so they show up in the right order
					if (World.Instance.MainPlayer.ActiveCharacter.X < _char.X &&
					    World.Instance.MainPlayer.ActiveCharacter.Y < _char.Y)
						DrawOrder = World.Instance.ActiveCharacterRenderer.DrawOrder + 1;
					else
						DrawOrder = World.Instance.ActiveCharacterRenderer.DrawOrder - 1;
				}
			}
			else
				drawFilter = Color.White;

			SpriteBatch.Begin();
			SpriteBatch.Draw(_charRenderTarget, new Vector2(0, 0), drawFilter);
			SpriteBatch.End();
		}

		//changes the frame for walking - used by the walk timer
		private void _walkTimerCallback(object state)
		{
			if (_char == null || !_char.Walking) return;
			
			//change character frame
			if (Data.walkFrame < 4)
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
			
			if(Data.walkFrame == 4)
			{
				_char.DoneWalking();
				_walkTimer.Change(Timeout.Infinite, Timeout.Infinite);
			}
			World.Instance.ActiveMapRenderer.UpdateOtherPlayers(); //SetUpdate(true) for all other character's renderdata
			Data.SetUpdate(true); //not concerned about multithreaded implications of this member
		}

		protected override void Dispose(bool disposing)
		{
			if(levelLabel != null)
				levelLabel.Dispose();
			if(nameLabel != null)
				nameLabel.Dispose();
			if (_walkTimer != null)
				_walkTimer.Dispose();
			if (_charRenderTarget != null)
				_charRenderTarget.Dispose();
			base.Dispose(disposing);
		}

		private void _drawCharToRenderTarget()
		{
			bool flipped = (int)Data.facing > 1; //flipped if direction is Up or Right (IE Facing > 1)

			GraphicsDevice.SetRenderTarget(_charRenderTarget);
			GraphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 1, 0);

			SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
			//if subtype for this item is wings or arrows, draw at bottom, otherwise draw on top of armor
			bool shieldDrawn = false;
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

			if (weapon != null)
			{
				Vector2 loc = (int)Facing > 1 ? new Vector2(DrawAreaWithOffset.X - 10, DrawAreaWithOffset.Y - 7) :
					new Vector2(DrawAreaWithOffset.X - 28, DrawAreaWithOffset.Y - 7);

				SpriteBatch.Draw(weapon, loc, null, Color.White, 0.0f,
					Vector2.Zero, 1.0f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.0f);
			}

			if (characterSkin != null)
			{
				int dirWalkingOffset = 0;
				if (_data != null && _data.gender == 1) //male
				{
					switch (Facing)
					{
						case EODirection.Down:
							dirWalkingOffset = -1;
							break;
						case EODirection.Right:
							dirWalkingOffset = 1;
							break;
					}
				}
				Vector2 skinLoc = _char.Walking ? new Vector2(2 + DrawAreaWithOffset.X + dirWalkingOffset, (Data.gender == 0 ? 11 : 12) + DrawAreaWithOffset.Y)
					: new Vector2(6 + DrawAreaWithOffset.X, (Data.gender == 0 ? 12 : 13) + DrawAreaWithOffset.Y);
				SpriteBatch.Draw(characterSkin, skinLoc, null, Color.White, 0.0f, Vector2.Zero, 1.0f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.0f);
			}

			if (boots != null)
				SpriteBatch.Draw(boots, new Vector2(DrawAreaWithOffset.X - 2, DrawAreaWithOffset.Y + 49), null, Color.White, 0.0f, Vector2.Zero, 1.0f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.0f);

			if (armor != null)
			{
				int yAdjust = 0;
				if (_char.Walking && _data != null && _data.gender == 0) //female
					yAdjust = -1;
				SpriteBatch.Draw(armor, new Vector2(DrawAreaWithOffset.X - 2, DrawAreaWithOffset.Y + yAdjust), null, Color.White, 0.0f,
					Vector2.Zero, 1.0f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.0f);
			}

			if (hatInfo != null && hatInfo.SubType == EOLib.Data.ItemSubType.FaceMask)
			{
				if (hat != null)
					SpriteBatch.Draw(hat, new Vector2(DrawAreaWithOffset.X, DrawAreaWithOffset.Y - 3), null, Color.White, 0.0f, Vector2.Zero, 1.0f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.0f);
				if (hair != null)
					SpriteBatch.Draw(hair, new Vector2(DrawAreaWithOffset.X + (flipped ? 2 : 0), DrawAreaWithOffset.Y), null, Color.White, 0.0f, Vector2.Zero, 1.0f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.0f);
			}
			else
			{
				if (hair != null)
					SpriteBatch.Draw(hair, new Vector2(DrawAreaWithOffset.X + (flipped ? 2 : 0), DrawAreaWithOffset.Y), null, Color.White, 0.0f, Vector2.Zero, 1.0f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.0f);
				if (hat != null)
					SpriteBatch.Draw(hat, new Vector2(DrawAreaWithOffset.X, DrawAreaWithOffset.Y - 3), null, Color.White, 0.0f, Vector2.Zero, 1.0f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.0f);
			}

			if (shield != null && !shieldDrawn)
				SpriteBatch.Draw(shield, new Vector2(DrawAreaWithOffset.X - 10, DrawAreaWithOffset.Y - 7), null, Color.White, 0.0f, Vector2.Zero, 1.0f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.0f);

			SpriteBatch.End();

			GraphicsDevice.SetRenderTarget(null);
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

			Color[] hatPixels = new Color[hat.Width * hat.Height], hairPixels = new Color[hair.Width * hair.Height];
			hat.GetData(hatPixels, 0, hatPixels.Length);
			hair.GetData(hairPixels, 0, hairPixels.Length);

			for(int i = 1; i <= hat.Height; ++i)
			{
				for(int j = 0; j < hat.Width; ++j)
				{
					if(hatPixels[i * j] == new Color(0, 0, 0))
					{
						hatPixels[i * j].A = 0;
						if(i < hair.Height && j < hair.Width)
							hairPixels[i * j].A = 0;
					}
				}
			}

			hat.SetData(hatPixels);
			hair.SetData(hairPixels);
		}
	}
}
