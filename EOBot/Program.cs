using System;
using System.Collections.Generic;
using System.Threading;

namespace EOBot
{
	static class Program
	{
		static void Main(string[] args)
		{
			ArgumentsParser argsParser = new ArgumentsParser(args);

			switch (argsParser.Error)
			{
				case ArgsError.WrongNumberOfArgs:
					Console.WriteLine("Invalid: specify host, port, and the number of bots to run");
					Console.WriteLine("Usage: EOBot.exe <host> <port> <numbots>");
					return;
				case ArgsError.InvalidPort:
					Console.WriteLine("Invalid: port number could not be parsed!");
					return;
				case ArgsError.InvalidNumberOfBots:
					Console.WriteLine("Invalid: specify an integer argument for number of bots");
					return;
				case ArgsError.TooManyBots:
					Console.WriteLine("Invalid: unable to launch > 25 threads of execution. Please use 25 or less.");
					return;
				case ArgsError.NotEnoughBots:
					Console.WriteLine("Invalid: unable to launch < 1 thread of execution. Please use 1 or more.");
					return;
			}

			Console.WriteLine("Starting bots...");

			try
			{ 
				using (BotFramework f = new BotFramework())
				{
					Win32.SetConsoleCtrlHandler(type => { f.TerminateBots(); return true; }, true);

					f.BotInitFailed += Console.WriteLine;
					f.BotInitSuccess += index => Console.WriteLine("Bot {0} created.", index);
					f.BotsRunning += () => Console.WriteLine("All bots created. Waiting for termination (press CTRL+C to end early)");

					f.Initialize(argsParser.NumBots, argsParser.Host, argsParser.Port);
					f.Run();
					f.WaitForCompletion();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error: {0}", ex.Message);
			}

			Console.WriteLine("All threads completed.");
		}
	}
}
