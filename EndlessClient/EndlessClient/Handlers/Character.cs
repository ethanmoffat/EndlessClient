using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EOLib;

namespace EndlessClient.Handlers
{
	enum CharacterReply : short
	{
		Exists = 1,
		Full = 2,
		NotApproved = 4,
		Ok = 5,
		Deleted = 6,
		THIS_IS_WRONG = 255
	}

	public static class Character
	{
		//used to signal a response was received
		private static System.Threading.ManualResetEvent response = new System.Threading.ManualResetEvent(false);

		//used to store the response : add additional response variables here as necessary
		private static CharacterReply ServerResponse = CharacterReply.THIS_IS_WRONG;

		//indicates that the server response (if one was expected) is okay
		public static bool CanProceed
		{
			get
			{
				return ServerResponse == CharacterReply.Ok;
			}
		}

		public static int CharacterTakeID
		{
			get;
			private set;
		}

		public static bool TooManyCharacters { get { return ServerResponse == CharacterReply.Full; } }

		//sends CHARACTER_REQUEST
		public static bool CharacterRequest()
		{
			EOClient client = (EOClient)World.Instance.Client;
			if (!client.Connected)
				return false;

			response.Reset();

			Packet builder = new Packet(PacketFamily.Character, PacketAction.Request);
			
			if (!client.SendPacket(builder))
				return false;

			if (!response.WaitOne(Constants.ResponseTimeout))
				return false;
			response.Reset();

			return true;
		}

		//sends CHARACTER_CREATE to server
		public static bool CharacterCreate(byte gender, byte hairStyle, byte hairColor, byte race, string name)
		{
			EOClient client = (EOClient)World.Instance.Client;
			if (!client.Connected)
				return false;

			response.Reset();
			ServerResponse = CharacterReply.THIS_IS_WRONG;

			Packet builder = new Packet(PacketFamily.Character, PacketAction.Create);
			builder.AddShort(255);
			builder.AddShort(gender);
			builder.AddShort(hairStyle);
			builder.AddShort(hairColor);
			builder.AddShort(race);
			builder.AddByte(255);
			builder.AddBreakString(name);

			if (!client.SendPacket(builder))
				return false;

			if (!response.WaitOne(Constants.ResponseTimeout))
				return false;
			response.Reset();

			return true;
		}

		//sends Character_Take to server
		public static bool CharacterTake(int id)
		{
			EOClient client = (EOClient)World.Instance.Client;
			if (!client.Connected)
				return false;

			response.Reset();

			Packet builder = new Packet(PacketFamily.Character, PacketAction.Take);
			builder.AddInt(id);

			if (!client.SendPacket(builder))
				return false;

			if (!response.WaitOne(Constants.ResponseTimeout))
				return false;
			response.Reset();

			return true;
		}

		//sends CHARACTER_REMOVE to server
		public static bool CharacterRemove(int id)
		{
			EOClient client = (EOClient)World.Instance.Client;
			if (!client.Connected)
				return false;

			ServerResponse = CharacterReply.THIS_IS_WRONG;
			response.Reset();

			Packet builder = new Packet(PacketFamily.Character, PacketAction.Remove);
			builder.AddShort(255);
			builder.AddInt(id);

			if (!client.SendPacket(builder))
				return false;

			if (!response.WaitOne(Constants.ResponseTimeout))
				return false;
			response.Reset();

			return true;
		}

		//handler function for when server sends CHARACTER_REPLY
		public static void CharacterResponse(Packet pkt)
		{
			//set server response to the value in the packet
			short characterResponse = pkt.GetShort();

			if(characterResponse > (short)CharacterReply.Deleted)
			{
				//this is the value used in eoserv for character request
				ServerResponse = CharacterReply.Ok;
			}
			else
			{
				ServerResponse = (CharacterReply)characterResponse;
				if (ServerResponse == CharacterReply.Ok || ServerResponse == CharacterReply.Deleted)
				{
					World.Instance.MainPlayer.ProcessCharacterData(pkt);
				}
			}

			response.Set();
		}

		//handler function for when server sends CHARACTER_PLAYER (in response to CHARACTER_TAKE)
		public static void CharacterPlayerResponse(Packet pkt)
		{
			pkt.GetShort();
			CharacterTakeID = pkt.GetInt();
			response.Set();
		}

		public static string ResponseMessage(out string caption)
		{
			string message;
			switch (ServerResponse)
			{
				case CharacterReply.Ok:
					message = "Your character has been created and is ready to explore a new world";
					caption = "Character created";
					break;
				case CharacterReply.Full:
					message = "You can only have 3 characters. Please delete a character and try again.";
					caption = "Request denied";
					break;
				case CharacterReply.Exists:
					message = "The character name you provided already exists in our database, use another.";
					caption = "Already exists";
					break;
				case CharacterReply.NotApproved:
					message = "The character name you provided is not approved, try another name.";
					caption = "Not approved";
					break;
				case CharacterReply.Deleted:
				default:
					message = caption = "";
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
