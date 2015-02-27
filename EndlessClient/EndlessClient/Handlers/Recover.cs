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
	}
}
