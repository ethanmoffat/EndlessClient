// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Localization;
using XNAControls.Old;

namespace EndlessClient.Dialogs.Factories
{
    public interface IEOMessageBoxFactory
    {
        EOMessageBox CreateMessageBox(string message, 
                                      string caption = "", 
                                      XNADialogButtons whichButtons = XNADialogButtons.Ok,
                                      EOMessageBoxStyle style = EOMessageBoxStyle.SmallDialogSmallHeader);

        EOMessageBox CreateMessageBox(DialogResourceID resource,
                                      XNADialogButtons whichButtons = XNADialogButtons.Ok,
                                      EOMessageBoxStyle style = EOMessageBoxStyle.SmallDialogSmallHeader);

        EOMessageBox CreateMessageBox(string prependData,
                                      DialogResourceID resource,
                                      XNADialogButtons whichButtons = XNADialogButtons.Ok,
                                      EOMessageBoxStyle style = EOMessageBoxStyle.SmallDialogSmallHeader);

        EOMessageBox CreateMessageBox(DialogResourceID resource,
                                      string extraData,
                                      XNADialogButtons whichButtons = XNADialogButtons.Ok,
                                      EOMessageBoxStyle style = EOMessageBoxStyle.SmallDialogSmallHeader);
    }
}
