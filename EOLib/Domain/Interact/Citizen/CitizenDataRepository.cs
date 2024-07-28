using AutomaticTypeMapper;
using Optional;
using System.Collections.Generic;

namespace EOLib.Domain.Interact.Citizen
{
    public interface ICitizenDataRepository
    {
        Option<int> BehaviorID { get; set; }

        Option<int> CurrentHomeID { get; set; }

        List<string> Questions { get; set; }
    }

    public interface ICitizenDataProvider
    {
        Option<int> BehaviorID { get; }

        Option<int> CurrentHomeID { get; }

        IReadOnlyList<string> Questions { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class CitizenDataRepository : ICitizenDataRepository, ICitizenDataProvider
    {
        public Option<int> BehaviorID { get; set; }

        public Option<int> CurrentHomeID { get; set; }

        public List<string> Questions { get; set; }

        IReadOnlyList<string> ICitizenDataProvider.Questions => Questions;

        public CitizenDataRepository()
        {
            Questions = new List<string>();
        }
    }
}