﻿using AutomaticTypeMapper;
using EndlessClient.Dialogs.Services;
using EndlessClient.GameExecution;
using EOLib.Config;
using EOLib.Graphics;
using EOLib.Localization;
using XNAControls;

namespace EndlessClient.Dialogs.Factories
{
    [MappedType(BaseType = typeof(ICreateAccountProgressDialogFactory))]
    public class CreateAccountProgressDialogFactory : ICreateAccountProgressDialogFactory
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IGameStateProvider _gameStateProvider;
        private readonly IConfigurationProvider _configProvider;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly IEODialogButtonService _eoDialogButtonService;

        public CreateAccountProgressDialogFactory(INativeGraphicsManager nativeGraphicsManager,
                                                  IGameStateProvider gameStateProvider,
                                                  IConfigurationProvider configProvider,
                                                  ILocalizedStringFinder localizedStringFinder,
                                                  IEODialogButtonService eoDialogButtonService)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _gameStateProvider = gameStateProvider;
            _configProvider = configProvider;
            _localizedStringFinder = localizedStringFinder;
            _eoDialogButtonService = eoDialogButtonService;
        }

        public IXNADialog BuildCreateAccountProgressDialog()
        {
            var message = _localizedStringFinder.GetString(DialogResourceID.ACCOUNT_CREATE_ACCEPTED + 1);
            var caption = _localizedStringFinder.GetString(DialogResourceID.ACCOUNT_CREATE_ACCEPTED);

            return new ProgressDialog(_nativeGraphicsManager,
                                      _gameStateProvider,
                                      _configProvider,
                                      _eoDialogButtonService,
                                      message, caption);
        }
    }

    public interface ICreateAccountProgressDialogFactory
    {
        IXNADialog BuildCreateAccountProgressDialog();
    }
}