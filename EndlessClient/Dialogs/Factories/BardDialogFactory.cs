using AutomaticTypeMapper;
using EndlessClient.Controllers;
using EndlessClient.Dialogs.Services;
using EOLib.Graphics;

namespace EndlessClient.Dialogs.Factories;

[AutoMappedType]
public class BardDialogFactory : IBardDialogFactory
{
    private readonly INativeGraphicsManager _nativeGraphicsManager;
    private readonly IBardController _bardController;
    private readonly IEODialogButtonService _dialogButtonService;

    public BardDialogFactory(INativeGraphicsManager nativeGraphicsManager,
                             IBardController bardController,
                             IEODialogButtonService dialogButtonService)
    {
        _nativeGraphicsManager = nativeGraphicsManager;
        _bardController = bardController;
        _dialogButtonService = dialogButtonService;
    }

    public BardDialog Create()
    {
        return new BardDialog(_nativeGraphicsManager,
                              _bardController,
                              _dialogButtonService);
    }
}

public interface IBardDialogFactory
{
    BardDialog Create();
}