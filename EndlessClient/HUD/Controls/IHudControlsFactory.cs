// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace EndlessClient.HUD.Controls
{
	public interface IHudControlsFactory
	{
		IReadOnlyDictionary<HudControlIdentifier, IGameComponent> CreateHud();
	}
}
