using System.Threading;
using EOLib;
using EOLib.Net;

namespace EndlessClient.Handlers
{
	enum LoginReply : short
	{
		WrongUser = 1,
		WrongUserPass = 2,
		Ok = 3,
		LoggedIn = 5,
		Busy = 6,
		THIS_IS_WRONG = 255
	}

	public static class Login
	{
		private static readonly ManualResetEvent response = new ManualResetEvent(false);
		private static LoginReply ServerResponse = LoginReply.THIS_IS_WRONG;

		public static bool CanProceed { get { return ServerResponse == LoginReply.Ok; } }

		public static bool LoginRequest(string user, string pass)
		{
			EOClient client = (EOClient)World.Instance.Client;
			if (!client.ConnectedAndInitialized)
				return false;
			response.Reset();

			Packet pkt = new Packet(PacketFamily.Login, PacketAction.Request);
			pkt.AddBreakString(user);
			pkt.AddBreakString(pass);

			client.SendPacket(pkt);

			if (!response.WaitOne(Constants.ResponseTimeout))
				return false;
			response.Reset();
			World.Instance.MainPlayer.SetAccountName(user);

			return true;
		}

		//handler for LOGIN_REPLY received from server
		public static void LoginResponse(Packet pkt)
		{
			ServerResponse = (LoginReply)pkt.GetShort();

			if(ServerResponse == LoginReply.Ok)
			{
				//CanProceed will be true: set the player's character data with the response info
				World.Instance.MainPlayer.ProcessCharacterData(pkt);
			}

			response.Set();
		}

		public static DATCONST1 ResponseMessage()
		{
			DATCONST1 message = DATCONST1.NICE_TRY_HAXOR;
			switch(ServerResponse)
			{
				case LoginReply.LoggedIn:
					message = DATCONST1.LOGIN_ACCOUNT_ALREADY_LOGGED_ON;
					break;
				case LoginReply.Busy:
					message = DATCONST1.CONNECTION_SERVER_IS_FULL;
					break;
				case LoginReply.WrongUser:
					message = DATCONST1.LOGIN_ACCOUNT_NAME_NOT_FOUND;
					break;
				case LoginReply.WrongUserPass:
					message = DATCONST1.LOGIN_ACCOUNT_NAME_OR_PASSWORD_NOT_FOUND;
					break;
			}

			return message;
		}

		public static void Cleanup()
		{
			response.Dispose();
		}
	}
}
