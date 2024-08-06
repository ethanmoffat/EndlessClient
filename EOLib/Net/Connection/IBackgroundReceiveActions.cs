namespace EOLib.Net.Connection
{
    public interface IBackgroundReceiveActions
    {
        void RunBackgroundReceiveLoop();

        void CancelBackgroundReceiveLoop();
    }
}
