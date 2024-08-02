using System;
using AutomaticTypeMapper;

namespace EndlessClient.Input
{
    public interface IUserInputTimeRepository
    {
        DateTime LastInputTime { get; set; }
    }

    public interface IUserInputTimeProvider
    {
        DateTime LastInputTime { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class UserInputTimeRepository : IUserInputTimeRepository, IUserInputTimeProvider
    {
        public DateTime LastInputTime { get; set; }

        public UserInputTimeRepository()
        {
            LastInputTime = DateTime.Now;
        }
    }
}