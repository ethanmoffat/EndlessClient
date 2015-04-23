namespace EOLib.Net
{
	partial class PacketAPI
	{
		/// <summary>
		/// Confirms initialization with server
		/// </summary>
		/// <param name="SendMulti">Multiplier for send (encrypt multi)</param>
		/// <param name="RecvMulti">Multiplier for recv (decrypt multi)</param>
		/// <param name="clientID">Connection identifier</param>
		/// <returns>True on successful send operation</returns>
		public bool ConfirmInit(byte SendMulti, byte RecvMulti, short clientID)
		{
			if (!m_client.ConnectedAndInitialized)
				return false;

			Packet confirm = new Packet(PacketFamily.Connection, PacketAction.Accept);
			confirm.AddShort(SendMulti);
			confirm.AddShort(RecvMulti);
			confirm.AddShort(clientID);

			if (m_client.SendPacket(confirm))
			{
				Initialized = true;
				return true;
			}

			return false;
		}
	}
}
