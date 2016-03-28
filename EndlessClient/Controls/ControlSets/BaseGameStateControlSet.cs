// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using EOLib;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient.Controls.ControlSets
{
	public abstract class BaseGameStateControlSet
	{
		protected readonly List<IGameComponent> _allComponents;

		public IReadOnlyList<IGameComponent> AllComponents
		{
			get { return _allComponents; }
		}

		public IReadOnlyList<XNAControl> XNAControlComponents
		{
			get { return _allComponents.OfType<XNAControl>().ToList(); }
		}

		private readonly Texture2D _mainButtonTexture;

		protected BaseGameStateControlSet(INativeGraphicsManager gfxManager)
		{
			_allComponents = new List<IGameComponent>(16);

			_mainButtonTexture = gfxManager.TextureFromResource(GFXTypes.PreLoginUI, 13, true);
		}

		public abstract void CreateControls(IGameStateControlSet currentControlSet);

		protected static IGameComponent GetControl(IGameStateControlSet _currentControlSet,
												   GameControlIdentifier whichControl,
												   Func<IGameComponent> componentFactory)
		{
			return _currentControlSet.FindComponentByControlIdentifier(whichControl) ?? componentFactory();
		}

		#region Initial State

		protected XNAButton GetCreateAccountButton()
		{
			return MainButtonCreationHelper(GameControlIdentifier.InitialCreateAccount);
		}

		protected XNAButton GetLoginButton()
		{
			return MainButtonCreationHelper(GameControlIdentifier.InitialLogin);
		}

		protected XNAButton GetViewCreditsButton()
		{
			return MainButtonCreationHelper(GameControlIdentifier.InitialViewCredits);
		}

		protected XNAButton GetExitButton()
		{
			return MainButtonCreationHelper(GameControlIdentifier.InitialExitGame);
		}

		private XNAButton MainButtonCreationHelper(GameControlIdentifier whichControl)
		{
			int i;
			switch (whichControl)
			{
				case GameControlIdentifier.InitialCreateAccount: i = 0; break;
				case GameControlIdentifier.InitialLogin: i = 1; break;
				case GameControlIdentifier.InitialViewCredits: i = 2; break;
				case GameControlIdentifier.InitialExitGame: i = 3; break;
				default: throw new ArgumentException("Invalid control specified for helper", "whichControl");
			}

			var widthFactor = _mainButtonTexture.Width / 2;
			var heightFactor = _mainButtonTexture.Height / 4;
			var outSource = new Rectangle(0, i * heightFactor, widthFactor, heightFactor);
			var overSource = new Rectangle(widthFactor, i * heightFactor, widthFactor, heightFactor);

			return new XNAButton(_mainButtonTexture, new Vector2(26, 278 + i * 40), outSource, overSource);
		}

		protected XNALabel GetVersionInfoLabel()
		{
			return new XNALabel(new Rectangle(25, 453, 1, 1), Constants.FontSize07)
			{
				Text = " ",
				ForeColor = Constants.BeigeText
			};
		}

		#endregion
	}
}
