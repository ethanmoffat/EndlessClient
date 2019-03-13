// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using AutomaticTypeMapper;
using Microsoft.Xna.Framework.Input;

namespace EndlessClient.Input
{
    public interface IKeyStateRepository
    {
        KeyboardState PreviousKeyState { get; set; }

        KeyboardState CurrentKeyState { get; set; }
    }

    public interface IKeyStateProvider
    {
        KeyboardState PreviousKeyState { get; }

        KeyboardState CurrentKeyState { get; }
    }

    [MappedType(BaseType = typeof(IKeyStateRepository), IsSingleton = true)]
    [MappedType(BaseType = typeof(IKeyStateProvider), IsSingleton = true)]
    public class KeyStateRepository : IKeyStateRepository, IKeyStateProvider
    {
        public KeyboardState PreviousKeyState { get; set; }

        public KeyboardState CurrentKeyState { get; set; }
    }
}
