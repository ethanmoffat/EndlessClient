using AutomaticTypeMapper;
using EndlessClient.Dialogs.Services;
using EOLib.Graphics;

namespace EndlessClient.Dialogs.Factories;

[AutoMappedType]
public class ScrollingListDialogFactory : IScrollingListDialogFactory
{
    private readonly INativeGraphicsManager _nativeGraphicsManager;
    private readonly IEODialogButtonService _dialogButtonService;

    public ScrollingListDialogFactory(INativeGraphicsManager nativeGraphicsManager,
                                      IEODialogButtonService dialogButtonService)
    {
        _nativeGraphicsManager = nativeGraphicsManager;
        _dialogButtonService = dialogButtonService;
    }

    public ScrollingListDialog Create(DialogType size)
    {
        return new ScrollingListDialog(_nativeGraphicsManager, _dialogButtonService, size);
    }
}

public interface IScrollingListDialogFactory
{
    ScrollingListDialog Create(DialogType size);
}