using AutomaticTypeMapper;
using XNAControls;

namespace EndlessClient.Input
{
    public interface IKeyboardDispatcherRepository
    {
        KeyboardDispatcher Dispatcher { get; set; }
    }

    public interface IKeyboardDispatcherProvider
    {
        KeyboardDispatcher Dispatcher { get; }
    }

    [MappedType(BaseType = typeof(IKeyboardDispatcherRepository), IsSingleton = true)]
    [MappedType(BaseType = typeof(IKeyboardDispatcherProvider), IsSingleton = true)]
    public class KeyboardDispatcherRepository : IKeyboardDispatcherRepository, IKeyboardDispatcherProvider
    {
        public KeyboardDispatcher Dispatcher { get; set; }
    }
}
