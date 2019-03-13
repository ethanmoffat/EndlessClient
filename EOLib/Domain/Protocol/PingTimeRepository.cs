// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using AutomaticTypeMapper;

namespace EOLib.Domain.Protocol
{
    public interface IPingTimeRepository
    {
        Dictionary<ushort, DateTime> PingRequests { get; set; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class PingTimeRepository : IPingTimeRepository
    {
        public Dictionary<ushort, DateTime> PingRequests { get; set; }

        public PingTimeRepository()
        {
            PingRequests = new Dictionary<ushort, DateTime>();
        }
    }
}
