using EndlessClient.Controllers;
using EndlessClient.GameExecution;

namespace EndlessClient.ControlSets
{
    public interface IControlSetFactory
    {
        IControlSet CreateControlsForState(GameStates newState, IControlSet currentControlSet);

        void InjectControllers(IMainButtonController mainButtonController,
                               IAccountController accountController,
                               ILoginController loginController,
                               ICharacterManagementController characterManagementController);
    }
}
