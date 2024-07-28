using EndlessClient.Controllers;
using EndlessClient.GameExecution;
using EOLib.Domain.Map;
using Microsoft.Xna.Framework.Input;
using Optional;

namespace EndlessClient.Input;

public class ControlKeyHandler : InputHandlerBase
{
    private readonly IControlKeyController _controlKeyController;

    public ControlKeyHandler(IEndlessGameProvider endlessGameProvider,
                             IUserInputProvider userInputProvider,
                             IUserInputTimeRepository userInputTimeRepository,
                             IControlKeyController controlKeyController,
                             ICurrentMapStateRepository currentMapStateRepository)
        : base(endlessGameProvider, userInputProvider, userInputTimeRepository, currentMapStateRepository)
    {
        _controlKeyController = controlKeyController;
    }

    protected override Option<Keys> HandleInput()
    {
        if (IsKeyHeld(Keys.LeftControl) && _controlKeyController.Attack())
            return Option.Some(Keys.LeftControl);
        if (IsKeyHeld(Keys.RightControl) && _controlKeyController.Attack())
            return Option.Some(Keys.RightControl);

        return Option.None<Keys>();
    }
}