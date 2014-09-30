using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EOLib;

namespace EndlessClient.Handlers
{
	public static class Players
	{
		/// <summary>
		/// Handles PLAYERS_AGREE packet which is sent when a player enters a map by warp or upon spawning
		/// </summary>
		public static void PlayersAgree(Packet pkt)
		{
			if (pkt.GetByte() != 255)
				return;
			string charName = pkt.GetBreakString();
			if (charName.Length > 1)
				charName = char.ToUpper(charName[0]) + charName.Substring(1);
			short id = pkt.GetShort();

			EndlessClient.Character newGuy = new EndlessClient.Character(id, null);
			newGuy.Name = charName;
			newGuy.CurrentMap = pkt.GetShort();
			newGuy.X = pkt.GetShort();
			newGuy.Y = pkt.GetShort();

			EODirection direction = (EODirection) pkt.GetChar();
			pkt.GetChar(); //value is always 6? unknown
			newGuy.PaddedGuildTag = pkt.GetFixedString(3);

			newGuy.RenderData = new CharRenderData
			{
				facing = direction,
				level = pkt.GetChar(),
				gender = pkt.GetChar(),
				hairstyle = pkt.GetChar(),
				haircolor = pkt.GetChar(),
				race = pkt.GetChar()
			};

			newGuy.Stats = new CharStatData
			{
				maxhp = pkt.GetShort(),
				hp = pkt.GetShort(),
				maxtp = pkt.GetShort(),
				tp = pkt.GetShort()
			};
			
			newGuy.RenderData.SetBoots(pkt.GetShort());
			pkt.Skip(3 * sizeof(short)); //other paperdoll data is 0'd out
			newGuy.RenderData.SetArmor(pkt.GetShort());
			pkt.Skip(sizeof(short));
			newGuy.RenderData.SetHat(pkt.GetShort());
			newGuy.RenderData.SetShield(pkt.GetShort());
			newGuy.RenderData.SetWeapon(pkt.GetShort());

			newGuy.RenderData.SetSitting((SitState)pkt.GetChar());
			newGuy.RenderData.SetHidden(pkt.GetChar() != 0);

			WarpAnimation anim = (WarpAnimation)pkt.GetChar();
			if (pkt.GetByte() != 255)
				return;
			if (pkt.GetChar() != 1) //0 for NPC, 1 for player
				return;

			World.Instance.ActiveMapRenderer.AddOtherPlayer(newGuy, anim);
		}
	}
}
