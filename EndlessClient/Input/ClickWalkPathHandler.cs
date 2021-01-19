using EndlessClient.Controllers;
using EndlessClient.GameExecution;
using EndlessClient.HUD;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.Map;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EndlessClient.Input
{
    public class ClickWalkPathHandler : GameComponent, IClickWalkPathHandler
    {
        private readonly IArrowKeyController _arrowKeyController;
        private readonly ICharacterProvider _characterProvider;
        private readonly IPathFinder _pathFinder;

        private MapCoordinate _walkTarget;
        private List<MapCoordinate> _walkPath;

        public ClickWalkPathHandler(IEndlessGameProvider endlessGameProvider,
                                    IArrowKeyController arrowKeyController,
                                    ICharacterProvider characterProvider,
                                    IPathFinder pathFinder)
            : base((Game)endlessGameProvider.Game)
        {
            _arrowKeyController = arrowKeyController;
            _characterProvider = characterProvider;
            _pathFinder = pathFinder;

            _walkPath = new List<MapCoordinate>();
        }

        public override void Update(GameTime gameTime)
        {
            if (_walkPath.Any())
            {
                if (!_characterProvider.MainCharacter.RenderProperties.IsActing(CharacterActionState.Walking))
                {
                    var characterCoord = new MapCoordinate(RenderProperties.MapX, RenderProperties.MapY);
                    var next = _walkPath.First();
                    _walkPath.RemoveAt(0);

                    var diff = next - characterCoord;
                    if (Math.Abs(diff.X) > 1 || Math.Abs(diff.Y) > 1)
                    {
                        characterCoord = new MapCoordinate(RenderProperties.GetDestinationX(), RenderProperties.GetDestinationY());
                        diff = next - characterCoord;
                    }

                    if (diff.X != 0 && diff.Y != 0)
                        throw new InvalidOperationException("Trying to move in a diagonal. Something is wrong with the A* implementation.");

                    if (diff.X < 0)
                    {
                        _arrowKeyController.MoveLeft(faceAndMove: true);
                    }
                    else if (diff.X > 0)
                    {
                        _arrowKeyController.MoveRight(faceAndMove: true);
                    }
                    else if (diff.Y < 0)
                    {
                        _arrowKeyController.MoveUp(faceAndMove: true);
                    }
                    else if (diff.Y > 0)
                    {
                        _arrowKeyController.MoveDown(faceAndMove: true);
                    }
                }
                else
                {
                    var characterCoord = new MapCoordinate(RenderProperties.GetDestinationX(), RenderProperties.GetDestinationY());
                    _walkPath = _pathFinder.FindPath(characterCoord, _walkTarget);
                }
            }

            base.Update(gameTime);
        }

        public void StartWalking(MapCoordinate destination)
        {
            var characterPosition = new MapCoordinate(RenderProperties.MapX, RenderProperties.MapY);
            _walkPath = _pathFinder.FindPath(characterPosition, _walkTarget = destination);
        }

        public void CancelWalking()
        {
            _walkPath.Clear();
        }

        private ICharacterRenderProperties RenderProperties => _characterProvider.MainCharacter.RenderProperties;
    }

    public interface IClickWalkPathHandler : IGameComponent
    {
        void StartWalking(MapCoordinate destination);

        void CancelWalking();
    }
}
