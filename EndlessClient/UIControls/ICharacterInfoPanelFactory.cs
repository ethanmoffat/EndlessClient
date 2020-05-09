using System.Collections.Generic;
using EndlessClient.Controllers;

namespace EndlessClient.UIControls
{
    public interface ICharacterInfoPanelFactory
    {
        IEnumerable<CharacterInfoPanel> CreatePanels();
        void InjectLoginController(ILoginController loginController);
        void InjectCharacterManagementController(ICharacterManagementController characterManagementController);
    }
}
