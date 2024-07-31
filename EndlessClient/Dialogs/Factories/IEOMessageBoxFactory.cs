using EOLib.Localization;
using XNAControls;

namespace EndlessClient.Dialogs.Factories
{
    public interface IEOMessageBoxFactory
    {
        IXNADialog CreateMessageBox(string message,
                                    string caption = "",
                                    EODialogButtons whichButtons = EODialogButtons.Ok,
                                    EOMessageBoxStyle style = EOMessageBoxStyle.SmallDialogSmallHeader);

        IXNADialog CreateMessageBox(DialogResourceID resource,
                                    EODialogButtons whichButtons = EODialogButtons.Ok,
                                    EOMessageBoxStyle style = EOMessageBoxStyle.SmallDialogSmallHeader);

        IXNADialog CreateMessageBox(string prependData,
                                    DialogResourceID resource,
                                    EODialogButtons whichButtons = EODialogButtons.Ok,
                                    EOMessageBoxStyle style = EOMessageBoxStyle.SmallDialogSmallHeader);

        IXNADialog CreateMessageBox(DialogResourceID resource,
                                    string extraData,
                                    EODialogButtons whichButtons = EODialogButtons.Ok,
                                    EOMessageBoxStyle style = EOMessageBoxStyle.SmallDialogSmallHeader);

        IXNADialog CreateMessageBox(EOResourceID message,
                                    EOResourceID caption,
                                    EODialogButtons whichButtons = EODialogButtons.Ok,
                                    EOMessageBoxStyle style = EOMessageBoxStyle.SmallDialogSmallHeader);
    }
}