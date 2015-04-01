using EOLib;

namespace EndlessClient.Handlers
{
	public static class Recover
	{
		public static void RecoverPlayer(Packet pkt)
		{
			World.Instance.MainPlayer.ActiveCharacter.Stats.SetHP(pkt.GetShort());
			World.Instance.MainPlayer.ActiveCharacter.Stats.SetTP(pkt.GetShort());
			EOGame.Instance.Hud.RefreshStats();
		}

		public static void RecoverReply(Packet pkt)
		{
			World.Instance.MainPlayer.ActiveCharacter.Stats.exp   = pkt.GetInt();
			World.Instance.MainPlayer.ActiveCharacter.Stats.karma = pkt.GetShort();
			byte level = pkt.GetChar();
			if(level > 0)
				World.Instance.MainPlayer.ActiveCharacter.Stats.level = level;

			EOGame.Instance.Hud.RefreshStats();
		}

		public static void RecoverList(Packet pkt)
		{
			//almost identical to STATSKILL_PLAYER packet
			CharStatData localStats = World.Instance.MainPlayer.ActiveCharacter.Stats;
			World.Instance.MainPlayer.ActiveCharacter.Class = (byte)pkt.GetShort();
			localStats.SetStr(pkt.GetShort());
			localStats.SetInt(pkt.GetShort());
			localStats.SetWis(pkt.GetShort());
			localStats.SetAgi(pkt.GetShort());
			localStats.SetCon(pkt.GetShort());
			localStats.SetCha(pkt.GetShort());
			localStats.SetMaxHP(pkt.GetShort());
			localStats.SetMaxTP(pkt.GetShort());
			localStats.SetMaxSP(pkt.GetShort());
			World.Instance.MainPlayer.ActiveCharacter.MaxWeight = (byte)pkt.GetShort();
			localStats.SetMinDam(pkt.GetShort());
			localStats.SetMaxDam(pkt.GetShort());
			localStats.SetAccuracy(pkt.GetShort());
			localStats.SetEvade(pkt.GetShort());
			localStats.SetArmor(pkt.GetShort());
			EOGame.Instance.Hud.RefreshStats();
		}

		public static void RecoverAgree(Packet pkt)
		{
			//when a heal item is used
			short playerID = pkt.GetShort();
			int hpGain = pkt.GetInt();
			byte playerPctHealth = pkt.GetChar();

			World.Instance.ActiveMapRenderer.OtherPlayerHeal(playerID, hpGain, playerPctHealth);
		}
	}
}
