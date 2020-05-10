using System;
using System.Collections.Generic;
using EndlessClient.Controllers;
using EndlessClient.GameExecution;
using EOLib.Domain.Map;
using Microsoft.Xna.Framework;

namespace EndlessClient.Input
{
    public class UserInputHandler : GameComponent, IUserInputHandler
    {
        private readonly List<IInputHandler> _handlers;

        public UserInputHandler(IEndlessGameProvider endlessGameProvider,
                                IKeyStateProvider keyStateProvider,
                                IUserInputTimeRepository userInputTimeRepository,
                                IArrowKeyController arrowKeyController,
                                IControlKeyController controlKeyController,
                                ICurrentMapStateProvider currentMapStateProvider)
            : base((Game)endlessGameProvider.Game)
        {
            _handlers = new List<IInputHandler>
            {
                new ArrowKeyHandler(endlessGameProvider,
                    keyStateProvider,
                    userInputTimeRepository,
                    arrowKeyController,
                    currentMapStateProvider),
                new ControlKeyHandler(endlessGameProvider,
                    keyStateProvider,
                    userInputTimeRepository,
                    controlKeyController,
                    currentMapStateProvider)
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
