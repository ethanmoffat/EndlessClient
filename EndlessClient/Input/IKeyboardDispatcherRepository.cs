// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using XNAControls.Old;

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

    public class KeyboardDispatcherRepository : IKeyboardDispatcherRepository, IKeyboardDispatcherProvider
    {
        public KeyboardDispatcher Dispatcher { get; set; }
    }
}
