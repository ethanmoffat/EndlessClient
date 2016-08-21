﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Dialogs.Factories;
using EOLib.Domain.Login;
using EOLib.Localization;

namespace EndlessClient.Dialogs.Actions
{
    public interface IFirstTimePlayerActions
    {
        void WarnFirstTimePlayers();
    }

    public class FirstTimePlayerActions : IFirstTimePlayerActions
    {
        private readonly IPlayerInfoProvider _playerInfoProvider;
        private readonly IEOMessageBoxFactory _messageBoxFactory;

        public FirstTimePlayerActions(IPlayerInfoProvider playerInfoProvider,
                                      IEOMessageBoxFactory messageBoxFactory)
        {
            _playerInfoProvider = playerInfoProvider;
            _messageBoxFactory = messageBoxFactory;
        }

        public void WarnFirstTimePlayers()
        {
            if (_playerInfoProvider.IsFirstTimePlayer)
                _messageBoxFactory.CreateMessageBox(DialogResourceID.WARNING_FIRST_TIME_PLAYERS);
        }
    }
}
