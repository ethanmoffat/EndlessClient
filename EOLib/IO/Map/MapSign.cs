// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Domain.Map;

namespace EOLib.IO.Map
{
    public struct MapSign : IMapElement
    {
        public byte X { get; set; }
        public byte Y { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
    }
}