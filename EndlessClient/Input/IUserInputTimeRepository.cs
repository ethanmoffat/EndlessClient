using System;
using AutomaticTypeMapper;

namespace EndlessClient.Input
{
    public interface IUserInputTimeRepository
    {
        DateTime LastInputTime { get; set; }
    }

    [MappedType(BaseType = typeof(IUserInputTimeRepository), IsSingleton = true)]
    public class UserInputTimeRepository : IUserInputTimeRepository
    {
        public DateTime LastInputTime { get; set; }

        public UserInputTimeRepository()
        {
            LastInputTime = DateTime.Now;
        }
    }
}
