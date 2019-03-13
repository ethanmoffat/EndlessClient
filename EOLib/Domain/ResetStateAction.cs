// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using AutomaticTypeMapper;

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