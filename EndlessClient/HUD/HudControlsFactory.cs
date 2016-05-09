// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using EndlessClient.GameExecution;
using EndlessClient.UIControls;
using EOLib.Domain.Character;
using EOLib.Graphics;
using Microsoft.Xna.Framework;

namespace EndlessClient.HUD
{
	public class HudControlsFactory : IHudControlsFactory
	{
		private readonly INativeGraphicsManager _nativeGraphicsManager;
		private readonly IGraphicsDeviceProvider _graphicsDeviceProvider;
		private readonly IClientWindowSizeProvider _clientWindowSizeProvider;
		private readonly IEndlessGameProvider _endlessGameProvider;
		private readonly ICharacterRepository _characterRepository;

		public HudControlsFactory(INativeGraphicsManager nativeGraphicsManager,
								  IGraphicsDeviceProvider graphicsDeviceProvider,
								  IClientWindowSizeProvider clientWindowSizeProvider,
								  IEndlessGameProvider endlessGameProvider,
								  ICharacterRepository characterRepository)
		{
			_nativeGraphicsManager = nativeGraphicsManager;
			_graphicsDeviceProvider = graphicsDeviceProvider;
			_clientWindowSizeProvider = clientWindowSizeProvider;
			_endlessGameProvider = endlessGameProvider;
			_characterRepository = characterRepository;
		}

		public IList<IGameComponent> CreateHud()
		{
			//todo: draw order for controls

			var hudBackground = new HudBackgroundFrame(_nativeGraphicsManager, _graphicsDeviceProvider);

			var clockLabel = new TimeLabel(_clientWindowSizeProvider);
			var usageTracker = new UsageTrackerComponent(_endlessGameProvider, _characterRepository);

			return new List<IGameComponent>
			{
				hudBackground,

				//time keeping
				clockLabel,
				usageTracker
			};
		}
	}
}