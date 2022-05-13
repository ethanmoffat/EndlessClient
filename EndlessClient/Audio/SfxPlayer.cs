using AutomaticTypeMapper;
using EndlessClient.Content;
using Microsoft.Xna.Framework.Audio;
using System;

namespace EndlessClient.Audio
{
    [AutoMappedType(IsSingleton = true)]
    public sealed class SfxPlayer : ISfxPlayer
    {
        private readonly IContentProvider _contentProvider;
        private SoundEffectInstance _loopingSfx;

        public SfxPlayer(IContentProvider contentProvider)
        {
            _contentProvider = contentProvider;
        }

        public void PlaySfx(SoundEffectID id)
        {
            _contentProvider.SFX[id].Play();
        }

        public void PlayHarpNote(int index)
        {
            if (index < 0 || index >= _contentProvider.HarpNotes.Count)
                return;

            _contentProvider.HarpNotes[index].Play();
        }

        public void PlayGuitarNote(int index)
        {
            if (index < 0 || index >= _contentProvider.GuitarNotes.Count)
                return;

            _contentProvider.GuitarNotes[index].Play();
        }

        public void PlayLoopingSfx(SoundEffectID id)
        {
            if (_loopingSfx != null && _loopingSfx.State != SoundState.Stopped)
                return;

            StopLoopingSfx();

            _loopingSfx = _contentProvider.SFX[id].CreateInstance();
            _loopingSfx.IsLooped = true;
            _loopingSfx.Volume = 0.5f;
            _loopingSfx.Play();
        }

        public void SetLoopingSfxVolume(float volume)
        {
            if (volume < 0 || volume > 1)
                throw new ArgumentException($"Volume {volume} must be between 0 and 1", nameof(volume));

            if (_loopingSfx != null)
                _loopingSfx.Volume = volume;
        }

        public void StopLoopingSfx()
        {
            _loopingSfx?.Stop();
            _loopingSfx?.Dispose();
        }

        public void Dispose()
        {
            StopLoopingSfx();
        }
    }

    public interface ISfxPlayer : IDisposable
    {
        void PlaySfx(SoundEffectID id);

        void PlayHarpNote(int index);

        void PlayGuitarNote(int index);

        void PlayLoopingSfx(SoundEffectID id);

        void SetLoopingSfxVolume(float volume);

        void StopLoopingSfx();
    }
}
