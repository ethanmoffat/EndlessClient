using EndlessClient.Controllers;
using EndlessClient.GameExecution;
using EOLib;
using EOLib.Config;
using Microsoft.Xna.Framework;
using XNAControls;

namespace EndlessClient.ControlSets
{
    public class ViewCreditsControlSet : InitialControlSet
    {
        private IXNALabel _creditsLabel;

        public override GameStates GameState => GameStates.ViewCredits;

        public ViewCreditsControlSet(IConfigurationProvider configProvider,
                                     IMainButtonController mainButtonController)
            : base(configProvider, mainButtonController) { }

        protected override void InitializeControlsHelper(IControlSet currentControlSet)
        {
            base.InitializeControlsHelper(currentControlSet);
            ExcludePersonPicture1();

            _creditsLabel = GetControl(currentControlSet, GameControlIdentifier.CreditsLabel, GetCreditsLabel);
            _allComponents.Add(_creditsLabel);
        }

        public override IGameComponent FindComponentByControlIdentifier(GameControlIdentifier control)
        {
            switch (control)
            {
                case GameControlIdentifier.CreditsLabel: return _creditsLabel;
                default: return base.FindComponentByControlIdentifier(control);
            }
        }

        private XNALabel GetCreditsLabel()
        {
            return new XNALabel(Constants.FontSize10)
            {
                AutoSize = true,
                Text = Constants.CreditsText,
                DrawPosition = new Vector2(300, 200)
            };
        }
    }
}
