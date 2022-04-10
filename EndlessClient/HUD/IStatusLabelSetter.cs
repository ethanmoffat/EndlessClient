using EOLib.Domain.Interact.Quest;
using EOLib.Localization;

namespace EndlessClient.HUD
{
    public interface IStatusLabelSetter : IStatusLabelNotifier
    {
        void SetStatusLabel(EOResourceID type, EOResourceID text, string appended = "");

        void SetStatusLabel(EOResourceID type, string prepended, EOResourceID text);

        void SetStatusLabel(EOResourceID type, string text);

        void SetStatusLabel(string text);
    }
}
