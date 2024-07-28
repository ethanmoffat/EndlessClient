using AutomaticTypeMapper;
using EndlessClient.GameExecution;
using EndlessClient.HUD.Controls;
using EndlessClient.HUD.Panels;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace EndlessClient.ControlSets
{
    public interface IHudControlProvider
    {
        bool IsInGame { get; }

        InGameControlSet ControlSet { get; }

        IReadOnlyList<IHudPanel> HudPanels { get; }

        T GetComponent<T>(HudControlIdentifier identifier) where T : IGameComponent;
    }

    [MappedType(BaseType = typeof(IHudControlProvider), IsSingleton = true)]
    public class HudControlProvider : IHudControlProvider
    {
        private readonly IGameStateProvider _gameStateProvider;
        private readonly IControlSetProvider _controlSetProvider;

        public bool IsInGame => _gameStateProvider.CurrentState == GameStates.PlayingTheGame &&
                                _controlSetProvider.CurrentControlSet.GameState == GameStates.PlayingTheGame &&
                                _controlSetProvider.CurrentControlSet is InGameControlSet;

        public InGameControlSet ControlSet
        {
            get
            {
                if (!IsInGame)
                    throw new InvalidOperationException("Not currently in game, in game control set does not exist");

                return (InGameControlSet)_controlSetProvider.CurrentControlSet;
            }
        }

        public IReadOnlyList<IHudPanel> HudPanels => new[]
        {
            GetComponent<IHudPanel>(HudControlIdentifier.NewsPanel),
            GetComponent<IHudPanel>(HudControlIdentifier.InventoryPanel),
            GetComponent<IHudPanel>(HudControlIdentifier.ActiveSpellsPanel),
            GetComponent<IHudPanel>(HudControlIdentifier.PassiveSpellsPanel),
            GetComponent<IHudPanel>(HudControlIdentifier.ChatPanel),
            GetComponent<IHudPanel>(HudControlIdentifier.StatsPanel),
            GetComponent<IHudPanel>(HudControlIdentifier.OnlineListPanel),
            GetComponent<IHudPanel>(HudControlIdentifier.PartyPanel),
            //macro panel
            GetComponent<IHudPanel>(HudControlIdentifier.SettingsPanel),
            GetComponent<IHudPanel>(HudControlIdentifier.HelpPanel)
        };

        public HudControlProvider(IGameStateProvider gameStateProvider,
                                  IControlSetProvider controlSetProvider)
        {
            _gameStateProvider = gameStateProvider;
            _controlSetProvider = controlSetProvider;
        }

        public T GetComponent<T>(HudControlIdentifier identifier) where T : IGameComponent
        {
            return ControlSet.GetHudComponent<T>(identifier);
        }
    }
}