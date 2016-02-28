// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using EOLib.Net.PacketProcessing;

namespace EOLib.Net.Communication
{
	public class NetworkClient : INetworkClient<IPacketQueue>
	{
		private readonly IPacketProcessorActions _packetProcessActions;
		private readonly IPacketEncoderService _packetEncoderService;

		private readonly IAsyncSocket _socket;

		private readonly CancellationTokenSource _backgroundReceiveCTS;
		private readonly Thread _receiveThread;

		public IPacketQueue PacketQueue { get; private set; }

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

		public NetworkClient(IPacketQueue queue,
							 IPacketProcessorActions packetProcessActions)
		{
			PacketQueue = queue;
			_packetProcessActions = packetProcessActions;
			_packetEncoderService = new PacketEncoderService();

			_socket = new AsyncSocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			
			_backgroundReceiveCTS = new CancellationTokenSource();
			_receiveThread = new Thread(BackgroundReceiveThread);
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

		public void StartBackgroundReceiveLoop()
		{
			_receiveThread.Start();
		}

		public void CancelBackgroundReceiveLoop()
		{
			_backgroundReceiveCTS.Cancel();
			_receiveThread.Join();
		}

		private async void BackgroundReceiveThread()
		{
			while (!_backgroundReceiveCTS.IsCancellationRequested)
			{
				var lengthData = await _socket.ReceiveAsync(2, _backgroundReceiveCTS.Token);
				var length = _packetEncoderService.DecodeNumber(lengthData);

				var packetData = await _socket.ReceiveAsync(length, _backgroundReceiveCTS.Token);
				var packet = _packetProcessActions.DecodeData((IEnumerable<byte>) packetData);

				PacketQueue.EnqueuePacketForHandling(packet);
			}
		}

		public int Send(IPacket packet)
		{
			var sendTask = SendAsync(packet);
			return sendTask.Result;
		}

		public async Task<int> SendAsync(IPacket packet, int timeout = 5000)
		{
			var bytesToSend = _packetProcessActions.EncodePacket(packet);
			return await _socket.SendAsync(bytesToSend, CancellationToken.None);
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
