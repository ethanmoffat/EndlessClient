using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EOLib.Domain.Map
{
    public class MapEntityCollectionHashSet<TValue> : IReadOnlyMapEntityCollection<TValue>
    {
        private readonly Dictionary<int, int> _uniqueIdToHash;
        private readonly Dictionary<MapCoordinate, HashSet<int>> _mapCoordinateToHashList;

        private readonly Dictionary<int, TValue> _valueSet;

        private readonly Func<TValue, int> _uniqueIdSelector;
        private readonly Func<TValue, MapCoordinate> _mapCoordinateSelector;

        public MapEntityCollectionHashSet(Func<TValue, int> uniqueIdSelector,
                                          Func<TValue, MapCoordinate> mapCoordinateSelector)
        {
            _uniqueIdToHash = new Dictionary<int, int>();
            _mapCoordinateToHashList = new Dictionary<MapCoordinate, HashSet<int>>();
            _valueSet = new Dictionary<int, TValue>();

            _uniqueIdSelector = uniqueIdSelector;
            _mapCoordinateSelector = mapCoordinateSelector;
        }

        public MapEntityCollectionHashSet(Func<TValue, int> uniqueIdSelector,
                                          Func<TValue, MapCoordinate> mapCoordinateSelector,
                                          IEnumerable<TValue> values)
            : this(uniqueIdSelector, mapCoordinateSelector)
        {
            foreach (var value in values)
            {
                Add(value);
            }
        }

        public TValue this[int key1] => _valueSet[_uniqueIdToHash[key1]];

        public HashSet<TValue> this[MapCoordinate key2] => new HashSet<TValue>(_mapCoordinateToHashList[key2].Select(x => _valueSet[x]));

        public void Add(TValue value)
        {
            var key1 = _uniqueIdSelector.Invoke(value);

            var hash = value.GetHashCode();
            _uniqueIdToHash[key1] = hash;

            var key2 = _mapCoordinateSelector.Invoke(value);
            if (!_mapCoordinateToHashList.ContainsKey(key2))
                _mapCoordinateToHashList.Add(key2, new HashSet<int>());

            _mapCoordinateToHashList[key2].Add(hash);
            _valueSet[hash] = value;
        }

        public void Update(TValue oldValue, TValue newValue)
        {
            Remove(oldValue);
            Add(newValue);
        }

        public void Remove(TValue value)
        {
            var key1 = _uniqueIdSelector.Invoke(value);
            var key2 = _mapCoordinateSelector.Invoke(value);
            _uniqueIdToHash.Remove(key1);

            var hash = value.GetHashCode();
            _mapCoordinateToHashList[key2].Remove(hash);
            if (_mapCoordinateToHashList[key2].Count == 0)
                _mapCoordinateToHashList.Remove(key2);

            _valueSet.Remove(hash);
        }

        public bool ContainsKey(int uniqueId) => _uniqueIdToHash.ContainsKey(uniqueId);

        public bool ContainsKey(MapCoordinate coordinate) => _mapCoordinateToHashList.ContainsKey(coordinate) && _mapCoordinateToHashList[coordinate].Count > 0;

        public bool TryGetValue(int uniqueId, out TValue value)
        {
            value = default;

            if (!_uniqueIdToHash.ContainsKey(uniqueId))
                return false;

            var hash = _uniqueIdToHash[uniqueId];
            if (!_valueSet.ContainsKey(hash))
                return false;

            value = _valueSet[hash];

            return true;
        }

        public bool TryGetValues(MapCoordinate mapCoordinate, out HashSet<TValue> values)
        {
            values = new HashSet<TValue>();

            if (!_mapCoordinateToHashList.ContainsKey(mapCoordinate))
                return false;

            var hashes = _mapCoordinateToHashList[mapCoordinate];
            if (!_valueSet.Any(x => hashes.Contains(x.Key)))
                return false;

            values = this[mapCoordinate];

            return true;
        }

        public IEnumerator<TValue> GetEnumerator() => _valueSet.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public interface IReadOnlyMapEntityCollection<TValue> : IEnumerable<TValue>
    {
        TValue this[int key1] { get; }
        HashSet<TValue> this[MapCoordinate key2] { get; }

        bool ContainsKey(int characterID);
        bool ContainsKey(MapCoordinate mapCoordinate);

        bool TryGetValue(int key1, out TValue value);
        bool TryGetValues(MapCoordinate key2, out HashSet<TValue> values);
    }
}