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
                    throw new ArgumentOutOfRangeException(nameof(key), key, null);
                return _requiredVersionNumber;
            }
        }

        public InitializationOutOfDateData(byte requiredVersionNumber)
        {
            _requiredVersionNumber = requiredVersionNumber;
        }
    }
}
