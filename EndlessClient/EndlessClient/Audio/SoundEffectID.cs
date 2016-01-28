// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EndlessClient.Audio
{

	//sfx001 will be ID int 0
	public enum SoundEffectID
	{
		LayeredTechIntro,
		ButtonClick,
		DialogButtonClick,
		TextBoxFocus, //also the sound when opening chest?
		Login = 4, //also the sound from a server message?
		UnknownShimmerSound,
		UnknownStaticSound,
		ScreenCapture,
		PMReceived = 8,
		PunchAttack,
		UnknownWarpSound,
		UnknownPingSound,
		UnknownClickSound = 12,
		UnknownHarpSound,
		MeleeWeaponAttack,
		UnknownClickSound2,
		TradeAccepted = 16,
		UnknownNotificationSound,
		UnknownWhooshSound,
		ItemInventoryPickup,
		ItemInventoryPlace = 20,
		Earthquake,
		DoorClose,
		DoorOpen,
		UnknownClickSound3 = 24,
		BuySell,
		Craft,
		UnknownBuzzSound,
		UnknownBloopSound = 28,
		UnknownAttackLikeSound,
		PotionOfFlamesEffect,
		AdminWarp,
		NoWallWalk = 32,
		PotionOfEvilTerrorEffect,
		PotionOfFireworksEffect,
		PotionOfSparklesEffect,
		LearnNewSpell = 36,
		AttackBow,
		LevelUp,
		Dead,
		JumpStone = 40,
		Water,
		Heal,
		Harp1,
		Harp2 = 44,
		Harp3,
		Guitar1,
		Guitar2,
		Guitar3 = 48,
		Thunder,
		UnknownTimerSound,
		UnknownFanfareSound,
		Gun = 52,
		UltimaBlastSpell,
		ShieldSpell,
		UnknownAggressiveShieldSound,
		IceBlastSpell1 = 56,
		EnergyBallSpell,
		WhirlSpell,
		BouldersSpell,
		HeavenSpell = 60,
		//there's another ice blast spell in here
		MapEffectHPDrain = 69,
		MapEffectTPDrain = 70,
		Spikes = 71,
		//not sure what the remaining sounds are but I think map ambient noises start eventually
		//map noises seem to fade out as you change maps or get farther away from them
	}
}
