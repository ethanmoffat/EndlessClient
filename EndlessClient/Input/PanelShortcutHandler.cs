using System.Linq;
using EndlessClient.GameExecution;
using EndlessClient.HUD;
using EOLib.Domain.Map;
using Microsoft.Xna.Framework.Input;
using Optional;

namespace EndlessClient.Input
{
    public class PanelShortcutHandler : InputHandlerBase
    {
        private readonly IHudButtonController _hudButtonController;

        public PanelShortcutHandler(IEndlessGameProvider endlessGameProvider,
                                    IUserInputProvider userInputProvider,
                                    IUserInputTimeRepository userInputTimeRepository,
                                    ICurrentMapStateRepository currentMapStateRepository,
                                    IHudButtonController hudButtonController)
            : base(endlessGameProvider, userInputProvider, userInputTimeRepository, currentMapStateRepository)
        {
            _hudButtonController = hudButtonController;
        }

        protected override Option<Keys> HandleInput()
        {
            if (!IsKeyHeld(Keys.LeftAlt) && !IsKeyHeld(Keys.RightAlt))
                return Option.None<Keys>();

            var keys = Enumerable.Range((int)Keys.D0, 10).Select(i => (Keys)i).Concat(new[] { Keys.OemTilde });

            foreach (var key in keys)
            {
                if (IsKeyPressedOnce(key))
                {
                    switch (key)
                    {
                        case Keys.OemTilde: _hudButtonController.ShowNews(); break;

                        case Keys.D1: _hudButtonController.ClickInventory(); break;
                        case Keys.D2: _hudButtonController.ClickViewMapToggle(); break;
                        case Keys.D3: _hudButtonController.ClickActiveSpells(); break;
                        case Keys.D4: _hudButtonController.ClickPassiveSpells(); break;
                        case Keys.D5: _hudButtonController.ClickChat(); break;
                        case Keys.D6: _hudButtonController.ClickStats(); break;

                        case Keys.D7: _hudButtonController.ClickOnlineList(); break;
                        case Keys.D8: _hudButtonController.ClickParty(); break;
                        // macro: intentionally not implemented
                        case Keys.D9: _hudButtonController.ClickSettings(); break;
                        case Keys.D0: _hudButtonController.ClickHelp(); break;
                    }

                    return Option.Some(key);
                }
            }

            return Option.None<Keys>();
        }
    }
}
