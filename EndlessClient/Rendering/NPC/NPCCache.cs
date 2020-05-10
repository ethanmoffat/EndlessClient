using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.NPC;

namespace EndlessClient.Rendering.NPC
{
    [MappedType(BaseType = typeof(INPCStateCache), IsSingleton = true)]
    public class NPCStateCache : INPCStateCache
    {
        private readonly Dictionary<int, INPC> _npcStates;

        public IReadOnlyDictionary<int, INPC> NPCStates => _npcStates;

        public NPCStateCache()
        {
            _npcStates = new Dictionary<int, INPC>();
        }

        public bool HasNPCStateWithIndex(int index)
        {
            return _npcStates.ContainsKey(index) && _npcStates[index] != null;
        }

        public void UpdateNPCState(int index, INPC npc)
        {
            if (!_npcStates.ContainsKey(index))
                _npcStates.Add(index, npc);
            else
                _npcStates[index] = npc;
        }

        public void RemoveStateByIndex(int index)
        {
            if (_npcStates.ContainsKey(index))
                _npcStates.Remove(index);
        }

        public void Reset()
        {
            _npcStates.Clear();
        }
    }

    public interface INPCStateCache
    {
        IReadOnlyDictionary<int, INPC> NPCStates { get; }

        bool HasNPCStateWithIndex(int index);

        void UpdateNPCState(int index, INPC npc);

        void RemoveStateByIndex(int index);

        void Reset();
    }
}
