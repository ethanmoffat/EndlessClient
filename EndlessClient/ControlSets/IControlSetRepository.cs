// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using AutomaticTypeMapper;

namespace EndlessClient.ControlSets
{
    public interface IControlSetRepository
    {
        IControlSet CurrentControlSet { get; set; }
    }

    public interface IControlSetProvider
    {
        IControlSet CurrentControlSet { get; }
    }

    [MappedType(BaseType = typeof(IControlSetRepository), IsSingleton = true)]
    [MappedType(BaseType = typeof(IControlSetProvider), IsSingleton = true)]
    public class ControlSetRepository : IControlSetRepository, IControlSetProvider
    {
        public IControlSet CurrentControlSet { get; set; }

        public ControlSetRepository()
        {
            CurrentControlSet = new EmptyControlSet();
        }
    }
}
