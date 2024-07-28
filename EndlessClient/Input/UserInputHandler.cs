﻿using EndlessClient.Controllers;
using EndlessClient.Dialogs;
using EndlessClient.GameExecution;
using EndlessClient.HUD;
using EndlessClient.Rendering;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using XNAControls;

namespace EndlessClient.Input
{
    public class UserInputHandler : XNAControl, IUserInputHandler
    {
        private readonly List<IInputHandler> _handlers;
        private readonly IActiveDialogProvider _activeDialogProvider;

        public UserInputHandler(IEndlessGameProvider endlessGameProvider,
                                IUserInputProvider userInputProvider,
                                IUserInputTimeRepository userInputTimeRepository,
                                IArrowKeyController arrowKeyController,
                                IControlKeyController controlKeyController,
                                IFunctionKeyController functionKeyController,
                                INumPadController numPadController,
                                IHudButtonController hudButtonController,
                                ICurrentMapStateRepository currentMapStateRepository,
                                IActiveDialogProvider activeDialogProvider,
                                IClientWindowSizeProvider clientWindowSizeProvider,
                                IPlayerInfoProvider playerInfoProvider)
        {
            _handlers = new List<IInputHandler>
            {
                new ArrowKeyHandler(endlessGameProvider,
                    userInputProvider,
                    userInputTimeRepository,
                    arrowKeyController,
                    currentMapStateRepository,
					playerInfoProvider),
                new ControlKeyHandler(endlessGameProvider,
                    userInputProvider,
                    userInputTimeRepository,
                    controlKeyController,
                    currentMapStateRepository,
					playerInfoProvider),
                new FunctionKeyHandler(endlessGameProvider,
                    userInputProvider,
                    userInputTimeRepository,
                    functionKeyController,
                    currentMapStateRepository,
					playerInfoProvider),
                new NumPadHandler(endlessGameProvider,
                    userInputProvider,
                    userInputTimeRepository,
                    currentMapStateRepository,
                    numPadController,
					playerInfoProvider),
            };

            if (clientWindowSizeProvider.Resizable)
            {
                _handlers.Add(new PanelShortcutHandler(endlessGameProvider, userInputProvider, userInputTimeRepository, currentMapStateRepository, hudButtonController, playerInfoProvider));
            }

            _activeDialogProvider = activeDialogProvider;
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            if (_activeDialogProvider.ActiveDialogs.Any(x => x.HasValue))
                return;

            var timeAtBeginningOfUpdate = DateTime.Now;

            foreach (var handler in _handlers)
                handler.HandleKeyboardInput(timeAtBeginningOfUpdate);

            base.OnUpdateControl(gameTime);
        }
    }

    public interface IUserInputHandler : IGameComponent
    {
    }
}
