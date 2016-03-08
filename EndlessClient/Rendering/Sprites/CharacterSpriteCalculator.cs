// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib;
using EOLib.Data.BLL;
using EOLib.Graphics;
using EOLib.Net.API;

namespace EndlessClient.Rendering.Sprites
{
	public class CharacterSpriteCalculator : ICharacterSpriteCalculator
	{
		private readonly INativeGraphicsManager _gfxManager;
		private readonly ICharacterRenderProperties _characterRenderProperties;

		public CharacterSpriteCalculator(INativeGraphicsManager gfxManager,
										 ICharacterRenderProperties characterRenderProperties)
		{
			_gfxManager = gfxManager;
			_characterRenderProperties = characterRenderProperties;
		}

		public ISpriteSheet GetBootsTexture(bool isBow)
		{
			BootsSpriteType type = BootsSpriteType.Standing;
			switch (_characterRenderProperties.CurrentAction)
			{
				case CharacterActionState.Walking:
					switch (_characterRenderProperties.WalkFrame)
					{
						case 1: type = BootsSpriteType.WalkFrame1; break;
						case 2: type = BootsSpriteType.WalkFrame2; break;
						case 3: type = BootsSpriteType.WalkFrame3; break;
						case 4: type = BootsSpriteType.WalkFrame4; break;
					}
					break;
				case CharacterActionState.Attacking:
					if (!isBow && _characterRenderProperties.AttackFrame == 2 ||
						isBow && _characterRenderProperties.AttackFrame == 1)
						type = BootsSpriteType.Attack;
					break;
				case CharacterActionState.Sitting:
					switch (_characterRenderProperties.SitState)
					{
						case SitState.Chair: type = BootsSpriteType.SitChair; break;
						case SitState.Floor: type = BootsSpriteType.SitGround; break;
					}
					break;
			}

			var gfxFile = _characterRenderProperties.Gender == 0 ? GFXTypes.FemaleShoes : GFXTypes.MaleShoes;

			var offset = GetOffsetBasedOnState(type) * GetBaseFactorFromDirection();
			var baseBootGraphic = GetBaseBootGraphic();
			var gfxNumber = baseBootGraphic + (int)type + offset;

			return new SpriteSheet(_gfxManager.TextureFromResource(gfxFile, gfxNumber, true));
		}

		public ISpriteSheet GetArmorTexture(bool isBow)
		{
			throw new System.NotImplementedException();
		}

		public ISpriteSheet GetHatTexture()
		{
			throw new System.NotImplementedException();
		}

		public ISpriteSheet GetShieldTexture(bool shieldIsOnBack)
		{
			throw new System.NotImplementedException();
		}

		public ISpriteSheet GetWeaponTexture(bool isBow)
		{
			throw new System.NotImplementedException();
		}

		public ISpriteSheet GetSkinTexture(bool isBow)
		{
			throw new System.NotImplementedException();
		}

		public ISpriteSheet GetHairTexture()
		{
			throw new System.NotImplementedException();
		}

		public ISpriteSheet GetFaceTexture()
		{
			throw new System.NotImplementedException();
		}

		public ISpriteSheet GetEmoteTexture()
		{
			throw new System.NotImplementedException();
		}

		private short GetBaseBootGraphic()
		{
			return (short)((_characterRenderProperties.BootsGraphic - 1) * 40);
		}

		private int GetBaseFactorFromDirection()
		{
			return _characterRenderProperties.Direction == EODirection.Down ||
				   _characterRenderProperties.Direction  == EODirection.Right ? 0 : 1;
		}

		private int GetOffsetBasedOnState(BootsSpriteType type)
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
