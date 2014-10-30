
using EOLib;

namespace EndlessClient.Handlers
{
	public static class Avatar
	{
		/// <summary>
		/// Remove a player from view (sent by server when someone is out of range)
		/// </summary>
		public static void AvatarRemove(Packet pkt)
		{
			short id = pkt.GetShort();
			World.Instance.ActiveMapRenderer.RemoveOtherPlayer(id);
		}
	}
}
