﻿using System.Collections.Generic;
using AutomaticTypeMapper;
using EndlessClient.Rendering.Metadata;
using EndlessClient.Rendering.Metadata.Models;
using EOLib.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.Effects
{
    [AutoMappedType]
    public class EffectSpriteManager : IEffectSpriteManager
    {
        private readonly INativeGraphicsManager _graphicsManager;
        private readonly IMetadataProvider<EffectMetadata> _effectMetadataProvider;

        public EffectSpriteManager(INativeGraphicsManager graphicsManager,
                                   IMetadataProvider<EffectMetadata> effectMetadataProvider)
        {
            _graphicsManager = graphicsManager;
            _effectMetadataProvider = effectMetadataProvider;
        }

        public EffectMetadata GetEffectMetadata(int graphic)
        {
            return _effectMetadataProvider.GetValueOrDefault(graphic);
        }

        public IList<IEffectSpriteInfo> GetEffectInfo(int graphic)
        {
            return GetEffectInfo(graphic, GetEffectMetadata(graphic));
        }

        public IList<IEffectSpriteInfo> GetEffectInfo(int graphic, EffectMetadata metadata)
        {
            var baseGraphic = 101 + (graphic - 1) * 3;

            var retList = new List<IEffectSpriteInfo>();
            if (metadata.HasBehindLayer)
                retList.Add(new EffectSpriteInfo(metadata, EffectLayer.Behind, GetGraphic(baseGraphic)));
            if (metadata.HasTransparentLayer)
                retList.Add(new EffectSpriteInfo(metadata, EffectLayer.Transparent, GetGraphic(baseGraphic + 1)));
            if (metadata.HasInFrontLayer)
                retList.Add(new EffectSpriteInfo(metadata, EffectLayer.InFront, GetGraphic(baseGraphic + 2)));

            return retList;
        }

        private Texture2D GetGraphic(int actualResourceID)
        {
            return _graphicsManager.TextureFromResource(GFXTypes.Spells, actualResourceID - 100, true);
        }
    }

    public interface IEffectSpriteManager
    {
        EffectMetadata GetEffectMetadata(int graphic);

        IList<IEffectSpriteInfo> GetEffectInfo(int graphic);

        IList<IEffectSpriteInfo> GetEffectInfo(int graphic, EffectMetadata metadata);
    }
}
