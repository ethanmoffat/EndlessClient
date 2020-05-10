using System;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EOLib.Graphics.Test
{
    [ExcludeFromCodeCoverage]
    internal sealed class GraphicsDeviceTestHelper : IDisposable
    {
        private readonly Form _form;
        private readonly Game _game;
        //private readonly ServiceContainer _serviceContainer;

        public GraphicsDeviceManager GraphicsDeviceManager { get; }

        public GraphicsDeviceTestHelper()
        {
            _form = new Form();
            _game = new Game();
            GraphicsDeviceManager = new GraphicsDeviceManager(_game);
            _game.RunOneFrame();

            //var gds = GraphicsDeviceServiceTestHelper.AddRef(_form.Handle, _form.ClientSize.Width, _form.ClientSize.Height);

            //_serviceContainer = new ServiceContainer();
            //_serviceContainer.AddService(typeof(IGraphicsDeviceService), gds);

            //GraphicsDeviceManager.ApplyChanges();
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
                _form.Dispose();
            }
        }
    }
}
