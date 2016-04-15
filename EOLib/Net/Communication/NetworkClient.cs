// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using EOLib.Data;
using EOLib.Net.PacketProcessing;

namespace EOLib.Net.Communication
{
	public class NetworkClient : INetworkClient
	{
		private readonly IPacketQueueProvider _packetQueueProvider;
		private readonly IPacketProcessorActions _packetProcessActions;
		private readonly INumberEncoderService _numberEncoderService;

		private readonly IAsyncSocket _socket;

		private readonly CancellationTokenSource _backgroundReceiveCTS;
		
		public bool Connected
		{
			get
			{
				using (var cts = new CancellationTokenSource())
				{
					cts.CancelAfter(5000);
					var t = _socket.CheckIsConnectedAsync(cts.Token);
					return !t.IsCanceled && t.Result;
				}
			}
		}

		public NetworkClient(IPacketQueueProvider packetQueueProvider,
							 IPacketProcessorActions packetProcessActions,
							 INumberEncoderService numberEncoderService)
		{
			_packetQueueProvider = packetQueueProvider;
			_packetProcessActions = packetProcessActions;
			_numberEncoderService = numberEncoderService;

			_socket = new AsyncSocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			
			_backgroundReceiveCTS = new CancellationTokenSource();
		}

		public async Task<ConnectResult> ConnectToServer(string host, int port)
		{
			IPAddress ip;
			if (!IPAddress.TryParse(host, out ip))
			{
				var addressList = Dns.GetHostEntry(host).AddressList;
				var ipv4Addresses = Array.FindAll(addressList, a => a.AddressFamily == AddressFamily.InterNetwork);

				if (ipv4Addresses.Length == 0)
					return ConnectResult.InvalidEndpoint;

				ip = ipv4Addresses[0];
			}

			var endPoint = new IPEndPoint(ip, port);
			using (var cts = new CancellationTokenSource())
			{
				cts.CancelAfter(5000);
				
				var task = _socket.ConnectAsync(endPoint, cts.Token);
				await task;

				return task.IsCanceled ? ConnectResult.Timeout : task.Result;
			}
		}

		public void Disconnect()
		{
			_socket.DisconnectAsync(CancellationToken.None);
		}

		public async Task RunReceiveLoopAsync()
		{
			while (!_backgroundReceiveCTS.IsCancellationRequested)
			{
				var lengthData = await _socket.ReceiveAsync(2, _backgroundReceiveCTS.Token);
				if (_backgroundReceiveCTS.IsCancellationRequested)
					break;

				var length = _numberEncoderService.DecodeNumber(lengthData);

				var packetData = await _socket.ReceiveAsync(length, _backgroundReceiveCTS.Token);
				if (_backgroundReceiveCTS.IsCancellationRequested)
					break;

				var packet = _packetProcessActions.DecodeData((IEnumerable<byte>) packetData);
				_packetQueueProvider.PacketQueue.EnqueuePacketForHandling(packet);
			}
		}

		public void CancelBackgroundReceiveLoop()
		{
			_backgroundReceiveCTS.Cancel();
		}

		public int Send(IPacket packet)
		{
			var sendTask = SendAsync(packet);
			return sendTask.Result;
		}

		public async Task<int> SendAsync(IPacket packet, int timeout = 500)
		{
			var bytesToSend = _packetProcessActions.EncodePacket(packet);
			using (var cts = new CancellationTokenSource(timeout))
				return await _socket.SendAsync(bytesToSend, cts.Token);
		}

		public async Task<int> SendRawPacketAsync(IPacket packet, int timeout = 500)
		{
			var bytesToSend = _packetProcessActions.EncodeRawPacket(packet);
			using (var cts = new CancellationTokenSource(timeout))
				return await _socket.SendAsync(bytesToSend, cts.Token);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~NetworkClient()
		{
			Dispose(false);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Connected)
				{
					CancelBackgroundReceiveLoop();
					Disconnect();
				}

				_backgroundReceiveCTS.Dispose();
				_socket.Dispose();
			}
		}
	}
}
