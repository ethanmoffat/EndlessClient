using AutomaticTypeMapper;

namespace EOLib.Domain.Notifiers
{
    public interface IArenaNotifier
    {
        void NotifyArenaBusy();

        void NotifyArenaStart(int players);

        void NotifyArenaKill(int killCount, string killer, string victim);

        void NotifyArenaWin(string winner);
    }

    [AutoMappedType]
    public class NoOpArenaNotifer : IArenaNotifier
    {
        public void NotifyArenaBusy() { }

        public void NotifyArenaStart(int players) { }

        public void NotifyArenaKill(int killCount, string killer, string victim) { }

        public void NotifyArenaWin(string winner) { }
    }
}