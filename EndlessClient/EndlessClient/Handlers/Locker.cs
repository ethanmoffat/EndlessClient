using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EOLib;

namespace EndlessClient.Handlers
{
	public static class Locker
	{
		/// <summary>
		/// Opens a locker at X/Y coordinates
		/// </summary>
		public static bool OpenLocker(byte x, byte y)
		{
			if (EOBankVaultDialog.Instance == null) return true;

			EOClient client = (EOClient)World.Instance.Client;
			if (!client.ConnectedAndInitialized)
				return false;

			Packet pkt = new Packet(PacketFamily.Locker, PacketAction.Open);
			pkt.AddChar(x);
			pkt.AddChar(y);

			return client.SendPacket(pkt);
		}

		/// <summary>
		/// Deposit an item in your private locker
		/// </summary>
		public static bool AddItem(short id, int amount)
		{
			if (EOBankVaultDialog.Instance == null) return true;

			EOClient client = (EOClient)World.Instance.Client;
			if (!client.ConnectedAndInitialized)
				return false;

			Packet pkt = new Packet(PacketFamily.Locker, PacketAction.Add);
			pkt.AddChar(EOBankVaultDialog.Instance.X);
			pkt.AddChar(EOBankVaultDialog.Instance.Y);
			pkt.AddShort(id);
			pkt.AddThree(amount);

			return client.SendPacket(pkt);
		}

		/// <summary>
		/// Withdraw an item from your private locker
		/// </summary>
		public static bool TakeItem(short id)
		{
			if (EOBankVaultDialog.Instance == null) return true;

			EOClient client = (EOClient)World.Instance.Client;
			if (!client.ConnectedAndInitialized)
				return false;

			Packet pkt = new Packet(PacketFamily.Locker, PacketAction.Take);
			pkt.AddChar(EOBankVaultDialog.Instance.X);
			pkt.AddChar(EOBankVaultDialog.Instance.Y);
			pkt.AddShort(id);

			return client.SendPacket(pkt);
		}

		/// <summary>
		/// Handles LOCKER_OPEN from server for opening a locker
		/// </summary>
		public static void LockerOpen(Packet pkt)
		{
			if (EOBankVaultDialog.Instance == null) return;

			byte x = pkt.GetChar();
			byte y = pkt.GetChar();

			if (EOBankVaultDialog.Instance.X != x || EOBankVaultDialog.Instance.Y != y)
				return;

			List<InventoryItem> items = new List<InventoryItem>();
			while (pkt.ReadPos != pkt.Length)
			{
				items.Add(new InventoryItem {id = pkt.GetShort(), amount = pkt.GetThree()});
			}

			EOBankVaultDialog.Instance.SetVaultData(items);
		}

		/// <summary>
		/// Handles LOCKER_REPLY from server for adding an item to locker
		/// </summary>
		public static void LockerReply(Packet pkt)
		{
			if (EOBankVaultDialog.Instance == null) return;

			//inventory info for amount remaining for character
			short itemID = pkt.GetShort();
			int amount = pkt.GetInt();
			byte weight = pkt.GetChar();
			byte maxWeight = pkt.GetChar();
			World.Instance.MainPlayer.ActiveCharacter.UpdateInventoryItem(itemID, amount, weight, maxWeight);

			//items in the locker
			List<InventoryItem> items = new List<InventoryItem>();
			while (pkt.ReadPos != pkt.Length)
			{
				items.Add(new InventoryItem {id = pkt.GetShort(), amount = pkt.GetThree()});
			}
			EOBankVaultDialog.Instance.SetVaultData(items);
		}

		/// <summary>
		/// Handles LOCKER_GET from server for taking an item from locker
		/// </summary>
		public static void LockerGet(Packet pkt)
		{
			if (EOBankVaultDialog.Instance == null) return;

			short itemID = pkt.GetShort();
			int amount = pkt.GetThree();
			byte weight = pkt.GetChar();
			byte maxWeight = pkt.GetChar();

			World.Instance.MainPlayer.ActiveCharacter.UpdateInventoryItem(itemID, amount, weight, maxWeight, true);

			List<InventoryItem> items = new List<InventoryItem>();
			while (pkt.ReadPos != pkt.Length)
			{
				items.Add(new InventoryItem {id = pkt.GetShort(), amount = pkt.GetThree()});
			}
			EOBankVaultDialog.Instance.SetVaultData(items);
		}
	}
}
