// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

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
            _createAdminInteractMembers();
            _createAttackMembers();
            _createAvatarMembers();
            _createBankMembers();
            _createChestMembers();
            _createDoorMembers();
            _createEffectMembers();
            _createEmoteMembers();
            _createInitMembers();
            _createItemMembers();
            _createLockerMembers();
            _createMessageMembers();
            _createMusicMembers();
            _createPaperdollMembers();
            _createPartyMembers();
            _createPlayersMembers();
            _createNPCMembers();
            _createQuestMembers();
            _createRecoverMembers();
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
