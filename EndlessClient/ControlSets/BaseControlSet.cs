// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using EndlessClient.GameExecution;
using EndlessClient.UIControls;
using EOLib;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient.ControlSets
{
	public abstract class BaseControlSet : IControlSet
	{
		#region IGameStateControlSet implementation

		protected readonly List<IGameComponent> _allComponents;

		public IReadOnlyList<IGameComponent> AllComponents
		{
			get { return _allComponents; }
		}

		public IReadOnlyList<XNAControl> XNAControlComponents
		{
			get { return _allComponents.OfType<XNAControl>().ToList(); }
		}

		public abstract GameStates GameState { get; }

		public abstract IGameComponent FindComponentByControlIdentifier(GameControlIdentifier control);

		#endregion

		protected Texture2D _mainButtonTexture;
		private Texture2D _secondaryButtonTexture, _smallButtonSheet;
		private Texture2D[] _textBoxTextures;

		private Texture2D[] _backgroundImages;
		private IGameComponent _backgroundTexture;

		private bool _resourcesInitialized, _controlsInitialized;

		protected BaseControlSet()
		{
			_allComponents = new List<IGameComponent>(16);
		}

		public virtual void InitializeResources(INativeGraphicsManager gfxManager,
												ContentManager xnaContentManager)
		{
			if (_resourcesInitialized)
				throw new InvalidOperationException("Error initializing resources: resources have already been initialized");

			_mainButtonTexture = gfxManager.TextureFromResource(GFXTypes.PreLoginUI, 13, true);
			_secondaryButtonTexture = gfxManager.TextureFromResource(GFXTypes.PreLoginUI, 14, true);
			_smallButtonSheet = gfxManager.TextureFromResource(GFXTypes.PreLoginUI, 15, true);

			_textBoxTextures = new[]
			{
				xnaContentManager.Load<Texture2D>("tbBack"),
				xnaContentManager.Load<Texture2D>("tbLeft"),
				xnaContentManager.Load<Texture2D>("tbRight"),
				xnaContentManager.Load<Texture2D>("cursor")
			};

			_backgroundImages = new Texture2D[7];
			for (int i = 0; i < _backgroundImages.Length; ++i)
				_backgroundImages[i] = gfxManager.TextureFromResource(GFXTypes.PreLoginUI, 30 + i);

			_resourcesInitialized = true;
		}

		public void InitializeControls(IControlSet currentControlSet)
		{
			if (!_resourcesInitialized)
				throw new InvalidOperationException("Error initializing controls: resources have not yet been initialized");
			if (_controlsInitialized)
				throw new InvalidOperationException("Error initializing controls: controls have already been initialized");

			if (GameState != GameStates.PlayingTheGame)
			{
				_backgroundTexture = GetControl(currentControlSet, GameControlIdentifier.BackgroundImage, GetBackgroundImage);
				_allComponents.Add(_backgroundTexture);
			}

			InitializeControlsHelper(currentControlSet);

			_controlsInitialized = true;
		}

		protected abstract void InitializeControlsHelper(IControlSet currentControlSet);

		protected static IGameComponent GetControl(IControlSet currentControlSet,
												   GameControlIdentifier whichControl,
												   Func<IGameComponent> componentFactory)
		{
			return currentControlSet.FindComponentByControlIdentifier(whichControl) ?? componentFactory();
		}

		private PictureBox GetBackgroundImage()
		{
			var rnd = new Random();
			var texture = _backgroundImages[rnd.Next(7)];
			return new PictureBox(texture) { DrawOrder = 0 };
		}

		#region Create Account State

		protected XNATextBox GetCreateAccountNameTextBox()
		{
			var tb = AccountInputTextBoxCreationHelper(GameControlIdentifier.CreateAccountName);
			tb.MaxChars = 16;
			return tb;
		}

		protected XNATextBox GetCreateAccountPasswordTextBox()
		{
			var tb = AccountInputTextBoxCreationHelper(GameControlIdentifier.CreateAccountPassword);
			tb.PasswordBox = true;
			tb.MaxChars = 12;
			return tb;
		}

		protected XNATextBox GetCreateAccountConfirmTextBox()
		{
			var tb = AccountInputTextBoxCreationHelper(GameControlIdentifier.CreateAccountPasswordConfirm);
			tb.PasswordBox = true;
			tb.MaxChars = 12;
			return tb;
		}

		protected XNATextBox GetCreateAccountRealNameTextBox()
		{
			return AccountInputTextBoxCreationHelper(GameControlIdentifier.CreateAccountRealName);
		}

		protected XNATextBox GetCreateAccountLocationTextBox()
		{
			return AccountInputTextBoxCreationHelper(GameControlIdentifier.CreateAccountLocation);
		}

		protected XNATextBox GetCreateAccountEmailTextBox()
		{
			return AccountInputTextBoxCreationHelper(GameControlIdentifier.CreateAccountEmail);
		}

		private XNATextBox AccountInputTextBoxCreationHelper(GameControlIdentifier whichControl)
		{
			int i;
			switch (whichControl)
			{
				case GameControlIdentifier.CreateAccountName: i = 0; break;
				case GameControlIdentifier.CreateAccountPassword: i = 1; break;
				case GameControlIdentifier.CreateAccountPasswordConfirm: i = 2; break;
				case GameControlIdentifier.CreateAccountRealName: i = 3; break;
				case GameControlIdentifier.CreateAccountLocation: i = 4; break;
				case GameControlIdentifier.CreateAccountEmail: i = 5; break;
				default: throw new ArgumentException("Invalid control specified for helper", "whichControl");
			}

			//set the first  3 Y coord to start at 69  and move up by 51 each time
			//set the second 3 Y coord to start at 260 and move up by 51 each time
			var txtYCoord = (i < 3 ? 69 : 260) + i%3*51;
			var drawArea = new Rectangle(358, txtYCoord, 240, _textBoxTextures[0].Height);
			return new XNATextBox(drawArea, _textBoxTextures, Constants.FontSize08)
			{
				LeftPadding = 4,
				MaxChars = 35,
				DefaultText = " "
			};
		}

		protected XNAButton GetCreateButton(bool isCreateCharacterButton)
		{
			return new XNAButton(_secondaryButtonTexture,
								 new Vector2(isCreateCharacterButton ? 334 : 359, 417),
								 new Rectangle(0, 0, 120, 40),
								 new Rectangle(120, 0, 120, 40));
		}

		protected XNAButton GetCreateAccountCancelButton()
		{
			return new XNAButton(_secondaryButtonTexture,
								 new Vector2(481, 417),
								 new Rectangle(0, 40, 120, 40),
								 new Rectangle(120, 40, 120, 40));
		}

		#endregion

		#region Log In State

		protected XNATextBox GetLoginUserNameTextBox()
		{
			return new XNATextBox(new Rectangle(402, 322, 140, _textBoxTextures[0].Height), _textBoxTextures, Constants.FontSize08)
			{
				MaxChars = 16,
				DefaultText = "Username",
				LeftPadding = 4
			};
		}

		protected XNATextBox GetLoginPasswordTextBox()
		{
			return new XNATextBox(new Rectangle(402, 358, 140, _textBoxTextures[0].Height), _textBoxTextures, Constants.FontSize08)
			{
				MaxChars = 12,
				PasswordBox = true,
				LeftPadding = 4,
				DefaultText = "Password"
			};
		}

		protected XNAButton GetLoginAccountButton()
		{
			return new XNAButton(_smallButtonSheet, new Vector2(361, 389), new Rectangle(0, 0, 91, 29), new Rectangle(91, 0, 91, 29));
		}

		protected XNAButton GetLoginCancelButton()
		{
			return new XNAButton(_smallButtonSheet, new Vector2(453, 389), new Rectangle(0, 29, 91, 29), new Rectangle(91, 29, 91, 29));
		}

		#endregion

		#region View Credits State

		protected XNALabel GetCreditsLabel()
		{
			return new XNALabel(new Rectangle(300, 260, 1, 1), Constants.FontSize10) { Text = Constants.CreditsText };
		}

		#endregion

		public void Dispose()
		{
			Dispose(true);
		}

		~BaseControlSet()
		{
			Dispose(false);
		}

		protected virtual void Dispose(bool disposing) { }
	}
}
