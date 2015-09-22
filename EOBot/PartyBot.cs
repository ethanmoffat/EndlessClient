using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EOLib.Net;

namespace EOBot
{
	/// <summary>
	/// Represents a bot that connects to a server and attempts to join the party of someone on the map
	/// </summary>
	class PartyBot : BotBase
	{
		private readonly string _host;
		private readonly int _port;
		private readonly int _index;

		private EOClient _client;
		private PacketAPI _api;

		public PartyBot(int index, string host, int port)
		{
			_host = host;
			_port = port;
			_index = index + 1;
		}

		public override void Initialize()
		{
			base.Initialize();

			_client = new EOClient();
			if (!_client.ConnectToServer(_host, _port))
				throw new ArgumentException(string.Format("Bot {2}: Unable to connect to server! Host={0} Port={1}", _host, _port, _index));
			_api = new PacketAPI(_client);

			InitData data;
			if (!_api.Initialize(0, 0, 28, EOLib.Win32.GetHDDSerial(), out data))
				throw new TimeoutException(string.Format("Bot {0}: Failed initialization handshake with server!", _index));
			_client.SetInitData(data);

			if (!_api.ConfirmInit(data.emulti_e, data.emulti_d, data.clientID))
				throw new TimeoutException(string.Format("Bot {0}: Failed initialization handshake with server!", _index));

			if (!_api.Initialized || !_client.ConnectedAndInitialized || data.ServerResponse != InitReply.INIT_OK)
				throw new InvalidOperationException(string.Format("Bot {0}: Invalid response from server or connection failed! Must receive an OK reply.", _index));
		}

		protected override void DoWork(CancellationToken ct)
		{
			string name = NamesList.Get(_index - 1);

			//create account if needed
			AccountReply accReply;
			bool res = _api.AccountCheckName(name, out accReply);
			if (res && accReply != AccountReply.Exists)
			{
				if (ct.IsCancellationRequested)
					return;
				Thread.Sleep(500);

				if (_api.AccountCreate(name, name, name + " " + name, "COMPY-" + name, name + "@BOT.COM", EOLib.Win32.GetHDDSerial(), out accReply))
				{
					Console.WriteLine("Created account {0}", name);
				}
				else
				{
					_errorMessage();
					return;
				}
			}
			else if (!res)
			{
				_errorMessage();
				return;
			}

			if (ct.IsCancellationRequested) return;
			Thread.Sleep(500);

			//log in
			LoginReply loginReply;
			CharacterRenderData[] loginData;
			res = _api.LoginRequest(name, name, out loginReply, out loginData);
			if (!res)
			{
				_errorMessage();
				return;
			}
			if (loginReply != LoginReply.Ok)
			{
				_errorMessage("Login reply was invalid");
				return;
			}

			if (ct.IsCancellationRequested) return;
			Thread.Sleep(500);

			//create character if needed
			if (loginData == null || loginData.Length == 0)
			{
				CharacterReply charReply;
				res = _api.CharacterRequest(out charReply);

				if (!res || charReply != CharacterReply.Ok)
				{
					_errorMessage("Character create request failed");
					return;
				}

				Random gen = new Random();
				res = _api.CharacterCreate((byte)gen.Next(1), (byte)gen.Next(0, 20), (byte)gen.Next(0, 9), (byte)gen.Next(0, 5), name, out charReply, out loginData);
				if (!res || charReply != CharacterReply.Ok || loginData == null || loginData.Length == 0)
				{
					_errorMessage("Character create failed");
					return;
				}

				Console.WriteLine("Created character {0}", name);

				if (ct.IsCancellationRequested) return;
				Thread.Sleep(500);
			}

			WelcomeRequestData welcomeReqData;
			res = _api.SelectCharacter(loginData[0].ID, out welcomeReqData);
			if (!res)
			{
				_errorMessage();
				return;
			}

			//should check file status for maps and pubs here

			if (ct.IsCancellationRequested) return;
			Thread.Sleep(500);

			WelcomeMessageData welcomeMsgData;
			res = _api.WelcomeMessage(welcomeReqData.ActiveCharacterID, out welcomeMsgData);
			if (!res)
			{
				_errorMessage();
				return;
			}

			Console.WriteLine("{0} logged in and executing.", name);
			//should be logged in and good to go at this point

			Random rand = new Random();

			var charlist = welcomeMsgData.CharacterData.ToList();
			int testuserNdx = charlist.FindIndex(_data => _data.Name.ToLower() == "testuser");
			if (testuserNdx >= 0)
			{
				//testing party stuff - large parties
				PartyRequest(_api, charlist[testuserNdx].ID);
			}

			Timer speechTimer = new Timer(state => _api.Speak(TalkType.Local, name + " standing by!"), null, rand.Next(15000, 30000), rand.Next(15000, 30000));
			speechTimer.Dispose();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_client != null)
				{
					_client.Dispose();
					_client = null;
				}
				if (_api != null)
				{
					_api.Dispose();
					_api = null;
				}
				if (s_partyEvent != null)
				{
					s_partyEvent.Dispose();
					s_partyEvent = null;
				}
			}

			base.Dispose(disposing);
		}

		private void _errorMessage(string msg = null)
		{
			if (msg == null)
			{
				Console.WriteLine("Error contacting server - server did not respond. Quitting {0}", _index);
				return;
			}

			Console.WriteLine("Error - {0}. Quitting {1}.", msg, _index);
		}

		private static AutoResetEvent s_partyEvent = new AutoResetEvent(true);
		private static void PartyRequest(PacketAPI api, short id)
		{
			s_partyEvent.WaitOne();
			Action<List<PartyMember>> action = _list => s_partyEvent.Set();

			api.OnPartyDataRefresh += action;
			api.OnPartyDataRefresh += t => api.OnPartyDataRefresh -= action;

			api.PartyRequest(PartyRequestType.Join, id);
		}
	}
}
