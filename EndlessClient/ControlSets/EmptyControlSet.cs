// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using System.Linq;
using EndlessClient.GameExecution;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using XNAControls.Old;

namespace EndlessClient.ControlSets
{
    /// <summary>
    /// An empty control set that represents the initial state of the game with no controls
    /// </summary>
    public class EmptyControlSet : IControlSet
    {
        public GameStates GameState { get { return GameStates.None; } }

        public IReadOnlyList<IGameComponent> AllComponents { get { return Enumerable.Empty<IGameComponent>().ToList(); } }

        public IReadOnlyList<XNAControl> XNAControlComponents { get { return AllComponents.OfType<XNAControl>().ToList(); } }

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
