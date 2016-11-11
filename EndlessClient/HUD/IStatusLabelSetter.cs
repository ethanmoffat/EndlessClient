// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Localization;

namespace EndlessClient.HUD
{
    public interface IStatusLabelSetter
    {
        void SetStatusLabel(EOResourceID type, EOResourceID text, string appended = "");

        void SetStatusLabel(EOResourceID type, string prepended, EOResourceID text);
    }
}
