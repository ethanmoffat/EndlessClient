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
        private readonly INPCSpriteSheet _npcSpriteSheet;
        private readonly Dictionary<int, Dictionary<NPCFrame, Memory<Color>>> _spriteData;

        public NPCSpriteDataCache(INPCSpriteSheet npcSpriteSheet)
        {
            _npcSpriteSheet = npcSpriteSheet;
            _spriteData = new Dictionary<int, Dictionary<NPCFrame, Memory<Color>>>();
        }

        public void Populate(int graphic)
        {
            if (_spriteData.ContainsKey(graphic))
                return;

            _spriteData[graphic] = new Dictionary<NPCFrame, Memory<Color>>();

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

            return _spriteData[graphic][NPCFrame.Standing].Span.ToArray().Any(AlphaIsZero);
        }

        private static bool AlphaIsZero(Color input) => input.A == 0;
    }

    public interface INPCSpriteDataCache
    {
        void Populate(int graphic);

        ReadOnlySpan<Color> GetData(int graphic, NPCFrame frame);

        bool IsBlankSprite(int graphic);
    }
}
