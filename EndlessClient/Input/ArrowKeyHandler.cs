using EndlessClient.Controllers;
using EndlessClient.GameExecution;
using EOLib.Domain.Map;
using Microsoft.Xna.Framework.Input;
using Optional;

namespace EndlessClient.Input;

public class ArrowKeyHandler : InputHandlerBase
{
    private readonly IArrowKeyController _arrowKeyController;

    public ArrowKeyHandler(IEndlessGameProvider endlessGameProvider,
                           IUserInputProvider userInputProvider,
                           IUserInputTimeRepository userInputTimeRepository,
                           IArrowKeyController arrowKeyController,
                           ICurrentMapStateRepository currentMapStateRepository)
        : base(endlessGameProvider, userInputProvider, userInputTimeRepository, currentMapStateRepository)
    {
        _arrowKeyController = arrowKeyController;
    }

    protected override Option<Keys> HandleInput()
    {
        if (IsKeyHeld(Keys.Left) && _arrowKeyController.MoveLeft())
            return Option.Some(Keys.Left);
        if (IsKeyHeld(Keys.Right) && _arrowKeyController.MoveRight())
            return Option.Some(Keys.Right);
        if (IsKeyHeld(Keys.Up) && _arrowKeyController.MoveUp())
            return Option.Some(Keys.Up);
        if (IsKeyHeld(Keys.Down) && _arrowKeyController.MoveDown())
            return Option.Some(Keys.Down);

        if (KeysAreUp(Keys.Left, Keys.Right, Keys.Up, Keys.Down))
            _arrowKeyController.KeysUp();

        return Option.None<Keys>();
    }
}