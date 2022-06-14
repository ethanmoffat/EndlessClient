using AutomaticTypeMapper;
using System.Collections.Generic;

namespace EOLib.Domain.Party
{
    public interface IPartyDataRepository
    {
        List<PartyMember> Members { get; }
    }

    public  interface IPartyDataProvider
    {
        IReadOnlyList<PartyMember> Members { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class PartyDataRepository : IPartyDataRepository, IPartyDataProvider
    {
        public List<PartyMember> Members { get; set; }

        IReadOnlyList<PartyMember> IPartyDataProvider.Members => Members;

        public PartyDataRepository()
        {
            Members = new List<PartyMember>();
        }
    }
}
