using AutomaticTypeMapper;

namespace EOLib.Domain.Interact.Quest
{
    public interface IStatusLabelNotifier
    {
        void ShowWarning(string message);
    }

    [AutoMappedType]
    public class NoOpStatusLabelNotifier : IStatusLabelNotifier
    {
        public void ShowWarning(string message) { }
    }
}