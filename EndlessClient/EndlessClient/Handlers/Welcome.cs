using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EndlessClient.Handlers
{
	enum GENERICREPLYENUM : short
	{
		SOMEOKAYVALUE = 1,
		SOMEDEFAULTVALUE = 255
	}

	public static class Welcome //rename to packet family
	{
		//used to signal a response was received
		private static System.Threading.ManualResetEvent response = new System.Threading.ManualResetEvent(false);

		//used to store the response : add additional response variables here as necessary
		private static GENERICREPLYENUM ServerResponse = GENERICREPLYENUM.SOMEDEFAULTVALUE;

		//indicates that the server response (if one was expected) is okay
		public static bool CanProceed
		{
			get
			{
				return ServerResponse == GENERICREPLYENUM.SOMEOKAYVALUE;
			}
		}

		//Sends WELCOME_REQUEST to server
		public static bool SelectCharacter(int charID)
		{
			EOClient client = (EOClient)World.Instance.Client;
			if (!client.Connected)
				return false;

			response.Reset();

			Packet builder = new Packet(PacketFamily.Welcome, PacketAction.Request);

			//add stuff to the packet as necessary

			if (!client.SendPacket(builder))
				return false;

			if (!response.WaitOne(Constants.ResponseTimeout))
				return false;
			response.Reset();

			return true;
		}

		public static void TemplateResponse(Packet pkt)
		{
			//set server response to the value in the packet
			response.Set();
		}

		public static string ResponseMessage(out string caption)
		{
			//enumerate over the reply enum and return message/caption for
			//	dialog boxes based on the response from server
			return caption = "";
		}

		//classes that have a manualresetevent in them need to be cleaned up.
		//this call should be put in the dispose method of the EOClient class
		public static void Cleanup()
		{
			response.Dispose();
		}
	}
}
