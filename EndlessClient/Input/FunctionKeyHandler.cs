using EndlessClient.Controllers;
using EndlessClient.GameExecution;
using EOLib;
using EOLib.Domain.Map;
using Microsoft.Xna.Framework.Input;
using Optional;

namespace EndlessClient.Input
{
    public class FunctionKeyHandler : InputHandlerBase
    {
        private readonly IFunctionKeyController _functionKeyController;

        public FunctionKeyHandler(IEndlessGameProvider endlessGameProvider,
                                  IUserInputProvider userInputProvider,
                                  IUserInputTimeRepository userInputTimeRepository,
                                  IFunctionKeyController functionKeyController,
                                  ICurrentMapStateProvider currentMapStateProvider)
            : base(endlessGameProvider, userInputProvider, userInputTimeRepository, currentMapStateProvider)
        {
            _functionKeyController = functionKeyController;
        }

        protected override Option<Keys> HandleInput()
        {
            if (IsKeyPressedOnce(Keys.F11) && _functionKeyController.Sit())
                return Option.Some(Keys.F11);

            if (IsKeyPressedOnce(Keys.F12) && _functionKeyController.RefreshMapState())
                return Option.Some(Keys.F12);

            // todo: spell selection
            // commented code from FunctionKeyListener
            //if (!IgnoreInput && Character.State == CharacterActionState.Standing && !Character.PreparingSpell)
            //{
            //    UpdateInputTime();

            //    //F1-F8 should be handled the same way: invoke the spell
            //    for (int key = (int)Keys.F1; key <= (int)Keys.F8; ++key)
            //    {
            //        if (currState.IsKeyHeld(PreviousKeyState, (Keys)key))
            //        {
            //            //hidden feature! holding shift calls spell in second row (just learned that, crazy!!!!)
            //            var shiftHeld = currState.IsKeyHeld(PreviousKeyState, Keys.LeftShift) ||
            //                            currState.IsKeyHeld(PreviousKeyState, Keys.RightShift) ? OldActiveSpells.SPELL_ROW_LENGTH : 0;
            //            _handleSpellFunc(key - (int)Keys.F1 + shiftHeld);
            //            break;
            //        }
            //    }
            //}

            return Option.None<Keys>();
        }

        //private void _handleSpellFunc(int spellIndex)
        //{
        //    OldWorld.Instance.ActiveCharacterRenderer.SelectSpell(spellIndex);
        //    ((EOGame)Game).Hud.SetSelectedSpell(spellIndex);
        //}
    }
}
