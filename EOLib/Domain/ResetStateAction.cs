using AutomaticTypeMapper;
using System.Collections.Generic;

namespace EOLib.Domain
{
    [AutoMappedType]
    public class ResetStateAction : IResetStateAction
    {
        private readonly IEnumerable<IResettable> _resettables;

        public ResetStateAction(IEnumerable<IResettable> resettables)
        {
            _resettables = resettables;
        }

        public void ResetState()
        {
            foreach (var resettable in _resettables)
                resettable.ResetState();
        }
    }
}