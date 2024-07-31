using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;

namespace EOLib.Graphics.Test
{
    [ExcludeFromCodeCoverage]
    internal sealed class GraphicsDeviceTestHelper : IDisposable
    {
        private readonly Game _game;

        public GraphicsDeviceManager GraphicsDeviceManager { get; }

        public GraphicsDeviceTestHelper()
        {
            _game = new Game();
            GraphicsDeviceManager = new GraphicsDeviceManager(_game);
            _game.RunOneFrame();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~GraphicsDeviceTestHelper()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                GraphicsDeviceManager.Dispose();
                _game.Dispose();
            }
        }
    }
}