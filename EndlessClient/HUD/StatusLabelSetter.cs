// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EOLib;
using EOLib.IO.Services;

namespace EndlessClient.HUD
{
    public class StatusLabelSetter : IStatusLabelSetter
    {
        private readonly IStatusLabelTextRepository _statusLabelTextRepository;
        private readonly ILocalizedStringService _localizedStringService;

        public StatusLabelSetter(IStatusLabelTextRepository statusLabelTextRepository,
            ILocalizedStringService localizedStringService)
        {
            _statusLabelTextRepository = statusLabelTextRepository;
            _localizedStringService = localizedStringService;
        }

        public void SetStatusLabel(DATCONST2 type, DATCONST2 text, string extra = "")
        {
            CheckStatusLabelType(type);

            SetStatusLabelText(_localizedStringService.GetString(type),
                               _localizedStringService.GetString(text));
        }

        private void SetStatusLabelText(string type, string text, string extra = "")
        {
            _statusLabelTextRepository.StatusText = string.Format("[ {0} ] {1} {2}", type, text, extra);
            _statusLabelTextRepository.SetTime = DateTime.Now;
        }

        private void CheckStatusLabelType(DATCONST2 type)
        {
            switch (type)
            {
                case DATCONST2.STATUS_LABEL_TYPE_ACTION:
                case DATCONST2.STATUS_LABEL_TYPE_BUTTON:
                case DATCONST2.STATUS_LABEL_TYPE_INFORMATION:
                case DATCONST2.STATUS_LABEL_TYPE_WARNING:
                case DATCONST2.STATUS_LABEL_TYPE_ITEM:
                case DATCONST2.SKILLMASTER_WORD_SPELL:
                    break;
                default: throw new ArgumentOutOfRangeException("type", "Use either ACTION, BUTTION, INFORMATION, WARNING, ITEM, or SPELL for this.");
            }
        }
    }
}