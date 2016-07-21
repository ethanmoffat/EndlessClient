// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.IO.Map
{
    //todo: make this internal once it is no longer depended upon
    public interface IMapEntityItem<T>
    {
        int X { get; set; }
        T Value { get; set; }
    }
}
