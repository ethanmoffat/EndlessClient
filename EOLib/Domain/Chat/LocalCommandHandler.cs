using System;
using System.Collections.Generic;
using System.Linq;
using AutomaticTypeMapper;
using EOLib.Domain.Chat.Commands;

namespace EOLib.Domain.Chat
{
    [AutoMappedType]
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

    public interface ILocalCommandHandler
    {
        bool HandleCommand(string inputCommand, string arguments);
    }
}