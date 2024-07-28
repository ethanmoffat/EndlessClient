using AutomaticTypeMapper;
using Optional;
using System;
using System.Collections.Generic;

namespace EndlessClient.Rendering.Character;

public interface ICharacterRendererRepository : IDisposable
{
    Option<ICharacterRenderer> MainCharacterRenderer { get; set; }

    Dictionary<int, ICharacterRenderer> CharacterRenderers { get; set; }

    HashSet<int> NeedsWarpArriveAnimation { get; }
}

public interface ICharacterRendererProvider
{
    Option<ICharacterRenderer> MainCharacterRenderer { get; }

    IReadOnlyDictionary<int, ICharacterRenderer> CharacterRenderers { get; }

    HashSet<int> NeedsWarpArriveAnimation { get; }
}

[AutoMappedType(IsSingleton = true)]
public class CharacterRendererRepository : ICharacterRendererRepository, ICharacterRendererProvider
{
    public Option<ICharacterRenderer> MainCharacterRenderer { get; set; }

    public Dictionary<int, ICharacterRenderer> CharacterRenderers { get; set; }

    IReadOnlyDictionary<int, ICharacterRenderer> ICharacterRendererProvider.CharacterRenderers => CharacterRenderers;

    public HashSet<int> NeedsWarpArriveAnimation { get; set; }

    public CharacterRendererRepository()
    {
        CharacterRenderers = new Dictionary<int, ICharacterRenderer>(64);
        NeedsWarpArriveAnimation = new HashSet<int>();
    }

    public void Dispose()
    {
        MainCharacterRenderer.MatchSome(x => x.Dispose());
        MainCharacterRenderer = Option.None<ICharacterRenderer>();

        foreach (var renderer in CharacterRenderers.Values)
            renderer.Dispose();
        CharacterRenderers.Clear();
    }
}