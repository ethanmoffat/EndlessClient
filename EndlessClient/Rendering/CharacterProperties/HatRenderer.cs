// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Linq;
using EOLib;
using EOLib.Data.BLL;
using EOLib.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.CharacterProperties
{
	public class HatRenderer : ICharacterPropertyRenderer
	{
		private readonly SpriteBatch _spriteBatch;
		private readonly ICharacterRenderProperties _renderProperties;
		private readonly Texture2D _hatTexture;
		private readonly IDataFile<ItemRecord> _itemFile;

		public HatRenderer(SpriteBatch spriteBatch,
						   ICharacterRenderProperties renderProperties,
						   Texture2D hatTexture,
						   IDataFile<ItemRecord> itemFile)
		{
			_spriteBatch = spriteBatch;
			_renderProperties = renderProperties;
			_hatTexture = hatTexture;
			_itemFile = itemFile;
		}

		public void Render(Rectangle parentCharacterDrawArea)
		{
			var offsets = GetOffsets();
			var drawLoc = new Vector2(parentCharacterDrawArea.X + offsets.X, parentCharacterDrawArea.Y - 3 + offsets.Y);

			_spriteBatch.Draw(_hatTexture, drawLoc, null, Color.White, 0.0f, Vector2.Zero, 1.0f,
							  IsFacing(EODirection.Up, EODirection.Right) ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
							  0.0f);
		}

		private bool IsFacing(params EODirection[] directions)
		{
			return directions.Contains(_renderProperties.Direction);
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
			var weaponIsMelee = IsWeaponAMeleeWeapon();
			int hatOffX = 0, hatOffY = 0;

			if (weaponIsMelee && _renderProperties.AttackFrame == 2)
			{
				hatOffX = _renderProperties.Gender == 1 ? 6 : 8;
				if (IsFacing(EODirection.Down, EODirection.Left))
					hatOffX *= -1;

				if (IsFacing(EODirection.Down, EODirection.Right))
					hatOffY = _renderProperties.Gender == 1 ? 5 : 6;
			}
			else if (!weaponIsMelee && _renderProperties.AttackFrame == 1)
			{
				hatOffX = _renderProperties.Gender == 1 ? 3 : 1;
				if (IsFacing(EODirection.Down, EODirection.Left))
					hatOffX *= -1;

				if (IsFacing(EODirection.Down, EODirection.Right))
					hatOffY = _renderProperties.Gender == 1 ? 1 : 0;
			}

			return new Vector2(hatOffX, hatOffY);
		}
	}
}
