using AutomaticTypeMapper;
using System.Collections.Generic;

namespace EndlessClient.Rendering.NPC;

[MappedType(BaseType = typeof(INPCStateCache), IsSingleton = true)]
public class NPCStateCache : INPCStateCache
{
    private readonly Dictionary<int, EOLib.Domain.NPC.NPC> _npcStates;

    public IReadOnlyDictionary<int, EOLib.Domain.NPC.NPC> NPCStates => _npcStates;

    public NPCStateCache()
    {
        _npcStates = new Dictionary<int, EOLib.Domain.NPC.NPC>();
    }

    public bool HasNPCStateWithIndex(int index)
    {
        return _npcStates.ContainsKey(index) && _npcStates[index] != null;
    }

    public void UpdateNPCState(int index, EOLib.Domain.NPC.NPC npc)
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
    IReadOnlyDictionary<int, EOLib.Domain.NPC.NPC> NPCStates { get; }

    bool HasNPCStateWithIndex(int index);

    void UpdateNPCState(int index, EOLib.Domain.NPC.NPC npc);

    void RemoveStateByIndex(int index);

    void Reset();
}