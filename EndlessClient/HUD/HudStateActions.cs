// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Linq;
using EndlessClient.ControlSets;
using EndlessClient.HUD.Panels;
using EOLib;
using EOLib.Domain.Map;
using EOLib.IO.Repositories;
using EOLib.Localization;

namespace EndlessClient.HUD
{
    public class HudStateActions : IHudStateActions
    {
        private readonly IStatusLabelSetter _statusLabelSetter;
        private readonly IHudControlProvider _hudControlProvider;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly IMapFileProvider _mapFileProvider;

        public HudStateActions(IStatusLabelSetter statusLabelSetter,
                               IHudControlProvider hudControlProvider,
                               ICurrentMapStateRepository currentMapStateRepository,
                               IMapFileProvider mapFileProvider)
        {
            _statusLabelSetter = statusLabelSetter;
            _hudControlProvider = hudControlProvider;
            _currentMapStateRepository = currentMapStateRepository;
            _mapFileProvider = mapFileProvider;
        }

        public void SwitchToState(InGameStates newState)
        {
            if (!_hudControlProvider.IsInGame)
                return;

            _hudControlProvider.HudPanels.Single(x => x.Visible).Visible = false;
            _hudControlProvider.HudPanels.Single(x => IsPanelForRequestedState(x, newState)).Visible = true;
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
                case InGameStates.Inventory: return hudPanel is InventoryPanel;
                case InGameStates.ActiveSpells: return hudPanel is ActiveSpellsPanel;
                case InGameStates.PassiveSpells: return hudPanel is PassiveSpellsPanel;
                case InGameStates.Chat: return hudPanel is ChatPanel;
                case InGameStates.Stats: return hudPanel is StatsPanel;
                case InGameStates.OnlineList: return hudPanel is OnlineListPanel;
                case InGameStates.Party: return hudPanel is PartyPanel;
                case InGameStates.Settings: return hudPanel is SettingsPanel;
                case InGameStates.Help: return hudPanel is HelpPanel;
                default: throw new ArgumentOutOfRangeException("newState", newState, null);
            }
        }
    }
}