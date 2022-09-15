using AutomaticTypeMapper;
using System.Collections.Generic;

namespace EOLib.Domain.Notifiers
{
    public interface IUserInterfaceNotifier
    {
        void NotifyMessageDialog(string title, IReadOnlyList<string> messages);
    }

    [AutoMappedType]
    public class NoOpUserInterfaceNotifier : IUserInterfaceNotifier
    {
        public void NotifyMessageDialog(string title, IReadOnlyList<string> messages) { }
    }
}
