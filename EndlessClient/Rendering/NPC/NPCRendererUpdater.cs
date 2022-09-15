using System;
using System.Linq;
using AutomaticTypeMapper;
using EOLib;
using EOLib.Domain.Map;
using EOLib.Domain.NPC;
using Microsoft.Xna.Framework;
using Optional;

namespace EndlessClient.Rendering.NPC
{
    [MappedType(BaseType = typeof(INPCRendererUpdater))]
    public class NPCRendererUpdater : INPCRendererUpdater
    {
        private readonly ICurrentMapStateProvider _currentMapStateProvider;
        private readonly INPCRendererRepository _npcRendererRepository;
        private readonly INPCStateCache _npcStateCache;
        private readonly INPCRendererFactory _npcRendererFactory;

        public NPCRendererUpdater(ICurrentMapStateProvider currentMapStateProvider,
                                  INPCRendererRepository npcRendererRepository,
                                  INPCStateCache npcStateCache,
                                  INPCRendererFactory npcRendererFactory)
        {
            _currentMapStateProvider = currentMapStateProvider;
            _npcRendererRepository = npcRendererRepository;
            _npcStateCache = npcStateCache;
            _npcRendererFactory = npcRendererFactory;
        }

        public void UpdateNPCs(GameTime gameTime)
        {
            CleanUpDeadNPCs();
            CreateAndCacheNPCRenderers();
            UpdateNPCRenderers(gameTime);
        }

        private void CleanUpDeadNPCs()
        {
            var deadNPCs = _npcRendererRepository.NPCRenderers.Values.Where(x => x.IsDead).ToList();
            if (!deadNPCs.Any())
                return;

            foreach (var npc in deadNPCs)
            {
                npc.Dispose();
                _npcRendererRepository.NPCRenderers.Remove(npc.NPC.Index);
                _npcStateCache.RemoveStateByIndex(npc.NPC.Index);
            }
        }

        private void CreateAndCacheNPCRenderers()
        {
            foreach (var npc in _currentMapStateProvider.NPCs)
            {
                _npcStateCache.HasNPCStateWithIndex(npc.Index)
                    .SomeWhen(b => b)
                    .Map(_ => _npcStateCache.NPCStates[npc.Index])
                    .Match(
                        some: n =>
                        {
                            if (n != npc)
                            {
                                UpdateCachedNPC(npc);
                            }
                        },
                        none: () => CreateAndCacheRendererForNPC(npc));
            }
        }

        private void CreateAndCacheRendererForNPC(EOLib.Domain.NPC.NPC npc)
        {
            _npcStateCache.UpdateNPCState(npc.Index, npc);

            var renderer = _npcRendererFactory.CreateRendererFor(npc);
            renderer.Initialize();

            if (_npcRendererRepository.NPCRenderers.ContainsKey(npc.Index) &&
                _npcRendererRepository.NPCRenderers[npc.Index] != null)
            {
                _npcRendererRepository.NPCRenderers[npc.Index].Dispose();
                _npcRendererRepository.NPCRenderers.Remove(npc.Index);
            }

            _npcRendererRepository.NPCRenderers.Add(npc.Index, renderer);
        }

        private void UpdateCachedNPC(EOLib.Domain.NPC.NPC npc)
        {
            _npcRendererRepository.NPCRenderers[npc.Index].NPC = npc;
            _npcStateCache.UpdateNPCState(npc.Index, npc);
        }

        private void UpdateNPCRenderers(GameTime gameTime)
        {
            foreach (var renderer in _npcRendererRepository.NPCRenderers.Values)
                renderer.Update(gameTime);
        }

        public void Dispose()
        {
            _npcRendererRepository.Dispose();
        }
    }

    public interface INPCRendererUpdater : IDisposable
    {
        void UpdateNPCs(GameTime gameTime);
    }
}
