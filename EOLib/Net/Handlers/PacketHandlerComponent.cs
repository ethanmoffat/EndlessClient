// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EOLib.Net.Communication;
using Microsoft.Xna.Framework;

//disable "event not used" warnings for EnabledChanged and UpdateOrderChanged
#pragma warning disable 67

namespace EOLib.Net.Handlers
{
	public class PacketHandlerComponent : IGameComponent, IUpdateable
	{
		private readonly IPacketQueueProvider _packetQueueProvider;
		private readonly IPacketHandlerFinderService _packetHandlerFinder;

		public bool Enabled { get { return true; } }

		public int UpdateOrder { get { return 0; } }

		public event EventHandler<EventArgs> EnabledChanged;

		public event EventHandler<EventArgs> UpdateOrderChanged;

		public PacketHandlerComponent(IPacketQueueProvider packetQueueProvider,
									  IPacketHandlerFinderService packetHandlerFinder)
		{
			_packetQueueProvider = packetQueueProvider;
			_packetHandlerFinder = packetHandlerFinder;
		}

		public void Initialize() { }

		public void Update(GameTime gameTime)
		{
			if (OutOfBandPacketQueue.QueuedPacketCount == 0)
				return;

			if (Constants.OutOfBand_Packets_Handled_Per_Update >= OutOfBandPacketQueue.QueuedPacketCount)
			{
				var packets = OutOfBandPacketQueue.DequeueAllPackets();
				foreach (var nextPacket in packets)
					FindAndHandlePacket(nextPacket);
			}
			else
			{
				for (int i = 0; i < Constants.OutOfBand_Packets_Handled_Per_Update && OutOfBandPacketQueue.QueuedPacketCount > 0; ++i)
				{
					var nextPacket = OutOfBandPacketQueue.DequeueFirstPacket();
					if (!FindAndHandlePacket(nextPacket))
						i -= 1;
				}
			}
		}

		private bool FindAndHandlePacket(IPacket packet)
		{
			if (!_packetHandlerFinder.HandlerExists(packet.Family, packet.Action))
				return false;

			var handler = _packetHandlerFinder.FindHandler(packet.Family, packet.Action);
			if (!handler.CanHandle)
				return false;

			handler.HandlePacket(packet);
			return true;
		}

		private IPacketQueue OutOfBandPacketQueue { get { return _packetQueueProvider.HandleOutOfBandPacketQueue; } }
	}
}
