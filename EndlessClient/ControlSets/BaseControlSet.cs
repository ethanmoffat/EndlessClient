// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using EndlessClient.GameExecution;
using EndlessClient.UIControls;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient.ControlSets
{
    public abstract class BaseControlSet : IControlSet
    {
        #region IGameStateControlSet implementation

        protected readonly List<IGameComponent> _allComponents;

        public IReadOnlyList<IGameComponent> AllComponents
        {
            get { return _allComponents; }
        }

        public IReadOnlyList<XNAControl> XNAControlComponents
        {
            get { return _allComponents.OfType<XNAControl>().ToList(); }
        }

        public abstract GameStates GameState { get; }

        #endregion

        protected Texture2D _mainButtonTexture;
        protected Texture2D _secondaryButtonTexture;
        protected Texture2D _smallButtonSheet;
        protected Texture2D[] _textBoxTextures;

        private Texture2D[] _backgroundImages;
        private PictureBox _backgroundImage;

        private bool _resourcesInitialized, _controlsInitialized;

        protected BaseControlSet()
        {
            _allComponents = new List<IGameComponent>(16);
        }

        public virtual void InitializeResources(INativeGraphicsManager gfxManager,
                                                ContentManager xnaContentManager)
        {
            if (_resourcesInitialized)
                throw new InvalidOperationException("Error initializing resources: resources have already been initialized");

            _mainButtonTexture = gfxManager.TextureFromResource(GFXTypes.PreLoginUI, 13, true);
            _secondaryButtonTexture = gfxManager.TextureFromResource(GFXTypes.PreLoginUI, 14, true);
            _smallButtonSheet = gfxManager.TextureFromResource(GFXTypes.PreLoginUI, 15, true);

            _textBoxTextures = new[]
            {
                xnaContentManager.Load<Texture2D>("tbBack"),
                xnaContentManager.Load<Texture2D>("tbLeft"),
                xnaContentManager.Load<Texture2D>("tbRight"),
                xnaContentManager.Load<Texture2D>("cursor")
            };

            _backgroundImages = new Texture2D[7];
            for (int i = 0; i < _backgroundImages.Length; ++i)
                _backgroundImages[i] = gfxManager.TextureFromResource(GFXTypes.PreLoginUI, 30 + i);

            _resourcesInitialized = true;
        }

        public void InitializeControls(IControlSet currentControlSet)
        {
            if (!_resourcesInitialized)
                throw new InvalidOperationException("Error initializing controls: resources have not yet been initialized");
            if (_controlsInitialized)
                throw new InvalidOperationException("Error initializing controls: controls have already been initialized");

            if (GameState != GameStates.PlayingTheGame)
            {
                _backgroundImage = GetControl(currentControlSet, GameControlIdentifier.BackgroundImage, GetBackgroundImage);
                _allComponents.Add(_backgroundImage);
            }

            InitializeControlsHelper(currentControlSet);

            _controlsInitialized = true;
        }

        protected abstract void InitializeControlsHelper(IControlSet currentControlSet);

        protected static T GetControl<T>(IControlSet currentControlSet,
                                         GameControlIdentifier whichControl,
                                         Func<T> componentFactory)
            where T : class, IGameComponent
        {
            return (T)currentControlSet.FindComponentByControlIdentifier(whichControl) ?? componentFactory();
        }

        public virtual IGameComponent FindComponentByControlIdentifier(GameControlIdentifier control)
        {
            return control == GameControlIdentifier.BackgroundImage ? _backgroundImage : null;
        }

        private PictureBox GetBackgroundImage()
        {
            var rnd = new Random();
            var texture = _backgroundImages[rnd.Next(7)];
            return new PictureBox(texture) { DrawOrder = 0 };
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~BaseControlSet()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing) { }
    }
}
