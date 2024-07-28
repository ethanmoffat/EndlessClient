using EndlessClient.Controllers;
using EndlessClient.GameExecution;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using Microsoft.Xna.Framework.Input;
using Optional;

namespace EndlessClient.Input;


public class NumPadHandler : InputHandlerBase
{
    private readonly INumPadController _numPadController;

    public NumPadHandler(IEndlessGameProvider endlessGameProvider,
                         IUserInputProvider userInputProvider,
                         IUserInputTimeRepository userInputTimeRepository,
                         ICurrentMapStateRepository currentMapStateRepository,
                         INumPadController numPadController)
        : base(endlessGameProvider, userInputProvider, userInputTimeRepository, currentMapStateRepository)
    {
        _numPadController = numPadController;
    }

    protected override Option<Keys> HandleInput()
    {
        for (var key = Keys.NumPad0; key <= Keys.NumPad9; ++key)
        {
            if (IsKeyHeld(key))
            {
                var emote = key == Keys.NumPad0 ? Emote.Playful : (Emote)(key - Keys.NumPad0);
                _numPadController.Emote(emote);
                return Option.Some(key);
            }
        }

        // Keys.Decimal == ./DEL on the num pad
        if (IsKeyHeld(Keys.Decimal))
        {
            _numPadController.Emote(Emote.Embarassed);
            return Option.Some(Keys.Decimal);
        }

        return Option.None<Keys>();
    }
}