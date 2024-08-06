using AutomaticTypeMapper;
using EndlessClient.Dialogs.Services;
using EOLib.Domain.Character;
using EOLib.Graphics;
using EOLib.IO.Repositories;

namespace EndlessClient.Dialogs.Factories
{
    [AutoMappedType]
    public class BookDialogFactory : IBookDialogFactory
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IEODialogButtonService _eoDialogButtonService;
        private readonly IPubFileProvider _pubFileProvider;
        private readonly IPaperdollProvider _paperdollProvider;

        public BookDialogFactory(INativeGraphicsManager nativeGraphicsManager,
                                 IEODialogButtonService eoDialogButtonService,
                                 IPubFileProvider pubFileProvider,
                                 IPaperdollProvider paperdollProvider)
        {

            _nativeGraphicsManager = nativeGraphicsManager;
            _eoDialogButtonService = eoDialogButtonService;
            _pubFileProvider = pubFileProvider;
            _paperdollProvider = paperdollProvider;
        }

        public BookDialog Create(Character character, bool isMainCharacter)
        {
            return new BookDialog(_nativeGraphicsManager,
                _eoDialogButtonService,
                _pubFileProvider,
                _paperdollProvider,
                character,
                isMainCharacter);
        }
    }

    public interface IBookDialogFactory
    {
        BookDialog Create(Character character, bool isMainCharacter);
    }
}
