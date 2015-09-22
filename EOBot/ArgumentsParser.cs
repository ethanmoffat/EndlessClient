using System;

namespace EOBot
{
	enum ArgsError
	{
		NoError,
		WrongNumberOfArgs,
		InvalidPort,
		InvalidNumberOfBots,
		TooManyBots,
		NotEnoughBots
	}
	class ArgumentsParser
	{
		public ArgsError Error { get; private set; }

		public string Host { get; private set; }

		public ushort Port { get; private set; }

		public int NumBots { get; private set; }

		public ArgumentsParser(string[] args)
		{
			Error = ArgsError.NoError;

			if (args.Length != 3)
			{
				Error = ArgsError.WrongNumberOfArgs;
				return;
			}

			Host = args[0];

			ushort port;
			if (!ushort.TryParse(args[1], out port))
			{
				Error = ArgsError.InvalidPort;
				return;
			}
			Port = port;

			int numBots;
			if (!int.TryParse(args[2], out numBots))
			{
				Error = ArgsError.InvalidNumberOfBots;
				return;
			}
			if (numBots > 25)
			{
				Error = ArgsError.TooManyBots;
				return;
			}
			if (numBots < 1)
			{
				Error = ArgsError.NotEnoughBots;
				return;
			}

			NumBots = numBots;
		}
	}
}
