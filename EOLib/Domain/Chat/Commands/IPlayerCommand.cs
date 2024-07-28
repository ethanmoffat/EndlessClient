namespace EOLib.Domain.Chat.Commands
{
    public interface IPlayerCommand
    {
        string CommandText { get; }

        bool Execute(string parameter);
    }
}