using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EOLib;

namespace EndlessClient.Handlers
{
	enum WelcomeReply : short
	{
		RequestGranted = 1, //response from welcome_request
		WelcomeMessage = 2, //response from welcome_message
	}

	public static class Welcome
	{
		//used to signal a response was received
		private static System.Threading.ManualResetEvent response = new System.Threading.ManualResetEvent(false);

		public static bool FirstTimePlayer { get; private set; }

		//Sends WELCOME_REQUEST to server
		public static bool SelectCharacter(int charID)
		{
			EOClient client = (EOClient)World.Instance.Client;
			if (!client.Connected)
				return false;

			response.Reset();

			Packet builder = new Packet(PacketFamily.Welcome, PacketAction.Request);
			builder.AddInt(charID);

			if (!client.SendPacket(builder))
				return false;

			if (!response.WaitOne(Constants.ResponseTimeout))
				return false;
			response.Reset();

			return true;
		}
		
		//this is the handler method registered in EOClient
		public static void WelcomeResponse(Packet pkt)
		{
			//set server response to the value in the packet
			WelcomeReply reply = (WelcomeReply)pkt.GetShort();
			Player main = World.Instance.MainPlayer;
			//returning from the switch block will prompt an error back to the user for connection timeout
			//use 'return' on errors instead of break (since 'response' is never set at the end of the method)
			switch(reply)
			{
				//clicking login was successful. gets files from server.
				//response for SelectCharacter method
				case WelcomeReply.RequestGranted:
					{
						short playerID = pkt.GetShort();
						if (playerID != main.PlayerID)
							return;
						main.SetPlayerID(playerID);

						if (!main.SetActiveCharacter(pkt.GetInt()))
							return;

						short mapID = pkt.GetShort();
						byte[] mapRid = pkt.GetBytes(4);
						int mapFileSize = pkt.GetThree();
						main.ActiveCharacter.Welcome(mapID, mapRid[0] == 0xFF && mapRid[1] == 0x01);
						World.Instance.CheckMap(mapID, Packet.DecodeNumber(mapRid), mapFileSize);

						byte[] eifRid = pkt.GetBytes(4);
						byte[] eifLen = pkt.GetBytes(2);
						World.Instance.CheckPub(InitFileType.Item, Packet.DecodeNumber(eifRid), (short)Packet.DecodeNumber(eifLen));

						byte[] enfRid = pkt.GetBytes(4);
						byte[] enfLen = pkt.GetBytes(2);
						World.Instance.CheckPub(InitFileType.Npc, Packet.DecodeNumber(enfRid), (short)Packet.DecodeNumber(enfLen));

						byte[] esfRid = pkt.GetBytes(4);
						byte[] esfLen = pkt.GetBytes(2);
						World.Instance.CheckPub(InitFileType.Spell, Packet.DecodeNumber(esfRid), (short)Packet.DecodeNumber(esfLen));

						byte[] ecfRid = pkt.GetBytes(4);
						byte[] ecfLen = pkt.GetBytes(2);
						World.Instance.CheckPub(InitFileType.Class, Packet.DecodeNumber(ecfRid), (short)Packet.DecodeNumber(ecfLen));

						main.ActiveCharacter.Name = pkt.GetBreakString();
						main.ActiveCharacter.Title = pkt.GetBreakString();
						main.ActiveCharacter.GuildName = pkt.GetBreakString();
						main.ActiveCharacter.GuildRankStr = pkt.GetBreakString();
						main.ActiveCharacter.Class = pkt.GetChar();
						main.ActiveCharacter.PaddedGuildTag = pkt.GetFixedString(3); //padded guild tag is 3 characters
						main.ActiveCharacter.AdminLevel = (AdminLevel)pkt.GetChar();

						main.ActiveCharacter.Stats = new CharStatData()
						{
							level = pkt.GetChar(),
							exp = pkt.GetInt(),
							usage = pkt.GetInt(),

							hp = pkt.GetShort(),
							maxhp = pkt.GetShort(),
							tp = pkt.GetShort(),
							maxtp = pkt.GetShort(),
							maxsp = pkt.GetShort(),

							statpoints = pkt.GetShort(),
							skillpoints = pkt.GetShort(),
							karma = pkt.GetShort(),
							mindam = pkt.GetShort(),
							maxdam = pkt.GetShort(),
							accuracy = pkt.GetShort(),
							evade = pkt.GetShort(),
							armor = pkt.GetShort(),
							disp_str = pkt.GetShort(),
							disp_int = pkt.GetShort(),
							disp_wis = pkt.GetShort(),
							disp_agi = pkt.GetShort(),
							disp_con = pkt.GetShort(),
							disp_cha = pkt.GetShort()
						};

						for (int i = 0; i < (int)EquipLocation.PAPERDOLL_MAX; ++i )
						{
							main.ActiveCharacter.PaperDoll[i] = pkt.GetShort();
						}

						main.ActiveCharacter.GuildRankNum = pkt.GetChar();
						World.Instance.JailMap = pkt.GetShort();
						pkt.Skip(12); //i think these can safely be skipped for the moment
						FirstTimePlayer = pkt.GetChar() == 2; //signal that the player should see the "first timer" message
						if (pkt.GetByte() != 255)
							return; //error, something is off.
					}
					break;
				case WelcomeReply.WelcomeMessage:
					//the game screen is loading! get character info for other players, welcome message, etc.
					//response for WELCOME_MSG packet (no method defined here yet)
					break;
			}
			response.Set();
		}
		
		//classes that have a manualresetevent in them need to be cleaned up.
		//this call should be put in the dispose method of the EOClient class
		public static void Cleanup()
		{
			response.Dispose();
		}
	}
}
