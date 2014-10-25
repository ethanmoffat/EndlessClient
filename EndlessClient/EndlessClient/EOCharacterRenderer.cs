using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using XNAControls;
using EOLib;
using EOLib.Data;

namespace EndlessClient
{
	public class EOCharacterRenderer : XNAControl
	{
		private readonly Character _char;
		public Character Character { get { return _char; } }

		private CharRenderData _data;
		private CharRenderData Data
		{
			get
			{
				CharRenderData data = _char != null ? _char.RenderData : _data;
				updateAll = data.update;
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
		private bool updateAll; //setting this will update all parameters at once

		public byte HairColor
		{
			get { return Data.haircolor; }
			set
			{
				Data.SetHairColor(value);
				hair = EOSpriteSheet.GetHair(Facing, HairType, HairColor, Gender);
				maskTheHair();
			}
		}
		public byte HairType
		{
			get { return Data.hairstyle; }
			set
			{
				Data.SetHairStyle(value);
				hair = EOSpriteSheet.GetHair(Facing, HairType, HairColor, Gender);
				maskTheHair();
			}
		}
		public byte Gender
		{
			get { return Data.gender; }
			set
			{
				Data.SetGender(value);
				updateAll = true;
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
				characterSkin = EOSpriteSheet.GetSkin(Facing, SkinColor, Gender); //method automatically clips sprite sheet, removing need for source rectangle
			}
		}

		public short Shield
		{
			get { return Data.shield; }
			set
			{
				Data.SetShield(value);
				if (Data.shield != 0)
				{
					shield = EOSpriteSheet.GetShield(ArmorShieldSpriteType.Standing, Facing, Shield, Gender);
					if(World.Instance.EIF != null)
						shieldInfo = (ItemRecord)World.Instance.EIF.Data.Find(x => (x as ItemRecord).DollGraphic == Data.shield && (x as ItemRecord).Type == ItemType.Shield);
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
					weapon = EOSpriteSheet.GetWeapon(WeaponSpriteType.Standing, Facing, Weapon, Gender);
			}
		}
		public short Boots
		{
			get { return Data.boots; }
			set
			{
				Data.SetBoots(value);
				if (Data.boots != 0)
					boots = EOSpriteSheet.GetBoots(BootsSpriteType.Standing, Facing, Boots, Gender);
			}
		}
		public short Armor
		{
			get { return Data.armor; }
			set
			{
				Data.SetArmor(value);
				if (Data.armor != 0)
					armor = EOSpriteSheet.GetArmor(ArmorShieldSpriteType.Standing, Facing, Armor, Gender);
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
					hat = EOSpriteSheet.GetHat(Facing, Hat, Gender);
					if(World.Instance.EIF != null)
						hatInfo = (ItemRecord)World.Instance.EIF.Data.Find(x => (x as ItemRecord).DollGraphic == Data.hat && (x as ItemRecord).Type == ItemType.Hat);
				}
				maskTheHair();
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
				updateAll = true;
			}
		}
		private Texture2D shield, weapon, boots, armor, hat;
		private Texture2D hair, characterSkin;

		private readonly XNALabel levelLabel, nameLabel;
		private Rectangle? adminRect;
		private readonly Texture2D adminGraphic;

		private ItemRecord shieldInfo/*, weaponInfo, bootsInfo, armorInfo*/, hatInfo;

		private KeyboardState _prevState;
		private GameTime _startWalking;

		public EOCharacterRenderer(Game g, Character charToRender)
			: base(g)
		{
			_char = charToRender;
			_data = charToRender.RenderData;
			Texture2D tmpSkin = EOSpriteSheet.GetSkin(0, Data.race, Data.gender);
			if (_char != World.Instance.MainPlayer.ActiveCharacter)
			{//These coordinates are a little bit off still
				drawArea = new Rectangle(
					_char.OffsetX + 310 - World.Instance.MainPlayer.ActiveCharacter.OffsetX,
					_char.OffsetY + 106 - World.Instance.MainPlayer.ActiveCharacter.OffsetY,
					tmpSkin.Width, tmpSkin.Height); //set based on size of the sprite and location of charToRender
			}
			else
			{
				drawArea = new Rectangle((618 - tmpSkin.Width) / 2 + 4, (298 - tmpSkin.Height) / 2 - 29, tmpSkin.Width, tmpSkin.Height);
			}
			updateAll = true;
			_prevState = Keyboard.GetState();
		}

		public EOCharacterRenderer(Game encapsulatingGame, Vector2 drawLocation, CharRenderData data)
			: base(encapsulatingGame, drawLocation, null)
		{
			//when this is a part of a dialog, the drawareaoffset will be set accordingly and is used in the draw method
			//otherwise, it will just draw it at the absolute location specified by drawArea

			drawArea = new Rectangle((int) drawLocation.X, (int) drawLocation.Y, 1, 1);
			Data = data;
			updateAll = true;

			if (data.name.Length > 0)
			{
				//362, 167 abs loc
				levelLabel = new XNALabel(encapsulatingGame, new Rectangle(-32, 75, 1, 1), "Microsoft Sans Serif", 8.75f)
				{
					ForeColor = System.Drawing.Color.FromArgb(0xFF, 0xb4, 0xa0, 0x8c),
					Text = data.level.ToString()
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

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			if (_char != null && _char.Walking)
			{
				TimeSpan elapsedSinceLast = gameTime.TotalGameTime - _startWalking.TotalGameTime;
			}

			//refresh all the textures from the GFX files or image cache
			if (updateAll)
			{
				if (Data.shield != 0)
				{
					shield = EOSpriteSheet.GetShield(ArmorShieldSpriteType.Standing, Facing, Shield, Gender);
					if(World.Instance.EIF != null)
						shieldInfo = (ItemRecord)World.Instance.EIF.Data.Find(x => (x as ItemRecord).DollGraphic == Data.shield && (x as ItemRecord).Type == ItemType.Shield);
				}
				//TODO: update ArmorShieldSpriteType to be dynamic based on the character's action!
				//TODO: take ArmorShieldSpriteType into account for character's skin!
				if (Data.weapon != 0)
					weapon = EOSpriteSheet.GetWeapon(WeaponSpriteType.Standing, Facing, Weapon, Gender);
				if (characterSkin != null)
					characterSkin.Dispose();
				characterSkin = EOSpriteSheet.GetSkin(Facing, SkinColor, Gender); //method automatically clips sprite sheet, removing need for source rectangle
				if (Data.boots != 0)
					boots = EOSpriteSheet.GetBoots(BootsSpriteType.Standing, Facing, Boots, Gender);
				if (Data.armor != 0)
					armor = EOSpriteSheet.GetArmor(ArmorShieldSpriteType.Standing, Facing, Armor, Gender);
				hair = EOSpriteSheet.GetHair(Facing, HairType, HairColor, Gender);
				if (Data.hat != 0)
				{
					hat = EOSpriteSheet.GetHat(Facing, Hat, Gender);
					if(World.Instance.EIF != null)
						hatInfo = (ItemRecord)World.Instance.EIF.Data.Find(x => (x as ItemRecord).DollGraphic == Data.hat && (x as ItemRecord).Type == ItemType.Hat);
				}

				maskTheHair(); //this will set the combined hat/hair texture with proper data.

				updateAll = false;
			}

			//input handling for arrow keys done here
			KeyboardState currentState = Keyboard.GetState();
			
			//only check for a keypress if not currently walking and the renderer is for the active character
			if (_char != null && !_char.Walking && _char == World.Instance.MainPlayer.ActiveCharacter) 
			{
				if (currentState.IsKeyDown(Keys.Up) && _prevState.IsKeyDown(Keys.Up))
				{
					_char.Walk(EODirection.Up);
					updateAll = true;
					_startWalking = gameTime;
				}
				else if (currentState.IsKeyDown(Keys.Down) && _prevState.IsKeyDown(Keys.Down))
				{
					_char.Walk(EODirection.Down);
					updateAll = true;
					_startWalking = gameTime;
				}
				else if (currentState.IsKeyDown(Keys.Left) && _prevState.IsKeyDown(Keys.Left))
				{
					_char.Walk(EODirection.Left);
					updateAll = true;
					_startWalking = gameTime;
				}
				else if (currentState.IsKeyDown(Keys.Right) && _prevState.IsKeyDown(Keys.Right))
				{
					_char.Walk(EODirection.Right);
					updateAll = true;
					_startWalking = gameTime;
				}

				_prevState = currentState; //only set this when not walking already
			}
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
			if (characterSkin != null)
				drawArea = new Rectangle(
					_char.OffsetX + 310 - World.Instance.MainPlayer.ActiveCharacter.OffsetX,
					_char.OffsetY + 106 - World.Instance.MainPlayer.ActiveCharacter.OffsetY,
					characterSkin.Width, characterSkin.Height);

			base.Draw(gameTime);
			
			bool flipped = (int)Data.facing > 1; //flipped if direction is Up or Right (IE Facing > 1)
			
			SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

			if (adminRect != null)
			{
				SpriteBatch.Draw(adminGraphic, new Rectangle(DrawAreaWithOffset.X + 48, DrawAreaWithOffset.Y + 73, adminRect.Value.Width, adminRect.Value.Height), adminRect, Color.White);
			}

			//if subtype for this item is wings or arrows, draw at bottom, otherwise draw on top of armor
			//TODO: factor in Rotation to the order of drawing things...should only matter for arrows/bags
			bool shieldDrawn = false;
			if (shield != null && World.Instance.EIF != null && shieldInfo != null)
			{
				//draw it now if: shield type is Wings/Arrows/Bag && facing down/right
				//also draw it now if: shield type is NOT Wings/Arrows/Bag && facing up/left (ie normal shields)
				if (((Facing == EODirection.Down || Facing == EODirection.Right) && (shieldInfo.SubType == EOLib.Data.ItemSubType.Wings || shieldInfo.SubType == EOLib.Data.ItemSubType.Arrows))
					|| ((Facing == EODirection.Left || Facing == EODirection.Up) && ! (shieldInfo.SubType == EOLib.Data.ItemSubType.Wings || shieldInfo.SubType == EOLib.Data.ItemSubType.Arrows)))
				{
					SpriteBatch.Draw(shield, new Vector2(DrawAreaWithOffset.X - 10, DrawAreaWithOffset.Y - 7), null, Color.White, 0.0f,
						Vector2.Zero, 1.0f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.0f);
					shieldDrawn = true;
				}
			}

			if (weapon != null)
			{
				Vector2 loc = (int) Facing > 1 ? new Vector2(DrawAreaWithOffset.X - 10, DrawAreaWithOffset.Y - 7) : 
					new Vector2(DrawAreaWithOffset.X - 28, DrawAreaWithOffset.Y - 7);

				SpriteBatch.Draw(weapon, loc, null, Color.White, 0.0f,
					Vector2.Zero, 1.0f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.0f);
			}

			if(characterSkin != null)
				SpriteBatch.Draw(characterSkin, new Vector2(6 + DrawAreaWithOffset.X, (Data.gender == 0 ? 12 : 13) + DrawAreaWithOffset.Y), null, Color.White, 0.0f, Vector2.Zero, 1.0f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.0f);

			if (boots != null)
				SpriteBatch.Draw(boots, new Vector2(DrawAreaWithOffset.X - 2, DrawAreaWithOffset.Y + 49), null, Color.White, 0.0f, Vector2.Zero, 1.0f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.0f);

			if (armor != null)
				SpriteBatch.Draw(armor, new Vector2(DrawAreaWithOffset.X - 2, DrawAreaWithOffset.Y), null, Color.White, 0.0f, Vector2.Zero, 1.0f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.0f);

			if(hatInfo != null && hatInfo.SubType == EOLib.Data.ItemSubType.FaceMask)
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
		}
		
		protected override void Dispose(bool disposing)
		{
			if(levelLabel != null)
				levelLabel.Dispose();
			if(nameLabel != null)
				nameLabel.Dispose();
			base.Dispose(disposing);
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
