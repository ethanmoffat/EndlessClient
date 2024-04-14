using AutomaticTypeMapper;
using EndlessClient.Dialogs.Factories;
using EOLib.Domain.Report;
using XNAControls;

namespace EndlessClient.Dialogs.Actions
{
    [AutoMappedType]
    public class HelpActions : IHelpActions
    {
        private readonly IReportActions _reportActions;
        private readonly ITextInputDialogFactory _textInputDialogFactory;

        public HelpActions(IReportActions reportActions,
                           ITextInputDialogFactory textInputDialogFactory)
        {
            _reportActions = reportActions;
            _textInputDialogFactory = textInputDialogFactory;
        }

        public void ResetPassword()
        {
            // todo
        }

        public void ReportSomeone()
        {
            // gfx002 / 157
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
