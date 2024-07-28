using AutomaticTypeMapper;
using EndlessClient.Dialogs.Factories;
using EOLib.Domain.Notifiers;
using EOLib.Localization;

namespace EndlessClient.Subscribers;

[AutoMappedType]
public class JukeboxEventSubscriber : IJukeboxNotifier
{
    private readonly IEOMessageBoxFactory _messageBoxFactory;

    public JukeboxEventSubscriber(IEOMessageBoxFactory messageBoxFactory)
    {
        _messageBoxFactory = messageBoxFactory;
    }

    public void JukeboxUnavailable()
    {
        var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.JUKEBOX_REQUESTED_RECENTLY);
        dlg.ShowDialog();
    }
}