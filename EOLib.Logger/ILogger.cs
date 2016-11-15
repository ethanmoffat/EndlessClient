// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;

namespace EOLib.Logger
{
    public interface ILogger : IDisposable
    {
        void Log(string format, params object[] parameters);
    }
}