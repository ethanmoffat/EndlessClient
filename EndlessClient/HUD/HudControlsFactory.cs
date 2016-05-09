// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using EOLib.Graphics;
using Microsoft.Xna.Framework;

namespace EndlessClient.HUD
{
	public class HudControlsFactory : IHudControlsFactory
	{
		private readonly INativeGraphicsManager _nativeGraphicsManager;
		private readonly IGraphicsDeviceProvider _graphicsDeviceProvider;

		public HudControlsFactory(INativeGraphicsManager nativeGraphicsManager,
								  IGraphicsDeviceProvider graphicsDeviceProvider)
		{
			_nativeGraphicsManager = nativeGraphicsManager;
			_graphicsDeviceProvider = graphicsDeviceProvider;
		}

		public IList<IGameComponent> CreateHud()
		{
			var hudBackground = new HudBackgroundFrame(_nativeGraphicsManager, _graphicsDeviceProvider);

			return new List<IGameComponent>
			{
				hudBackground
			};
		}
	}
}