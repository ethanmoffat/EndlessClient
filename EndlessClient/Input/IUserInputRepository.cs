using AutomaticTypeMapper;
using Microsoft.Xna.Framework.Input;

namespace EndlessClient.Input
{
    public interface IUserInputRepository
    {
        KeyboardState PreviousKeyState { get; set; }

        KeyboardState CurrentKeyState { get; set; }

        MouseState PreviousMouseState { get; set; }

        MouseState CurrentMouseState { get; set; }
    }

    public interface IUserInputProvider
    {
        KeyboardState PreviousKeyState { get; }

        KeyboardState CurrentKeyState { get; }

        MouseState PreviousMouseState { get; }

        MouseState CurrentMouseState { get; }
    }

    [MappedType(BaseType = typeof(IUserInputRepository), IsSingleton = true)]
    [MappedType(BaseType = typeof(IUserInputProvider), IsSingleton = true)]
    public class KeyStateRepository : IUserInputRepository, IUserInputProvider
    {
        public KeyboardState PreviousKeyState { get; set; }

        public KeyboardState CurrentKeyState { get; set; }

        public MouseState PreviousMouseState { get; set; }

        public MouseState CurrentMouseState { get; set; }
    }
}