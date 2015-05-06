using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using EOLib;
using EOLib.Net;

namespace EOBot
{
	public static class Win32
	{
		public delegate bool ConsoleCtrlDelegate(CtrlTypes ctrlType);

		public enum CtrlTypes : uint
		{
			CTRL_C_EVENT = 0,
			CTRL_BREAK_EVENT,
			CTRL_CLOSE_EVENT,
			CTRL_LOGOFF_EVENT = 5,
			CTRL_SHUTDOWN_EVENT
		}

		[DllImport("kernel32.dll")]
		public static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate handlerRoutine, bool add);
	}

	static class Program
	{
		private static List<EOBot> BotList;

		static void Main(string[] args)
		{
			if (args.Length != 3)
			{
				Console.WriteLine("Invalid: specify host, port, and the number of bots to run");
				Console.WriteLine("Usage: EOBot.exe <host> <port> <numbots>");
				return;
			}

			string host = args[0];
			ushort port;
			if (!ushort.TryParse(args[1], out port))
			{
				Console.WriteLine("Invalid: port number could not be parsed!");
				return;
			}

			int numBots;
			if (!int.TryParse(args[2], out numBots))
			{
				Console.WriteLine("Invalid: specify an integer argument");
				return;
			}
			if (numBots > 25)
			{
				Console.WriteLine("Invalid: unable to launch > 25 threads of execution. Please use 25 or less.");
				return;
			}
			if (numBots < 1)
			{
				Console.WriteLine("Invalid: unable to launch < 1 thread of execution. Please use 1 or more.");
				return;
			}

			Console.WriteLine("Starting bots...");
			using (Semaphore SignalDone = new Semaphore(numBots, numBots))
			{
				BotList = new List<EOBot>(numBots);
				for (int i = 0; i < numBots; ++i)
				{
					SignalDone.WaitOne();
					
					EOBot bot;
					try
					{
						bot = new EOBot(i, host, port);
					}
					catch (ArgumentException)
					{
						Console.WriteLine("Unable to connect to server! Host={0} Port={1}", host, port);
						return;
					}
					catch (TimeoutException)
					{
						Console.WriteLine("Failed initialization handshake with server! Bot num={0}", i + 1);
						return;
					}
					catch (InvalidOperationException)
					{
						Console.WriteLine("Invalid response from server or connection failed! Must receive an OK reply.");
						return;
					}

// ReSharper disable once AccessToDisposedClosure
					bot.OnWorkCompleted += () => SignalDone.Release(); //this will not be disposed, as it is waited on below
					bot.Run();
					BotList.Add(bot);
					Console.WriteLine("Bot {0} created. Sleeping 1100ms.", i + 1);
					Thread.Sleep(1100); //minimum for this is 1sec server-side
				}

				Console.WriteLine("All bots created. Waiting for termination (press CTRL+C to end early)");

				Win32.SetConsoleCtrlHandler(type =>
				{
					foreach (EOBot bot in BotList)
					{
						bot.Terminate();
					}
					return true;
				}, true);

				//wait for each of the bots to complete work
				for (int i = 0; i < numBots; ++i)
				{
					SignalDone.WaitOne();
				}
			}

			Console.WriteLine("All threads completed.");
			foreach (EOBot bot in BotList)
			{
				bot.Dispose();
			}
		}
	}

	static class Names
	{
		private static readonly string[] namesArray =
		{
			"AlphaAA",
			"BravoBB",
			"Charlie",
			"DeltaDD",
			"EchoEE",
			"Foxtrot",
			"GolfGG",
			"HotelHH",
			"IndiaII",
			"Juliett",
			"KiloKK",
			"LimaLL",
			"MikeMM",
			"November",
			"OscarOO",
			"PapaPO",
			"Quebec",
			"RomeoRR",
			"Sierra",
			"TangoTT",
			"Uniform",
			"Victor",
			"Whiskey",
			"XrayXX",
			"Yankee"
		};

		public static string Get(int index)
		{
			if (index < 0 || index >= namesArray.Length)
				index = 0;
			return namesArray[index];
		}

		public static string Rand()
		{
			Random gen = new Random();
			int len = gen.Next(5, 11);
			string ret = "";
			for (int i = 0; i < len; ++i)
			{
				ret += Convert.ToChar(Convert.ToInt32('a') + gen.Next(0, 25));
			}
			return ret;
		}
	}

	class EOBot : IDisposable
	{
		private EOClient m_client;
		private PacketAPI m_api;
		private bool m_terminateWasCalled;
		private readonly int m_index;
		private AutoResetEvent m_terminationEvent;

		public EOBot(int index, string host, int port)
		{
			m_index = index + 1;

			m_client = new EOClient();
			if (!m_client.ConnectToServer(host, port))
				throw new ArgumentException();
			m_api = new PacketAPI(m_client);

			InitData data;
			if (!m_api.Initialize(0, 0, 28, EOLib.Win32.GetHDDSerial(), out data))
				throw new TimeoutException();
			m_client.SetInitData(data);

			if (!m_api.ConfirmInit(data.emulti_e, data.emulti_d, data.clientID))
				throw new TimeoutException();

			if (!m_api.Initialized || !m_client.ConnectedAndInitialized || data.ServerResponse != InitReply.INIT_OK)
				throw new InvalidOperationException();

			m_terminationEvent = new AutoResetEvent(false);
		}

		public void Run()
		{
			Thread t = new Thread(_doWork);
			t.Start();
		}

		private void _doWork()
		{
			string name = Names.Get(m_index - 1);
			
			//create account if needed
			AccountReply accReply;
			bool res = m_api.AccountCheckName(name, out accReply);
			if (res && accReply != AccountReply.Exists)
			{
				if (m_terminateWasCalled)
					return;
				Thread.Sleep(500);

				if (m_api.AccountCreate(name, name, name + " " + name, "COMPY-" + name, name + "@BOT.COM", EOLib.Win32.GetHDDSerial(), out accReply))
				{
					Console.WriteLine("Created account {0}", name);
				}
				else
				{
					_errorMessage();
					return;
				}
			}
			else if(!res)
			{
				_errorMessage();
				return;
			}

			if (m_terminateWasCalled) return;
			Thread.Sleep(500);

			//log in
			LoginReply loginReply;
			CharacterRenderData[] loginData;
			res = m_api.LoginRequest(name, name, out loginReply, out loginData);
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

			if (m_terminateWasCalled) return;
			Thread.Sleep(500);

			//create character if needed
			if (loginData == null || loginData.Length == 0)
			{
				CharacterReply charReply;
				res = m_api.CharacterRequest(out charReply);

				if (!res || charReply != CharacterReply.Ok)
				{
					_errorMessage("Character create request failed");
					return;
				}

				Random gen = new Random();
				res = m_api.CharacterCreate((byte)gen.Next(1), (byte)gen.Next(0, 20), (byte)gen.Next(0, 9), (byte)gen.Next(0, 5), name, out charReply, out loginData);
				if (!res || charReply != CharacterReply.Ok || loginData == null || loginData.Length == 0)
				{
					_errorMessage("Character create failed");
					return;
				}

				Console.WriteLine("Created character {0}", name);

				if (m_terminateWasCalled) return;
				Thread.Sleep(500);
			}

			WelcomeRequestData welcomeReqData;
			res = m_api.SelectCharacter(loginData[0].ID, out welcomeReqData);
			if (!res)
			{
				_errorMessage();
				return;
			}

			//should check file status for maps and pubs here

			if (m_terminateWasCalled) return;
			Thread.Sleep(500);

			WelcomeMessageData welcomeMsgData;
			res = m_api.WelcomeMessage(welcomeReqData.ActiveCharacterID, out welcomeMsgData);
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
				PartyRequest(m_api, charlist[testuserNdx].ID);
			}

			Timer speechTimer = new Timer(state => m_api.Speak(TalkType.Local, name + " standing by!"), null, rand.Next(15000, 30000), rand.Next(15000, 30000));
			m_terminationEvent.WaitOne();
			speechTimer.Dispose();
		}

		private void _errorMessage(string msg = null)
		{
			if (msg == null)
			{
				Console.WriteLine("Error contacting server - server did not respond. Quitting {0}", m_index);
				return;
			}

			Console.WriteLine("Error - {0}. Quitting {1}.", msg, m_index);
		}

		public void Terminate()
		{
			m_terminateWasCalled = true;
			m_terminationEvent.Set();
			OnWorkCompleted();
		}

		public event Action OnWorkCompleted;

		public void Dispose()
		{
			if (m_client != null)
			{
				m_client.Dispose();
				m_client = null;
			}
			if (m_api != null)
			{
				m_api.Dispose();
				m_api = null;
			}
			if (m_terminationEvent != null)
			{
				m_terminationEvent.Dispose();
				m_terminationEvent = null;
			}
			if (s_partyEvent != null)
			{
				s_partyEvent.Dispose();
				s_partyEvent = null;
			}
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
