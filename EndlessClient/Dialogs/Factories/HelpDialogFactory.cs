using AutomaticTypeMapper;
using EndlessClient.Content;
using EndlessClient.Dialogs.Actions;
using EndlessClient.Dialogs.Services;
using EOLib;
using EOLib.Graphics;
using EOLib.Localization;
using System;
using System.Collections.Generic;

namespace EndlessClient.Dialogs.Factories
{
    [AutoMappedType(IsSingleton = true)]
    public class HelpDialogFactory : IHelpDialogFactory
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IEODialogButtonService _dialogButtonService;
        private readonly IContentProvider _contentProvider;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly IHelpActions _helpActions;

        public HelpDialogFactory(INativeGraphicsManager nativeGraphicsManager,
                                 IEODialogButtonService dialogButtonService,
                                 IContentProvider contentProvider,
                                 ILocalizedStringFinder localizedStringFinder,
                                 IHelpActions helpActions)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _dialogButtonService = dialogButtonService;
            _contentProvider = contentProvider;
            _localizedStringFinder = localizedStringFinder;
            _helpActions = helpActions;
        }

        public ScrollingListDialog Create()
        {
            var dlg = new ScrollingListDialog(_nativeGraphicsManager, _dialogButtonService, DialogType.Help)
            {
                Buttons = ScrollingListDialogButtons.Cancel,
                ListItemType = ListDialogItem.ListItemStyle.Small,
            };

            dlg.AddTextAsListItems(_contentProvider.Fonts[Constants.FontSize08pt5], GetActions(), GetMessages());

            return dlg;
        }

        private string[] GetMessages()
        {
            return new[]
            {
                _localizedStringFinder.GetString(EOResourceID.ENDLESS_HELP_SUMMARY_1),
                string.Empty,
                _localizedStringFinder.GetString(EOResourceID.ENDLESS_HELP_SUMMARY_2),
                string.Empty,
                _localizedStringFinder.GetString(EOResourceID.ENDLESS_HELP_LINK_RESET_PASSWORD),
                _localizedStringFinder.GetString(EOResourceID.ENDLESS_HELP_LINK_REPORT_SOMEONE),
                _localizedStringFinder.GetString(EOResourceID.ENDLESS_HELP_LINK_SPEAK_TO_ADMIN),
            };
        }

        private List<Action> GetActions()
        {
            return new List<Action>
            {
                _helpActions.ResetPassword,
                _helpActions.ReportSomeone,
                _helpActions.SpeakToAdmin,
            };
        }
    }

    public interface IHelpDialogFactory
    {
        ScrollingListDialog Create();
    }
}
