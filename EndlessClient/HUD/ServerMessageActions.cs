using AutomaticTypeMapper;
using EndlessClient.Dialogs.Actions;
using EOLib.Domain.Notifiers;
using System.Collections.Generic;

namespace EndlessClient.HUD
{
    [AutoMappedType]
    public class ServerMessageActions : IUserInterfaceNotifier
    {
        private readonly IInGameDialogActions _inGameDialogActions;

        public ServerMessageActions(IInGameDialogActions inGameDialogActions)
        {
            _inGameDialogActions = inGameDialogActions;
        }

        public void NotifyMessageDialog(string title, IReadOnlyList<string> messages)
        {
            _inGameDialogActions.ShowMessageDialog(title, messages);
        }
    }
}
