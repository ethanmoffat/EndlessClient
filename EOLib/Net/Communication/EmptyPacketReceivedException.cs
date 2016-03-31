// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;

namespace EOLib.Net.Communication
{
	public class EmptyPacketReceivedException : Exception
	{
		public EmptyPacketReceivedException()
			: base("No data was received from the server") { }
	}
}
