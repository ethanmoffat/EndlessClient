using AutomaticTypeMapper;
using EndlessClient.Audio;
using EOLib.Config;
using Microsoft.Xna.Framework;
using Optional;
using XNAControls;

namespace EndlessClient.UIControls
{
    [AutoMappedType]
    public class XnaControlSoundMapper : IXnaControlSoundMapper
    {
        private readonly IConfigurationProvider _configurationProvider;
        private readonly ISfxPlayer _sfxPlayer;

        public XnaControlSoundMapper(IConfigurationProvider configurationProvider,
                                     ISfxPlayer sfxPlayer)
        {
            _configurationProvider = configurationProvider;
            _sfxPlayer = sfxPlayer;
        }

        // other audio todos:
        //  - chat messages - nowall (spell activation sound), command (login sound)
        //  - chat messages received (group, admin, announce, server notification, PM)
        //  - buy/sell/craft item
        //  - chest/locker interaction
        //    - walk into or click chest/locker: spell activation sound
        //  - quest npc interaction?
        //    - links: button click
        //    - ok/cancel: button click
        //    - next/back: dialog button click
        //    - quest reward: trade accepted sound
        //  - bank interaction?
        //    - SoundEffectID.BuySell noise for all actions
        //  - paperdoll
        //    - item drag/drop

        public void BindSoundToControl(IGameComponent component, Option<SoundEffectID> soundEffectOverride = default)
        {
            soundEffectOverride.MatchSome(x => SetupSpecificSound((dynamic)component, x));
            soundEffectOverride.MatchNone(() => SetupSound((dynamic)component));
        }

        private void SetupSound(IXNAButton button) => SetupSpecificSound(button, SoundEffectID.ButtonClick);
        private void SetupSpecificSound(IXNAButton button, SoundEffectID sound) => button.OnClick += (_, _) => _sfxPlayer.PlaySfx(sound);

        private void SetupSound(IXNATextBox textBox) => SetupSpecificSound(textBox, SoundEffectID.TextBoxFocus);
        private void SetupSpecificSound(IXNATextBox textBox, SoundEffectID sound) => textBox.OnFocused += (_, _) => _sfxPlayer.PlaySfx(sound);

        private void SetupSound(CreateCharacterControl characterControl) => SetupSpecificSound(characterControl, SoundEffectID.TextBoxFocus);
        private void SetupSpecificSound(CreateCharacterControl characterControl, SoundEffectID sound) => characterControl.Clicked += (_, _) => _sfxPlayer.PlaySfx(sound);

        private void SetupSound(object unmatched) { }
        private void SetupSpecificSound(object unmatched, SoundEffectID sound) { }
    }

    public interface IXnaControlSoundMapper
    {
        void BindSoundToControl(IGameComponent component, Option<SoundEffectID> soundEffectOverride = default);
    }
}
