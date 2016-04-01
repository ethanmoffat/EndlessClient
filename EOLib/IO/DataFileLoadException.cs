// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;

namespace EOLib.IO
{
	public class DataFileLoadException : Exception
	{
		public DataFileLoadException()
			: base("Unable to find data files! Check that the data directory exists and has ALL the edf files copied over")
		{
			
		}
	}
}
