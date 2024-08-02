using System;
using System.IO;
using System.Linq;
using AutomaticTypeMapper;
using Commons.Music.Midi;
using EOLib;
using EOLib.Config;
using EOLib.IO.Map;

namespace EndlessClient.Audio
{
    [AutoMappedType(IsSingleton = true)]
    public sealed class MfxPlayer : IMfxPlayer
    {
        private readonly IConfigurationProvider _configurationProvider;

        private readonly string[] _mfxFiles;
        private readonly string[] _jboxFiles;
        private readonly IMidiOutput _output;
        private MidiPlayer _activePlayer;
        private int _activeId;

        public MfxPlayer(IConfigurationProvider configurationProvider)
        {
            _configurationProvider = configurationProvider;

            _mfxFiles = Directory.GetFiles(Constants.MfxDirectory, "*.mid");
            Array.Sort(_mfxFiles);

            _jboxFiles = Directory.GetFiles(Constants.JboxDirectory, "*.mid");
            Array.Sort(_jboxFiles);

            try
            {
                var access = MidiAccessManager.Default;
                _output = access.OpenOutputAsync(access.Outputs.Last().Id).Result;
            }
            catch
            {
                Console.WriteLine("WARNING: Unable to initialize the midi sound system. Background music will not play.");
            }
        }

        public void PlayBackgroundMusic(int id, MusicControl musicControl, bool isJukebox = false)
        {
            if (!_configurationProvider.MusicEnabled)
                return;

            if ((!isJukebox && (id < 1 || id > _mfxFiles.Length)) ||
                (isJukebox && (id < 1 || id > _jboxFiles.Length)))
                throw new ArgumentException("ID should be 1-based index", nameof(id));

            var interrupt = false;
            var repeat = false;

            switch (musicControl)
            {
                case MusicControl.InterruptIfDifferentPlayOnce:
                    interrupt = id != _activeId;
                    break;
                case MusicControl.InterruptPlayOnce:
                    interrupt = true;
                    break;
                case MusicControl.FinishPlayOnce:
                    break;
                case MusicControl.InterruptIfDifferentPlayRepeat:
                    interrupt = id != _activeId;
                    repeat = true;
                    break;
                case MusicControl.InterruptPlayRepeat:
                    interrupt = repeat = true;
                    break;
                case MusicControl.FinishPlayRepeat:
                    repeat = true;
                    break;
                case MusicControl.TurnOff:
                    StopBackgroundMusic();
                    return;
            }

            if (_activePlayer != null)
            {
                if (interrupt || _activePlayer.State == PlayerState.Stopped)
                {
                    StopBackgroundMusic();
                    StartPlaying();
                }
            }
            else
            {
                StartPlaying();
            }

            _activePlayer.PlaybackCompletedToEnd -= PlaybackCompleteAction;
            if (repeat)
            {
                _activePlayer.PlaybackCompletedToEnd += PlaybackCompleteAction;
            }

            void StartPlaying()
            {
                if (_output != null)
                {
                    var music = MidiMusic.Read(File.OpenRead(isJukebox ? _jboxFiles[id - 1] : _mfxFiles[id - 1]));
                    _activePlayer = new MidiPlayer(music, _output);
                    _activePlayer.Play();
                }

                _activeId = id;
            }

            void PlaybackCompleteAction()
            {
                PlayBackgroundMusic(id, musicControl);
            }
        }

        public void StopBackgroundMusic()
        {
            _activePlayer?.Stop();
            _activePlayer?.Dispose();
            _activePlayer = null;
        }

        public void Dispose()
        {
            StopBackgroundMusic();
            _output?.Dispose();
        }
    }

    public interface IMfxPlayer : IDisposable
    {
        void PlayBackgroundMusic(int id, MusicControl musicControl, bool isJukebox = false);

        void StopBackgroundMusic();
    }
}