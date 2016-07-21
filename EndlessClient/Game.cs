// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using EndlessClient.Audio;
using EndlessClient.Dialogs;
using EndlessClient.GameExecution;
using EndlessClient.Rendering;
using EOLib;
using EOLib.Graphics;
using EOLib.IO;
using EOLib.IO.Old;
using EOLib.Localization;
using EOLib.Net.API;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNAControls;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace EndlessClient
{
    public partial class EOGame : Game
    {
        private static EOGame inst;
        /// <summary>
        /// Singleton Instance: used/disposed from main in Program.cs
        /// </summary>
        public static EOGame Instance
        {
            get { return inst ?? (inst = new EOGame()); }
        }

        public SpriteFont DBGFont { get; private set; }

        private const int WIDTH = 640;
        private const int HEIGHT = 480;

        public GameStates State { get; private set; }

        public PacketAPI API { get { return null; } }

        public INativeGraphicsManager GFXManager { get; private set; }

#if DEBUG //don't do FPS render on release builds
        private TimeSpan? lastFPSRender;
        private int localFPS;
#endif

        public void ShowLostConnectionDialog()
        {
            if (_backButtonPressed) return;
            EOMessageBox.Show(State == GameStates.PlayingTheGame
                ? DATCONST1.CONNECTION_LOST_IN_GAME
                : DATCONST1.CONNECTION_LOST_CONNECTION);    
        }

        public void ResetWorldElements()
        {
            OldWorld.Instance.ResetGameElements();
        }

        public void DisconnectFromGameServer()
        {
            if (OldWorld.Instance.Client.ConnectedAndInitialized)
                OldWorld.Instance.Client.Disconnect();
        }

        public void SetInitialGameState()
        {
            doStateChange(GameStates.Initial);
        }

        public void DoShowLostConnectionDialogAndReturnToMainMenu()
        {
            ShowLostConnectionDialog();
            ResetWorldElements();
            DisconnectFromGameServer();
            SetInitialGameState();
        }

        private void doStateChange(GameStates newState)
        {
            GameStates prevState = State;
            State = newState;

            if (prevState == GameStates.TestMode && newState == GameStates.Initial)
            {
                var testComponents = Components.OfType<CharacterStateTest>().Cast<GameComponent>().ToList();
                testComponents = testComponents.Concat(Components.OfType<CharacterRenderer>()).ToList();
                foreach (var component in testComponents)
                {
                    component.Dispose();
                    Components.Remove(component);
                }
            }

            if(prevState == GameStates.PlayingTheGame && State != GameStates.PlayingTheGame)
            {
                Components.OfType<IDisposable>()
                          .ToList()
                          .ForEach(x => x.Dispose());
                Components.Clear();

                foreach (var dlg in XNAControl.Dialogs)
                {
                    dlg.Visible = true;
                    Components.Add(dlg);
                }
            }
            
            List<DrawableGameComponent> toRemove = new List<DrawableGameComponent>();
            foreach (var component in Components.OfType<DrawableGameComponent>())
            {
                //don't hide dialogs
                if (component is XNAControl &&
                    (XNAControl.Dialogs.Contains(component as XNAControl) ||
                    XNAControl.Dialogs.Contains((component as XNAControl).TopParent)))
                    continue;

                if (prevState == GameStates.PlayingTheGame && State != GameStates.PlayingTheGame)
                {
                    toRemove.Add(component);
                }
                else
                {
                    if (component is OldCharacterRenderer)
                        toRemove.Add(component as OldCharacterRenderer); //this needs to be done separately because it's a foreach loop

                    if (component is XNATextBox)
                    {
                        (component as XNATextBox).Text = "";
                        (component as XNATextBox).Selected = false;
                    }
                    if (component != null) component.Visible = false;
                }
            }
            foreach (DrawableGameComponent comp in toRemove)
            {
                if (comp is XNAControl)
                    (comp as XNAControl).Close();
                else
                    comp.Dispose();
                if (Components.Contains(comp))
                    Components.Remove(comp);
            }
            toRemove.Clear();

            if (prevState == GameStates.PlayingTheGame && State != GameStates.PlayingTheGame)
                InitializeControls(true); //reinitialize to defaults

            switch (State)
            {
                case GameStates.PlayingTheGame:
                    FieldInfo[] fi = GetType().GetFields(BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic);
                    for (int i = Components.Count - 1; i >= 0; --i)
                    {
                        IGameComponent comp = Components[i];
                        if (comp != _backButton && (comp as DrawableGameComponent) != null && 
                            fi.Count(_fi => _fi.GetValue(this) == comp) == 1)
                        {
                            (comp as DrawableGameComponent).Dispose();
                            Components.Remove(comp);
                        }
                    }

                    _backButton.Visible = true;
                    //note: HUD construction moved to successful welcome message in GameLoadingDialog close event handler

                    break;
                case GameStates.TestMode:
                    var file = new ItemFile();
                    file.Load(PubFileNameConstants.PathToEIFFile);
                    var testComponent = new CharacterStateTest(this, file);
                    Components.Add(testComponent);
                    break;
            }
        }

        //-----------------------------
        //***** DEFAULT XNA STUFF *****
        //-----------------------------

        private EOGame()
        {
            _graphicsDeviceManager = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = WIDTH,
                PreferredBackBufferHeight = HEIGHT
            };
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            IsMouseVisible = true;

            if (!InitializeSoundManager())
                return;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            InitializeControls();
        }

#if DEBUG

        private KeyboardState _lastState = Keyboard.GetState();

        protected override void Update(GameTime gameTime)
        {
            if (!IsActive || (State != GameStates.Initial && State != GameStates.TestMode))
            {
                base.Update(gameTime);
                return;
            }

            var currentState = Keyboard.GetState();

            if (State == GameStates.Initial && _lastState.IsKeyDown(Keys.F5) && currentState.IsKeyUp(Keys.F5))
            {
                doStateChange(GameStates.TestMode);
            }
            else if (State == GameStates.TestMode && _lastState.IsKeyDown(Keys.Escape) && currentState.IsKeyUp(Keys.Escape))
            {
                doStateChange(GameStates.Initial);
            }

            _lastState = currentState;

            base.Update(gameTime);
        }

#endif

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(State == GameStates.TestMode ? Color.White : Color.Black);

#if DEBUG
            if (State != GameStates.TestMode)
            {
                if (lastFPSRender == null)
                    lastFPSRender = gameTime.TotalGameTime;

                localFPS++;
                if (gameTime.TotalGameTime.TotalMilliseconds - lastFPSRender.Value.TotalMilliseconds > 1000)
                {
                    OldWorld.FPS = localFPS;
                    localFPS = 0;
                    lastFPSRender = gameTime.TotalGameTime;
                }
            }
#endif
            base.Draw(gameTime);
        }

        private bool InitializeSoundManager()
        {
            try
            {
                SoundManager = new SoundManager();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format("There was an error (type: {2}) initializing the sound manager: {0}\n\nCall Stack:\n {1}", ex.Message,
                        ex.StackTrace, ex.GetType()), "Sound Manager Error");
                Exit();
                return false;
            }

            if (OldWorld.Instance.MusicEnabled)
                SoundManager.PlayBackgroundMusic(1); //mfx001 == main menu theme
            return true;
        }

        //-------------------
        //***** CLEANUP *****
        //-------------------

        protected override void Dispose(bool disposing)
        {
            if (!OldWorld.Initialized)
                return;

            if (OldWorld.Instance.Client.ConnectedAndInitialized)
                OldWorld.Instance.Client.Disconnect();

            if(_backButton != null)
                _backButton.Close();

            if(_spriteBatch != null)
                _spriteBatch.Dispose();
            ((IDisposable)_graphicsDeviceManager).Dispose();

            GFXManager.Dispose();

            Dispatcher.Dispose();

            OldWorld.Instance.Dispose();

            base.Dispose(disposing);
        }
    }
}
