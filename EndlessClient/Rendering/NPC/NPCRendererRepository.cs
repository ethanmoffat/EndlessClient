using AutomaticTypeMapper;
using System;
using System.Collections.Generic;

namespace EndlessClient.Rendering.NPC
{
    public interface INPCRendererRepository : IDisposable
    {
        Dictionary<int, INPCRenderer> NPCRenderers { get; set; }
    }

    public interface INPCRendererProvider
    {
        IReadOnlyDictionary<int, INPCRenderer> NPCRenderers { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class NPCRendererRepository : INPCRendererRepository, INPCRendererProvider
    {
        public Dictionary<int, INPCRenderer> NPCRenderers { get; set; }

        IReadOnlyDictionary<int, INPCRenderer> INPCRendererProvider.NPCRenderers => NPCRenderers;

        public NPCRendererRepository()
        {
            NPCRenderers = new Dictionary<int, INPCRenderer>();
        }

        public void Dispose()
        {
            foreach (var renderer in NPCRenderers.Values)
                renderer.Dispose();
            NPCRenderers.Clear();
        }
    }
}
