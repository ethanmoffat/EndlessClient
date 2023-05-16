using AutomaticTypeMapper;
using EndlessClient.Controllers;
using EndlessClient.ControlSets;
using EndlessClient.HUD.Controls;
using EndlessClient.HUD.Spells;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.NPC;
using EOLib.Domain.Board;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.Map;
using EOLib.IO.Map;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Input;
using MonoGame.Extended.Input.InputListeners;
using Optional;
using System;
using System.Collections.Generic;
using System.Linq;
using XNAControls;

using DomainCharacter = EOLib.Domain.Character.Character;
using DomainNPC = EOLib.Domain.NPC.NPC;

namespace EndlessClient.Rendering.Map
{
    public class ClickDispatcher : XNAControl, IClickDispatcher
    {
        private readonly IHudControlProvider _hudControlProvider;
        private readonly IClientWindowSizeProvider _clientWindowSizeProvider;
        private readonly ICurrentMapProvider _currentMapProvider;
        private readonly ICurrentMapStateProvider _currentMapStateProvider;
        private readonly ICharacterProvider _characterProvider;
        private readonly IMapCellStateProvider _mapCellStateProvider;
        private readonly ICharacterRendererProvider _characterRendererProvider;
        private readonly INPCRendererProvider _npcRendererProvider;
        private readonly ISpellSlotDataProvider _spellSlotDataProvider;
        private readonly IContextMenuRepository _contextMenuRepository;
        private readonly IMapObjectBoundsCalculator _mapObjectBoundsCalculator;
        private readonly IGridDrawCoordinateCalculator _gridDrawCoordinateCalculator;
        private readonly IMapInteractionController _mapInteractionController;
        private readonly INPCInteractionController _npcInteractionController;

        public override Rectangle EventArea => new(0, 0, _clientWindowSizeProvider.Width, _clientWindowSizeProvider.Height);

        public ClickDispatcher(IHudControlProvider hudControlProvider,
                               IClientWindowSizeProvider clientWindowSizeProvider,
                               ICurrentMapProvider currentMapProvider,
                               ICurrentMapStateProvider currentMapStateProvider,
                               ICharacterProvider characterProvider,
                               IMapCellStateProvider mapCellStateProvider,
                               ICharacterRendererProvider characterRendererProvider,
                               INPCRendererProvider npcRendererProvider,
                               ISpellSlotDataProvider spellSlotDataProvider,
                               IContextMenuRepository contextMenuRepository,

                               IMapObjectBoundsCalculator mapObjectBoundsCalculator,
                               IGridDrawCoordinateCalculator gridDrawCoordinateCalculator,

                               IMapInteractionController mapInteractionController,
                               INPCInteractionController npcInteractionController)
        {
            _hudControlProvider = hudControlProvider;
            _clientWindowSizeProvider = clientWindowSizeProvider;
            _currentMapProvider = currentMapProvider;
            _currentMapStateProvider = currentMapStateProvider;
            _characterProvider = characterProvider;
            _mapCellStateProvider = mapCellStateProvider;
            _characterRendererProvider = characterRendererProvider;
            _npcRendererProvider = npcRendererProvider;
            _spellSlotDataProvider = spellSlotDataProvider;
            _contextMenuRepository = contextMenuRepository;
            _mapObjectBoundsCalculator = mapObjectBoundsCalculator;
            _gridDrawCoordinateCalculator = gridDrawCoordinateCalculator;

            _mapInteractionController = mapInteractionController;
            _npcInteractionController = npcInteractionController;
        }

        protected override bool HandleClick(IXNAControl control, MouseEventArgs eventArgs)
        {
            _contextMenuRepository.ContextMenu.MatchSome(contextMenu =>
            {
                Game.Components.Remove(contextMenu);
                contextMenu.Dispose();

                _contextMenuRepository.ContextMenu = Option.None<IContextMenuRenderer>();
            });

            var mapRenderer = _hudControlProvider.GetComponent<IMapRenderer>(HudControlIdentifier.MapRenderer);
            var cellState = _mapCellStateProvider.GetCellStateAt(mapRenderer.GridCoordinates);

            // map items should always get click priority so they can be picked up even when under a character/npc/other object
            var mapItemPickupResult = CheckForMapItemClicks(cellState, eventArgs);
            return CheckForEntityClicks(eventArgs) || CheckForTileClicks(cellState, eventArgs) || mapItemPickupResult;
        }

        private bool CheckForMapItemClicks(IMapCellState cellState, MouseEventArgs eventArgs)
        {
            if (eventArgs.Button != MouseButton.Left) return false;

            if (cellState.Items.Any())
                _mapInteractionController.LeftClick(cellState);

            return cellState.Items.Any();
        }

        private bool CheckForEntityClicks(MouseEventArgs eventArgs)
        {
            var entities = new List<IMapEntity>(_currentMapStateProvider.Characters);
            entities.Add(_characterProvider.MainCharacter);
            entities.AddRange(_currentMapStateProvider.NPCs);
            entities.AddRange(_currentMapProvider.CurrentMap.Signs);
            entities.AddRange(_currentMapProvider.CurrentMap
                .GetTileSpecs(TileSpec.Board1, TileSpec.Board2, TileSpec.Board3, TileSpec.Board4,
                              TileSpec.Board5, TileSpec.Board6, TileSpec.Board7, TileSpec.Board8)
                .Select(x => new BoardMapEntity(x.X, x.Y)));

            entities.Sort((a, b) =>
            {
                return (a.Y * _currentMapProvider.CurrentMap.Properties.Width + a.X)
                    .CompareTo(b.Y * _currentMapProvider.CurrentMap.Properties.Width + b.X) * -1;
            });

            foreach (var entity in entities)
            {
                var bounds = GetEntityBounds(entity);

                if (bounds.Contains(eventArgs.Position))
                {
                    if (DispatchClickToEntity(entity, eventArgs))
                    {
                        _hudControlProvider.GetComponent<ICharacterAnimator>(HudControlIdentifier.CharacterAnimator)
                            .CancelClickToWalk();
                        return true;
                    }
                }
            }

            return false;
        }

        private bool CheckForTileClicks(IMapCellState cellState, MouseEventArgs eventArgs)
        {
            if (eventArgs.Button == MouseButton.Left)
            {
                _mapInteractionController.LeftClick(cellState);
                return true;
            }

            return false;
        }

        private bool DispatchClickToEntity(IMapEntity entity, MouseEventArgs eventArgs)
        {
            return entity switch
            {
                DomainCharacter c => HandleCharacterClick(c, eventArgs.Button),
                DomainNPC n => eventArgs.Button == MouseButton.Left && HandleNPCClick(n, eventArgs.Position),
                SignMapEntity s => eventArgs.Button == MouseButton.Left && HandleSignClick(s),
                BoardMapEntity b => eventArgs.Button == MouseButton.Left && HandleBoardClick(b),
                _ => throw new ArgumentException()
            };
        }

        private Rectangle GetEntityBounds(IMapEntity entity)
        {
            return entity switch
            {
                DomainCharacter c => GetCharacterRendererArea(c.ID),
                DomainNPC n => GetNPCRendererArea(n.Index),
                SignMapEntity or BoardMapEntity => GetObjectBounds(entity),
                _ => throw new ArgumentException()
            };
        }

        private Rectangle GetCharacterRendererArea(int characterId)
        {
            return _characterRendererProvider.MainCharacterRenderer.Match(
                some: mcr => mcr.Character.ID == characterId ? mcr.DrawArea : RendererDrawAreaOrEmpty(characterId),
                none: () => RendererDrawAreaOrEmpty(characterId));

            Rectangle RendererDrawAreaOrEmpty(int id) => _characterRendererProvider.CharacterRenderers.ContainsKey(characterId)
                    ? _characterRendererProvider.CharacterRenderers[characterId].DrawArea
                    : Rectangle.Empty;
        }

        private Rectangle GetNPCRendererArea(int npcIndex)
        {
            return _npcRendererProvider.NPCRenderers.ContainsKey(npcIndex)
                ? _npcRendererProvider.NPCRenderers[npcIndex].DrawArea
                : Rectangle.Empty;
        }

        private Rectangle GetObjectBounds(IMapEntity entity)
        {
            var gfx = _currentMapProvider.CurrentMap.GFX[MapLayer.Objects][entity.Y, entity.X];
            return gfx > 0
                ? _mapObjectBoundsCalculator.GetMapObjectBounds(entity.X, entity.Y, gfx)
                // if there is no map object GFX at the sign's position, use the default tile coordinates
                : new Rectangle(
                    _gridDrawCoordinateCalculator.CalculateBaseLayerDrawCoordinatesFromGridUnits(entity.X, entity.Y).ToPoint(),
                    new Point(64, 32));
        }

        private bool HandleCharacterClick(DomainCharacter c, MouseButton button)
        {
            switch (button)
            {
                case MouseButton.Left: return _mapInteractionController.LeftClick(c);
                case MouseButton.Right: _mapInteractionController.RightClick(c); break;
                default: return false;
            }

            return true;
        }

        private bool HandleNPCClick(DomainNPC n, Point currentMousePosition)
        {
            var renderer = _npcRendererProvider.NPCRenderers[n.Index];

            if (_spellSlotDataProvider.SpellIsPrepared)
            {
                _mapInteractionController.LeftClick(n);
            }
            else if (renderer.IsClickablePixel(currentMousePosition))
            {
                _npcInteractionController.ShowNPCDialog(n);
            }

            return true;
        }

        private bool HandleSignClick(SignMapEntity s)
        {
            var cellState = new MapCellState
            {
                Sign = Option.Some(new Sign(s)),
                Coordinate = new MapCoordinate(s.X, s.Y)
            };

            _mapInteractionController.LeftClick(cellState);

            return true;
        }

        private bool HandleBoardClick(BoardMapEntity b)
        {
            var cellState = new MapCellState
            {
                Coordinate = new MapCoordinate(b.X, b.Y),
                TileSpec = _currentMapProvider.CurrentMap.Tiles[b.Y, b.X]
            };

            _mapInteractionController.LeftClick(cellState);

            return false;
        }
    }

    public interface IClickDispatcher : IGameComponent
    {
        int DrawOrder { get; set; }
    }
}
