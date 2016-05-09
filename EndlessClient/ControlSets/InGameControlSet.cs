// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EndlessClient.Controllers;
using EndlessClient.Dialogs.Factories;
using EndlessClient.GameExecution;
using EndlessClient.HUD;
using EOLib;
using XNAControls;

namespace EndlessClient.ControlSets
{
	public class InGameControlSet : BackButtonControlSet
	{
		private readonly IEOMessageBoxFactory _messageBoxFactory;
		private readonly IHudControlsFactory _hudControlsFactory;

		public override GameStates GameState
		{
			get { return GameStates.PlayingTheGame; }
		}

		public InGameControlSet(IMainButtonController mainButtonController,
								IEOMessageBoxFactory messageBoxFactory,
								IHudControlsFactory hudControlsFactory)
			: base(mainButtonController)
		{
			_messageBoxFactory = messageBoxFactory;
			_hudControlsFactory = hudControlsFactory;
		}

		protected override void InitializeControlsHelper(IControlSet currentControlSet)
		{
			var controls = _hudControlsFactory.CreateHud();
			_allComponents.AddRange(controls);

			base.InitializeControlsHelper(currentControlSet);
		}

		protected override async void DoBackButtonClick(object sender, EventArgs e)
		{
			var messageBox = _messageBoxFactory.CreateMessageBox(
				DATCONST1.EXIT_GAME_ARE_YOU_SURE,
				XNADialogButtons.OkCancel);

			var result = await messageBox.Show();
			if (result == XNADialogResult.OK)
				_mainButtonController.GoToInitialStateAndDisconnect();
		}
	}
}
