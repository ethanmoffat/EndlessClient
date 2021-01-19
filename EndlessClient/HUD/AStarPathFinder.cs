using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EndlessClient.HUD
{
    // Implemented from https://en.wikipedia.org/wiki/A*_search_algorithm#Pseudocode
    [AutoMappedType]
    public class AStarPathFinder : IPathFinder
    {
        private readonly IWalkValidationActions _walkValidationActions;

        public AStarPathFinder(IWalkValidationActions walkValidationActions)
        {
            _walkValidationActions = walkValidationActions;
        }

        public List<MapCoordinate> FindPath(MapCoordinate start, MapCoordinate finish)
        {
            var openSet = new HashSet<MapCoordinate>(new[] { start });
            var cameFrom = new Dictionary<MapCoordinate, MapCoordinate>();

            var scores = new Dictionary<MapCoordinate, int>
            {
                { start, 0 }
            };

            var guessScores = new Dictionary<MapCoordinate, int>
            {
                { start, heuristic(start, finish) }
            };

            while (openSet.Any())
            {
                MapCoordinate current = new MapCoordinate(0, 0);
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
                    return reconstructPath(cameFrom, current);

                openSet.Remove(current);
                foreach (var neighbor in getNeighbors(current))
                {
                    var tentativeScore = scores[current] + 1;
                    if (!scores.TryGetValue(neighbor, out var _))
                        scores.Add(neighbor, int.MaxValue);

                    if (tentativeScore < scores[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        scores[neighbor] = tentativeScore;
                        guessScores[neighbor] = tentativeScore + heuristic(neighbor, finish);

                        if (!openSet.Contains(neighbor))
                            openSet.Add(neighbor);
                    }
                }
            }

            return new List<MapCoordinate>();
        }

        private static int heuristic(MapCoordinate current, MapCoordinate goal)
            => Math.Abs(current.X - goal.X) + Math.Abs(current.Y - goal.Y);

        private List<MapCoordinate> reconstructPath(Dictionary<MapCoordinate, MapCoordinate> cameFrom, MapCoordinate current)
        {
            var retList = new List<MapCoordinate> { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                retList.Insert(0, current);
            }
            return retList;
        }

        private IEnumerable<MapCoordinate> getNeighbors(MapCoordinate current)
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
                if (_walkValidationActions.CanMoveToCoordinates(current.X + coordinateOffset.X, current.Y + coordinateOffset.Y))
                    yield return new MapCoordinate(current.X + coordinateOffset.X, current.Y + coordinateOffset.Y);
            }
        }
    }

    public interface IPathFinder
    {
        List<MapCoordinate> FindPath(MapCoordinate start, MapCoordinate finish);
    }
}
