using EndlessClient.Content;
using EndlessClient.GameExecution;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using XNAControls;

namespace EndlessClient.ControlSets;

public abstract class BaseControlSet : IControlSet
{
    #region IGameStateControlSet implementation

    protected readonly List<IGameComponent> _allComponents;

    public IReadOnlyList<IGameComponent> AllComponents => _allComponents;

    public IReadOnlyList<IXNAControl> XNAControlComponents => _allComponents.OfType<IXNAControl>().ToList();

    public abstract GameStates GameState { get; }

    #endregion

    protected Texture2D _mainButtonTexture;
    protected Texture2D _secondaryButtonTexture;
    protected Texture2D _smallButtonSheet;
    protected Texture2D _textBoxCursor;
    protected Texture2D _textBoxRight;
    protected Texture2D _textBoxLeft;
    protected Texture2D _textBoxBackground;

    private Texture2D[] _backgroundImages;
    private IXNAPictureBox _backgroundImage;

    private bool _resourcesInitialized, _controlsInitialized;

    protected BaseControlSet()
    {
        _allComponents = new List<IGameComponent>(16);
    }

    public virtual void InitializeResources(INativeGraphicsManager gfxManager,
                                            IContentProvider contentProvider)
    {
        if (_resourcesInitialized)
            throw new InvalidOperationException("Error initializing resources: resources have already been initialized");

        _mainButtonTexture = gfxManager.TextureFromResource(GFXTypes.PreLoginUI, 13, true);
        _secondaryButtonTexture = gfxManager.TextureFromResource(GFXTypes.PreLoginUI, 14, true);
        _smallButtonSheet = gfxManager.TextureFromResource(GFXTypes.PreLoginUI, 15, true);

        _textBoxBackground = contentProvider.Textures[ContentProvider.TBBack];
        _textBoxLeft = contentProvider.Textures[ContentProvider.TBLeft];
        _textBoxRight = contentProvider.Textures[ContentProvider.TBRight];
        _textBoxCursor = contentProvider.Textures[ContentProvider.Cursor];

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

        foreach (var control in XNAControlComponents)
            control.AddControlToDefaultGame();

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

    private IXNAPictureBox GetBackgroundImage()
    {
        var rnd = new Random();
        var texture = _backgroundImages[rnd.Next(7)];
        return new XNAPictureBox
        {
            Texture = texture,
            DrawArea = new Rectangle(0, 0, texture.Width, texture.Height),
            DrawOrder = 0
        };
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