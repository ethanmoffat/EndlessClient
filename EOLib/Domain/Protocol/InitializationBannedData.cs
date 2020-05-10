using System;

namespace EOLib.Domain.Protocol
{
    public class InitializationBannedData : IInitializationData
    {
        private readonly BanType _banType;
        private readonly byte _banTimeRemaining;

        public InitReply Response => InitReply.BannedFromServer;

        public int this[InitializationDataKey key] => GetValueHelper(key);

        public InitializationBannedData(BanType banType, byte banTimeRemaining)
        {
            _banType = banType;
            _banTimeRemaining = banTimeRemaining;
        }

        private int GetValueHelper(InitializationDataKey key)
        {
            switch (key)
            {
                case InitializationDataKey.BanType: return (int)_banType;
                case InitializationDataKey.BanTimeRemaining: return _banTimeRemaining;
                default: throw new ArgumentOutOfRangeException(nameof(key), key, null);
            }
        }
    }
}
