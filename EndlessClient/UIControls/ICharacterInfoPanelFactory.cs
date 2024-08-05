using System.Collections.Generic;
using EndlessClient.Controllers;
using EOLib.Domain.Character;

namespace EndlessClient.UIControls
{
    public interface ICharacterInfoPanelFactory
    {
        IEnumerable<CharacterInfoPanel> CreatePanels(IEnumerable<Character> characters);
        void InjectLoginController(ILoginController loginController);
        void InjectCharacterManagementController(ICharacterManagementController characterManagementController);
    }
}