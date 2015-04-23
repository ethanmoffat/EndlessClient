using EOLib;
using EOLib.Net;

namespace EndlessClient.Handlers
{
	public static class Bank
	{
		public static bool BankOpen(short npcIndex)
		{
			EOClient client = (EOClient) World.Instance.Client;
			if (!client.ConnectedAndInitialized || npcIndex < 0)
				return false;

			Packet pkt = new Packet(PacketFamily.Bank, PacketAction.Open);
			pkt.AddShort(npcIndex);

			return client.SendPacket(pkt);
		}

		public static void BankOpenReply(Packet pkt)
		{
			if (EOBankAccountDialog.Instance == null)
				return;

			int bankGold = pkt.GetInt();
			pkt.Skip(3); /*Session token - eoserv always sets 0*/
			int lockerUpgrades = pkt.GetChar(); //number of locker upgrades that have been done

			EOBankAccountDialog.Instance.AccountBalance = string.Format("{0}", bankGold);
			EOBankAccountDialog.Instance.LockerUpgrades = lockerUpgrades;
		}

		public static bool BankDeposit(int amount)
		{
			EOClient client = (EOClient)World.Instance.Client;
			if (!client.ConnectedAndInitialized || amount < 0)
				return false;

			Packet pkt = new Packet(PacketFamily.Bank, PacketAction.Add);
			pkt.AddInt(amount);

			return client.SendPacket(pkt);
		}

		public static bool BankWithdraw(int amount)
		{
			EOClient client = (EOClient)World.Instance.Client;
			if (!client.ConnectedAndInitialized || amount < 0)
				return false;

			Packet pkt = new Packet(PacketFamily.Bank, PacketAction.Take);
			pkt.AddInt(amount);

			return client.SendPacket(pkt);
		}

		public static void BankReply(Packet pkt)
		{
			if (EOBankAccountDialog.Instance == null)
				return;

			int characterGold = pkt.GetInt();
			int bankAmount = pkt.GetInt();

			World.Instance.MainPlayer.ActiveCharacter.UpdateInventoryItem(1, characterGold);
			EOBankAccountDialog.Instance.AccountBalance = string.Format("{0}", bankAmount);
		}
	}
}
