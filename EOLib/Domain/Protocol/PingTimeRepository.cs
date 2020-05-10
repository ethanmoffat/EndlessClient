using System;
using System.Collections.Generic;
using AutomaticTypeMapper;

namespace EOLib.Domain.Protocol
{
    public interface IPingTimeRepository
    {
        Dictionary<short, DateTime> PingRequests { get; set; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class PingTimeRepository : IPingTimeRepository
    {
        public Dictionary<short, DateTime> PingRequests { get; set; }

        public PingTimeRepository()
        {
            PingRequests = new Dictionary<short, DateTime>();
        }
    }
}
