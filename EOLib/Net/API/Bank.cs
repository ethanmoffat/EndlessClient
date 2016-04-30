// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Net.Handlers;

namespace EOLib.Net.API
{
	partial class PacketAPI
	{
		public delegate void BankOpenEvent(int bankGold, int lockerUpgrades);
		public delegate void BankChangeEvent(int characterGold, int bankGold);

		public event BankOpenEvent OnBankOpen;
		public event BankChangeEvent OnBankChange;

		private void _createBankMembers()
		{
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Bank, PacketAction.Open), _handleBankOpen, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Bank, PacketAction.Reply), _handleBankReply, true);
		}

		public bool BankOpen(short npcIndex)
		{
			if (!m_client.ConnectedAndInitialized || !Initialized || npcIndex < 0)
				return false;

			OldPacket pkt = new OldPacket(PacketFamily.Bank, PacketAction.Open);
			pkt.AddShort(npcIndex);

			return m_client.SendPacket(pkt);
		}

		private void _handleBankOpen(OldPacket pkt)
		{
			int bankGold = pkt.GetInt();
			pkt.Skip(3); /*Session token - eoserv always sets 0*/
			int lockerUpgrades = pkt.GetChar(); //number of locker upgrades that have been done

			if (OnBankOpen != null)
				OnBankOpen(bankGold, lockerUpgrades);
		}

		public bool BankDeposit(int amount)
		{
			if (!m_client.ConnectedAndInitialized || !Initialized || amount < 0)
				return false;

			OldPacket pkt = new OldPacket(PacketFamily.Bank, PacketAction.Add);
			pkt.AddInt(amount);

			return m_client.SendPacket(pkt);
		}

		public bool BankWithdraw(int amount)
		{
			if (!m_client.ConnectedAndInitialized || !Initialized || amount < 0)
				return false;

			OldPacket pkt = new OldPacket(PacketFamily.Bank, PacketAction.Take);
			pkt.AddInt(amount);

			return m_client.SendPacket(pkt);
		}

		private void _handleBankReply(OldPacket pkt)
		{
			int characterGold = pkt.GetInt();
			int bankAmount = pkt.GetInt();

			if(OnBankChange != null)
				OnBankChange(characterGold, bankAmount);
		}
	}
}
