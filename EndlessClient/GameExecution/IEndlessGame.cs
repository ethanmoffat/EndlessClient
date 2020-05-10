using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace EndlessClient.GameExecution
{
    public interface IEndlessGame : IDisposable
    {
        GameComponentCollection Components { get; }

        ContentManager Content { get; }

        GameWindow Window { get; }

        bool IsActive { get; }

        void Run();

        void Exit();
    }
}
