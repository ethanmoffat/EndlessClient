using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EOLib.Net
{
	partial class PacketAPI
	{
		public event Action<int> OnPlaySoundEffect;

		private void _createMusicMembers()
		{
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Music, PacketAction.Player), _handleMusicPlayer, true);
		}

		private void _handleMusicPlayer(Packet pkt)
		{
			if (OnPlaySoundEffect != null)
				OnPlaySoundEffect(pkt.GetChar());
		}
	}
}
