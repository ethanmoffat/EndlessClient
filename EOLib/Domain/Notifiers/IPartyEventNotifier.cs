using AutomaticTypeMapper;
using EOLib.Domain.Party;

namespace EOLib.Domain.Notifiers
{
    public interface IPartyEventNotifier
    {
        void NotifyPartyRequest(PartyRequestType type, short playerId, string name);

        void NotifyPartyJoined();

        void NotifyPartyMemberAdd(string name);

        void NotifyPartyMemberRemove(string name);
    }

    [AutoMappedType]
    public class NoOpPartyEventNotifier : IPartyEventNotifier
    {
        public void NotifyPartyRequest(PartyRequestType type, short playerId, string name) { }

        public void NotifyPartyJoined() { }

        public void NotifyPartyMemberAdd(string name) { }

        public void NotifyPartyMemberRemove(string name) { }
    }
}
