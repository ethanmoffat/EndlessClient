using System;
using System.Collections.Generic;
using AutomaticTypeMapper;

namespace EOLib.Domain.Protocol
{
    public interface IPingTimeRepository
    {
        Dictionary<int, DateTime> PingRequests { get; set; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class PingTimeRepository : IPingTimeRepository
    {
        public Dictionary<int, DateTime> PingRequests { get; set; }

        public PingTimeRepository()
        {
            PingRequests = new Dictionary<int, DateTime>();
        }
    }
}
