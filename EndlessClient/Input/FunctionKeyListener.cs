// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Linq;
using EndlessClient.HUD;
using EndlessClient.HUD.Panels;
using EndlessClient.HUD.Panels.Old;
using EOLib.Domain.Character;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace EndlessClient.Input
{
    public class FunctionKeyListener : InputKeyListenerBase
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
                    if (IsKeyPressed((Keys) key, currState))
                    {
                        //hidden feature! holding shift calls spell in second row (just learned that, crazy!!!!)
                        var shiftHeld = IsKeyPressed(Keys.LeftShift) || IsKeyPressed(Keys.RightShift) ? OldActiveSpells.SPELL_ROW_LENGTH : 0;
                        _handleSpellFunc(key - (int) Keys.F1 + shiftHeld);
                        break;
                    }
                }
            }

            if (IsKeyPressedOnce(Keys.F12, currState))
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
