// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using AutomaticTypeMapper;
using EndlessClient.Dialogs.Services;
using EndlessClient.GameExecution;
using EOLib.Graphics;
using EOLib.Localization;
using XNAControls;

namespace EndlessClient.Dialogs.Factories
{
    [MappedType(BaseType = typeof(IEOMessageBoxFactory))]
    public class EOMessageBoxFactory : IEOMessageBoxFactory
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IGameStateProvider _gameStateProvider;
        private readonly IEODialogButtonService _eoDialogButtonService;
        private readonly ILocalizedStringFinder _localizedStringFinder;

        public EOMessageBoxFactory(INativeGraphicsManager nativeGraphicsManager,
                                   IGameStateProvider gameStateProvider,
                                   IEODialogButtonService eoDialogButtonService,
                                   ILocalizedStringFinder localizedStringFinder)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _gameStateProvider = gameStateProvider;
            _eoDialogButtonService = eoDialogButtonService;
            _localizedStringFinder = localizedStringFinder;
        }

        public IXNADialog CreateMessageBox(string message,
                                           string caption = "",
                                           EODialogButtons whichButtons = EODialogButtons.Ok,
                                           EOMessageBoxStyle style = EOMessageBoxStyle.SmallDialogSmallHeader)
        {
            var messageBox = new EOMessageBox(_nativeGraphicsManager,
                                              _gameStateProvider,
                                              _eoDialogButtonService,
                                              message,
                                              caption,
                                              style,
                                              whichButtons);

            return messageBox;
        }

        public IXNADialog CreateMessageBox(DialogResourceID resource,
                                           EODialogButtons whichButtons = EODialogButtons.Ok,
                                           EOMessageBoxStyle style = EOMessageBoxStyle.SmallDialogSmallHeader)
        {
            return CreateMessageBox(_localizedStringFinder.GetString(resource + 1),
                _localizedStringFinder.GetString(resource),
                whichButtons,
                style);
        }

        public IXNADialog CreateMessageBox(string prependData,
                                           DialogResourceID resource,
                                           EODialogButtons whichButtons = EODialogButtons.Ok,
                                           EOMessageBoxStyle style = EOMessageBoxStyle.SmallDialogSmallHeader)
        {
            var message = prependData + _localizedStringFinder.GetString(resource + 1);
            return CreateMessageBox(message,
                _localizedStringFinder.GetString(resource),
                whichButtons,
                style);
        }

        public IXNADialog CreateMessageBox(DialogResourceID resource,
                                           string extraData,
                                           EODialogButtons whichButtons = EODialogButtons.Ok,
                                           EOMessageBoxStyle style = EOMessageBoxStyle.SmallDialogSmallHeader)
        {
            var message = _localizedStringFinder.GetString(resource + 1) + extraData;
            return CreateMessageBox(message,
                _localizedStringFinder.GetString(resource),
                whichButtons,
                style);
        }
    }
}