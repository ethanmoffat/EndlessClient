using System.Linq;
using AutomaticTypeMapper;
using EndlessClient.Audio;
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
        private readonly ISfxPlayer _sfxPlayer;

        public HelpActions(IReportActions reportActions,
                           ITextMultiInputDialogFactory textMultiInputDialogFactory,
                           ITextInputDialogFactory textInputDialogFactory,
                           IEOMessageBoxFactory eoMessageBoxFactory,
                           ISfxPlayer sfxPlayer)
        {
            _reportActions = reportActions;
            _textMultiInputDialogFactory = textMultiInputDialogFactory;
            _textInputDialogFactory = textInputDialogFactory;
            _eoMessageBoxFactory = eoMessageBoxFactory;
            _sfxPlayer = sfxPlayer;
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
                new TextMultiInputDialog.InputInfo("Name", MaxChars: 16),
                new TextMultiInputDialog.InputInfo("Reason", MaxChars: 48));
            dlg.DialogClosing += (_, e) =>
            {
                if (e.Result == XNADialogResult.OK)
                {
                    if (dlg.Responses[0].Length > 0 && dlg.Responses[1].Length > 0)
                    {
                        _reportActions.ReportPlayer(dlg.Responses[0], dlg.Responses[1]);
                        _sfxPlayer.PlaySfx(SoundEffectID.AdminRequestSent);
                    }

                    var dlg2 = _eoMessageBoxFactory.CreateMessageBox(DialogResourceID.HELP_REQUEST_RECEIVED);
                    dlg2.ShowDialog();
                }
            };

            UseDefaultDialogSounds(dlg);

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
                    if (dlg.ResponseText.Length > 0)
                    {
                        _reportActions.SpeakToAdmin(dlg.ResponseText);
                        _sfxPlayer.PlaySfx(SoundEffectID.AdminRequestSent);
                    }

                    var dlg2 = _eoMessageBoxFactory.CreateMessageBox(DialogResourceID.HELP_REQUEST_RECEIVED);
                    dlg2.ShowDialog();
                }
            };

            UseDefaultDialogSounds(dlg);

            dlg.ShowDialog();
        }

        // copied from InGameDialogActions
        private void UseDefaultDialogSounds(BaseEODialog dialog)
        {
            dialog.DialogClosing += (_, _) => _sfxPlayer.PlaySfx(SoundEffectID.DialogButtonClick);

            foreach (var textbox in dialog.ChildControls.OfType<IXNATextBox>())
                textbox.OnGotFocus += (_, _) => _sfxPlayer.PlaySfx(SoundEffectID.TextBoxFocus);
        }
    }

    public interface IHelpActions
    {
        void ResetPassword();

        void ReportSomeone();

        void SpeakToAdmin();
    }
}
