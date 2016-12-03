// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Localization;

namespace EndlessClient.Dialogs.Factories
{
    public interface IEOMessageBoxFactory
    {
        EOMessageBox CreateMessageBox(string message, 
                                      string caption = "",
                                      EODialogButtons whichButtons = EODialogButtons.Ok,
                                      EOMessageBoxStyle style = EOMessageBoxStyle.SmallDialogSmallHeader);

        EOMessageBox CreateMessageBox(DialogResourceID resource,
                                      EODialogButtons whichButtons = EODialogButtons.Ok,
                                      EOMessageBoxStyle style = EOMessageBoxStyle.SmallDialogSmallHeader);

        EOMessageBox CreateMessageBox(string prependData,
                                      DialogResourceID resource,
                                      EODialogButtons whichButtons = EODialogButtons.Ok,
                                      EOMessageBoxStyle style = EOMessageBoxStyle.SmallDialogSmallHeader);

        EOMessageBox CreateMessageBox(DialogResourceID resource,
                                      string extraData,
                                      EODialogButtons whichButtons = EODialogButtons.Ok,
                                      EOMessageBoxStyle style = EOMessageBoxStyle.SmallDialogSmallHeader);
    }
}
