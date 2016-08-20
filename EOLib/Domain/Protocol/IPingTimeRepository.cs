// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;

namespace EOLib.Domain.Protocol
{
    public interface IPingTimeRepository
    {
        Dictionary<short, DateTime> PingRequests { get; set; }
    }

    public interface IPingTimeProvider
    {
        IReadOnlyDictionary<short, DateTime> PingRequests { get; }
    }

    public class PingTimeRepository : IPingTimeRepository, IPingTimeProvider
    {
        public Dictionary<short, DateTime> PingRequests { get; set; }

        IReadOnlyDictionary<short, DateTime> IPingTimeProvider.PingRequests
        {
            get { return PingRequests; }
        }

        public PingTimeRepository()
        {
            PingRequests = new Dictionary<short, DateTime>();
        }
    }
}
