using EOLib.IO.Map;

namespace EOLib.Domain.Spells
{
    public interface ISpellTargetable : IMapEntity
    {
        int ID { get; }

        int Index { get; }
    }
}
