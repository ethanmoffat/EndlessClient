using AutomaticTypeMapper;
using EndlessClient.Content;
using EndlessClient.Dialogs.Services;
using EndlessClient.HUD;
using EOLib.Domain.Character;
using EOLib.Domain.Interact.Skill;
using EOLib.Graphics;
using EOLib.IO.Repositories;
using EOLib.Localization;

namespace EndlessClient.Dialogs.Factories;

[AutoMappedType]
public class SkillmasterDialogFactory : ISkillmasterDialogFactory
{
    private readonly INativeGraphicsManager _nativeGraphicsManager;
    private readonly ISkillmasterActions _skillmasterActions;
    private readonly IEODialogButtonService _dialogButtonService;
    private readonly IEODialogIconService _dialogIconService;
    private readonly ILocalizedStringFinder _localizedStringFinder;
    private readonly IStatusLabelSetter _statusLabelSetter;
    private readonly IEOMessageBoxFactory _messageBoxFactory;
    private readonly ITextInputDialogFactory _textInputDialogFactory;
    private readonly ISkillDataProvider _skillDataProvider;
    private readonly ICharacterProvider _characterProvider;
    private readonly ICharacterInventoryProvider _characterInventoryProvider;
    private readonly IPubFileProvider _pubFileProvider;
    private readonly IContentProvider _contentProvider;

    public SkillmasterDialogFactory(INativeGraphicsManager nativeGraphicsManager,
                                    ISkillmasterActions skillmasterActions,
                                    IEODialogButtonService dialogButtonService,
                                    IEODialogIconService dialogIconService,
                                    ILocalizedStringFinder localizedStringFinder,
                                    IStatusLabelSetter statusLabelSetter,
                                    IEOMessageBoxFactory messageBoxFactory,
                                    ITextInputDialogFactory textInputDialogFactory,
                                    ISkillDataProvider skillDataProvider,
                                    ICharacterProvider characterProvider,
                                    ICharacterInventoryProvider characterInventoryProvider,
                                    IPubFileProvider pubFileProvider,
                                    IContentProvider contentProvider)
    {
        _nativeGraphicsManager = nativeGraphicsManager;
        _skillmasterActions = skillmasterActions;
        _dialogButtonService = dialogButtonService;
        _dialogIconService = dialogIconService;
        _localizedStringFinder = localizedStringFinder;
        _statusLabelSetter = statusLabelSetter;
        _messageBoxFactory = messageBoxFactory;
        _textInputDialogFactory = textInputDialogFactory;
        _skillDataProvider = skillDataProvider;
        _characterProvider = characterProvider;
        _characterInventoryProvider = characterInventoryProvider;
        _pubFileProvider = pubFileProvider;
        _contentProvider = contentProvider;
    }

    public SkillmasterDialog Create()
    {
        return new SkillmasterDialog(_nativeGraphicsManager,
                                     _skillmasterActions,
                                     _dialogButtonService,
                                     _dialogIconService,
                                     _localizedStringFinder,
                                     _statusLabelSetter,
                                     _messageBoxFactory,
                                     _textInputDialogFactory,
                                     _skillDataProvider,
                                     _characterProvider,
                                     _characterInventoryProvider,
                                     _pubFileProvider,
                                     _contentProvider);
    }
}

public interface ISkillmasterDialogFactory
{
    SkillmasterDialog Create();
}