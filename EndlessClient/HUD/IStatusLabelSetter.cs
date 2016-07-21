// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib;
using EOLib.Localization;

namespace EndlessClient.HUD
{
    public interface IStatusLabelSetter
    {
        void SetStatusLabel(DATCONST2 type, DATCONST2 text, string extra = "");
    }
}
