// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EndlessClient.Rendering.Sprites;
using EOLib;
using EOLib.Domain.BLL;
using EOLib.Graphics;
using EOLib.Net.API;
using Microsoft.Xna.Framework.Graphics;
using XNA = Microsoft.Xna.Framework;

namespace EndlessClient.Rendering
{
	//---------------------------------------------------
	// SPRITE SHEET LAYER FOR CHARACTER RENDERING HELPER (and NPCs)
	//---------------------------------------------------

	//enums are stored with values that are the actual numbers instead of being indexes.
	//so Standing=1 would refer to image number 101 (graphic no. 0 in game)

	public class EOSpriteSheet
	{
		private readonly INativeGraphicsManager _gfxManager;
		private readonly Character _charRef;

		public EOSpriteSheet(INativeGraphicsManager gfxManager, Character charToWatch)
		{
			_gfxManager = gfxManager;
			_charRef = charToWatch;
		}

		private CharRenderData _data
		{
			get { return _charRef.RenderData; }
		}

		public Texture2D GetArmor(bool isBow = false)
		{
			ArmorShieldSpriteType type = ArmorShieldSpriteType.Standing;
			switch(_charRef.State)
			{
				case CharacterActionState.Walking:
					switch (_data.walkFrame)
					{
						case 1: type = ArmorShieldSpriteType.WalkFrame1; break;
						case 2: type = ArmorShieldSpriteType.WalkFrame2; break;
						case 3: type = ArmorShieldSpriteType.WalkFrame3; break;
						case 4: type = ArmorShieldSpriteType.WalkFrame4; break;
					}
					break;
				case CharacterActionState.Attacking:
					if(isBow)
					{
						switch (_data.attackFrame)
						{
							case 1: type = ArmorShieldSpriteType.Bow; break;
							case 2: type = ArmorShieldSpriteType.Standing; break;
						}
					}
					else
					{
						switch (_data.attackFrame)
						{
							case 1: type = ArmorShieldSpriteType.PunchFrame1; break;
							case 2: type = ArmorShieldSpriteType.PunchFrame2; break;
						}
					}
					break;
				case CharacterActionState.SpellCast:
					type = ArmorShieldSpriteType.SpellCast;
					break;
				case CharacterActionState.Sitting:
					switch (_data.sitting)
					{
						case SitState.Chair: 
							type = ArmorShieldSpriteType.SitChair;
							break;
						case SitState.Floor:
							type = ArmorShieldSpriteType.SitGround;
							break;
					}
					break;
			}

			short baseArmorValue = (short)((_data.armor - 1) * 50);
			GFXTypes gfxFile = (_data.gender == 0) ? GFXTypes.FemaleArmor : GFXTypes.MaleArmor;
			int factor = (_data.facing == EODirection.Down || _data.facing == EODirection.Right) ? 0 : 1; //multiplier for the direction faced
			factor *= getFactor(type);
			int gfxNumber = baseArmorValue + (int)type + factor;
			return _gfxManager.TextureFromResource(gfxFile, gfxNumber, true);
		}

		public Texture2D GetShield(bool shieldIsOnBack)
		{
			//front shields have one size gfx, back arrows/wings have another size.
			ArmorShieldSpriteType type = ArmorShieldSpriteType.Standing;
			int factor;
			if (!shieldIsOnBack)
			{
				if(_charRef.State == CharacterActionState.Walking)
				{
					switch (_data.walkFrame)
					{
						case 1: type = ArmorShieldSpriteType.WalkFrame1; break;
						case 2: type = ArmorShieldSpriteType.WalkFrame2; break;
						case 3: type = ArmorShieldSpriteType.WalkFrame3; break;
						case 4: type = ArmorShieldSpriteType.WalkFrame4; break;
					}
				}
				else if(_charRef.State == CharacterActionState.Attacking)
				{
					switch (_data.attackFrame)
					{
						case 1: type = ArmorShieldSpriteType.PunchFrame1; break;
						case 2: type = ArmorShieldSpriteType.PunchFrame2; break;
					}
				}
				else if (_charRef.State == CharacterActionState.SpellCast)
				{
					type = ArmorShieldSpriteType.SpellCast;
				}
				else
				{
					//hide shield graphic when sitting
					return null;
				}

				factor = (_data.facing == EODirection.Down || _data.facing == EODirection.Right) ? 0 : 1;
				factor *= getFactor(type);
			}
			else
			{
				//sitting is valid for arrows and wings and bag
				//Standing = 1/2
				//Attacking = 3/4
				//Extra = 5 (unused?)
				if (_charRef.State == CharacterActionState.Attacking && _data.attackFrame == 1)
				{
					type = (ArmorShieldSpriteType)3;
				}
				factor = (_data.facing == EODirection.Down || _data.facing == EODirection.Right) ? 0 : 1;
			}

			short baseShieldValue = (short)((_data.shield - 1) * 50);
			GFXTypes gfxFile = _data.gender == 0 ? GFXTypes.FemaleBack : GFXTypes.MaleBack;
			int gfxNumber = baseShieldValue + (int)type + factor;
			return _gfxManager.TextureFromResource(gfxFile, gfxNumber, true);
		}

		public Texture2D GetWeapon(bool isBow = false)
		{
			WeaponSpriteType type = WeaponSpriteType.Standing;
			switch(_charRef.State)
			{
				case CharacterActionState.Walking:
					switch (_data.walkFrame)
					{
						case 1: type = WeaponSpriteType.WalkFrame1; break;
						case 2: type = WeaponSpriteType.WalkFrame2; break;
						case 3: type = WeaponSpriteType.WalkFrame3; break;
						case 4: type = WeaponSpriteType.WalkFrame4; break;
					}
					break;
				case CharacterActionState.Attacking:
					if (isBow)
					{
						switch (_data.attackFrame)
						{
							case 1: type = WeaponSpriteType.Shooting; break;
							case 2: type = WeaponSpriteType.Standing; break;
						}
					}
					else
					{
						switch (_data.attackFrame)
						{
							case 1: type = WeaponSpriteType.SwingFrame1; break;
							case 2:
								type = _data.facing == EODirection.Down || _data.facing == EODirection.Right
									? WeaponSpriteType.SwingFrame2Spec
									: WeaponSpriteType.SwingFrame2;
									break;
						}
					}
					break;
				case CharacterActionState.SpellCast:
					type = WeaponSpriteType.SpellCast;
					break;
				case CharacterActionState.Sitting: return null; //no weapon when sitting
			}

			short baseWeaponValue = (short)((_data.weapon - 1) * 100);
			GFXTypes gfxFile = _data.gender == 0 ? GFXTypes.FemaleWeapons : GFXTypes.MaleWeapons;
			int factor = (_data.facing == EODirection.Down || _data.facing == EODirection.Right) ? 0 : 1;
			factor *= getFactor(type);
			int gfxNumber = baseWeaponValue + (int)type + factor;
			return _gfxManager.TextureFromResource(gfxFile, gfxNumber, true);
		}

		public Texture2D GetBoots(bool isBow = false)
		{
			BootsSpriteType type = BootsSpriteType.Standing;
			switch(_charRef.State)
			{
				case CharacterActionState.Walking:
					switch (_data.walkFrame)
					{
						case 1: type = BootsSpriteType.WalkFrame1; break;
						case 2: type = BootsSpriteType.WalkFrame2; break;
						case 3: type = BootsSpriteType.WalkFrame3; break;
						case 4: type = BootsSpriteType.WalkFrame4; break;
					}
					break;
				case CharacterActionState.Attacking:
					if(!isBow && _data.attackFrame == 2 || 
						isBow && _data.attackFrame == 1)
						type = BootsSpriteType.Attack;
					break;
				case CharacterActionState.Sitting:
					switch (_data.sitting)
					{
						case SitState.Chair: type = BootsSpriteType.SitChair; break;
						case SitState.Floor: type = BootsSpriteType.SitGround; break;
					}
					break;
			}
			short baseBootsValue = (short)((_data.boots - 1) * 40);
			GFXTypes gfxFile = _data.gender == 0 ? GFXTypes.FemaleShoes : GFXTypes.MaleShoes;
			int factor = (_data.facing == EODirection.Down || _data.facing == EODirection.Right) ? 0 : 1;
			factor *= getFactor(type);
			int gfxNumber = baseBootsValue + (int)type + factor;
			return _gfxManager.TextureFromResource(gfxFile, gfxNumber, true);
		}

		/// <summary>
		/// Gets the hair texture from the GFX file based on gender, direction, style, and color
		/// </summary>
		/// <param name="refresh">True to refresh from the GFX file, false to use the hair texture cached in this EOSpriteSheet instance</param>
		/// <returns>Texture2D with the hair data</returns>
		public Texture2D GetHair(bool refresh)
		{
			byte turnedOffset = (byte)((_data.facing == EODirection.Left || _data.facing == EODirection.Up) ? 2 : 0);
			GFXTypes gfxFile = (_data.gender == 0) ? GFXTypes.FemaleHair : GFXTypes.MaleHair;
			int gfxNumber = 2 + ((_data.hairstyle - 1) * 40) + (_data.haircolor * 4) + turnedOffset;
			return _gfxManager.TextureFromResource(gfxFile, gfxNumber, true, refresh);
		}

		public Texture2D GetHat()
		{
			short baseHatValue = (short)((_data.hat - 1) * 10);
			GFXTypes gfxFile = _data.gender == 0 ? GFXTypes.FemaleHat : GFXTypes.MaleHat;
			int factor = (_data.facing == EODirection.Down || _data.facing == EODirection.Right) ? 0 : 2;
			int gfxNumber = baseHatValue + factor + 1;
			return _gfxManager.TextureFromResource(gfxFile, gfxNumber, true);
		}

		public Texture2D GetSkin(bool isBow, out XNA.Rectangle skinRect)
		{
			const byte sheetRows = 7;
			byte sheetColumns = 4;
			byte gfxNum = 1;

			//change up which gfx resource to load, and the size of the resource, based on the _data
			if (_charRef.State == CharacterActionState.Walking && _data.walkFrame > 0)
			{
				//walking
				gfxNum = 2;
				sheetColumns = 16;
			}
			else if (_charRef.State == CharacterActionState.Attacking && _data.attackFrame > 0)
			{
				if (!isBow)
				{
					//attacking
					gfxNum = 3;
					sheetColumns = 8;
				}
				else if(_data.attackFrame == 1) //only 1 frame of bow/gun animation
				{
					gfxNum = 7; //4 columns in this one too
				}
			}
			else if (_charRef.State == CharacterActionState.SpellCast)
			{
				gfxNum = 4;
			}
			else if (_charRef.State == CharacterActionState.Sitting)
			{
				if (_data.sitting == SitState.Floor) gfxNum = 6;
				else if (_data.sitting == SitState.Chair) gfxNum = 5;
			}
			//similar if statements for spell, emote, etc

			bool rotated = _data.facing == EODirection.Left || _data.facing == EODirection.Up;
			Texture2D sheet = _gfxManager.TextureFromResource(GFXTypes.SkinSprites, gfxNum, true);
			int heightDelta = sheet.Height / sheetRows; //the height of one 'row' in the sheet
			int widthDelta = sheet.Width / sheetColumns; //the width of one 'column' in the sheet
			int section = sheet.Width/4; //each 'section' for a different set of graphics

			int walkExtra = _data.walkFrame > 0 ? widthDelta * (_data.walkFrame - 1) : 0;
			walkExtra = !isBow && _data.attackFrame > 0 ? widthDelta*(_data.attackFrame - 1) : walkExtra;

			skinRect = new XNA.Rectangle(
				_data.gender * widthDelta * (sheetColumns / 2) + (rotated ? section : 0) + walkExtra,
				_data.race * heightDelta,
				widthDelta,
				heightDelta);

			return sheet;
		}

		public Texture2D GetFace(out XNA.Rectangle faceRect)
		{
			if (_data.emoteFrame < 0 ||
				_data.emote == Emote.Trade || _data.emote == Emote.LevelUp)
			{
				faceRect = new XNA.Rectangle();
				return null;
			}

			//14 rows (7 female - 7 male) / 11 columns
			const int ROWS = 14;
			const int COLS = 11;

			Texture2D face = _gfxManager.TextureFromResource(GFXTypes.SkinSprites, 8, true);

			int widthDelta = face.Width/COLS;
			int heightDelta = face.Height/ROWS;
			int genderOffset = (face.Height/2)*_data.gender;
			//'playful' is the last face in the gfx (ndx 10), even though it has enum value of 14 (ndx 13)
			int emote = _data.emote == Emote.Playful || _data.emote == Emote.Drunk ? 10 : (int)_data.emote - 1;

			faceRect = new XNA.Rectangle(widthDelta*emote, heightDelta*_data.race + genderOffset, widthDelta, heightDelta);

			return face;
		}

		public Texture2D GetEmote(out XNA.Rectangle emoteRect)
		{
			emoteRect = new XNA.Rectangle();
			if (_data.emoteFrame < 0)
				return null;

			const int NUM_EMOTES = 15;
			const int NUM_FRAMES = 4;

			int convertedEmote = 0;
			switch (_data.emote)
			{
				case Emote.Happy: /*0*/break;
				case Emote.Depressed: convertedEmote = 7; break;
				case Emote.Sad: convertedEmote = 1; break;
				case Emote.Angry: convertedEmote = 5; break;
				case Emote.Confused: convertedEmote = 3; break;
				case Emote.Surprised: convertedEmote = 2; break;
				case Emote.Hearts: convertedEmote = 6; break;
				case Emote.Moon: convertedEmote = 4; break;
				case Emote.Suicidal: convertedEmote = 9; break;
				case Emote.Embarassed: convertedEmote = 8; break;
				case Emote.Drunk:
				case Emote.Trade:
				case Emote.LevelUp:
				case Emote.Playful: convertedEmote = (int)_data.emote - 1; break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			Texture2D emote = _gfxManager.TextureFromResource(GFXTypes.PostLoginUI, 38, true);
			int eachSet = emote.Width/NUM_EMOTES;
			int eachFrame = emote.Width/(NUM_EMOTES*NUM_FRAMES);

			emoteRect = new XNA.Rectangle((convertedEmote * eachSet) + (_data.emoteFrame * eachFrame), 0, eachFrame, emote.Height);

			return emote;
		}

		private int getFactor(ArmorShieldSpriteType type)
		{
			switch (type)
			{
				case ArmorShieldSpriteType.WalkFrame1:
				case ArmorShieldSpriteType.WalkFrame2:
				case ArmorShieldSpriteType.WalkFrame3:
				case ArmorShieldSpriteType.WalkFrame4:
					return 4;
				case ArmorShieldSpriteType.PunchFrame1:
				case ArmorShieldSpriteType.PunchFrame2:
					return 2;
			}
			return 1;
		}

		private int getFactor(WeaponSpriteType type)
		{
			switch (type)
			{
				case WeaponSpriteType.WalkFrame1:
				case WeaponSpriteType.WalkFrame2:
				case WeaponSpriteType.WalkFrame3:
				case WeaponSpriteType.WalkFrame4:
					return 4;
				case WeaponSpriteType.SwingFrame1:
				case WeaponSpriteType.SwingFrame2:
					return 2;
			}
			return 1;
		}

		private int getFactor(BootsSpriteType type)
		{
			switch (type)
			{
				case BootsSpriteType.WalkFrame1:
				case BootsSpriteType.WalkFrame2:
				case BootsSpriteType.WalkFrame3:
				case BootsSpriteType.WalkFrame4:
					return 4;
			}
			return 1;
		}
	}
}
