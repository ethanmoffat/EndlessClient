using System.IO;
using System.Runtime.InteropServices;
using AutomaticTypeMapper;
using EndlessClient.Content;
using EndlessClient.ControlSets;
using EndlessClient.Rendering;
using EndlessClient.Rendering.Chat;
using EndlessClient.Test;
using EOLib.Graphics;
using EOLib.IO;
using EOLib.IO.Actions;
using EOLib.Logger;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace EndlessClient.GameExecution
{
    [MappedType(BaseType = typeof(IEndlessGame), IsSingleton = true)]
    public class EndlessGame : Game, IEndlessGame
    {
        private readonly IGraphicsDeviceRepository _graphicsDeviceRepository;
        private readonly IControlSetRepository _controlSetRepository;
        private readonly IControlSetFactory _controlSetFactory;
        private readonly ITestModeLauncher _testModeLauncher;
        private readonly IPubFileLoadActions _pubFileLoadActions;
        private readonly ILoggerProvider _loggerProvider;
        private readonly IChatBubbleTextureProvider _chatBubbleTextureProvider;
        private readonly IShaderRepository _shaderRepository;
        private readonly IGraphicsDeviceManager _graphicsDeviceManager;

        private KeyboardState _previousKeyState;

        public EndlessGame(IClientWindowSizeProvider windowSizeProvider,
                           IGraphicsDeviceRepository graphicsDeviceRepository,
                           IControlSetRepository controlSetRepository,
                           IControlSetFactory controlSetFactory,
                           ITestModeLauncher testModeLauncher,
                           IPubFileLoadActions pubFileLoadActions,
                           ILoggerProvider loggerProvider,
                           IChatBubbleTextureProvider chatBubbleTextureProvider,
                           IShaderRepository shaderRepository)
        {
            _graphicsDeviceRepository = graphicsDeviceRepository;
            _controlSetRepository = controlSetRepository;
            _controlSetFactory = controlSetFactory;
            _testModeLauncher = testModeLauncher;
            _pubFileLoadActions = pubFileLoadActions;
            _loggerProvider = loggerProvider;
            _chatBubbleTextureProvider = chatBubbleTextureProvider;
            _shaderRepository = shaderRepository;
            _graphicsDeviceManager = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = windowSizeProvider.Width,
                PreferredBackBufferHeight = windowSizeProvider.Height
            };

            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            base.Initialize();

            AttemptToLoadPubFiles();

            IsMouseVisible = true;
            _previousKeyState = Keyboard.GetState();

            SetUpInitialControlSet();
        }

        protected override void LoadContent()
        {
            //todo: all the things that should load stuff as part of game's load/initialize should be broken into a pattern
            _chatBubbleTextureProvider.LoadContent();

            //the GraphicsDevice doesn't exist until Initialize() is called by the framework
            //Ideally, this would be set in a DependencyContainer, but I'm not sure of a way to do that now
            _graphicsDeviceRepository.GraphicsDevice = GraphicsDevice;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                _shaderRepository.Shaders[ShaderRepository.HairClip] = Content.Load<Effect>(ShaderRepository.HairClip);

            base.LoadContent();
        }

#if DEBUG

        protected override void Update(GameTime gameTime)
        {
            //todo: this is a debug-only mode launched with the F5 key.
            //todo: move this to be handled by some sort of key listener once function keys are handled in-game
            var currentKeyState = Keyboard.GetState();
            if (_previousKeyState.IsKeyDown(Keys.F5) && currentKeyState.IsKeyUp(Keys.F5))
            {
                _testModeLauncher.LaunchTestMode();
            }

            _previousKeyState = currentKeyState;

            base.Update(gameTime);
        }

#endif

        protected override void Draw(GameTime gameTime)
        {
            var isTestMode = _controlSetRepository.CurrentControlSet.GameState == GameStates.TestMode;
            GraphicsDevice.Clear(isTestMode ? Color.White : Color.Black);

            base.Draw(gameTime);
        }

        private void AttemptToLoadPubFiles()
        {
            const string PUB_LOG_MSG = "**** Unable to load default PUB file: {0}. Exception message: {1}";

            try
            {
                _pubFileLoadActions.LoadItemFile();
            }
            catch (IOException ioe)
            {
                _loggerProvider.Logger.Log(PUB_LOG_MSG, PubFileNameConstants.PathToEIFFile, ioe.Message);
            }

            try
            {
                _pubFileLoadActions.LoadNPCFile();
            }
            catch (IOException ioe)
            {
                _loggerProvider.Logger.Log(PUB_LOG_MSG, PubFileNameConstants.PathToENFFile, ioe.Message);
            }

            try
            {
                _pubFileLoadActions.LoadSpellFile();
            }
            catch (IOException ioe)
            {
                _loggerProvider.Logger.Log(PUB_LOG_MSG, PubFileNameConstants.PathToESFFile, ioe.Message);
            }

            try
            {
                _pubFileLoadActions.LoadClassFile();
            }
            catch (IOException ioe)
            {
                _loggerProvider.Logger.Log(PUB_LOG_MSG, PubFileNameConstants.PathToECFFile, ioe.Message);
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
