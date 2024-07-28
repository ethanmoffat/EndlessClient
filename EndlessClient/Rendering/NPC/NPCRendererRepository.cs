using AutomaticTypeMapper;
using EOLib.Domain.Map;
using System;
using System.Collections.Generic;

namespace EndlessClient.Rendering.NPC;

public interface INPCRendererRepository : IDisposable
{
    Dictionary<int, INPCRenderer> NPCRenderers { get; set; }

    Dictionary<MapCoordinate, int> DyingNPCs { get; set; }
}

public interface INPCRendererProvider
{
    IReadOnlyDictionary<int, INPCRenderer> NPCRenderers { get; }

    IReadOnlyDictionary<MapCoordinate, int> DyingNPCs { get; }
}

[AutoMappedType(IsSingleton = true)]
public class NPCRendererRepository : INPCRendererRepository, INPCRendererProvider
{
    public Dictionary<int, INPCRenderer> NPCRenderers { get; set; }

    public Dictionary<MapCoordinate, int> DyingNPCs { get; set; }

    IReadOnlyDictionary<int, INPCRenderer> INPCRendererProvider.NPCRenderers => NPCRenderers;

    IReadOnlyDictionary<MapCoordinate, int> INPCRendererProvider.DyingNPCs => DyingNPCs;

    public NPCRendererRepository()
    {
        NPCRenderers = new Dictionary<int, INPCRenderer>();
        DyingNPCs = new Dictionary<MapCoordinate, int>();
    }

    public void Dispose()
    {
        foreach (var renderer in NPCRenderers.Values)
            renderer.Dispose();
        NPCRenderers.Clear();

        DyingNPCs.Clear();
    }
}