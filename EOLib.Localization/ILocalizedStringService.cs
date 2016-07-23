// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Config;

namespace EOLib.Localization
{
    public interface ILocalizedStringService
    {
        string GetString(EOLanguage language, DialogResourceID dataConstant);
        string GetString(EOLanguage langauge, EOResourceID dataConstant);

        string GetString(DialogResourceID dataConstant);
        string GetString(EOResourceID dataConstant);
    }
}
