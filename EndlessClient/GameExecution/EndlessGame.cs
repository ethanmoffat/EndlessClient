// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.ControlSets;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace EndlessClient.GameExecution
{
    public class EndlessGame : Game, IEndlessGame
    {
        private readonly IGraphicsDeviceRepository _graphicsDeviceRepository;
        private readonly IControlSetRepository _controlSetRepository;
        private readonly IControlSetFactory _controlSetFactory;
        private readonly ITestModeLauncher _testModeLauncher;
        private readonly IGraphicsDeviceManager _graphicsDeviceManager;

        private KeyboardState _previousKeyState;

        public EndlessGame(IClientWindowSizeProvider windowSizeProvider,
                           IGraphicsDeviceRepository graphicsDeviceRepository,
                           IControlSetRepository controlSetRepository,
                           IControlSetFactory controlSetFactory,
                           ITestModeLauncher testModeLauncher)
        {
            _graphicsDeviceRepository = graphicsDeviceRepository;
            _controlSetRepository = controlSetRepository;
            _controlSetFactory = controlSetFactory;
            _testModeLauncher = testModeLauncher;

            _graphicsDeviceManager = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = windowSizeProvider.Width,
                PreferredBackBufferHeight = windowSizeProvider.Height
            };

            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            IsMouseVisible = true;
            _previousKeyState = Keyboard.GetState();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            //the GraphicsDevice doesn't exist until Initialize() is called by the framework
            //Ideally, this would be set in a DependencyContainer, but I'm not sure of a way to do that now
            _graphicsDeviceRepository.GraphicsDevice = GraphicsDevice;

            var controls = _controlSetFactory.CreateControlsForState(
                GameStates.Initial,
                _controlSetRepository.CurrentControlSet);

            _controlSetRepository.CurrentControlSet = controls;

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
    }
}
