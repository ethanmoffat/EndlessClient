using System;

namespace EOLib.Net.API
{
    public sealed partial class PacketAPI : IDisposable
    {
        private readonly EOClient m_client;

        /// <summary>
        /// Indicates that the connection handshake has completed successfully. ( Initialize()->HandleInit()->ConfirmInit() )
        /// </summary>
        public bool Initialized { get; private set; }

        public PacketAPI(EOClient client)
        {
            if (!client.Connected)
            {
                throw new ArgumentException("The client must be connected to the server in order to construct the API!");
            }
            m_client = client;

            //each of these sets up members of the partial PacketAPI class relevant to a particular packet family
            _createBankMembers();
            _createChestMembers();
            _createInitMembers();
            _createLockerMembers();
            _createMessageMembers();
            _createMusicMembers();
            _createPartyMembers();
            _createNPCMembers();
            _createQuestMembers();
            _createShopMembers();
            _createSpellMembers();
            _createStatSkillMembers();
            _createTradeMembers();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
                _disposeInitMembers();
        }
    }
}
