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
			if (PacketQueue.QueuedPacketCount == 0)
				return;

			//For packets without handlers or that are only handled in-band (account, login, character, etc):
			//	store in list and re-queue at end of handling
			var unhandledPackets = new List<IPacket>();
			if (Max_Packets_Per_Update >= PacketQueue.QueuedPacketCount)
			{
				var packets = PacketQueue.DequeueAllPackets();
				foreach (var nextPacket in packets)
				{
					if (!_packetHandlerFinder.HandlerExists(nextPacket.Family, nextPacket.Action))
					{
						unhandledPackets.Add(nextPacket);
						continue;
					}

					var handler = _packetHandlerFinder.FindHandler(nextPacket.Family, nextPacket.Action);
					handler.HandlePacket(nextPacket);
				}
			}
			else
			{
				for (int i = 0; i < Max_Packets_Per_Update && PacketQueue.QueuedPacketCount > 0; ++i)
				{
					var nextPacket = PacketQueue.DequeueFirstPacket();
					if (!_packetHandlerFinder.HandlerExists(nextPacket.Family, nextPacket.Action))
					{
						i -= 1; //don't count this packet in handling counts for this update
						unhandledPackets.Add(nextPacket);
						continue;
					}

					var handler = _packetHandlerFinder.FindHandler(nextPacket.Family, nextPacket.Action);
					handler.HandlePacket(nextPacket);
				}
			}

			foreach (var packet in unhandledPackets)
				PacketQueue.EnqueuePacketForHandling(packet);
		}

		private IPacketQueue PacketQueue { get { return _packetQueueProvider.PacketQueue; } }
	}
}
