// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Threading;

namespace EOBot
{
    sealed class BotFramework : IDisposable
    {
        public const int NUM_BOTS_MAX = 25;

        private readonly List<IBot> _botsList;
        private readonly IBotFrameworkOutputHandler _outputHandler;
        private readonly string _host;
        private readonly ushort _port;

        private Semaphore _doneSignal;
        private bool _initialized;
        private int _numBots;
        private bool _terminating;

        public BotFramework(IBotFrameworkOutputHandler outputHandler, ArgumentsParser parsedArgs)
        {
            _outputHandler = outputHandler;
            if(outputHandler == null || parsedArgs == null)
                throw new ArgumentNullException("One or more arguments to framework is null", new Exception());

            var numberOfBots = parsedArgs.NumBots;
            var simultaneousBots = parsedArgs.SimultaneousBots;
            var host = parsedArgs.Host;
            var port = parsedArgs.Port;

            if (numberOfBots > NUM_BOTS_MAX || simultaneousBots > NUM_BOTS_MAX || simultaneousBots > numberOfBots)
                throw new ArgumentException("Too many bots requested");

            if (numberOfBots <= 0 || simultaneousBots <= 0)
                throw new ArgumentException("Not enough bots requested");

            _numBots = numberOfBots;
            _host = host;
            _port = port;

            _botsList = new List<IBot>(numberOfBots);

            _doneSignal = new Semaphore(simultaneousBots, simultaneousBots);
        }

        public void Initialize(IBotFactory botFactory, int delayBetweenInitsMS = 1100)
        {
            if (_initialized)
                throw new InvalidOperationException("Unable to initialize bot framework a second time.");

            int numFailed = 0;
            for (int i = 0; i < _numBots; ++i)
            {
                if (_terminating)
                    throw new BotException("Received termination signal; initialization has been cancelled");

                try
                {
                    var bot = botFactory.CreateBot(i, _host, _port);
                    bot.WorkCompleted += () => _doneSignal.Release();
                    bot.Initialize();
                    _botsList.Add(bot);
                }
                catch(Exception ex)
                {
                    _outputHandler.OutputBotInitializationFailed(ex.Message);
                    numFailed++;
                    continue;
                }

                _outputHandler.OutputBotInitializationSucceeded(i);
                Thread.Sleep(delayBetweenInitsMS); //minimum for this is 1sec server-side
            }

            if (numFailed > 0)
            {
                _outputHandler.OutputWarnSomeBotsFailed();
                _numBots -= numFailed;
            }
            else if (numFailed == _numBots)
            {
                throw new BotException("All bots failed to initialize. No bots will run.");
            }

            _initialized = true;
        }

        public void Run(bool waitForTermination)
        {
            if(!_initialized)
                throw new InvalidOperationException("Must call Initialize() before running!");


            _outputHandler.OutputAllBotsAreRunning(waitForTermination);
            for (int i = 0; i < _numBots; ++i)
            {
                _doneSignal.WaitOne();
                //acquire mutex for bot
                //semaphore limits number of concurrently running bots based on cmd-line param
                _botsList[i].Run(waitForTermination);
            }
        }

        public void TerminateBots()
        {
            _terminating = true;
            if (!_initialized) return;

            _botsList.ForEach(_bot => _bot.Terminate());
            _botsList.Clear();
        }

        public void WaitForBotsToComplete()
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
                if (_doneSignal != null)
                {
                    _doneSignal.Dispose();
                    _doneSignal = null;
                }
            }
        }
    }
}
