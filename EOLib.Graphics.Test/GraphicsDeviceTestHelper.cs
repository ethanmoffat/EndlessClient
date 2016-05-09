// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.ComponentModel.Design;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EOLib.Graphics.Test
{
	internal sealed class GraphicsDeviceTestHelper : IDisposable
	{
		private readonly Form _form;
		private readonly Game _game;
		private readonly ServiceContainer _serviceContainer;

		public GraphicsDeviceManager GraphicsDeviceManager { get; private set; }

		public GraphicsDeviceTestHelper()
		{
			_form = new Form();
			_game = new Game();
			GraphicsDeviceManager = new GraphicsDeviceManager(_game);

			var gds = GraphicsDeviceServiceTestHelper.AddRef(_form.Handle, _form.ClientSize.Width, _form.ClientSize.Height);
			
			_serviceContainer = new ServiceContainer();
			_serviceContainer.AddService(typeof(IGraphicsDeviceService), gds);

			GraphicsDeviceManager.CreateDevice();
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
				_serviceContainer.Dispose();
				GraphicsDeviceManager.Dispose();
				_game.Dispose();
				_form.Dispose();
			}
		}
	}
}
