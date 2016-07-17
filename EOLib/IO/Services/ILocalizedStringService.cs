// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.IO.Services
{
    public interface ILocalizedStringService
    {
        string GetString(EOLanguage language, DATCONST1 dataConstant);
        string GetString(EOLanguage langauge, DATCONST2 dataConstant);

        string GetString(DATCONST1 dataConstant);
        string GetString(DATCONST2 dataConstant);
    }
}
