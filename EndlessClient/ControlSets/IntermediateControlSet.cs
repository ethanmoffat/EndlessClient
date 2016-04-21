// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EndlessClient.Controllers;
using EndlessClient.GameExecution;
using EndlessClient.UIControls;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient.ControlSets
{
	public abstract class IntermediateControlSet : BaseControlSet
	{
		protected readonly KeyboardDispatcher _dispatcher;
		protected readonly IMainButtonController _mainButtonController;
		private readonly Texture2D[] _personSet2;
		private readonly Random _randomGen;

		private Texture2D _backButtonTexture;

		private XNAButton _btnCreate,
						  _backButton;

		private PictureBox _person2Picture;

		protected IntermediateControlSet(KeyboardDispatcher dispatcher,
										 IMainButtonController mainButtonController)
		{
			_dispatcher = dispatcher;
			_mainButtonController = mainButtonController;
			_personSet2 = new Texture2D[8];
			_randomGen = new Random();
		}

		public override void InitializeResources(INativeGraphicsManager gfxManager, ContentManager xnaContentManager)
		{
			base.InitializeResources(gfxManager, xnaContentManager);

			for (int i = 0; i < _personSet2.Length; ++i)
				_personSet2[i] = gfxManager.TextureFromResource(GFXTypes.PreLoginUI, 61 + i, true);

			_backButtonTexture = gfxManager.TextureFromResource(GFXTypes.PreLoginUI, 24, true);
		}

		protected override void InitializeControlsHelper(IControlSet currentControlSet)
		{
			_btnCreate = GetControl(currentControlSet, GameControlIdentifier.CreateAccountButton, GetCreateButton);
			_backButton = GetControl(currentControlSet, GameControlIdentifier.BackButton, GetBackButton);
			_person2Picture = GetControl(currentControlSet, GameControlIdentifier.PersonDisplay2, GetPerson2Picture);

			_allComponents.Add(_btnCreate);
			_allComponents.Add(_backButton);
			_allComponents.Add(_person2Picture);
		}

		public override IGameComponent FindComponentByControlIdentifier(GameControlIdentifier control)
		{
			switch (control)
			{
				case GameControlIdentifier.CreateAccountButton: return _btnCreate;
				case GameControlIdentifier.BackButton: return _backButton;
				case GameControlIdentifier.PersonDisplay2: return _person2Picture;
				default: return base.FindComponentByControlIdentifier(control);
			}
		}

		protected virtual XNAButton GetCreateButton()
		{
			var isCreateCharacterButton = GameState == GameStates.LoggedIn;
			var button = new XNAButton(_secondaryButtonTexture,
									   new Vector2(isCreateCharacterButton ? 334 : 359, 417),
									   new Rectangle(0, 0, 120, 40),
									   new Rectangle(120, 0, 120, 40));
			return button;
		}

		private XNAButton GetBackButton()
		{
			var button = new XNAButton(
				_backButtonTexture,
				new Vector2(589, 0),
				new Rectangle(0, 0, _backButtonTexture.Width, _backButtonTexture.Height / 2),
				new Rectangle(0, _backButtonTexture.Height / 2, _backButtonTexture.Width, _backButtonTexture.Height / 2))
			{
				DrawOrder = 100,
				ClickArea = new Rectangle(4, 16, 16, 16)
			};
			button.OnClick += (o, e) => _mainButtonController.GoToInitialState();

			return button;
		}

		private PictureBox GetPerson2Picture()
		{
			var texture = _personSet2[_randomGen.Next(8)];
			return new PictureBox(texture) { DrawLocation = new Vector2(43, 140) };
		}
	}
}