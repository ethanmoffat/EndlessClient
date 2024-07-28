using EndlessClient.Content;
using EndlessClient.GameExecution;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using XNAControls;

namespace EndlessClient.ControlSets;

public interface IControlSet : IDisposable
{
    /// <summary>
    /// The game state that this control set represents
    /// </summary>
    GameStates GameState { get; }

    /// <summary>
    /// All components in this control set
    /// </summary>
    IReadOnlyList<IGameComponent> AllComponents { get; }

    /// <summary>
    /// Components in this control set that are XNAControls
    /// </summary>
    IReadOnlyList<IXNAControl> XNAControlComponents { get; }

    /// <summary>
    /// Initialize the required resources for the control set from the resource dependencies. Should be called before InitializeControls()
    /// </summary>
    /// <param name="gfxManager">An initialized native graphics manager</param>
    /// <param name="contentProvider">The ContentProvider for the game</param>
    void InitializeResources(INativeGraphicsManager gfxManager, IContentProvider contentProvider);

    /// <summary>
    /// Create the controls for this IControlSet based on an existing set of controls
    /// </summary>
    /// <param name="currentControlSet">The current active set of controls</param>
    void InitializeControls(IControlSet currentControlSet);

    /// <summary>
    /// Find an existing component from this game control set
    /// </summary>
    /// <param name="control">The control specification</param>
    /// <returns>The matching control</returns>
    IGameComponent FindComponentByControlIdentifier(GameControlIdentifier control);
}