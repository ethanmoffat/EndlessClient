using EndlessClient.Controllers;
using EndlessClient.GameExecution;
using EOLib;
using EOLib.Domain.Map;
using Microsoft.Xna.Framework.Input;

namespace EndlessClient.Input
{
    public class FunctionKeyHandler : InputHandlerBase
    {
        private readonly IFunctionKeyController _functionKeyController;

        public FunctionKeyHandler(IEndlessGameProvider endlessGameProvider,
                                  IKeyStateProvider keyStateProvider,
                                  IUserInputTimeRepository userInputTimeRepository,
                                  IFunctionKeyController functionKeyController,
                                  ICurrentMapStateProvider currentMapStateProvider)
            : base(endlessGameProvider, keyStateProvider, userInputTimeRepository, currentMapStateProvider)
        {
            _functionKeyController = functionKeyController;
        }

        protected override Optional<Keys> HandleInput()
        {
            if (IsKeyHeld(Keys.F12) && _functionKeyController.RefreshMapState())
                return Keys.F12;

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

            return Optional<Keys>.Empty;
        }

        //private void _handleSpellFunc(int spellIndex)
        //{
        //    OldWorld.Instance.ActiveCharacterRenderer.SelectSpell(spellIndex);
        //    ((EOGame)Game).Hud.SetSelectedSpell(spellIndex);
        //}
    }
}
