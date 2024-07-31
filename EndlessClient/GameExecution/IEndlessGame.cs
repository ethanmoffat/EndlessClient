using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;

namespace EndlessClient.GameExecution
{
    public interface IEndlessGame : IDisposable
    {
        event EventHandler<EventArgs> Exiting;

        GameComponentCollection Components { get; }

        ContentManager Content { get; }

        GameWindow Window { get; }

        bool IsActive { get; }

        void Run();

        void Exit();
    }
}