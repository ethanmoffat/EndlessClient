using AutomaticTypeMapper;

namespace EOLib.Domain.Notifiers
{
    public interface ITradeEventNotifier
    {
        void NotifyTradeRequest(int playerId, string name);

        void NotifyTradeAccepted();

        void NotifyTradeClose(bool cancel);
    }

    [AutoMappedType]
    public class NoopTradeEventNotifier : ITradeEventNotifier
    {
        public void NotifyTradeRequest(int playerId, string name) { }

        public void NotifyTradeAccepted() { }

        public void NotifyTradeClose(bool cancel) { }
    }
}