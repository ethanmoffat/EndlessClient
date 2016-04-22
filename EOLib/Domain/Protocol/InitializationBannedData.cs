// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;

namespace EOLib.Domain.Protocol
{
	public class InitializationBannedData : IInitializationData
	{
		private readonly BanType _banType;
		private readonly byte _banTimeRemaining;

		public InitReply Response { get { return InitReply.BannedFromServer; } }

		public int this[InitializationDataKey key]
		{
			get { return GetValueHelper(key); }
		}

		public InitializationBannedData(BanType banType, byte banTimeRemaining)
		{
			_banType = banType;
			_banTimeRemaining = banTimeRemaining;
		}

		private int GetValueHelper(InitializationDataKey key)
		{
			switch (key)
			{
				case InitializationDataKey.BanType: return (int)_banType;
				case InitializationDataKey.BanTimeRemaining: return _banTimeRemaining;
				default: throw new ArgumentOutOfRangeException("key", key, null);
			}
		}
	}
}
