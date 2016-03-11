// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Linq;
using EndlessClient.Rendering.Sprites;
using EOLib;
using EOLib.Data.BLL;
using EOLib.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.CharacterProperties
{
	public class SkinRenderer : ICharacterPropertyRenderer
	{
		private readonly SpriteBatch _spriteBatch;
		private readonly ICharacterRenderProperties _renderProperties;
		private readonly ISpriteSheet _skinSheet;
		private readonly IDataFile<ItemRecord> _itemFile;

		public SkinRenderer(SpriteBatch spriteBatch,
						   ICharacterRenderProperties renderProperties,
						   ISpriteSheet skinSheet,
						   IDataFile<ItemRecord> itemFile)
		{
			_spriteBatch = spriteBatch;
			_renderProperties = renderProperties;
			_skinSheet = skinSheet;
			_itemFile = itemFile;
		}

		public void Render(Rectangle parentCharacterDrawArea)
		{
			//todo: I most likely screwed something up when re-implementing this so it needs to be verified for all states!

			var offsets = GetOffsets();
			var genderOffset = _renderProperties.Gender == 0 ? 12 : 13;
			var drawLoc = new Vector2(parentCharacterDrawArea.X + 6 + offsets.X, parentCharacterDrawArea.Y + genderOffset + offsets.Y);

			_spriteBatch.Draw(_skinSheet.SheetTexture, drawLoc, _skinSheet.SourceRectangle, Color.White, 0.0f, Vector2.Zero, 1.0f,
							  _renderProperties.IsFacing(EODirection.Up, EODirection.Right) ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
							  0.0f);
		}

		private bool IsWeaponAMeleeWeapon()
		{
			var weaponInfo = _itemFile.Data.SingleOrDefault(
				x => x.Type == ItemType.Weapon &&
					 x.DollGraphic == _renderProperties.WeaponGraphic);

			return weaponInfo == null || weaponInfo.SubType != ItemSubType.Ranged;
		}

		private Vector2 GetOffsets()
		{
			switch (_renderProperties.CurrentAction)
			{
				case CharacterActionState.SpellCast: return GetSpellCastOffset();
				case CharacterActionState.Walking: return GetWalkingOffset();
				case CharacterActionState.Attacking: return GetAttackingOffset();
				//these actions don't require offsets for the skin:
				case CharacterActionState.Standing:
				case CharacterActionState.Sitting:
				case CharacterActionState.Emote: return Vector2.Zero;
				default: throw new ArgumentOutOfRangeException();
			}
		}

		private Vector2 GetSpellCastOffset()
		{
			return new Vector2(0, -4);
		}

		private Vector2 GetWalkingOffset()
		{
			int skinXOff = 0;
			if (_renderProperties.Gender == 1)
			{
				if (_renderProperties.IsFacing(EODirection.Down))
					skinXOff = -1;
				else if (_renderProperties.IsFacing(EODirection.Right))
					skinXOff = 1;
			}

			return new Vector2(skinXOff - 4, -1);
		}

		private Vector2 GetAttackingOffset()
		{
			var weaponIsMelee = IsWeaponAMeleeWeapon();

			int skinOffX, skinOffY = 0;
			if (_renderProperties.IsFacing(EODirection.Up, EODirection.Right))
				skinOffX = _renderProperties.Gender == 1 ? -1 : -2;
			else
				skinOffX = _renderProperties.Gender == 1 ? -5 : -4;

			if (weaponIsMelee && _renderProperties.AttackFrame == 2)
			{
				var extraOffX = _renderProperties.Gender == 1 ? 2 : 4;
				var extraOffY = _renderProperties.Direction == EODirection.Up ? -2 : 1;

				if (_renderProperties.IsFacing(EODirection.Down, EODirection.Left))
				{
					extraOffX *= -1;
					extraOffY *= -1;
				}

				skinOffX += extraOffX;
				skinOffY += extraOffY;
			}
			else if (!weaponIsMelee && _renderProperties.AttackFrame == 1)
			{
				var extraOffX = _renderProperties.Gender == 1 ? 2 : 1;
				var extraOffY = 0;

				if (_renderProperties.IsFacing(EODirection.Down, EODirection.Right))
				{
					extraOffX += 2;
					extraOffY = 1;
				}
				if (_renderProperties.IsFacing(EODirection.Down, EODirection.Left))
				{
					extraOffX += 7;
					extraOffX *= -1;
				}

				skinOffX += extraOffX;
				skinOffY += extraOffY;
			}

			return new Vector2(skinOffX, skinOffY);
		}
	}
}
