// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Game;
using Microsoft.Xna.Framework;

namespace EndlessClient.Controls.ControlSets
{
	public class ViewCreditsControlSet : InitialControlSet
	{
		private IGameComponent _creditsLabel;

		public override GameStates GameState { get { return GameStates.ViewCredits; } }

		protected override void InitializeControlsHelper(IControlSet currentControlSet)
		{
			base.InitializeControlsHelper(currentControlSet);

			_creditsLabel = GetControl(currentControlSet, GameControlIdentifier.CreditsLabel, GetCreditsLabel);
			_allComponents.Add(_creditsLabel);
		}

		public override IGameComponent FindComponentByControlIdentifier(GameControlIdentifier control)
		{
			switch (control)
			{
				case GameControlIdentifier.CreditsLabel: return _creditsLabel;
				default: return base.FindComponentByControlIdentifier(control);
			}
		}
	}
}
