using EOLib;

namespace EndlessClient.Handlers
{
	public static class Effect
	{
		public static void EffectPlayer(Packet pkt)
		{
			short player = pkt.GetShort();
			int effectID = pkt.GetThree();

			World.Instance.ActiveMapRenderer.OtherPlayerEffect(player, effectID);
		}
	}
}
