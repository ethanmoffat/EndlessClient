using System;
using AutomaticTypeMapper;
using EOLib.Localization;

namespace EndlessClient.HUD
{
    [MappedType(BaseType = typeof(IStatusLabelSetter))]
    public class StatusLabelSetter : IStatusLabelSetter
    {
        private readonly IStatusLabelTextRepository _statusLabelTextRepository;
        private readonly ILocalizedStringFinder _localizedStringFinder;

        public StatusLabelSetter(IStatusLabelTextRepository statusLabelTextRepository,
            ILocalizedStringFinder localizedStringFinder)
        {
            _statusLabelTextRepository = statusLabelTextRepository;
            _localizedStringFinder = localizedStringFinder;
        }

        public void SetStatusLabel(EOResourceID type, EOResourceID text, string appended = "")
        {
            CheckStatusLabelType(type);

            SetStatusLabelText(_localizedStringFinder.GetString(type),
                               _localizedStringFinder.GetString(text),
                               appended);
        }

        public void SetStatusLabel(EOResourceID type, string prepended, EOResourceID text)
        {
            CheckStatusLabelType(type);
            SetStatusLabelText(_localizedStringFinder.GetString(type),
                               prepended,
                               _localizedStringFinder.GetString(text));
        }

        private void SetStatusLabelText(string type, string text, string extra = "")
        {
            _statusLabelTextRepository.StatusText = $"[ {type} ] {text} {extra}";
            _statusLabelTextRepository.SetTime = DateTime.Now;
        }

        private void CheckStatusLabelType(EOResourceID type)
        {
            switch (type)
            {
                case EOResourceID.STATUS_LABEL_TYPE_ACTION:
                case EOResourceID.STATUS_LABEL_TYPE_BUTTON:
                case EOResourceID.STATUS_LABEL_TYPE_INFORMATION:
                case EOResourceID.STATUS_LABEL_TYPE_WARNING:
                case EOResourceID.STATUS_LABEL_TYPE_ITEM:
                case EOResourceID.SKILLMASTER_WORD_SPELL:
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(type), "Use either ACTION, BUTTION, INFORMATION, WARNING, ITEM, or SPELL for this.");
            }
        }
    }
}