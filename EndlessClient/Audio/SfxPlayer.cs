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
        private SoundEffectInstance _activeSfx;

        public SfxPlayer(IContentProvider contentProvider)
        {
            _contentProvider = contentProvider;
        }

        public void PlaySfx(SoundEffectID id)
        {
            _contentProvider.SFX[id-1].Play();
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
            if (_activeSfx != null && _activeSfx.State != SoundState.Stopped)
                return;

            StopLoopingSfx();

            _activeSfx = _contentProvider.SFX[id-1].CreateInstance();
            _activeSfx.IsLooped = true;
            _activeSfx.Play();
        }

        public void StopLoopingSfx()
        {
            _activeSfx?.Stop();
            _activeSfx?.Dispose();
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

        void StopLoopingSfx();
    }
}
