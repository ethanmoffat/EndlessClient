﻿using System.Collections.Generic;
using System.Linq;
using EndlessClient.GameExecution;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using XNAControls;

namespace EndlessClient.ControlSets
{
    /// <summary>
    /// An empty control set that represents the initial state of the game with no controls
    /// </summary>
    public class EmptyControlSet : IControlSet
    {
        public GameStates GameState => GameStates.None;

        public IReadOnlyList<IGameComponent> AllComponents => Enumerable.Empty<IGameComponent>().ToList();

        public IReadOnlyList<IXNAControl> XNAControlComponents => AllComponents.OfType<IXNAControl>().ToList();

        public void InitializeResources(INativeGraphicsManager gfxManager, ContentManager xnaContentManager)
        {
        }

        public void InitializeControls(IControlSet currentControlSet)
        {
        }

        public IGameComponent FindComponentByControlIdentifier(GameControlIdentifier control)
        {
            return null;
        }

        public void Dispose()
        {
        }
    }
}
