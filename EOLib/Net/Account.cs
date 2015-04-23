using System.Threading;

namespace EOLib.Net
{
	public enum AccountReply : short
	{
		THIS_IS_WRONG = 0,
		Exists = 1,
		NotApproved = 2,
		Created = 3,
		ChangeFailed = 5,
		ChangeSuccess = 6,
		Continue = 1000
	};

	partial class PacketAPI
	{
		private AutoResetEvent m_account_responseEvent;

		private AccountReply m_account_reply;

		private void _createAccountMembers()
		{
			m_account_responseEvent = new AutoResetEvent(false);
			m_account_reply = AccountReply.THIS_IS_WRONG;
			
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Account, PacketAction.Reply),
				pkt =>
				{
					m_account_reply = (AccountReply) pkt.GetShort();
					m_account_responseEvent.Set();
				}, false);
		}

		private void _disposeAccountMembers()
		{
			if (m_account_responseEvent != null)
			{
				m_account_responseEvent.Dispose();
				m_account_responseEvent = null;
			}
		}

		public bool AccountChangePassword(string username, string old_password, string new_password, out AccountReply result)
		{
			result = AccountReply.THIS_IS_WRONG;
			if (!m_client.ConnectedAndInitialized || !Initialized)
				return false;

			Packet builder = new Packet(PacketFamily.Account, PacketAction.Agree);
			builder.AddBreakString(username);
			builder.AddBreakString(old_password);
			builder.AddBreakString(new_password);
			
			if (!m_client.SendPacket(builder) || !m_account_responseEvent.WaitOne(Constants.ResponseTimeout))
				return false;

			result = m_account_reply;

			return true;
		}

		public bool AccountCheckName(string username, out AccountReply result)
		{
			result = AccountReply.THIS_IS_WRONG;
			if (!m_client.ConnectedAndInitialized || !Initialized)
				return false;

			Packet builder = new Packet(PacketFamily.Account, PacketAction.Request);
			builder.AddString(username);

			if (!m_client.SendPacket(builder) || !m_account_responseEvent.WaitOne(Constants.ResponseTimeout))
				return false;

			result = m_account_reply;

			return true;
		}

		public bool AccountCreate(string uName, string pass, string realName, string location, string email, string HDDSerial, out AccountReply result)
		{
			result = AccountReply.THIS_IS_WRONG;
			if (!m_client.ConnectedAndInitialized || !Initialized)
				return false;

			Packet builder = new Packet(PacketFamily.Account, PacketAction.Create);
			//eoserv doesn't care...
			builder.AddShort(1337);
			builder.AddByte(42);

			builder.AddBreakString(uName);
			builder.AddBreakString(pass);
			builder.AddBreakString(realName);
			builder.AddBreakString(location);
			builder.AddBreakString(email);
			builder.AddBreakString(System.Net.Dns.GetHostName());
			builder.AddBreakString(HDDSerial);

			if (!m_client.SendPacket(builder) || !m_account_responseEvent.WaitOne(Constants.ResponseTimeout))
				return false;

			result = m_account_reply;

			return true;
		}

		public DATCONST1 AccountResponseMessage()
		{
			DATCONST1 ret = DATCONST1.NICE_TRY_HAXOR;
			switch (m_account_reply)
			{
				case AccountReply.Exists:
					ret = DATCONST1.ACCOUNT_CREATE_NAME_EXISTS;
					break;
				case AccountReply.NotApproved:
					ret = DATCONST1.ACCOUNT_CREATE_NAME_NOT_APPROVED;
					break;
				case AccountReply.Created:
					ret = DATCONST1.ACCOUNT_CREATE_SUCCESS_WELCOME;
					break;
				case AccountReply.ChangeFailed:
					ret = DATCONST1.CHANGE_PASSWORD_MISMATCH;
					break;
				case AccountReply.ChangeSuccess:
					ret = DATCONST1.CHANGE_PASSWORD_SUCCESS;
					break;
			}
			return ret;
		}
	}
}
