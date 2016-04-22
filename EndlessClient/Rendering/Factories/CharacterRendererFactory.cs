// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.GameExecution;
using EOLib.Data.BLL;
using EOLib.Graphics;
using EOLib.IO.Repositories;
using Microsoft.Xna.Framework;

namespace EndlessClient.Rendering.Factories
{
	public class CharacterRendererFactory : ICharacterRendererFactory
	{
		private readonly IEndlessGame _game;
		private readonly INativeGraphicsManager _nativeGraphicsManager;
		private readonly IItemFileProvider _itemFileProvider;

		public CharacterRendererFactory(IEndlessGame game,
										INativeGraphicsManager nativeGraphicsManager,
										IItemFileProvider itemFileProvider)
		{
			_game = game;
			_nativeGraphicsManager = nativeGraphicsManager;
			_itemFileProvider = itemFileProvider;
		}

		public ICharacterRenderer CreateCharacterRenderer(ICharacterRenderProperties initialRenderProperties)
		{
			return new CharacterRenderer((Game)_game,
				_nativeGraphicsManager,
				_itemFileProvider,
				initialRenderProperties);
		}
	}
}