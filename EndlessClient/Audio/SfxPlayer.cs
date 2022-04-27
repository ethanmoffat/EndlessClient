using AutomaticTypeMapper;
using EndlessClient.Content;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;

namespace EndlessClient.Audio
{
    [AutoMappedType(IsSingleton = true)]
    public class SfxPlayer : ISfxPlayer
    {
        private readonly IContentProvider _contentProvider;
        private readonly Dictionary<SoundEffectID, SoundEffectInstance> _activeSfx;

        public SfxPlayer(IContentProvider contentProvider)
        {
            _contentProvider = contentProvider;
            _activeSfx = new Dictionary<SoundEffectID, SoundEffectInstance>();
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
            // todo: SFX

            //var res = _activeSfx.TryGetValue(id, out var sfxInstance);
            //if (res && sfxInstance.State != SoundState.Stopped)
            //    return;

            //if (res)
            //    _activeSfx[id].Dispose();
            //_activeSfx[id] = _contentProvider.SFX[id].CreateInstance();
            //_activeSfx[id]
        }
    }

    public interface ISfxPlayer
    {
        void PlayHarpNote(int index);

        void PlayGuitarNote(int index);
    }
}
