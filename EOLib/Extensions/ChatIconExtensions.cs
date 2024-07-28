using EOLib.Domain.Chat;
using Optional;

namespace EOLib.Extensions
{
    public static class ChatIconExtensions
    {
        public static Option<(int X, int Y, int Width, int Height)> GetChatIconRectangleBounds(this ChatIcon icon)
        {
            return icon == ChatIcon.None ? Option.None<(int, int, int, int)>() : Option.Some((0, (int)icon * 13, 13, 13));
        }
    }
}