namespace EndlessClient.HUD
{
    public interface IHudStateActions
    {
        void SwitchToState(InGameStates newState);

        void ToggleMapView();
    }
}