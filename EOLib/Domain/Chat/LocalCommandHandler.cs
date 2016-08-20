// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using EOLib.Domain.Chat.Commands;

namespace EOLib.Domain.Chat
{
    public class LocalCommandHandler : ILocalCommandHandler
    {
        private readonly IEnumerable<IPlayerCommand> _playerCommands;

        public LocalCommandHandler(IEnumerable<IPlayerCommand> playerCommands)
        {
            _playerCommands = playerCommands;
        }

        public bool HandleCommand(string inputCommand, string arguments)
        {
            var playerCommand = _playerCommands.SingleOrDefault(x => CommandTextEqual(x, inputCommand));
            return playerCommand != null && playerCommand.Execute(arguments);
        }

        private static bool CommandTextEqual(IPlayerCommand playerCommand, string inputCommand)
        {
            return string.Equals(playerCommand.CommandText, inputCommand, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}