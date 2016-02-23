// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Xna.Framework.Audio;

namespace EndlessClient.Audio
{
	public enum Note
	{
		
	}

	public class SoundManager : IDisposable
	{
		private const string SFX_DIR = "sfx";
		private const string MFX_DIR = "mfx";

		//singleton pattern -- any newly constructed instance is copied from the 'instance'
		private static readonly object _construction_locker_ = new object();
		private static SoundManager _singletonInstance;

		private List<SoundInfo> _soundEffects;
		private List<SoundInfo> _guitarSounds;
		private List<SoundInfo> _harpSounds;

#if !LINUX //todo: find MediaPlayer implementation that is cross-platform
		private readonly MediaPlayer _musicPlayer;
		private List<Uri> _musicFiles;
		private Dispatcher _dispatcher;
#endif

		public SoundManager()
		{
			lock (_construction_locker_)
			{
				if (_singletonInstance != null)
				{
					CopyFromInstance();
					return;
				}

				var soundFiles = Directory.GetFiles(SFX_DIR, "*.wav");
				Array.Sort(soundFiles);

				_soundEffects = new List<SoundInfo>(81);
				_guitarSounds = new List<SoundInfo>(36);
				_harpSounds = new List<SoundInfo>(36);

#if !LINUX
				var musicFiles = Directory.GetFiles(MFX_DIR, "*.mid");
				Array.Sort(musicFiles);
				_musicFiles = new List<Uri>(musicFiles.Length);
#endif

				foreach (var sfx in soundFiles)
				{
					using (var sfxStream = WAVFileValidator.GetStreamWithCorrectLengthHeader(sfx))
					{
						//Note: this MAY throw InvalidOperationException if the file is invalid. However, WAVFileValidator fixes
						//	this for the original sfx files.
						SoundEffect nextEffect = SoundEffect.FromStream(sfxStream);

						if (sfx.ToLower().Contains("gui"))
							_guitarSounds.Add(nextEffect == null ? null : new SoundInfo(nextEffect));
						else if (sfx.ToLower().Contains("har"))
							_harpSounds.Add(nextEffect == null ? null : new SoundInfo(nextEffect));
						else
							_soundEffects.Add(nextEffect == null ? null : new SoundInfo(nextEffect));
					}
				}

#if !LINUX
				_musicPlayer = new MediaPlayer();
				_musicPlayer.MediaEnded += (o, e) => _musicPlayer.Position = new TimeSpan(0);

				foreach (string mfx in musicFiles)
					_musicFiles.Add(new Uri(mfx, UriKind.Relative));

				_dispatcher = Dispatcher.CurrentDispatcher;
#endif
				_singletonInstance = this;
			}
		}

		private void CopyFromInstance()
		{
			//shallow copy is intended
			_soundEffects = _singletonInstance._soundEffects;
			_guitarSounds = _singletonInstance._guitarSounds;
			_harpSounds = _singletonInstance._harpSounds;
#if !LINUX
			_musicFiles = _singletonInstance._musicFiles;
			_dispatcher = _singletonInstance._dispatcher;
#endif
		}

		public SoundEffectInstance GetGuitarSoundRef(Note which)
		{
			return _guitarSounds[(int) which].GetNextAvailableInstance();
		}

		public SoundEffectInstance GetHarpSoundRef(Note which)
		{
			return _harpSounds[(int) which].GetNextAvailableInstance();
		}

		public SoundEffectInstance GetSoundEffectRef(SoundEffectID whichSoundEffect)
		{
			return _soundEffects[(int)whichSoundEffect].GetNextAvailableInstance();
		}

		public void PlayLoopingSoundEffect(int sfxID)
		{
			if (sfxID < 1 || sfxID > _soundEffects.Count)
				throw new ArgumentOutOfRangeException("sfxID", "Out of range -- use a 1-based index for sfx id");

			_soundEffects[sfxID - 1].PlayLoopingInstance();
		}

		public void StopLoopingSoundEffect(int sfxID)
		{
			if (sfxID < 1 || sfxID > _soundEffects.Count)
				throw new ArgumentOutOfRangeException("sfxID", "Out of range -- use a 1-based index for sfx id");

			_soundEffects[sfxID - 1].StopLoopingInstance();
		}

		public void PlayBackgroundMusic(int mfxID)
		{
#if !LINUX
			if(mfxID < 1 || mfxID >= _musicFiles.Count)
				throw new ArgumentOutOfRangeException("mfxID", "The MFX id is out of range. Use the 1-based index that matches the number in the file name.");

			InvokeIfNeeded(() =>
				{
					_musicPlayer.Stop();
					_musicPlayer.Close();
					_musicPlayer.Open(_musicFiles[mfxID - 1]);
					_musicPlayer.Play();
				});
#endif
		}

		public void StopBackgroundMusic()
		{
#if !LINUX
			InvokeIfNeeded(() => _musicPlayer.Stop());
#endif
		}

#if !LINUX
		private void InvokeIfNeeded(Action action)
		{
			if (_dispatcher.Thread != Thread.CurrentThread)
				_dispatcher.BeginInvoke(action);
			else
				action();
		}
#endif

		~SoundManager()
		{
			//ensure that primary instance is disposed of if the reference to it is not explicitly disposed
			Dispose(false);
		}

		public void Dispose()
		{
			if (this != _singletonInstance)
				return;

			Dispose(true);
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
#if !LINUX
				if (_musicPlayer.HasAudio)
				{
					_musicPlayer.Stop();
					_musicPlayer.Close();
				}
#endif

				foreach (var sfx in _soundEffects)
					sfx.Dispose();

				foreach (var gui in _guitarSounds)
					gui.Dispose();

				foreach (var har in _harpSounds)
					har.Dispose();
			}

			GC.SuppressFinalize(this);
		}
	}
}
