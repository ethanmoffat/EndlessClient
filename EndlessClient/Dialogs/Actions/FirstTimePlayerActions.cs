using AutomaticTypeMapper;
using EndlessClient.Dialogs.Factories;
using EOLib.Domain.Login;
using EOLib.Localization;

namespace EndlessClient.Dialogs.Actions;

[MappedType(BaseType = typeof(IFirstTimePlayerActions))]
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
        {
            var messageBox = _messageBoxFactory.CreateMessageBox(DialogResourceID.WARNING_FIRST_TIME_PLAYERS);
            messageBox.ShowDialog();
        }
    }
}

public interface IFirstTimePlayerActions
{
    void WarnFirstTimePlayers();
}