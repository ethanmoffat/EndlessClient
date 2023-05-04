using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using AutomaticTypeMapper;
using EndlessClient.Audio;
using EndlessClient.Content;
using EndlessClient.ControlSets;
using EndlessClient.Rendering;
using EndlessClient.Rendering.Chat;
using EndlessClient.Test;
using EndlessClient.UIControls;
using EOLib;
using EOLib.Config;
using EOLib.Graphics;
using EOLib.IO;
using EOLib.IO.Actions;
using EOLib.Logger;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.BitmapFonts;

namespace EndlessClient.GameExecution
{
    [MappedType(BaseType = typeof(IEndlessGame), IsSingleton = true)]
    public class EndlessGame : Game, IEndlessGame
    {
        private readonly IClientWindowSizeRepository _windowSizeRepository;
        private readonly IContentProvider _contentProvider;
        private readonly IGraphicsDeviceRepository _graphicsDeviceRepository;
        private readonly IGameWindowRepository _gameWindowRepository;
        private readonly IControlSetRepository _controlSetRepository;
        private readonly IControlSetFactory _controlSetFactory;
        private readonly ITestModeLauncher _testModeLauncher;
        private readonly IPubFileLoadActions _pubFileLoadActions;
        private readonly ILoggerProvider _loggerProvider;
        private readonly IChatBubbleTextureProvider _chatBubbleTextureProvider;
        private readonly IShaderRepository _shaderRepository;
        private readonly IConfigurationProvider _configurationProvider;
        private readonly IMfxPlayer _mfxPlayer;
        private readonly IXnaControlSoundMapper _soundMapper;
        private GraphicsDeviceManager _graphicsDeviceManager;

        private KeyboardState _previousKeyState;
        private TimeSpan _lastFrameUpdate;

#if DEBUG
        private SpriteBatch _spriteBatch;
        private Stopwatch _lastFrameRenderTime = Stopwatch.StartNew();
        private int _frames, _displayFrames;
        private Texture2D _black;
#endif

        public EndlessGame(IClientWindowSizeRepository windowSizeRepository,
                           IContentProvider contentProvider,
                           IGraphicsDeviceRepository graphicsDeviceRepository,
                           IGameWindowRepository gameWindowRepository,
                           IControlSetRepository controlSetRepository,
                           IControlSetFactory controlSetFactory,
                           ITestModeLauncher testModeLauncher,
                           IPubFileLoadActions pubFileLoadActions,
                           ILoggerProvider loggerProvider,
                           IChatBubbleTextureProvider chatBubbleTextureProvider,
                           IShaderRepository shaderRepository,
                           IConfigurationProvider configurationProvider,
                           IMfxPlayer mfxPlayer,
                           IXnaControlSoundMapper soundMapper)
        {
            _windowSizeRepository = windowSizeRepository;
            _contentProvider = contentProvider;
            _graphicsDeviceRepository = graphicsDeviceRepository;
            _gameWindowRepository = gameWindowRepository;
            _controlSetRepository = controlSetRepository;
            _controlSetFactory = controlSetFactory;
            _testModeLauncher = testModeLauncher;
            _pubFileLoadActions = pubFileLoadActions;
            _loggerProvider = loggerProvider;
            _chatBubbleTextureProvider = chatBubbleTextureProvider;
            _shaderRepository = shaderRepository;
            _configurationProvider = configurationProvider;
            _mfxPlayer = mfxPlayer;
            _soundMapper = soundMapper;
            _graphicsDeviceManager = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = ClientWindowSizeRepository.DEFAULT_BACKBUFFER_WIDTH,
                PreferredBackBufferHeight = ClientWindowSizeRepository.DEFAULT_BACKBUFFER_HEIGHT
            };

            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            Components.ComponentAdded += (o, e) =>
            {
                // this is bad hack
                // all pre-game controls have a specific sound that should be mapped to them.
                // in-game controls get their sounds mapped individually.
                //
                // Checking for GameStates.LoggedIn because the in-game controls are
                //     added to the components in the LoggedIn state
                if (_controlSetRepository.CurrentControlSet.GameState != GameStates.LoggedIn)
                {
                    _soundMapper.BindSoundToControl(e.GameComponent);
                }
            };

            base.Initialize();

            IsMouseVisible = true;
            IsFixedTimeStep = false;
            _previousKeyState = Keyboard.GetState();

            // setting Width/Height in window size repository applies the change to disable vsync
            _graphicsDeviceManager.SynchronizeWithVerticalRetrace = false;
            _graphicsDeviceManager.IsFullScreen = false;
            _windowSizeRepository.Width = ClientWindowSizeRepository.DEFAULT_BACKBUFFER_WIDTH;
            _windowSizeRepository.Height = ClientWindowSizeRepository.DEFAULT_BACKBUFFER_HEIGHT;

            _windowSizeRepository.GameWindowSizeChanged += (_, _) =>
            {
                if (_windowSizeRepository.Width < ClientWindowSizeRepository.DEFAULT_BACKBUFFER_HEIGHT)
                    _windowSizeRepository.Width = ClientWindowSizeRepository.DEFAULT_BACKBUFFER_WIDTH;

                if (_windowSizeRepository.Height < ClientWindowSizeRepository.DEFAULT_BACKBUFFER_HEIGHT)
                    _windowSizeRepository.Height = ClientWindowSizeRepository.DEFAULT_BACKBUFFER_HEIGHT;
            };

            Exiting += (_, _) => _mfxPlayer.StopBackgroundMusic();
        }

        protected override void LoadContent()
        {
#if DEBUG
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _black = new Texture2D(GraphicsDevice, 1, 1);
            _black.SetData(new[] { Color.Black });
#endif

            _contentProvider.Load();

            //todo: all the things that should load stuff as part of game's load/initialize should be broken into a pattern
            _chatBubbleTextureProvider.LoadContent();

            //the GraphicsDevice/Window don't exist until Initialize() is called by the framework
            //Ideally, this would be set in a DependencyContainer, but I'm not sure of a way to do that now
            _graphicsDeviceRepository.GraphicsDevice = GraphicsDevice;
            _graphicsDeviceRepository.GraphicsDeviceManager = _graphicsDeviceManager;
            _gameWindowRepository.Window = Window;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                if (!File.Exists(ShaderRepository.HairClipFile))
                {
                    throw new FileNotFoundException("Missing HairClip shader");
                }

                var shaderBytes = File.ReadAllBytes(ShaderRepository.HairClipFile);
                _shaderRepository.Shaders[ShaderRepository.HairClip] = new Effect(GraphicsDevice, shaderBytes);
            }

            SetUpInitialControlSet();

            if (_configurationProvider.MusicEnabled)
            {
                _mfxPlayer.PlayBackgroundMusic(1, EOLib.IO.Map.MusicControl.InterruptPlayRepeat);
            }

            AttemptToLoadPubFiles();

            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            // Force update at 60FPS
            // Some game components rely on ~60FPS update times. See: https://github.com/ethanmoffat/EndlessClient/issues/199
            // Using IsFixedTimeStep = true with TargetUpdateTime set to 60FPS also limits the draw rate, which is not desired
            if ((gameTime.TotalGameTime - _lastFrameUpdate).TotalMilliseconds > 1000.0 / 60)
            {

#if DEBUG
                //todo: this is a debug-only mode launched with the F5 key.
                //todo: move this to be handled by some sort of key listener once function keys are handled in-game
                var currentKeyState = Keyboard.GetState();
                if (_previousKeyState.IsKeyDown(Keys.F5) && currentKeyState.IsKeyUp(Keys.F5))
                {
                    _testModeLauncher.LaunchTestMode();
                }

                _previousKeyState = currentKeyState;
#endif

                try
                {
                    base.Update(gameTime);
                }
                catch (InvalidOperationException ioe) when (ioe.InnerException is NullReferenceException)
                {
                    // hide "failed to compare two elements in the array" error from Monogame
                }

                _lastFrameUpdate = gameTime.TotalGameTime;
            }
        }


        protected override void Draw(GameTime gameTime)
        {
            var isTestMode = _controlSetRepository.CurrentControlSet.GameState == GameStates.TestMode;
            GraphicsDevice.Clear(isTestMode ? Color.White : Color.Black);

            base.Draw(gameTime);
#if DEBUG
            _frames++;

            var fpsString = $"FPS: {_displayFrames}{(gameTime.IsRunningSlowly ? " (SLOW)" : string.Empty)}";
            var dim = _contentProvider.Fonts[Constants.FontSize09].MeasureString(fpsString);

            _spriteBatch.Begin();
            _spriteBatch.Draw(_black, new Rectangle(18, 18, (int)dim.Width + 4, (int)dim.Height + 4), Color.White);
            _spriteBatch.DrawString(_contentProvider.Fonts[Constants.FontSize09], fpsString, new Vector2(20, 20), Color.White);
            _spriteBatch.End();

            if (_lastFrameRenderTime.ElapsedMilliseconds > 1000)
            {
                _displayFrames = _frames;
                _frames = 0;
                _lastFrameRenderTime = Stopwatch.StartNew();
            }
#endif
        }

        private void AttemptToLoadPubFiles()
        {
            const string PUB_LOG_MSG = "**** Unable to load default PUB file: {0}. Exception message: {1}";

            try
            {
                _pubFileLoadActions.LoadItemFile(rangedWeaponIds: Constants.RangedWeaponIDs.Concat(Constants.InstrumentIDs));
            }
            catch (Exception ex) when (ex is IOException || ex is ArgumentException)
            {
                _loggerProvider.Logger.Log(PUB_LOG_MSG, string.Format(PubFileNameConstants.EIFFormat, 1), ex.Message);
            }

            try
            {
                _pubFileLoadActions.LoadNPCFile();
            }
            catch (Exception ex) when (ex is IOException || ex is ArgumentException)
            {
                _loggerProvider.Logger.Log(PUB_LOG_MSG, string.Format(PubFileNameConstants.ENFFormat, 1), ex.Message);
            }

            try
            {
                _pubFileLoadActions.LoadSpellFile();
            }
            catch (Exception ex) when (ex is IOException || ex is ArgumentException)
            {
                _loggerProvider.Logger.Log(PUB_LOG_MSG, string.Format(PubFileNameConstants.ESFFormat, 1), ex.Message);
            }

            try
            {
                _pubFileLoadActions.LoadClassFile();
            }
            catch (Exception ex) when (ex is IOException || ex is ArgumentException)
            {
                _loggerProvider.Logger.Log(PUB_LOG_MSG, string.Format(PubFileNameConstants.ECFFormat, 1), ex.Message);
            }
        }

        private void SetUpInitialControlSet()
        {
            var controls = _controlSetFactory.CreateControlsForState(
                GameStates.Initial,
                _controlSetRepository.CurrentControlSet);
            _controlSetRepository.CurrentControlSet = controls;

            //since the controls are being created in Initialize(), adding them to the default game
            //  doesn't call the Initialize() method on any controls, so it must be done here
            foreach (var xnaControl in _controlSetRepository.CurrentControlSet.AllComponents)
                xnaControl.Initialize();
        }
    }
}
