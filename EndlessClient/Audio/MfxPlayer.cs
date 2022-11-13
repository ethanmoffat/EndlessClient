using AutomaticTypeMapper;
using Commons.Music.Midi;
using EOLib;
using EOLib.IO.Map;
using System;
using System.IO;
using System.Linq;

namespace EndlessClient.Audio
{
    [AutoMappedType(IsSingleton = true)]
    public sealed class MfxPlayer : IMfxPlayer
    {
        private readonly string[] _fileNames;
        private readonly IMidiOutput _output;

        private MidiPlayer _activePlayer;
        private int _activeId;

        public MfxPlayer()
        {
            _fileNames = Directory.GetFiles(Constants.MfxDirectory, "*.mid");
            Array.Sort(_fileNames);

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

        public void PlayBackgroundMusic(int id, MusicControl musicControl)
        {
            if (id < 1 || id > _fileNames.Length)
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
                    var music = MidiMusic.Read(File.OpenRead(_fileNames[id - 1]));
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
        void PlayBackgroundMusic(int id, MusicControl musicControl);

        void StopBackgroundMusic();
    }
}
