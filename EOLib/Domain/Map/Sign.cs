using System.Linq;
using EOLib.IO.Map;

namespace EOLib.Domain.Map
{
    public class Sign
    {
        public string Title { get; private set; }

        public string Message { get; private set; }

        public static Sign None => new Sign();

        private Sign()
        {
            Title = string.Empty;
            Message = string.Empty;
        }

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
}
