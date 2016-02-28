// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using Microsoft.Xna.Framework;

namespace EOLib.Net.Communication
{
	public class PacketHandlerComponent<TQueue> : GameComponent
		where TQueue : IPacketQueue
	{
		private class NoGame : Game { }

		private readonly IPacketQueue _packetQueue;

		public PacketHandlerComponent(TQueue packetQueue)
			: base(new NoGame())
		{
			_packetQueue = packetQueue;
		}

		public override void Update(GameTime gameTime)
		{
			if (_packetQueue.QueuedPacketCount == 0)
				return;

			var nextPacket = _packetQueue.DequeueFirstPacket();
			//todo: packet handling scheme. needs some design thinking

			base.Update(gameTime);
		}
	}
}
