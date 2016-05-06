// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EndlessClient.Controllers;
using EndlessClient.Dialogs.Factories;
using EndlessClient.GameExecution;
using EOLib;
using XNAControls;

namespace EndlessClient.ControlSets
{
	public class InGameControlSet : BackButtonControlSet
	{
		private readonly IEOMessageBoxFactory _messageBoxFactory;

		public override GameStates GameState
		{
			get { return GameStates.PlayingTheGame; }
		}

		public InGameControlSet(IMainButtonController mainButtonController,
								IEOMessageBoxFactory messageBoxFactory)
			: base(mainButtonController)
		{
			_messageBoxFactory = messageBoxFactory;
		}

		//todo: redesign HUD
		//		need to decide if it will still be the same monolithic object or if the HUD will be built of components
		//		probably should build it out of components

		protected override async void DoBackButtonClick(object sender, EventArgs e)
		{
			var messageBox = _messageBoxFactory.CreateMessageBox(
				DATCONST1.EXIT_GAME_ARE_YOU_SURE,
				XNADialogButtons.OkCancel);

			var result = await messageBox.Show();
			if (result == XNADialogResult.OK)
				base.DoBackButtonClick(sender, e);
		}
	}
}
