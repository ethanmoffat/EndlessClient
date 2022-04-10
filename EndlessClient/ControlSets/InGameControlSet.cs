using System;
using System.Collections.Generic;
using System.Linq;
using EndlessClient.Controllers;
using EndlessClient.Dialogs;
using EndlessClient.Dialogs.Factories;
using EndlessClient.GameExecution;
using EndlessClient.HUD.Controls;
using EndlessClient.Input;
using EOLib.Localization;
using Microsoft.Xna.Framework;
using Optional;
using XNAControls;

namespace EndlessClient.ControlSets
{
    public class InGameControlSet : BackButtonControlSet
    {
        private readonly IEOMessageBoxFactory _messageBoxFactory;
        private readonly IHudControlsFactory _hudControlsFactory;
        private readonly IActiveDialogRepository _activeDialogRepository;
        private readonly IUserInputRepository _userInputRepository;
        private IReadOnlyDictionary<HudControlIdentifier, IGameComponent> _controls;

        public override GameStates GameState => GameStates.PlayingTheGame;

        public InGameControlSet(IMainButtonController mainButtonController,
                                IEOMessageBoxFactory messageBoxFactory,
                                IHudControlsFactory hudControlsFactory,
                                IActiveDialogRepository activeDialogRepository,
                                IUserInputRepository userInputRepository)
            : base(mainButtonController)
        {
            _messageBoxFactory = messageBoxFactory;
            _hudControlsFactory = hudControlsFactory;
            _activeDialogRepository = activeDialogRepository;
            _userInputRepository = userInputRepository;
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
            _userInputRepository.ClickHandled = true;

            var messageBox = _messageBoxFactory.CreateMessageBox(
                DialogResourceID.EXIT_GAME_ARE_YOU_SURE,
                EODialogButtons.OkCancel);

            var result = await messageBox.ShowDialogAsync();
            if (result == XNADialogResult.OK)
                _mainButtonController.GoToInitialStateAndDisconnect();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _activeDialogRepository.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
