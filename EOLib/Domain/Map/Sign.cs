using EOLib.IO.Map;
using System.Linq;

namespace EOLib.Domain.Map
{
    public class Sign : ISign
    {
        public string Title { get; private set; }

        public string Message { get; private set; }

        public Sign(SignMapEntity sign)
        {
            Title = Filter(sign.Title);
            Message = Filter(sign.Message);
        }

        private static string Filter(string input)
        {
            return new string(input.Where(x => !char.IsControl(x)).ToArray());
        }
    }

    public interface ISign
    {
        string Title { get; }

        string Message { get; }
    }
}
