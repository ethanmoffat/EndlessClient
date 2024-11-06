using EndlessClient.HUD.Panels;

namespace EndlessClient.HUD
{
    public interface IHudStateActions
    {
        IHudPanel SwitchToState(InGameStates newState);

        void ToggleMapView();
    }
}
