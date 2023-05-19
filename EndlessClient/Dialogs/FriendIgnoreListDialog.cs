using EndlessClient.Dialogs.Services;
using EOLib.Domain.Online;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace EndlessClient.Dialogs
{
    public class FriendIgnoreListDialog : ScrollingListDialog
    {
        private readonly IOnlinePlayerProvider _onlinePlayerProvider;

        private HashSet<OnlinePlayerInfo> _cachedOnlinePlayers;

        public FriendIgnoreListDialog(INativeGraphicsManager nativeGraphicsManager,
                                      IEODialogButtonService dialogButtonService,
                                      IOnlinePlayerProvider onlinePlayerProvider)
            : base(nativeGraphicsManager, dialogButtonService, DialogType.FriendIgnore)
        {
            _onlinePlayerProvider = onlinePlayerProvider;
            _cachedOnlinePlayers = new HashSet<OnlinePlayerInfo>();
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            if (!_cachedOnlinePlayers.SetEquals(_onlinePlayerProvider.OnlinePlayers))
            {
                _cachedOnlinePlayers = _onlinePlayerProvider.OnlinePlayers.ToHashSet();

                ClearHighlightedText();
                HighlightTextByLabel(_cachedOnlinePlayers.Select(x => x.Name).ToList());
            }

            base.OnUpdateControl(gameTime);
        }
    }
}
