using System;

namespace EOLib.Net
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
			_createAccountMembers();
			_createAdminInteractMembers();
			_createAttackMembers();
			_createAvatarMembers();
			_createCharacterMembers();
			_createChestMembers();
			_createConnectionMembers();
			_createDoorMembers();
			_createInitMembers();
			_createLoginMembers();
			_createMessageMembers();
			_createPaperdollMembers();
			_createPlayersMembers();
			_createNPCMembers();
			_createRefreshMembers();
			_createWalkMembers();
			_createWarpMembers();
			_createWelcomeMembers();
		}

		public void Disconnect()
		{
			Initialized = false;
		}

		public void Dispose()
		{
			Dispose(true);
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				_disposeAccountMembers();
				_disposeCharacterMembers();
				_disposeInitMembers();
				_disposeLoginMembers();
				_disposeWelcomeMembers();
			}
		}
	}
}
