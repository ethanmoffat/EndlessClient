// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

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
    }
}
