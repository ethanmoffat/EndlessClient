// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using EndlessClient.Game;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using XNAControls;

namespace EndlessClient.Controls.ControlSets
{
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
		IReadOnlyList<XNAControl> XNAControlComponents { get; }

		/// <summary>
		/// Initialize the required resources for the control set from the resource dependencies. Should be called before InitializeControls()
		/// </summary>
		/// <param name="gfxManager">An initialized native graphics manager</param>
		/// <param name="xnaContentManager">The ContentManager for the game</param>
		void InitializeResources(INativeGraphicsManager gfxManager, ContentManager xnaContentManager);

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
}
