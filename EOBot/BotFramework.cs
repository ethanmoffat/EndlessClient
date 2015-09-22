using System;
using System.Collections.Generic;
using System.Threading;

namespace EOBot
{
	sealed class BotFramework : IDisposable
	{
		private readonly List<IBot> _botsList;
		private Semaphore _doneSignal;
		private int _numBots;
		private bool _initialized;

		public event Action BotsRunning;
		public event Action<int> BotInitSuccess;
		public event Action<string> BotInitFailed;

		public BotFramework()
		{
			_botsList = new List<IBot>(25);
		}

		public void Initialize(int numBots, string host, ushort port)
		{
			_numBots = numBots;
			_doneSignal = new Semaphore(_numBots, _numBots);

			for (int i = 0; i < _numBots; ++i)
			{
				_doneSignal.WaitOne();

				try
				{
					IBot bot = new PartyBot(i, host, port);
					bot.WorkCompleted += () => _doneSignal.Release(); //this will not be disposed, as it is waited on below
					bot.Initialize();
					_botsList.Add(bot);
				}
				catch(Exception ex)
				{
					FireBotInitFailed(ex.Message);
					continue;
				}

				FireBotInitSuccess(i);
				Thread.Sleep(1100); //minimum for this is 1sec server-side
			}

			FireAllBotsCreated();
			_initialized = true;
		}

		private void FireBotInitFailed(string message)
		{
			if (BotInitFailed != null)
				BotInitFailed(message);
		}

		private void FireBotInitSuccess(int ndx)
		{
			if (BotInitSuccess != null)
				BotInitSuccess(ndx);
		}

		private void FireAllBotsCreated()
		{
			if (BotsRunning != null)
				BotsRunning();
		}

		public void Run()
		{
			if(!_initialized)
				throw new InvalidOperationException("Must call Initialize() before running!");

			_botsList.ForEach(_bot => _bot.Run(true));
		}

		public void TerminateBots()
		{
			if (!_initialized) return;

			_botsList.ForEach(_bot => _bot.Terminate());
		}

		public void WaitForCompletion()
		{
			if (!_initialized) return;

			for (int i = 0; i < _numBots; ++i)
			{
				_doneSignal.WaitOne();
			}
		}

		~BotFramework()
		{
			Dispose(false);
			GC.SuppressFinalize(this);
		}

		public void Dispose()
		{
			Dispose(true);
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				_botsList.ForEach(bot => bot.Dispose());
				_doneSignal.Dispose();
			}
		}
	}
}
