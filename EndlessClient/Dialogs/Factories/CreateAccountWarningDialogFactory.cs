// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.GameExecution;
using EOLib.Graphics;

namespace EndlessClient.Dialogs.Factories
{
    public class CreateAccountWarningDialogFactory : ICreateAccountWarningDialogFactory
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IGraphicsDeviceProvider _graphicsDeviceProvider;
        private readonly IGameStateProvider _gameStateProvider;

        public CreateAccountWarningDialogFactory(
            INativeGraphicsManager nativeGraphicsManager,
            IGraphicsDeviceProvider graphicsDeviceProvider,
            IGameStateProvider gameStateProvider)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _graphicsDeviceProvider = graphicsDeviceProvider;
            _gameStateProvider = gameStateProvider;
        }

        public void ShowCreateAccountWarningDialog(string warningMessage)
        {
            var warningDialog = new ScrollingMessageDialog(
                _nativeGraphicsManager,
                _graphicsDeviceProvider,
                _gameStateProvider)
            {
                MessageText = warningMessage
            };

            warningDialog.Show();
        }
    }
}
