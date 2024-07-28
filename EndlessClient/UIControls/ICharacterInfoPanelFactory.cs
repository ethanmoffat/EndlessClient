using EndlessClient.Controllers;
using EOLib.Domain.Character;
using System.Collections.Generic;

namespace EndlessClient.UIControls;

public interface ICharacterInfoPanelFactory
{
    IEnumerable<CharacterInfoPanel> CreatePanels(IEnumerable<Character> characters);
    void InjectLoginController(ILoginController loginController);
    void InjectCharacterManagementController(ICharacterManagementController characterManagementController);
}