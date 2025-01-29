using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EOBot
{
    sealed class BotFramework : IDisposable
    {
        public const int NUM_BOTS_MAX = 25;

        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly List<IBot> _botsList;
        private readonly string _host;
        private readonly ushort _port;
        private readonly int _initRetryAttempts;

        private Semaphore _doneSignal;
        private bool _initialized;
        private int _numBots;
        private bool _terminating;

        public BotFramework(ArgumentsParser parsedArgs)
        {
            if (parsedArgs == null)
                throw new ArgumentNullException(nameof(parsedArgs));

            _cancellationTokenSource = new CancellationTokenSource();

            var numberOfBots = parsedArgs.NumBots;
            var simultaneousBots = parsedArgs.SimultaneousBots;

            if (numberOfBots > NUM_BOTS_MAX || simultaneousBots > NUM_BOTS_MAX || simultaneousBots > numberOfBots)
                throw new ArgumentException("Too many bots requested");

            if (numberOfBots <= 0 || simultaneousBots <= 0)
                throw new ArgumentException("Not enough bots requested");

            _numBots = numberOfBots;
            _host = parsedArgs.Host;
            _port = parsedArgs.Port;
            _initRetryAttempts = parsedArgs.InitRetryAttempts;

            _botsList = new List<IBot>(numberOfBots);

            _doneSignal = new Semaphore(simultaneousBots, simultaneousBots);
        }

        public async Task InitializeAsync(IBotFactory botFactory, int delayBetweenInitsMS = 1100)
        {
            if (_initialized)
                throw new InvalidOperationException("Unable to initialize bot framework a second time.");

            int numFailed = 0;
            for (int i = 0; i < _numBots; i++)
            {
                if (_terminating)
                    throw new BotException("Received termination signal; initialization has been cancelled");

                for (int attempt = 1; attempt <= _initRetryAttempts; attempt++)
                {
                    ConsoleHelper.WriteMessage(ConsoleHelper.Type.None, $"Initializing bot {i} [attempt {attempt}/{_initRetryAttempts}]");
                    try
                    {
                        var bot = botFactory.CreateBot(i);
                        bot.WorkCompleted += () => _doneSignal.Release();
                        await bot.InitializeAsync(_host, _port).ConfigureAwait(false);
                        _botsList.Add(bot);

                        ConsoleHelper.WriteMessage(ConsoleHelper.Type.None, $"Bot {i} initialized.");
                        await Task.Delay(delayBetweenInitsMS).ConfigureAwait(false); // minimum for this is 1sec server-side

                        break;
                    }
                    catch (Exception ex)
                    {
                        ConsoleHelper.WriteMessage(ConsoleHelper.Type.Error, ex.Message, ConsoleColor.DarkRed);

                        if (attempt == _initRetryAttempts)
                        {
                            ConsoleHelper.WriteMessage(ConsoleHelper.Type.Error, $"Bot {i} failed to initialize after {_initRetryAttempts} attempts.", ConsoleColor.DarkRed);
                            numFailed++;
                        }
                        else
                        {
                            var retryDelayTime = TimeSpan.FromMilliseconds(delayBetweenInitsMS + (1000 * attempt * attempt));
                            ConsoleHelper.WriteMessage(ConsoleHelper.Type.Warning, $"Bot {i} failed to initialize. Retrying in {retryDelayTime.TotalMilliseconds}ms...", ConsoleColor.DarkYellow);
                            await Task.Delay(retryDelayTime).ConfigureAwait(false);
                        }
                    }
                }
            }

            if (numFailed > 0)
            {
                ConsoleHelper.WriteMessage(ConsoleHelper.Type.Warning, "Some bot instances failed to initialize. These bot instances will not be run.", ConsoleColor.DarkYellow);
                _numBots -= numFailed;
            }
            else if (numFailed == _numBots)
            {
                throw new BotException("All bots failed to initialize. No bots will run.");
            }

            _initialized = true;
        }

        public async Task RunAsync()
        {
            if (!_initialized)
                throw new InvalidOperationException("Must call Initialize() before running!");

            var botTasks = new List<Task>();

            ConsoleHelper.WriteMessage(ConsoleHelper.Type.None, "Bot framework run has started.\n");
            for (int i = 0; i < _numBots; ++i)
            {
                _doneSignal.WaitOne();
                //acquire mutex for bot
                //semaphore limits number of concurrently running bots based on cmd-line param
                botTasks.Add(_botsList[i].RunAsync(_cancellationTokenSource.Token));
            }

            // this is done to force handling of exceptions as an Aggregate exception so errors from multiple bots are shown properly
            // otherwise, only the first exception from the first faulting task will be thrown
            var continuation = Task.WhenAll(botTasks);
            try
            {
                await continuation.ConfigureAwait(false);
            }
            catch { }

            if (continuation.Status != TaskStatus.RanToCompletion && continuation.Exception != null)
            {
                throw continuation.Exception;
            }
        }

        public void TerminateBots()
        {
            _terminating = true;
            if (!_initialized) return;

            _cancellationTokenSource.Cancel();
        }

        public void Dispose()
        {
            _doneSignal?.Dispose();
            _cancellationTokenSource?.Dispose();
        }
    }
}
