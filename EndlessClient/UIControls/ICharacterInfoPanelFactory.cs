// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

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
