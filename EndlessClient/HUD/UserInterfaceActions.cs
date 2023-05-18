using AutomaticTypeMapper;
using EndlessClient.Dialogs.Actions;
using EOLib.Domain.Notifiers;
using EOLib.Net;
using System.Collections.Generic;

namespace EndlessClient.HUD
{
    [AutoMappedType]
    public class UserInterfaceActions : IUserInterfaceNotifier
    {
        private readonly IInGameDialogActions _inGameDialogActions;

        public UserInterfaceActions(IInGameDialogActions inGameDialogActions)
        {
            _inGameDialogActions = inGameDialogActions;
        }

        public void NotifyPacketDialog(PacketFamily packetFamily)
        {
            switch (packetFamily)
            {
                case PacketFamily.Locker: _inGameDialogActions.ShowLockerDialog(); break;
                case PacketFamily.Chest: _inGameDialogActions.ShowChestDialog(); break;
                case PacketFamily.Board: _inGameDialogActions.ShowBoardDialog(); break;
                case PacketFamily.JukeBox: _inGameDialogActions.ShowJukeboxDialog(); break;
            }
        }

        public void NotifyMessageDialog(string title, IReadOnlyList<string> messages)
        {
            _inGameDialogActions.ShowMessageDialog(title, messages);
        }
    }
}
