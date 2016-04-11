// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using Microsoft.Xna.Framework;

namespace EndlessClient.GameExecution
{
	public class EndlessGame : Game, IEndlessGame
	{
		private readonly IGraphicsDeviceManager _graphicsDeviceManager;

		public EndlessGame(IClientWindowSizeProvider windowSizeProvider)
		{
			_graphicsDeviceManager = new GraphicsDeviceManager(this)
			{
				PreferredBackBufferWidth = windowSizeProvider.Width,
				PreferredBackBufferHeight = windowSizeProvider.Height
			};
			Content.RootDirectory = "Content";
		}


	}
}
