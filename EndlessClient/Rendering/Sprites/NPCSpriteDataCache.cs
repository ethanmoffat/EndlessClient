using AutomaticTypeMapper;
using EOLib;
using EOLib.Domain.NPC;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EndlessClient.Rendering.Sprites
{
    [AutoMappedType(IsSingleton = true)]
    public class NPCSpriteDataCache : INPCSpriteDataCache
    {
        private const int CACHE_SIZE = 32;

        private readonly INPCSpriteSheet _npcSpriteSheet;

        private readonly Dictionary<int, Dictionary<NPCFrame, Memory<Color>>> _spriteData;
        private readonly List<int> _lru;
        private readonly HashSet<int> _reclaimable;

        public NPCSpriteDataCache(INPCSpriteSheet npcSpriteSheet)
        {
            _npcSpriteSheet = npcSpriteSheet;
            _spriteData = new Dictionary<int, Dictionary<NPCFrame, Memory<Color>>>(CACHE_SIZE);
            _lru = new List<int>(CACHE_SIZE);
            _reclaimable = new HashSet<int>(CACHE_SIZE);
        }

        public void Populate(int graphic)
        {
            if (_spriteData.ContainsKey(graphic))
                return;

            if (_lru.Count >= CACHE_SIZE && _reclaimable.Count > 0)
            {
                // find and "reclaim" the first available candidate based on the order they were added to the LRU
                // 'reclaimable' candidates are updated when the map changes
                // candidates will never be NPCs that are on the current map
                // a map with >= CACHE_SIZE different NPCs will cause problems here
                for (int i = 0; i < _lru.Count; i++)
                {
                    var candidate = _lru[i];
                    if (_reclaimable.Contains(candidate))
                    {
                        _spriteData.Remove(candidate);
                        _reclaimable.Remove(candidate);
                        _lru.RemoveAt(i);
                        break;
                    }
                }
            }

            _spriteData[graphic] = new Dictionary<NPCFrame, Memory<Color>>();
            _reclaimable.Remove(graphic);
            _lru.Add(graphic);

            foreach (NPCFrame frame in Enum.GetValues(typeof(NPCFrame)))
            {
                var text = _npcSpriteSheet.GetNPCTexture(graphic, frame, EODirection.Down);
                var data = Array.Empty<Color>();

                if (text != null)
                {
                    data = new Color[text.Width * text.Height];
                    text.GetData(data);
                }

                _spriteData[graphic][frame] = data;
            }
        }

        public void MarkForEviction(int graphic)
        {
            _reclaimable.Add(graphic);
        }

        public void UnmarkForEviction(int graphic)
        {
            _reclaimable.Remove(graphic);
        }

        public ReadOnlySpan<Color> GetData(int graphic, NPCFrame frame)
        {
            if (!_spriteData.ContainsKey(graphic))
            {
                Populate(graphic);
            }

            return _spriteData[graphic][frame].Span;
        }

        public bool IsBlankSprite(int graphic)
        {
            if (!_spriteData.ContainsKey(graphic))
            {
                Populate(graphic);
            }

            return _spriteData[graphic][NPCFrame.Standing].Span.ToArray().All(AlphaIsZero);
        }

        private static bool AlphaIsZero(Color input) => input.A == 0;
    }

    public interface INPCSpriteDataCache
    {
        void Populate(int graphic);

        void MarkForEviction(int graphic);

        void UnmarkForEviction(int graphic);

        ReadOnlySpan<Color> GetData(int graphic, NPCFrame frame);

        bool IsBlankSprite(int graphic);
    }
}
