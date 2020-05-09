using Microsoft.Xna.Framework.Input;

namespace EndlessClient.Input
{
    public static class KeyboardStateExtensions
    {
        /// <summary>
        /// Compares the state of a single key between two KeyboardStates to determine if the key is being held down
        /// </summary>
        /// <param name="keyState">The current keystate</param>
        /// <param name="previousKeyState">The previous keystate</param>
        /// <param name="key">The key to test</param>
        /// <returns>True if the key is held (down for both states); false otherwise</returns>
        public static bool IsKeyHeld(this KeyboardState keyState, KeyboardState previousKeyState, Keys key)
        {
            return previousKeyState.IsKeyDown(key) && keyState.IsKeyDown(key);
        }

        /// <summary>
        /// Compares the state of a single key between two KeyboardStates to determine if the key was pressed once
        /// </summary>
        /// <param name="keyState">The current keystate</param>
        /// <param name="previousKeyState">The previous keystate</param>
        /// <param name="key">The key to test</param>
        /// <returns>True if the key was pressed once (down in previous then up in current); false otherwise</returns>
        public static bool IsKeyPressedOnce(this KeyboardState keyState, KeyboardState previousKeyState, Keys key)
        {
            return previousKeyState.IsKeyDown(key) && keyState.IsKeyUp(key);
        }
    }
}
