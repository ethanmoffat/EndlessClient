using System;
using System.Collections.Generic;
using System.Linq;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Map;

namespace EOLib.Domain.Pathing
{
    // Implemented from https://en.wikipedia.org/wiki/A*_search_algorithm#Pseudocode
    [AutoMappedType]
    public class AStarPathFinder : IPathFinder
    {
        private readonly IMapCellStateProvider _cellStateProvider;
        private readonly IWalkValidationActions _walkValidationActions;

        public AStarPathFinder(IMapCellStateProvider cellStateProvider,
            IWalkValidationActions walkValidationActions)
        {
            _cellStateProvider = cellStateProvider;
            _walkValidationActions = walkValidationActions;
        }

        public Queue<MapCoordinate> FindPath(MapCoordinate start, MapCoordinate finish, bool shouldAvoidWarps = false)
        {
            if (start == finish)
                return new Queue<MapCoordinate>();

            var openSet = new HashSet<MapCoordinate>(new[] { start });
            var cameFrom = new Dictionary<MapCoordinate, MapCoordinate>();

            var scores = new Dictionary<MapCoordinate, int>
            {
                { start, 0 }
            };

            var guessScores = new Dictionary<MapCoordinate, int>
            {
                { start, Heuristic(start, finish) }
            };

            while (openSet.Any())
            {
                var current = new MapCoordinate(0, 0);
                var lowest = int.MaxValue;
                foreach (var n in openSet)
                {
                    if (guessScores[n] < lowest)
                    {
                        current = n;
                        lowest = guessScores[n];
                    }
                }

                if (current.Equals(finish))
                    return ReconstructPath(start, cameFrom, current);

                openSet.Remove(current);
                foreach (var neighbor in GetNeighbors(current, shouldAvoidWarps))
                {
                    var tentativeScore = scores[current] + 1;
                    if (!scores.TryGetValue(neighbor, out var _))
                        scores.Add(neighbor, int.MaxValue);

                    if (tentativeScore < scores[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        scores[neighbor] = tentativeScore;
                        guessScores[neighbor] = tentativeScore + Heuristic(neighbor, finish);

                        if (!openSet.Contains(neighbor))
                            openSet.Add(neighbor);
                    }
                }
            }

            return new Queue<MapCoordinate>();
        }

        private static int Heuristic(MapCoordinate current, MapCoordinate goal)
            => Math.Abs(current.X - goal.X) + Math.Abs(current.Y - goal.Y);

        private Queue<MapCoordinate> ReconstructPath(MapCoordinate start, Dictionary<MapCoordinate, MapCoordinate> cameFrom, MapCoordinate current)
        {
            var retList = new List<MapCoordinate> { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                if (current != start)
                    retList.Insert(0, current);
            }
            return new Queue<MapCoordinate>(retList);
        }

        private IEnumerable<MapCoordinate> GetNeighbors(MapCoordinate current, bool shouldAvoidWarps)
        {
            var points = new MapCoordinate[]
            {
                new MapCoordinate(-1, 0),
                new MapCoordinate(1, 0),
                new MapCoordinate(0, -1),
                new MapCoordinate(0, 1)
            };

            foreach (var coordinateOffset in points)
            {
                var cs = _cellStateProvider.GetCellStateAt(current.X + coordinateOffset.X, current.Y + coordinateOffset.Y);

                if (shouldAvoidWarps && cs.Warp.HasValue)
                    continue;

                if (_walkValidationActions.IsCellStateWalkable(cs) == WalkValidationResult.Walkable)
                    yield return new MapCoordinate(current.X + coordinateOffset.X, current.Y + coordinateOffset.Y);
            }
        }
    }

    public interface IPathFinder
    {
        Queue<MapCoordinate> FindPath(MapCoordinate start, MapCoordinate finish, bool shouldAvoidWarps = false);
    }
}
