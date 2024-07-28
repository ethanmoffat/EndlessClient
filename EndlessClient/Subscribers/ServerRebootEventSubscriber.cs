using AutomaticTypeMapper;
using EndlessClient.Audio;
using EndlessClient.HUD;
using EndlessClient.HUD.Chat;
using EOLib.Domain.Notifiers;
using EOLib.Localization;

namespace EndlessClient.Subscribers;

[AutoMappedType]
public class ServerRebootEventNotifier : IServerRebootNotifier
{
    private readonly ILocalizedStringFinder _localizedStringFinder;
    private readonly IServerMessageHandler _serverMessageHandler;
    private readonly IStatusLabelSetter _statusLabelSetter;

    public ServerRebootEventNotifier(ILocalizedStringFinder localizedStringFinder,
                                IServerMessageHandler serverMessageHandler,
                                IStatusLabelSetter statusLabelSetter,
                                ISfxPlayer sfxPlayer)
    {
        _localizedStringFinder = localizedStringFinder;
        _serverMessageHandler = serverMessageHandler;
        _statusLabelSetter = statusLabelSetter;
    }

    public void NotifyServerReboot()
    {
        var message = _localizedStringFinder.GetString(EOResourceID.REBOOT_SEQUENCE_STARTED);
        _serverMessageHandler.AddServerMessage(message, SoundEffectID.Reboot);
        _statusLabelSetter.ShowWarning(message);
    }
}