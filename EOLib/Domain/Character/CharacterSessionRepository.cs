using AutomaticTypeMapper;
using System;

namespace EOLib.Domain.Character
{
    public interface ICharacterSessionRepository : IResettable
    {
        DateTime SessionStartTime { get; set; }

        int BestKillExp { get; set; }

        int LastKillExp { get; set; }

        ulong TodayTotalExp { get; set; }
    }

    public interface ICharacterSessionProvider : IResettable
    {
        DateTime SessionStartTime { get; }

        int BestKillExp { get; }

        int LastKillExp { get; }

        ulong TodayTotalExp { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class CharacterSessionRepository : ICharacterSessionRepository, ICharacterSessionProvider
    {
        public DateTime SessionStartTime { get; set; }

        public int BestKillExp { get; set; }

        public int LastKillExp { get; set; }

        public ulong TodayTotalExp { get; set; }

        public CharacterSessionRepository()
        {
            ResetState();
        }

        public void ResetState()
        {
            SessionStartTime = DateTime.Now;
            BestKillExp = 0;
            LastKillExp = 0;
            TodayTotalExp = 0;
        }
    }
}