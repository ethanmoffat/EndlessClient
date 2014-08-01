using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
		Continue = 1000 // TODO: Check this for the real value
	};

	public static class Account
	{
		private static System.Threading.ManualResetEvent response = new System.Threading.ManualResetEvent(false);

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
			if (!client.Connected)
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
			if (!client.Connected)
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
			if (!client.Connected)
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
		public static string ResponseMessage(out string caption)
		{
			string ret = "";
			switch (ServerResponse)
			{
				case AccountReply.Exists:
					ret = "The account name you provided already exist in our database, use another.";
					caption = "Already exist";
					break;
				case AccountReply.NotApproved:
					ret = "The account name you provided is not approved, use another.";
					caption = "Not approved";
					break;
				case AccountReply.Created:
					ret = "Use your new account name and password to login to the game.";
					caption = "Welcome";
					break;
				case AccountReply.ChangeFailed:
					ret = "The account name or old password you provided do not match with our database.";
					caption = "Request denied";
					break;
				case AccountReply.ChangeSuccess:
					ret = "Your password has changed, please use your new password next time you login.";
					caption = "Password changed";
					break;
				case AccountReply.Continue: //no message associated with continue (i think?)
				default:
					ret = caption = "";
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
