using AutomaticTypeMapper;
using EndlessClient.Dialogs.Factories;
using EOLib.Domain.Report;
using EOLib.Localization;
using XNAControls;

namespace EndlessClient.Dialogs.Actions
{
    [AutoMappedType]
    public class HelpActions : IHelpActions
    {
        private readonly IReportActions _reportActions;
        private readonly ITextMultiInputDialogFactory _textMultiInputDialogFactory;
        private readonly ITextInputDialogFactory _textInputDialogFactory;
        private readonly IEOMessageBoxFactory _eoMessageBoxFactory;

        public HelpActions(IReportActions reportActions,
                           ITextMultiInputDialogFactory textMultiInputDialogFactory,
                           ITextInputDialogFactory textInputDialogFactory,
                           IEOMessageBoxFactory eoMessageBoxFactory)
        {
            _reportActions = reportActions;
            _textMultiInputDialogFactory = textMultiInputDialogFactory;
            _textInputDialogFactory = textInputDialogFactory;
            _eoMessageBoxFactory = eoMessageBoxFactory;
        }

        public void ResetPassword()
        {
            // no
        }

        public void ReportSomeone()
        {
            const string Title = "Report Player";
            const string Prompt = "Who do you want to report, and why?";

            var dlg = _textMultiInputDialogFactory.Create(Title, Prompt, TextMultiInputDialog.DialogSize.Two,
                new TextMultiInputDialog.InputInfo { Label = "Name", MaxChars = 16 },
                new TextMultiInputDialog.InputInfo { Label = "Reason", MaxChars = 48 });
            dlg.DialogClosing += (_, e) =>
            {
                if (e.Result == XNADialogResult.OK)
                {
                    _reportActions.ReportPlayer(dlg.Responses[0], dlg.Responses[1]);
                    var dlg2 = _eoMessageBoxFactory.CreateMessageBox(DialogResourceID.HELP_REQUEST_RECEIVED);
                    dlg2.ShowDialog();
                }
            };
            dlg.ShowDialog();
        }

        public void SpeakToAdmin()
        {
            const string Prompt = "Please enter your question, and we will try to get back to you.";

            var dlg = _textInputDialogFactory.Create(Prompt, maxInputChars: 48);
            dlg.DialogClosing += (_, e) =>
            {
                if (e.Result == XNADialogResult.OK)
                {
                    _reportActions.SpeakToAdmin(dlg.ResponseText);
                    var dlg2 = _eoMessageBoxFactory.CreateMessageBox(DialogResourceID.HELP_REQUEST_RECEIVED);
                    dlg2.ShowDialog();
                }
            };
            dlg.ShowDialog();
        }
    }

    public interface IHelpActions
    {
        void ResetPassword();

        void ReportSomeone();

        void SpeakToAdmin();
    }
}
