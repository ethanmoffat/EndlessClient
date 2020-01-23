// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using AutomaticTypeMapper;

namespace EOLib
{
#if LINUX
    [AutoMappedType]
    public class HDSerialNumberServiceLinux : IHDSerialNumberService
    {
        public string GetHDSerialNumber()
        {
            return "12345"; // Just like my luggage
        }
    }
#endif
}
