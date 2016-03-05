// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Net.API;

namespace EOLib.Data.BLL
{
	public class CharacterRenderProperties : ICharacterRenderProperties
	{
		private const int MAX_NUMBER_OF_WALK_FRAMES   = 4;
		private const int MAX_NUMBER_OF_ATTACK_FRAMES = 2;
		private const int MAX_NUMBER_OF_EMOTE_FRAMES  = 3;

		public byte HairStyle { get; private set; }
		public byte HairColor { get; private set; }
		public byte Race { get; private set; }
		public byte Gender { get; private set; }

		public short BootsGraphic { get; private set; }
		public short ArmorGraphic { get; private set; }
		public short HatGraphic { get; private set; }
		public short ShieldGraphic { get; private set; }
		public short WeaponGraphic { get; private set; }

		public EODirection Direction { get; private set; }

		public int WalkFrame { get; private set; }
		public int AttackFrame { get; private set; }
		public int EmoteFrame { get; private set; }

		public SitState SitState { get; private set; }

		public bool IsHidden { get; private set; }
		public bool IsDead { get; private set; }

		public ICharacterRenderProperties WithHairStyle(byte newHairStyle)
		{
			var props = MakeCopy(this);
			props.HairStyle = newHairStyle;
			return props;
		}

		public ICharacterRenderProperties WithHairColor(byte newHairColor)
		{
			var props = MakeCopy(this);
			props.HairColor = newHairColor;
			return props;
		}

		public ICharacterRenderProperties WithRace(byte newRace)
		{
			var props = MakeCopy(this);
			props.Race = newRace;
			return props;
		}

		public ICharacterRenderProperties WithGender(byte newGender)
		{
			var props = MakeCopy(this);
			props.Gender = newGender;
			return props;
		}

		public ICharacterRenderProperties WithBootsGraphic(short bootsGraphic)
		{
			var props = MakeCopy(this);
			props.BootsGraphic = bootsGraphic;
			return props;
		}

		public ICharacterRenderProperties WithArmorGraphic(short armorGraphic)
		{
			var props = MakeCopy(this);
			props.ArmorGraphic = armorGraphic;
			return props;
		}

		public ICharacterRenderProperties WithHatGraphic(short hatGraphic)
		{
			var props = MakeCopy(this);
			props.HatGraphic = hatGraphic;
			return props;
		}

		public ICharacterRenderProperties WithShieldGraphic(short shieldGraphic)
		{
			var props = MakeCopy(this);
			props.ShieldGraphic = shieldGraphic;
			return props;
		}

		public ICharacterRenderProperties WithWeaponGraphic(short weaponGraphic)
		{
			var props = MakeCopy(this);
			props.WeaponGraphic = weaponGraphic;
			return props;
		}

		public ICharacterRenderProperties WithDirection(EODirection newDirection)
		{
			var props = MakeCopy(this);
			props.Direction = newDirection;
			return props;
		}

		public ICharacterRenderProperties WithNextWalkFrame()
		{
			var props = MakeCopy(this);
			props.WalkFrame = (props.WalkFrame + 1) % MAX_NUMBER_OF_WALK_FRAMES;
			return props;
		}

		public ICharacterRenderProperties WithNextAttackFrame()
		{
			var props = MakeCopy(this);
			props.AttackFrame = (props.AttackFrame + 1) % MAX_NUMBER_OF_ATTACK_FRAMES;
			return props;
		}

		public ICharacterRenderProperties WithNextEmoteFrame()
		{
			var props = MakeCopy(this);
			props.EmoteFrame = (props.EmoteFrame + 1) % MAX_NUMBER_OF_EMOTE_FRAMES;
			return props;
		}

		public ICharacterRenderProperties WithSitState(SitState newState)
		{
			var props = MakeCopy(this);
			props.SitState = newState;
			return props;
		}

		public ICharacterRenderProperties WithIsHidden(bool hidden)
		{
			var props = MakeCopy(this);
			props.IsHidden = hidden;
			return props;
		}

		public ICharacterRenderProperties WithDead()
		{
			var props = MakeCopy(this);
			props.IsDead = true;
			return props;
		}

		private static CharacterRenderProperties MakeCopy(ICharacterRenderProperties other)
		{
			return new CharacterRenderProperties
			{
				HairStyle = other.HairStyle,
				HairColor = other.HairColor,
				Race = other.Race,
				Gender = other.Gender,

				BootsGraphic = other.BootsGraphic,
				ArmorGraphic = other.ArmorGraphic,
				HatGraphic = other.HatGraphic,
				ShieldGraphic = other.ShieldGraphic,
				WeaponGraphic = other.WeaponGraphic,

				Direction = other.Direction,
				WalkFrame = other.WalkFrame,
				AttackFrame = other.AttackFrame,
				EmoteFrame = other.EmoteFrame,

				IsHidden = other.IsHidden,
				IsDead = other.IsDead
			};
		}
	}
}
