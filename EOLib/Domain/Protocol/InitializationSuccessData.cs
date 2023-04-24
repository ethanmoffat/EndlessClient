using System;

namespace EOLib.Domain.Protocol
{
    public class InitializationSuccessData : IInitializationData
    {
        public InitReply Response => InitReply.Success;

        public int this[InitializationDataKey key] => GetValueHelper(key);

        private readonly int _seq1, _seq2, _sendMulti, _recvMulti;
        private readonly int _playerID;
        private readonly int _hashResponse;

        public InitializationSuccessData(int sequence1,
                                         int sequence2,
                                         int receiveMultiple,
                                         int sendMultiple,
                                         int playerID,
                                         int hashResponse)
        {
            _seq1 = sequence1;
            _seq2 = sequence2;
            _recvMulti = receiveMultiple;
            _sendMulti = sendMultiple;
            _playerID = playerID;
            _hashResponse = hashResponse;
        }

        private int GetValueHelper(InitializationDataKey key)
        {
            switch (key)
            {
                case InitializationDataKey.SequenceByte1: return _seq1;
                case InitializationDataKey.SequenceByte2: return _seq2;
                case InitializationDataKey.SendMultiple: return _sendMulti;
                case InitializationDataKey.ReceiveMultiple: return _recvMulti;
                case InitializationDataKey.PlayerID: return _playerID;
                case InitializationDataKey.HashResponse: return _hashResponse;
                default: throw new ArgumentOutOfRangeException(nameof(key), key, null);
            }
        }
    }
}
