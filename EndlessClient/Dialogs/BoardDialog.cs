using EndlessClient.Dialogs.Services;
using EOLib.Graphics;

namespace EndlessClient.Dialogs
{
    public class BoardDialog : ScrollingListDialog
    {
        public BoardDialog(INativeGraphicsManager nativeGraphicsManager,
                           IEODialogButtonService dialogButtonService)
            : base(nativeGraphicsManager, dialogButtonService, ScrollingListDialogSize.MediumWithHeader)
        {
        }
    }
}
