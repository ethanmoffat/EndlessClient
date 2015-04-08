using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Audio;

namespace EndlessClient
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

		//not sure what the remaining sounds are but I think map ambient noises start eventually
		//map noises seem to fade out as you change maps or get farther away from them
	}

	//mfx001 will be ID int 0
	public enum MusicEffectID
	{
		
	}

	public enum Note
	{
		
	}

	public class EOSoundManager : IDisposable
	{
		private const string SFX_DIR = "sfx";
		private const string MFX_DIR = "mfx";

		private List<SoundEffectInstance> m_sounds;
		private List<SoundEffectInstance> m_guitarSounds;
		private List<SoundEffectInstance> m_harpSounds;
		private List<SoundEffectInstance> m_music;
		private List<SoundEffect> m_effects;

		//singleton pattern -- any newly constructed instance is copied from the 'instance'
		private static readonly object _construction_locker_ = new object();
		private static EOSoundManager inst;

		private bool IsDisposed { get; set; }

		public EOSoundManager()
		{
			lock (_construction_locker_)
			{
				if (inst != null)
				{
					_copyFrom(inst);
					return;
				}

				string[] soundFiles = Directory.GetFiles(SFX_DIR, "*.wav");
				Array.Sort(soundFiles);

				string[] musicFiles = Directory.GetFiles(MFX_DIR, "*.mid");
				Array.Sort(musicFiles);

				m_sounds = new List<SoundEffectInstance>(81);
				m_guitarSounds = new List<SoundEffectInstance>(36);
				m_harpSounds = new List<SoundEffectInstance>(36);
				m_music = new List<SoundEffectInstance>(musicFiles.Length);
				m_effects = new List<SoundEffect>(soundFiles.Length + musicFiles.Length);

				foreach (string sfx in soundFiles)
				{
					using (FileStream fs = new FileStream(sfx, FileMode.Open))
					{
						SoundEffect nextEffect = SoundEffect.FromStream(fs);
						if(sfx.ToLower().Contains("gui"))
							m_guitarSounds.Add(nextEffect.CreateInstance());
						else if(sfx.ToLower().Contains("har"))
							m_harpSounds.Add(nextEffect.CreateInstance());
						else
							m_sounds.Add(nextEffect.CreateInstance());
						m_effects.Add(nextEffect);
					}
				}

				foreach (string mfx in musicFiles)
				{
					using (FileStream fs = new FileStream(mfx, FileMode.Open))
					{
						SoundEffect nextEffect = SoundEffect.FromStream(fs);
						m_music.Add(nextEffect.CreateInstance());
						m_effects.Add(nextEffect);
					}
				}

				inst = this;
			}
		}

		private void _copyFrom(EOSoundManager other)
		{
			//shallow copy is intended
			m_sounds = other.m_sounds;
			m_guitarSounds = other.m_guitarSounds;
			m_harpSounds = other.m_harpSounds;
			m_music = other.m_music;
			m_effects = other.m_effects;
		}

		public SoundEffectInstance GetGuitarSoundRef(Note which)
		{
			return m_guitarSounds[(int) which];
		}

		public SoundEffectInstance GetHarpSoundRef(Note which)
		{
			return m_harpSounds[(int) which];
		}

		public SoundEffectInstance GetSoundEffectRef(SoundEffectID whichSoundEffect)
		{
			return m_sounds[(int) whichSoundEffect];
		}

		public SoundEffectInstance GetMusicEffectRef(MusicEffectID whichMusicEffect)
		{
			return m_music[(int) whichMusicEffect];
		}

		~EOSoundManager()
		{
			//ensure that primary instance is disposed of if the reference to it is not explicitly disposed
			if (this == inst && !IsDisposed)
			{
				Dispose(true);
			}
		}

		public virtual void Dispose()
		{
			if (this != inst)
				return;

			Dispose(true);
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				foreach (SoundEffectInstance mfx in m_music)
				{
					mfx.Stop();
					mfx.Dispose();
				}

				foreach (SoundEffectInstance sfx in m_sounds)
				{
					sfx.Stop();
					sfx.Dispose();
				}

				foreach (SoundEffectInstance gui in m_guitarSounds)
				{
					gui.Stop();
					gui.Dispose();
				}

				foreach (SoundEffectInstance har in m_harpSounds)
				{
					har.Stop();
					har.Dispose();
				}

				foreach(SoundEffect sfx in m_effects)
					sfx.Dispose();
			}

			IsDisposed = true;
		}
	}
}
