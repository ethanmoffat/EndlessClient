
using EOLib;

namespace EndlessClient.Handlers
{
	enum AvatarSlot : byte
	{
		Clothes = 1,
		Hair = 2,
		HairColor = 3
	};

	public static class Avatar
	{
		/// <summary>
		/// Remove a player from view (sent by server when someone is out of range)
		/// </summary>
		public static void AvatarRemove(Packet pkt)
		{
			short id = pkt.GetShort();
			WarpAnimation anim = (WarpAnimation) (pkt.Length > pkt.ReadPos ? pkt.GetChar() : 0);
			World.Instance.ActiveMapRenderer.RemoveOtherPlayer(id, anim);
		}

		/// <summary>
		/// Player changes appearance (clothes, hair, etc)
		/// </summary>>
		public static void AvatarAgree(Packet pkt)
		{
			short playerID = pkt.GetShort();
			AvatarSlot slot = (AvatarSlot) pkt.GetChar();
			switch (slot)
			{
				case AvatarSlot.Clothes:
				{
					bool sound = pkt.GetChar() == 0;
					CharRenderData newRenderData = new CharRenderData
					{
						boots = pkt.GetShort(),
						armor = pkt.GetShort(),
						hat = pkt.GetShort(),
						weapon = pkt.GetShort(),
						shield = pkt.GetShort()
					};
					World.Instance.ActiveMapRenderer.UpdateOtherPlayer(playerID, sound, newRenderData);
				}
					break;
				case AvatarSlot.Hair:
				{
					if (pkt.GetChar() != 0) return; //subloc -- huh?
					byte style = pkt.GetChar();
					byte color = pkt.GetChar();
					World.Instance.ActiveMapRenderer.UpdateOtherPlayer(playerID, color, style);
				}
					break;
				case AvatarSlot.HairColor:
				{
					if (pkt.GetChar() != 0) return; //subloc -- huh?
					byte color = pkt.GetChar();
					World.Instance.ActiveMapRenderer.UpdateOtherPlayer(playerID, color);
				}
					break;
			}
		}
	}
}
