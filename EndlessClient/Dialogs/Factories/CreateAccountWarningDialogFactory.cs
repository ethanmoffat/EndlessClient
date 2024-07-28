using AutomaticTypeMapper;
using EndlessClient.Audio;
using EndlessClient.Content;
using EndlessClient.Dialogs.Services;
using EndlessClient.GameExecution;
using EOLib.Graphics;
using XNAControls;

namespace EndlessClient.Dialogs.Factories;

[MappedType(BaseType = typeof(ICreateAccountWarningDialogFactory))]
public class CreateAccountWarningDialogFactory : ICreateAccountWarningDialogFactory
{
    private readonly INativeGraphicsManager _nativeGraphicsManager;
    private readonly IContentProvider _contentProvider;
    private readonly IGameStateProvider _gameStateProvider;
    private readonly IEODialogButtonService _eoDialogButtonService;
    private readonly ISfxPlayer _sfxPlayer;

    public CreateAccountWarningDialogFactory(
        INativeGraphicsManager nativeGraphicsManager,
        IContentProvider contentProvider,
        IGameStateProvider gameStateProvider,
        IEODialogButtonService eoDialogButtonService,
        ISfxPlayer sfxPlayer)
    {
        _nativeGraphicsManager = nativeGraphicsManager;
        _contentProvider = contentProvider;
        _gameStateProvider = gameStateProvider;
        _eoDialogButtonService = eoDialogButtonService;
        _sfxPlayer = sfxPlayer;
    }

    public IXNADialog ShowCreateAccountWarningDialog(string warningMessage)
    {
        var dialog = new ScrollingMessageDialog(_nativeGraphicsManager, _contentProvider, _gameStateProvider, _eoDialogButtonService)
        {
            MessageText = warningMessage
        };
        dialog.DialogClosing += (_, _) => _sfxPlayer.PlaySfx(SoundEffectID.DialogButtonClick);

        return dialog;
    }
}

public interface ICreateAccountWarningDialogFactory
{
    IXNADialog ShowCreateAccountWarningDialog(string warningMessage);
}