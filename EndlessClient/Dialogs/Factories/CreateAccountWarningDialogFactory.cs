// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Dialogs.Services;
using EndlessClient.GameExecution;
using EOLib.Graphics;

namespace EndlessClient.Dialogs.Factories
{
    public class CreateAccountWarningDialogFactory : ICreateAccountWarningDialogFactory
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IGameStateProvider _gameStateProvider;
        private readonly IEODialogButtonService _eoDialogButtonService;

        public CreateAccountWarningDialogFactory(
            INativeGraphicsManager nativeGraphicsManager,
            IGameStateProvider gameStateProvider,
            IEODialogButtonService eoDialogButtonService)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _gameStateProvider = gameStateProvider;
            _eoDialogButtonService = eoDialogButtonService;
        }

        public void ShowCreateAccountWarningDialog(string warningMessage)
        {
            var warningDialog = new ScrollingMessageDialog(
                _nativeGraphicsManager,
                _gameStateProvider,
                _eoDialogButtonService)
            {
                MessageText = warningMessage
            };

            warningDialog.ShowDialog();
        }
    }
}
