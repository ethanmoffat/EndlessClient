using AutomaticTypeMapper;
using Moffat.EndlessOnline.SDK.Protocol.Net;

namespace EOLib.Domain.Notifiers
{
    public interface IPartyEventNotifier
    {
        void NotifyPartyRequest(PartyRequestType type, int playerId, string name);

        void NotifyPartyJoined();

        void NotifyPartyMemberAdd(string name);

        void NotifyPartyMemberRemove(string name);
    }

    [AutoMappedType]
    public class NoOpPartyEventNotifier : IPartyEventNotifier
    {
        public void NotifyPartyRequest(PartyRequestType type, int playerId, string name) { }

        public void NotifyPartyJoined() { }

        public void NotifyPartyMemberAdd(string name) { }

        public void NotifyPartyMemberRemove(string name) { }
    }
}