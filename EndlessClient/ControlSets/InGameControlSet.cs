// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using EndlessClient.Controllers;
using EndlessClient.Dialogs.Factories;
using EndlessClient.GameExecution;
using EndlessClient.HUD.Controls;
using EOLib;
using EOLib.Localization;
using Microsoft.Xna.Framework;
using XNAControls;

namespace EndlessClient.ControlSets
{
    public class InGameControlSet : BackButtonControlSet
    {
        private readonly IEOMessageBoxFactory _messageBoxFactory;
        private readonly IHudControlsFactory _hudControlsFactory;

        private IReadOnlyDictionary<HudControlIdentifier, IGameComponent> _controls;

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
            _controls = new Dictionary<HudControlIdentifier, IGameComponent>();
        }

        public T GetHudComponent<T>(HudControlIdentifier whichControl)
            where T : IGameComponent
        {
            return (T)_controls[whichControl];
        }

        protected override void InitializeControlsHelper(IControlSet currentControlSet)
        {
            _controls = _hudControlsFactory.CreateHud();
            _allComponents.AddRange(_controls.Select(x => x.Value));

            base.InitializeControlsHelper(currentControlSet);
        }

        protected override async void DoBackButtonClick(object sender, EventArgs e)
        {
            var messageBox = _messageBoxFactory.CreateMessageBox(
                DialogResourceID.EXIT_GAME_ARE_YOU_SURE,
                XNADialogButtons.OkCancel);

            var result = await messageBox.Show();
            if (result == XNADialogResult.OK)
                _mainButtonController.GoToInitialStateAndDisconnect();
        }
    }
}
