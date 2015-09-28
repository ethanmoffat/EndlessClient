using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EOBot
{
	public class BotException : Exception
	{
		public BotException(string message) : base(message) { }
	}
}
