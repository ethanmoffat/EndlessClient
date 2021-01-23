using System;
using System.Collections.Generic;
using EndlessClient.Controllers;
using EndlessClient.ControlSets;
using EndlessClient.GameExecution;
using EOLib.Domain.Map;
using Microsoft.Xna.Framework;

namespace EndlessClient.Input
{
    public class UserInputHandler : GameComponent, IUserInputHandler
    {
        private readonly List<IInputHandler> _handlers;

        public UserInputHandler(IEndlessGameProvider endlessGameProvider,
                                IUserInputProvider userInputProvider,
                                IUserInputTimeRepository userInputTimeRepository,
                                IArrowKeyController arrowKeyController,
                                IControlKeyController controlKeyController,
                                IFunctionKeyController functionKeyController,
                                ICurrentMapStateProvider currentMapStateProvider,
                                IHudControlProvider hudControlProvider)
            : base((Game)endlessGameProvider.Game)
        {
            _handlers = new List<IInputHandler>
            {
                new ArrowKeyHandler(endlessGameProvider,
                    userInputProvider,
                    userInputTimeRepository,
                    arrowKeyController,
                    currentMapStateProvider,
                    hudControlProvider),
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

        public override void Update(GameTime gameTime)
        {
            var timeAtBeginningOfUpdate = DateTime.Now;

            foreach (var handler in _handlers)
                handler.HandleKeyboardInput(timeAtBeginningOfUpdate);

            base.Update(gameTime);
        }
    }

    public interface IUserInputHandler : IGameComponent
    {
    }
}
