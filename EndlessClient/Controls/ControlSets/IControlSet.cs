// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
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
		/// Find an existing component from this game control set
		/// </summary>
		/// <param name="control">The control specification</param>
		/// <returns>The matching control</returns>
		IGameComponent FindComponentByControlIdentifier(GameControlIdentifier control);
	}
}
