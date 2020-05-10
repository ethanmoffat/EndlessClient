using System;
using System.Linq;
using EndlessClient.HUD.Panels.Old;
using EndlessClient.Old;
using EOLib.Domain.Character;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace EndlessClient.Input
{
    public class FunctionKeyListener : OldInputKeyListenerBase
    {
        public FunctionKeyListener()
        {
            if (Game.Components.Any(x => x is FunctionKeyListener))
                throw new InvalidOperationException("The game already contains an arrow key listener");
            Game.Components.Add(this);
        }

        public override void Update(GameTime gameTime)
        {
            KeyboardState currState = Keyboard.GetState();
            if (!IgnoreInput && Character.State == CharacterActionState.Standing && !Character.PreparingSpell)
            {
                UpdateInputTime();

                //F1-F8 should be handled the same way: invoke the spell
                for (int key = (int) Keys.F1; key <= (int) Keys.F8; ++key)
                {
                    if (currState.IsKeyHeld(PreviousKeyState, (Keys)key))
                    {
                        //hidden feature! holding shift calls spell in second row (just learned that, crazy!!!!)
                        var shiftHeld = currState.IsKeyHeld(PreviousKeyState, Keys.LeftShift) ||
                                        currState.IsKeyHeld(PreviousKeyState, Keys.RightShift) ? OldActiveSpells.SPELL_ROW_LENGTH : 0;
                        _handleSpellFunc(key - (int) Keys.F1 + shiftHeld);
                        break;
                    }
                }
            }

            if (currState.IsKeyPressedOnce(PreviousKeyState, Keys.F12))
                _handleF12();

            base.Update(gameTime);
        }

        private void _handleSpellFunc(int spellIndex)
        {
            OldWorld.Instance.ActiveCharacterRenderer.SelectSpell(spellIndex);
            ((EOGame) Game).Hud.SetSelectedSpell(spellIndex);
        }

        private void _handleF12()
        {
            if (!((EOGame) Game).API.RequestRefresh())
                ((EOGame) Game).DoShowLostConnectionDialogAndReturnToMainMenu();
        }
    }
}
