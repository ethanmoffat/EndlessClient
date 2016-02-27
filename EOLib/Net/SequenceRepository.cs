// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Net
{
	public class SequenceRepository : ISequenceRepository
	{
		public int SequenceStart { get; set; }
		public int SequenceIncrement { get; set; }
	}
}
