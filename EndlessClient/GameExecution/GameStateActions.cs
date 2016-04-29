// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using EndlessClient.ControlSets;
using Microsoft.Xna.Framework;
using XNAControls;

namespace EndlessClient.GameExecution
{
	public class GameStateActions : IGameStateActions
	{
		private readonly IGameStateRepository _gameStateRepository;
		private readonly IControlSetRepository _controlSetRepository;
		private readonly IControlSetFactory _controlSetFactory;
		private readonly IEndlessGameProvider _endlessGameProvider;

		public GameStateActions(IGameStateRepository gameStateRepository,
								IControlSetRepository controlSetRepository,
								IControlSetFactory controlSetFactory,
								IEndlessGameProvider endlessGameProvider)
		{
			_gameStateRepository = gameStateRepository;
			_controlSetRepository = controlSetRepository;
			_controlSetFactory = controlSetFactory;
			_endlessGameProvider = endlessGameProvider;
		}

		public void ChangeToState(GameStates newState)
		{
			if (newState == _gameStateRepository.CurrentState)
				return;

			var currentSet = _controlSetRepository.CurrentControlSet;
			var nextSet = _controlSetFactory.CreateControlsForState(newState, currentSet);
			
			RemoveOldComponents(currentSet, nextSet);
			AddNewComponents(nextSet);

			_gameStateRepository.CurrentState = newState;
			_controlSetRepository.CurrentControlSet = nextSet;
		}

		public void RefreshCurrentState()
		{
			var currentSet = _controlSetRepository.CurrentControlSet;
			var emptySet = new EmptyControlSet();

			RemoveOldComponents(currentSet, emptySet);
			currentSet.InitializeControls(emptySet);
		}

		public void ExitGame()
		{
			Game.Exit();
		}

		private void AddNewComponents(IControlSet nextSet)
		{
			foreach (var component in nextSet.AllComponents)
				if (!Game.Components.Contains(component))
					Game.Components.Add(component);
		}

		private void RemoveOldComponents(IControlSet currentSet, IControlSet nextSet)
		{
			var componentsToRemove = FindUnusedComponents(currentSet, nextSet);
			var xnaControlComponents = componentsToRemove.OfType<XNAControl>().ToList();
			var otherDisposableComponents = componentsToRemove.Except(xnaControlComponents).OfType<IDisposable>().ToList();

			foreach (var component in xnaControlComponents)
				component.Close();
			foreach (var component in otherDisposableComponents)
				component.Dispose();
		}

		private List<IGameComponent> FindUnusedComponents(IControlSet current, IControlSet next)
		{
			return current.AllComponents
				.Where(component => !next.AllComponents.Contains(component))
				.ToList();
		}

		private IEndlessGame Game { get { return _endlessGameProvider.Game; } }
	}
}
