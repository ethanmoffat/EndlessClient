using System;
using System.Linq;
using AutomaticTypeMapper;
using EndlessClient.ControlSets;
using EndlessClient.HUD.Panels;
using EndlessClient.Rendering;
using EOLib.Domain.Map;
using EOLib.IO.Repositories;
using EOLib.Localization;

namespace EndlessClient.HUD
{
    [AutoMappedType]
    public class HudStateActions : IHudStateActions
    {
        private const int HUD_CONTROL_LAYER = 130;

        private readonly IStatusLabelSetter _statusLabelSetter;
        private readonly IHudControlProvider _hudControlProvider;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly IMapFileProvider _mapFileProvider;
        private readonly IClientWindowSizeProvider _clientWindowSizeProvider;

        public HudStateActions(IStatusLabelSetter statusLabelSetter,
                               IHudControlProvider hudControlProvider,
                               ICurrentMapStateRepository currentMapStateRepository,
                               IMapFileProvider mapFileProvider,
                               IClientWindowSizeProvider clientWindowSizeProvider)
        {
            _statusLabelSetter = statusLabelSetter;
            _hudControlProvider = hudControlProvider;
            _currentMapStateRepository = currentMapStateRepository;
            _mapFileProvider = mapFileProvider;
            _clientWindowSizeProvider = clientWindowSizeProvider;
        }

        public void SwitchToState(InGameStates newState)
        {
            if (!_hudControlProvider.IsInGame)
                return;

            if (newState != InGameStates.News)
                _hudControlProvider.GetComponent<IHudPanel>(Controls.HudControlIdentifier.NewsPanel).Visible = false;

            foreach (var panel in _hudControlProvider.HudPanels.Where(x => x.Visible))
                panel.Visible = _clientWindowSizeProvider.Resizable;

            var targetPanel = _hudControlProvider.HudPanels.Single(x => IsPanelForRequestedState(x, newState));
            targetPanel.Visible = !targetPanel.Visible;

            if (targetPanel.Visible && _clientWindowSizeProvider.Resizable)
                targetPanel.DrawOrder = _hudControlProvider.HudPanels.Select(x => x.DrawOrder).Max() + 1;
        }

        public void ToggleMapView()
        {
            var mapFile = _mapFileProvider.MapFiles[_currentMapStateRepository.CurrentMapID];
            if (!mapFile.Properties.MapAvailable)
            {
                _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING, EOResourceID.STATUS_LABEL_NO_MAP_OF_AREA);
                return;
            }

            _currentMapStateRepository.ShowMiniMap = !_currentMapStateRepository.ShowMiniMap;
        }

        private bool IsPanelForRequestedState(IHudPanel hudPanel, InGameStates newState)
        {
            switch (newState)
            {
                case InGameStates.News: return hudPanel is NewsPanel;
                case InGameStates.Inventory: return hudPanel is InventoryPanel;
                case InGameStates.ActiveSpells: return hudPanel is ActiveSpellsPanel;
                case InGameStates.PassiveSpells: return hudPanel is PassiveSpellsPanel;
                case InGameStates.Chat: return hudPanel is ChatPanel;
                case InGameStates.Stats: return hudPanel is StatsPanel;
                case InGameStates.OnlineList: return hudPanel is OnlineListPanel;
                case InGameStates.Party: return hudPanel is PartyPanel;
                case InGameStates.Settings: return hudPanel is SettingsPanel;
                case InGameStates.Help: return hudPanel is HelpPanel;
                default: throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
            }
        }
    }
}
