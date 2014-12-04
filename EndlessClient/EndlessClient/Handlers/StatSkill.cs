using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EOLib;

namespace EndlessClient.Handlers
{
	public enum TrainType
	{
		Stat = 1,
		Skill = 2
	}

	public static class StatSkill
	{
		public static bool AddStatPoint(short statID)
		{
			EOClient client = (EOClient) World.Instance.Client;
			if (client == null || !client.ConnectedAndInitialized)
				return false;

			Packet pkt = new Packet(PacketFamily.StatSkill, PacketAction.Add);
			pkt.AddChar((byte)TrainType.Stat);
			pkt.AddShort(statID);

			return client.SendPacket(pkt);
		}

		//response to StatSkill_Player packet
		public static void StatSkillPlayer(Packet pkt)
		{
			EndlessClient.Character c = World.Instance.MainPlayer.ActiveCharacter; //local ref so less typing
			c.Stats.statpoints = pkt.GetShort();
			c.Stats.SetStr(pkt.GetShort());
			c.Stats.SetInt(pkt.GetShort());
			c.Stats.SetWis(pkt.GetShort());
			c.Stats.SetAgi(pkt.GetShort());
			c.Stats.SetCon(pkt.GetShort());
			c.Stats.SetCha(pkt.GetShort());
			c.Stats.SetMaxHP(pkt.GetShort());
			c.Stats.SetMaxTP(pkt.GetShort());
			c.Stats.maxsp = pkt.GetShort();
			c.MaxWeight = (byte)pkt.GetShort();
			c.Stats.SetMinDam(pkt.GetShort());
			c.Stats.SetMaxDam(pkt.GetShort());
			c.Stats.SetAccuracy(pkt.GetShort());
			c.Stats.SetEvade(pkt.GetShort());
			c.Stats.SetArmor(pkt.GetShort());
			EOGame.Instance.Hud.RefreshStats();
		}
	}
}
