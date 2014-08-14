using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EOLib;

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
		private static System.Threading.ManualResetEvent response = new System.Threading.ManualResetEvent(false);
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

		public static string ResponseMessage(out string caption)
		{
			string message;
			caption = "Login refused";
			switch(ServerResponse)
			{
				case LoginReply.LoggedIn:
					message = "This account is already logged on. Please try again in a few minutes.";
					break;
				case LoginReply.Busy:
					message = "The server is currently full. Please try again in a few minutes.";
					break;
				case LoginReply.WrongUser:
					message = "The account name you provided does not exist in our database.";
					break;
				case LoginReply.WrongUserPass:
					message = "The account or passsword you provided could not be found in our database.";
					break;
				case LoginReply.Ok:
				default:
					caption = "";
					message = "";
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
