// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EndlessClient.ControlSets
{
	public interface IControlSetRepository
	{
		IControlSet CurrentControlSet { get; set; }
	}

	public interface IControlSetProvider
	{
		IControlSet CurrentControlSet { get; }
	}

	public class ControlSetRepository : IControlSetRepository, IControlSetProvider
	{
		public IControlSet CurrentControlSet { get; set; }
	}
}
