using System;
using System.Collections.Generic;
using AutomaticTypeMapper;

namespace EndlessClient.Rendering.Character
{
    public interface ICharacterRendererRepository : IDisposable
    {
        ICharacterRenderer MainCharacterRenderer { get; set; }

        Dictionary<int, ICharacterRenderer> CharacterRenderers { get; set; }
    }

    public interface ICharacterRendererProvider
    {
        ICharacterRenderer MainCharacterRenderer { get; }

        IReadOnlyDictionary<int, ICharacterRenderer> CharacterRenderers { get; }
    }

    [MappedType(BaseType = typeof(ICharacterRendererProvider), IsSingleton = true)]
    [MappedType(BaseType = typeof(ICharacterRendererRepository), IsSingleton = true)]
    public class CharacterRendererRepository : ICharacterRendererRepository, ICharacterRendererProvider
    {
        public ICharacterRenderer MainCharacterRenderer { get; set; }

        public Dictionary<int, ICharacterRenderer> CharacterRenderers { get; set; }

        IReadOnlyDictionary<int, ICharacterRenderer> ICharacterRendererProvider.CharacterRenderers => CharacterRenderers;

        public CharacterRendererRepository()
        {
            CharacterRenderers = new Dictionary<int, ICharacterRenderer>(64);
        }

        public void Dispose()
        {
            MainCharacterRenderer?.Dispose();
            MainCharacterRenderer = null;

            foreach (var renderer in CharacterRenderers.Values)
                renderer.Dispose();
            CharacterRenderers.Clear();
        }
    }
}