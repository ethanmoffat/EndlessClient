using System;
using System.Collections.Generic;
using EOLib;

namespace EndlessClient.Handlers
{
	public static class Chest
	{
		private static byte lastX, lastY;

		public static bool ChestOpen(byte x, byte y)
		{
			EOClient client = (EOClient) World.Instance.Client;
			if (!client.ConnectedAndInitialized)
				return false;

			if(EOChestDialog.Instance != null)
			{
				if (lastX == x && lastY == y &&
					EOChestDialog.Instance.CurrentChestX == x &&
					EOChestDialog.Instance.CurrentChestY == y)
					return true; //chest is already open, back the FUCK off
			}

			lastX = x;
			lastY = y;

			Packet toSend = new Packet(PacketFamily.Chest, PacketAction.Open);
			toSend.AddChar(x);
			toSend.AddChar(y);

			return client.SendPacket(toSend);
		}

		public static void ChestOpenResponse(Packet pkt)
		{
			byte x = pkt.GetChar();
			byte y = pkt.GetChar();

			if(x != lastX || y != lastY)
				EODialog.Show("Uh oh, this is a different chest than you just opened.");

			List<Tuple<short, int>> chestItems = new List<Tuple<short, int>>();
			int numRemaining = pkt.PeekEndString().Length/5;
			for (int i = 0; i < numRemaining; ++i)
			{
				chestItems.Add(new Tuple<short, int>(pkt.GetShort(), pkt.GetThree()));
			}

			MapChest update = World.Instance.ActiveMapRenderer.MapRef.Chests.Find(_c => _c.x == x && _c.y == y);
			if (update != null)
			{
				update.backoff = false;
			}

// ReSharper disable once UnusedVariable
			EOChestDialog chestDialog = new EOChestDialog(x, y, chestItems);
			lastX = lastY = 255;
		}

		public static bool ChestTake(byte x, byte y, short itemID)
		{
			EOClient client = (EOClient) World.Instance.Client;
			if (!client.ConnectedAndInitialized)
				return false;

			Packet pkt = new Packet(PacketFamily.Chest, PacketAction.Take);
			pkt.AddChar(x);
			pkt.AddChar(y);
			pkt.AddShort(itemID);

			return client.SendPacket(pkt);
		}

		/// <summary>
		/// Handler for CHEST_GET packet, sent as confirmation to character that item is being taken
		/// </summary>
		public static void ChestGetResponse(Packet pkt)
		{
			if (EOChestDialog.Instance == null) return;

			short takenID = pkt.GetShort();
			int takenAmount = pkt.GetThree();
			byte characterWeight = pkt.GetChar();
			byte characterMaxWeight = pkt.GetChar();

			int numLeft = pkt.PeekEndString().Length / 5;
			List<Tuple<short, int>> newItems = new List<Tuple<short, int>>(numLeft);
			for (int i = 0; i < numLeft; ++i)
			{
				newItems.Add(new Tuple<short, int>(pkt.GetShort(), pkt.GetThree()));
			}

			//update chest dialog
			EOChestDialog.Instance.InitializeItems(newItems);

			//update item inventory
			World.Instance.MainPlayer.ActiveCharacter.UpdateInventoryItem(takenID, takenAmount, characterWeight, characterMaxWeight, true);
			//update stats
			EOGame.Instance.Hud.RefreshStats();
		}

		/// <summary>
		/// Handler for CHEST_AGREE packet, sent as update to other characters near a chest that was modified by another player
		/// </summary>
		public static void ChestAgreeResponse(Packet pkt)
		{
			//no need to do anything with the packet if the chest isn't open
			if (EOChestDialog.Instance == null) return;

			int numLeft = pkt.PeekEndString().Length / 5;
			List<Tuple<short, int>> newItems = new List<Tuple<short, int>>(numLeft);
			for (int i = 0; i < numLeft; ++i)
			{
				newItems.Add(new Tuple<short, int>(pkt.GetShort(), pkt.GetThree()));
			}
			EOChestDialog.Instance.InitializeItems(newItems);
		}

		public static bool AddItem(short id, int amount)
		{
			EOClient client = (EOClient) World.Instance.Client;
			if (!client.ConnectedAndInitialized || EOChestDialog.Instance == null)
				return false;

			Packet pkt = new Packet(PacketFamily.Chest, PacketAction.Add);
			pkt.AddChar(EOChestDialog.Instance.CurrentChestX);
			pkt.AddChar(EOChestDialog.Instance.CurrentChestY);
			pkt.AddShort(id);
			pkt.AddThree(amount);

			return client.SendPacket(pkt);
		}

		/// <summary>
		/// Handler for CHEST_REPLY packet, sent in response to main player adding an item to a chest
		/// </summary>
		public static void ChestReply(Packet pkt)
		{
			short id = pkt.GetShort();
			int characterAmount = pkt.GetInt();
			byte weight = pkt.GetChar();
			byte maxWeight = pkt.GetChar();

			World.Instance.MainPlayer.ActiveCharacter.UpdateInventoryItem(id, characterAmount, weight, maxWeight);

			ChestAgreeResponse(pkt);
		}
	}
}
