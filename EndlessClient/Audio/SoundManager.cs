using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Audio;

namespace EndlessClient.Audio
{
    public enum Note
    {
    }

    public class SoundManager : IDisposable
    {
        private const string SFX_DIR = "sfx";

        //singleton pattern -- any newly constructed instance is copied from the 'instance'
        private static readonly object _construction_locker_ = new object();
        private static SoundManager _singletonInstance;

        private List<SoundInfo> _soundEffects;
        private List<SoundInfo> _guitarSounds;
        private List<SoundInfo> _harpSounds;

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

                foreach (var sfx in soundFiles)
                {
                    using (var sfxStream = WAVFileValidator.GetStreamWithCorrectLengthHeader(sfx))
                    {
                        //Note: this MAY throw InvalidOperationException if the file is invalid. However, WAVFileValidator fixes
                        //    this for the original sfx files.
                        SoundEffect nextEffect = SoundEffect.FromStream(sfxStream);

                        if (sfx.ToLower().Contains("gui"))
                            _guitarSounds.Add(nextEffect == null ? null : new SoundInfo(nextEffect));
                        else if (sfx.ToLower().Contains("har"))
                            _harpSounds.Add(nextEffect == null ? null : new SoundInfo(nextEffect));
                        else
                            _soundEffects.Add(nextEffect == null ? null : new SoundInfo(nextEffect));
                    }
                }

                _singletonInstance = this;
            }
        }

        private void CopyFromInstance()
        {
            //shallow copy is intended
            _soundEffects = _singletonInstance._soundEffects;
            _guitarSounds = _singletonInstance._guitarSounds;
            _harpSounds = _singletonInstance._harpSounds;
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
                throw new ArgumentOutOfRangeException(nameof(sfxID), "Out of range -- use a 1-based index for sfx id");

            _soundEffects[sfxID - 1].PlayLoopingInstance();
        }

        public void StopLoopingSoundEffect(int sfxID)
        {
            if (sfxID < 1 || sfxID > _soundEffects.Count)
                throw new ArgumentOutOfRangeException(nameof(sfxID), "Out of range -- use a 1-based index for sfx id");

            _soundEffects[sfxID - 1].StopLoopingInstance();
        }

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
