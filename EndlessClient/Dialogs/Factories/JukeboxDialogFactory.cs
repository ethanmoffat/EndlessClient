using AutomaticTypeMapper;
using EndlessClient.Dialogs.Services;
using EOLib.Graphics;

namespace EndlessClient.Dialogs.Factories
{
    [AutoMappedType]
    public class JukeboxDialogFactory : IJukeboxDialogFactory
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IEODialogButtonService _dialogButtonService;
        private readonly IEODialogIconService _dialogIconService;

        public JukeboxDialogFactory(INativeGraphicsManager nativeGraphicsManager,
                                    IEODialogButtonService dialogButtonService,
                                    IEODialogIconService dialogIconService)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _dialogButtonService = dialogButtonService;
            _dialogIconService = dialogIconService;
        }

        public JukeboxDialog Create()
        {
            return new JukeboxDialog(_nativeGraphicsManager,
                                     _dialogButtonService,
                                     _dialogIconService);
        }
    }

    public interface IJukeboxDialogFactory
    {
        JukeboxDialog Create();
    }
}
