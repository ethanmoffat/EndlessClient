using EOBot.Interpreter.Variables;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EOBot
{
    public enum ArgsError
    {
        NoError,
        BadFormat,
        WrongNumberOfArgs,
        InvalidPort,
        InvalidNumberOfBots,
        TooManyBots,
        NotEnoughBots,
        InvalidSimultaneousNumberOfBots,
        InvalidWaitFlag,
        InvalidInitDelay,
        InvalidPath,
        InvalidScriptArgs,
        AutoConnectRequired
    }

    public class ArgumentsParser
    {
        public ArgsError Error { get; private set; }

        public string ScriptFile { get; private set; }

        public string Host { get; private set; }
        public ushort Port { get; private set; }

        public int NumBots { get; private set; }
        public int SimultaneousBots { get; private set; }

        public int InitDelay { get; private set; }

        public string Account { get; private set; }
        public string Password { get; private set; }
        public string Character { get; private set; }

        public bool AutoConnect { get; private set; } = true;

        public List<string> UserArgs { get; internal set; }

        public bool ExtendedHelp { get; private set; }


        public ArgumentsParser(string[] args)
        {
            InitDelay = 1100;

            Error = ArgsError.NoError;

            if ((!args.Contains("--") && args.Select(x => x.ToLower()).Contains("help")) ||
                (args.Contains("--") && args.TakeWhile(x => x != "--").Select(x => x.ToLower()).Contains("help")))
            {
                ExtendedHelp = true;
            }
            else
            {
                for (int i = 0; i < args.Length; i++)
                {
                    var arg = args[i];

                    if (arg == "--")
                    {
                        UserArgs = new List<string>();
                        for (i = i + 1; i < args.Length; i++)
                        {
                            UserArgs.Add(args[i]);
                        }
                        break;
                    }

                    var pair = arg.Split('=');

                    if (pair.Length != 2)
                    {
                        Error = ArgsError.BadFormat;
                        return;
                    }

                    switch (pair[0].ToLower())
                    {
                        case "script":
                            if (!File.Exists(pair[1]))
                            {
                                Error = ArgsError.InvalidPath;
                                return;
                            }
                            ScriptFile = pair[1];
                            break;
                        case "autoconnect":
                            AutoConnect = bool.Parse(pair[1]);
                            break;
                        case "host":
                            ParseHost(pair[1]);
                            break;
                        case "port":
                            if (!ParsePort(pair[1]))
                                return;
                            break;
                        case "bots":
                            if (!ParseNumBots(pair))
                                return;
                            break;
                        case "initdelay":
                            if (!ParseInitDelay(pair[1]))
                                return;
                            break;
                        case "account":
                            Account = pair[1];
                            break;
                        case "password":
                            Password = pair[1];
                            break;
                        case "character":
                            Character = pair[1];
                            break;
                        default:
                            Error = ArgsError.BadFormat;
                            return;
                    }
                }

                if (ScriptFile == null)
                {
                    if (Host == null || Port == 0 || NumBots == 0 || Account == null || Password == null || Character == null)
                    {
                        Error = ArgsError.WrongNumberOfArgs;
                    }
                    else if (UserArgs != null || !AutoConnect)
                    {
                        Error = ArgsError.InvalidScriptArgs;
                    }
                }
                else if (NumBots > 1 && ScriptFile != null && !AutoConnect)
                {
                    Error = ArgsError.AutoConnectRequired;
                }
            }
        }

        private void ParseHost(string hostStr)
        {
            Host = hostStr;
        }

        private bool ParsePort(string portStr)
        {
            ushort port;
            if (!ushort.TryParse(portStr, out port))
            {
                Error = ArgsError.InvalidPort;
                return false;
            }
            Port = port;
            return true;
        }

        private bool ParseNumBots(string[] pair)
        {
            var both = pair[1].Split(',');

            if (both.Length < 1 || both.Length > 2)
            {
                Error = ArgsError.BadFormat;
                return false;
            }

            bool needSimul = both.Length == 2;

            int numBots, simul = -1;
            if (!int.TryParse(both[0], out numBots))
            {
                Error = ArgsError.InvalidNumberOfBots;
                return false;
            }
            if (needSimul && !int.TryParse(both[1], out simul))
            {
                Error = ArgsError.InvalidSimultaneousNumberOfBots;
                return false;
            }
            if (numBots > BotFramework.NUM_BOTS_MAX || (needSimul && (simul > BotFramework.NUM_BOTS_MAX || simul > numBots)))
            {
                Error = ArgsError.TooManyBots;
                return false;
            }
            if (numBots < 1 || (needSimul && simul < 1))
            {
                Error = ArgsError.NotEnoughBots;
                return false;
            }

            NumBots = numBots;
            SimultaneousBots = needSimul ? simul : numBots;
            return true;
        }

        private bool ParseInitDelay(string initDelay)
        {
            int delay;
            if (!int.TryParse(initDelay, out delay) || delay < 1100)
            {
                Error = ArgsError.InvalidInitDelay;
                return false;
            }

            InitDelay = delay;

            return true;
        }
    }
}
