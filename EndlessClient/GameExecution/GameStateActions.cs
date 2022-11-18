﻿using System;
using System.Collections.Generic;
using System.Linq;
using AutomaticTypeMapper;
using EndlessClient.Audio;
using EndlessClient.ControlSets;
using EndlessClient.Network;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using Microsoft.Xna.Framework;

namespace EndlessClient.GameExecution
{
    [MappedType(BaseType = typeof(IGameStateActions))]
    public class GameStateActions : IGameStateActions
    {
        private readonly IGameStateRepository _gameStateRepository;
        private readonly IControlSetRepository _controlSetRepository;
        private readonly IControlSetFactory _controlSetFactory;
        private readonly IEndlessGameProvider _endlessGameProvider;
        private readonly IPlayerInfoRepository _playerInfoRepository;
        private readonly ISfxPlayer _sfxPlayer;
        private readonly IMfxPlayer _mfxPlayer;

        public GameStateActions(IGameStateRepository gameStateRepository,
                                IControlSetRepository controlSetRepository,
                                IControlSetFactory controlSetFactory,
                                IEndlessGameProvider endlessGameProvider,
                                IPlayerInfoRepository playerInfoRepository,
                                ISfxPlayer sfxPlayer,
                                IMfxPlayer mfxPlayer)
        {
            _gameStateRepository = gameStateRepository;
            _controlSetRepository = controlSetRepository;
            _controlSetFactory = controlSetFactory;
            _endlessGameProvider = endlessGameProvider;
            _playerInfoRepository = playerInfoRepository;
            _sfxPlayer = sfxPlayer;
            _mfxPlayer = mfxPlayer;
        }

        public void ChangeToState(GameStates newState)
        {
            if (newState == _gameStateRepository.CurrentState)
                return;

            if (_gameStateRepository.CurrentState == GameStates.PlayingTheGame)
                _playerInfoRepository.PlayerIsInGame = false;

            var currentSet = _controlSetRepository.CurrentControlSet;
            var nextSet = _controlSetFactory.CreateControlsForState(newState, currentSet);

            RemoveOldComponents(currentSet, nextSet);
            AddNewComponents(nextSet);

            _gameStateRepository.CurrentState = newState;
            _controlSetRepository.CurrentControlSet = nextSet;

            switch (_gameStateRepository.CurrentState)
            {
                case GameStates.None:
                case GameStates.Initial:
                    {
                        _sfxPlayer.StopLoopingSfx();

                        // this replicates behavior of the vanilla client where returning to the main menu stops playing the background music
                        _mfxPlayer.StopBackgroundMusic();
                    }
                    break;
                case GameStates.LoggedIn: _sfxPlayer.PlaySfx(SoundEffectID.Login); break;
            }
        }

        public void RefreshCurrentState()
        {
            var currentSet = _controlSetRepository.CurrentControlSet;
            var emptySet = new EmptyControlSet();

            RemoveOldComponents(currentSet, emptySet);
            var refreshedSet = _controlSetFactory.CreateControlsForState(currentSet.GameState, emptySet);
            AddNewComponents(refreshedSet);
            _controlSetRepository.CurrentControlSet = refreshedSet;
        }

        public void ExitGame()
        {
            Game.Exit();
        }

        private void AddNewComponents(IControlSet nextSet)
        {
            foreach (var component in nextSet.AllComponents.Except(nextSet.XNAControlComponents))
                if (!Game.Components.Contains(component))
                    Game.Components.Add(component);
        }

        private void RemoveOldComponents(IControlSet currentSet, IControlSet nextSet)
        {
            var componentsToRemove = FindUnusedComponents(currentSet, nextSet);
            var disposableComponents = componentsToRemove
                .Where(x => !(x is PacketHandlerGameComponent))
                .OfType<IDisposable>();

            foreach (var component in disposableComponents)
                component.Dispose();
            foreach (var component in componentsToRemove.Where(Game.Components.Contains))
                Game.Components.Remove(component);

            currentSet.Dispose();
        }

        private List<IGameComponent> FindUnusedComponents(IControlSet current, IControlSet next)
        {
            return current.AllComponents
                .Where(component => !next.AllComponents.Contains(component))
                .ToList();
        }

        private IEndlessGame Game => _endlessGameProvider.Game;
    }
}
