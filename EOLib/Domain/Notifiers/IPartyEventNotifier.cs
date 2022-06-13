using AutomaticTypeMapper;
using EOLib.Domain.Party;

namespace EOLib.Domain.Notifiers
{
    public interface IPartyEventNotifier
    {
        void NotifyPartyRequest(PartyRequestType type, short playerId, string name);
    }

    [AutoMappedType]
    public class NoOpPartyEventNotifier : IPartyEventNotifier
    {
        public void NotifyPartyRequest(PartyRequestType type, short playerId, string name) { }
    }
}
