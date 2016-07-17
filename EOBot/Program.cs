// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;

namespace EOBot
{
    static class Program
    {
        private static BotFramework f;

        static void Main(string[] args)
        {
            Win32.SetConsoleCtrlHandler(HandleCtrl, true);

            ArgumentsParser parsedArgs = new ArgumentsParser(args);

            if (parsedArgs.Error != ArgsError.NoError)
            {
                ShowError(parsedArgs);
                return;
            }

            Console.WriteLine("Starting bots...");

            try
            {
                using (f = new BotFramework(new BotConsoleOutputHandler(), parsedArgs))
                {
                    f.Initialize(new PartyBotFactory(), parsedArgs.InitDelay);
                    f.Run(parsedArgs.WaitForTermination);
                    f.WaitForBotsToComplete();
                }

                Console.WriteLine("All bots completed.");
            }
            catch (BotException bex)
            {
                Console.WriteLine(bex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nUnhandled error: {0}", ex.Message);
            }
        }

        static bool HandleCtrl(Win32.CtrlTypes type)
        {
            var name = Enum.GetName(type.GetType(), type);
            Console.WriteLine("\nExiting due to {0} event from system!\n", name);

            if (f != null)
                f.TerminateBots();
            return true;
        }

        static void ShowError(ArgumentsParser args)
        {
            switch (args.Error)
            {
                case ArgsError.WrongNumberOfArgs:
                    Console.WriteLine("Invalid: specify host, port, and the number of bots to run");
                    break;
                case ArgsError.InvalidPort:
                    Console.WriteLine("Invalid: port number could not be parsed!");
                    break;
                case ArgsError.InvalidNumberOfBots:
                    Console.WriteLine("Invalid: specify an integer argument for number of bots.");
                    break;
                case ArgsError.TooManyBots:
                    Console.WriteLine("Invalid: unable to launch > 25 threads of execution. Please use 25 or less.");
                    break;
                case ArgsError.NotEnoughBots:
                    Console.WriteLine("Invalid: unable to launch < 1 thread of execution. Please use 1 or more.");
                    break;
                case ArgsError.BadFormat:
                    Console.WriteLine("Badly formatted argument. Enter args like \"argument=value\".");
                    break;
                case ArgsError.InvalidSimultaneousNumberOfBots:
                    Console.WriteLine("Invalid: Simultaneous number of bots must not be more than total number of bots.");
                    break;
                case ArgsError.InvalidWaitFlag:
                    Console.WriteLine("Invalid: Wait flag could not be parsed. Use 0, 1, false, true, no, or yes.");
                    break;
                case ArgsError.InvalidInitDelay:
                    Console.WriteLine("Invalid: specify an integer argument for delay between inits (> 1100ms).");
                    break;
            }

            Console.WriteLine("\n\nUsage: (enter arguments in any order) (angle brackets is entry) (square brackets is optional)");
            Console.WriteLine("EOBot.exe host=<host>\n" +
                              "          port=<port>\n" +
                              "          bots=<numBots>[,<simultaneousBots>]\n" +
                              "          wait=<true/false>\n" +
                              "          initDelay=<timeInMS>\n");
            Console.WriteLine("\t host: hostname or IP address");
            Console.WriteLine("\t port: port to connect on (probably 8078)");
            Console.WriteLine("\t bots: number of bots to execute.    \n\t       numBots is the total number, simultaneousBots is how many will run at once");
            Console.WriteLine("\t wait: flag to wait for termination. \n\t       Set to true to wait for bots to be explicitly terminated via CTRL+C, false otherwise");
            Console.WriteLine("\t initDelay: Time in milliseconds to delay between doing the INIT handshake with the server");
        }
    }
}
