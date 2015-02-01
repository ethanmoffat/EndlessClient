using EOLib;

namespace EndlessClient.Handlers
{
	public static class AdminInteract
	{
		/// <summary>
		/// Handles ADMININTERACT_REMOVE which hides a shown admin
		/// </summary>
		public static void AdminHide(Packet pkt)
		{
			short id = pkt.GetShort();
			if (World.Instance.MainPlayer.ActiveCharacter.ID == id)
				World.Instance.MainPlayer.ActiveCharacter.RenderData.SetHidden(true);
			else
				World.Instance.ActiveMapRenderer.OtherPlayerHide(id, true);
		}

		/// <summary>
		/// Handles ADMININTERACT_AGREE which shows a hidden admin again
		/// </summary>
		public static void AdminShow(Packet pkt)
		{
			short id = pkt.GetShort();
			if (World.Instance.MainPlayer.ActiveCharacter.ID == id)
				World.Instance.MainPlayer.ActiveCharacter.RenderData.SetHidden(false);
			else
				World.Instance.ActiveMapRenderer.OtherPlayerHide(id, false);
		}
	}
}
