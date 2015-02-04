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
	}
}
