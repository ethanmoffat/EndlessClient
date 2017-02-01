// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;

namespace EOLib.Domain.Protocol
{
    public class InitializationOutOfDateData : IInitializationData
    {
        private readonly byte _requiredVersionNumber;
        public InitReply Response => InitReply.ClientOutOfDate;

        public int this[InitializationDataKey key]
        {
            get
            {
                if(key != InitializationDataKey.RequiredVersionNumber)
                    throw new ArgumentOutOfRangeException("key", key, null);
                return _requiredVersionNumber;
            }
        }

        public InitializationOutOfDateData(byte requiredVersionNumber)
        {
            _requiredVersionNumber = requiredVersionNumber;
        }
    }
}
