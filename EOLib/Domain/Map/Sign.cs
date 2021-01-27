using EOLib.IO.Map;

namespace EOLib.Domain.Map
{
    public class Sign : ISign
    {
        public string Title { get; private set; }

        public string Message { get; private set; }

        public Sign(SignMapEntity sign)
        {
            Title = sign.Title;
            Message = sign.Message;
        }
    }

    public interface ISign
    {
        string Title { get; }

        string Message { get; }
    }
}
