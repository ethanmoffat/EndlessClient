// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using EOLib.Net.Communication;
using Microsoft.Xna.Framework;

//disable "event not used" warnings for EnabledChanged and UpdateOrderChanged
#pragma warning disable 67

namespace EOLib.Net.Handlers
{
	public class PacketHandlerComponent : IGameComponent, IUpdateable
	{
		//the game will lag considerablly if this is too high.
		//todo: investigate making this a configurable option or move to Constants class
		private const int Max_Packets_Per_Update = 10;

		private readonly IPacketQueueProvider _packetQueueProvider;
		private readonly IPacketHandlerFinderService _packetHandlerFinder;

		private readonly List<IPacket> _unhandledPackets;

		public bool Enabled { get { return true; } }

		public int UpdateOrder { get { return 0; } }

		public event EventHandler<EventArgs> EnabledChanged;

		public event EventHandler<EventArgs> UpdateOrderChanged;

		public PacketHandlerComponent(IPacketQueueProvider packetQueueProvider,
									  IPacketHandlerFinderService packetHandlerFinder)
		{
			_packetQueueProvider = packetQueueProvider;
			_packetHandlerFinder = packetHandlerFinder;

			_unhandledPackets = new List<IPacket>(10);
		}

		public void Initialize() { }

		public void Update(GameTime gameTime)
		{
			if (PacketQueue.QueuedPacketCount == 0)
				return;

			_unhandledPackets.Clear();

			if (Max_Packets_Per_Update >= PacketQueue.QueuedPacketCount)
			{
				var packets = PacketQueue.DequeueAllPackets();
				foreach (var nextPacket in packets)
					FindAndHandlePacket(nextPacket);
			}
			else
			{
				for (int i = 0; i < Max_Packets_Per_Update && PacketQueue.QueuedPacketCount > 0; ++i)
				{
					var nextPacket = PacketQueue.DequeueFirstPacket();
					if (!FindAndHandlePacket(nextPacket))
						i -= 1;
				}
			}

			foreach (var packet in _unhandledPackets)
				PacketQueue.EnqueuePacketForHandling(packet);
		}

		private bool FindAndHandlePacket(IPacket packet)
		{
			if (!_packetHandlerFinder.HandlerExists(packet.Family, packet.Action))
			{
				_unhandledPackets.Add(packet);
				return false;
			}

			var handler = _packetHandlerFinder.FindHandler(packet.Family, packet.Action);
			if (!handler.CanHandle)
				return false;

			handler.HandlePacket(packet);
			return true;
		}

		private IPacketQueue PacketQueue { get { return _packetQueueProvider.PacketQueue; } }
	}
}
