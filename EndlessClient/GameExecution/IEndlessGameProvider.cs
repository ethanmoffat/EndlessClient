// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using AutomaticTypeMapper;

namespace EndlessClient.GameExecution
{
    public interface IEndlessGameRepository
    {
        IEndlessGame Game { get; set; }
    }

    public interface IEndlessGameProvider
    {
        IEndlessGame Game { get; }
    }

    [MappedType(BaseType = typeof(IEndlessGameRepository), IsSingleton = true)]
    [MappedType(BaseType = typeof(IEndlessGameProvider), IsSingleton = true)]
    public class EndlessGameRepository : IEndlessGameRepository, IEndlessGameProvider
    {
        public IEndlessGame Game { get; set; }
    }
}
