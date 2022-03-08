using EndlessClient.Controllers;
using EndlessClient.GameExecution;
using EOLib.Domain.Map;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using XNAControls;

namespace EndlessClient.Input
{
    public class UserInputHandler : XNAControl, IUserInputHandler
    {
        private readonly List<IInputHandler> _handlers;

        public UserInputHandler(IEndlessGameProvider endlessGameProvider,
                                IUserInputProvider userInputProvider,
                                IUserInputTimeRepository userInputTimeRepository,
                                IArrowKeyController arrowKeyController,
                                IControlKeyController controlKeyController,
                                IFunctionKeyController functionKeyController,
                                ICurrentMapStateProvider currentMapStateProvider)
        {
            _handlers = new List<IInputHandler>
            {
                new ArrowKeyHandler(endlessGameProvider,
                    userInputProvider,
                    userInputTimeRepository,
                    arrowKeyController,
                    currentMapStateProvider),
                new ControlKeyHandler(endlessGameProvider,
                    userInputProvider,
                    userInputTimeRepository,
                    controlKeyController,
                    currentMapStateProvider),
                new FunctionKeyHandler(endlessGameProvider,
                    userInputProvider,
                    userInputTimeRepository,
                    functionKeyController,
                    currentMapStateProvider),
            };
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
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
