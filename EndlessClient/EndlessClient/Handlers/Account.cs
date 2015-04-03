using EOLib;

namespace EndlessClient.Handlers
{
	enum AccountReply : short
	{
		Exists = 1,
		NotApproved = 2,
		Created = 3,
		ChangeFailed = 5,
		ChangeSuccess = 6,
		Continue = 1000
	};

	public static class Account
	{
		private static readonly System.Threading.ManualResetEvent response = new System.Threading.ManualResetEvent(false);

		private static AccountReply ServerResponse = AccountReply.Exists;

		public static bool CanProceed
		{
			get
			{
				return ServerResponse == AccountReply.Continue || ServerResponse == AccountReply.Created || ServerResponse == AccountReply.ChangeSuccess;
			}
		}

		//client wants to change their password. Sends ACCOUNT_AGREE
		public static bool AccountChangePassword(string username, string old_password, string new_password)
		{
			EOClient client = (EOClient)World.Instance.Client;
			if (!client.ConnectedAndInitialized)
				return false;

			response.Reset();

			Packet builder = new Packet(PacketFamily.Account, PacketAction.Agree);
			builder.AddBreakString(username);
			builder.AddBreakString(old_password);
			builder.AddBreakString(new_password);
			if (!client.SendPacket(builder))
				return false;

			if (!response.WaitOne(Constants.ResponseTimeout))
				return false;
			response.Reset();

			return true;
		}

		//client checks validity of an account name with the server
		public static bool AccountCheckName(string username)
		{
			EOClient client = (EOClient)World.Instance.Client;
			if (!client.ConnectedAndInitialized)
				return false;

			response.Reset();

			Packet builder = new Packet(PacketFamily.Account, PacketAction.Request);
			builder.AddString(username);

			if (!client.SendPacket(builder))
				return false;

			if (!response.WaitOne(Constants.ResponseTimeout))
				return false;
			response.Reset();

			return true;
		}

		//client attempts to create an account. data sent to server
		public static bool AccountCreate(string uName, string pass, string realName, string location, string email)
		{
			EOClient client = (EOClient)World.Instance.Client;
			if (!client.ConnectedAndInitialized)
				return false;

			response.Reset();

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
			builder.AddBreakString(Config.GetHDDSerial());

			if (!client.SendPacket(builder))
				return false;

			if (!response.WaitOne(Constants.ResponseTimeout))
				return false;
			response.Reset();

			return true;
		}

		//client receives a response from the server related to an account
		//this is the registered handler function for ACCOUNT_REPLY
		public static void AccountResponse(Packet pkt)
		{
			ServerResponse = (AccountReply)pkt.GetShort();
			response.Set();
		}

		//translates between a response from the server and what should be shown in a dialog box.
		public static DATCONST1 ResponseMessage()
		{
			DATCONST1 ret = DATCONST1.NICE_TRY_HAXOR;
			switch (ServerResponse)
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

		public static void Cleanup()
		{
			response.Dispose();
		}
	}
}
