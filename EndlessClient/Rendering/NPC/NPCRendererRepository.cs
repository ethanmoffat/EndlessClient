// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

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

    public class NPCRendererRepository : INPCRendererRepository, INPCRendererProvider
    {
        public Dictionary<int, INPCRenderer> NPCRenderers { get; set; }

        IReadOnlyDictionary<int, INPCRenderer> INPCRendererProvider.NPCRenderers { get { return NPCRenderers; } }

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
