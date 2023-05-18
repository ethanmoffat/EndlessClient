using EndlessClient.Dialogs.Services;
using EOLib.Graphics;

namespace EndlessClient.Dialogs
{
    public class JukeboxDialog : ScrollingListDialog
    {
        public JukeboxDialog(INativeGraphicsManager nativeGraphicsManager,
                             IEODialogButtonService dialogButtonService,
                             IEODialogIconService dialogIconService)
            : base(nativeGraphicsManager, dialogButtonService, DialogType.Jukebox)
        {
        }
    }
}
