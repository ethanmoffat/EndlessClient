#region File Description
//-----------------------------------------------------------------------------
// GraphicsDeviceService.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

//File modified by Ethan Moffat for use in testing

#region Using Statements

using System;
using System.Threading;
using Microsoft.Xna.Framework.Graphics;

#endregion

#pragma warning disable 67

namespace EOLib.Graphics.Test
{
	class GraphicsDeviceServiceTestHelper : IGraphicsDeviceService
	{
		#region Fields

		private static GraphicsDeviceServiceTestHelper _singletonInstance;
		private static int _referenceCount;

		#endregion

		GraphicsDeviceServiceTestHelper(IntPtr windowHandle, int width, int height)
		{
			var parameters = new PresentationParameters
			{
				BackBufferWidth = Math.Max(width, 1),
				BackBufferHeight = Math.Max(height, 1),
				BackBufferFormat = SurfaceFormat.Color,
				DepthStencilFormat = DepthFormat.Depth24,
				DeviceWindowHandle = windowHandle,
				PresentationInterval = PresentInterval.Immediate,
				IsFullScreen = false
			};

			GraphicsDevice = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile.HiDef, parameters);
		}

		public static GraphicsDeviceServiceTestHelper AddRef(IntPtr windowHandle, int width, int height)
		{
			if (Interlocked.Increment(ref _referenceCount) == 1)
				_singletonInstance = new GraphicsDeviceServiceTestHelper(windowHandle, width, height);

			return _singletonInstance;
		}

		public GraphicsDevice GraphicsDevice { get; private set; }

		public event EventHandler<EventArgs> DeviceCreated;
		public event EventHandler<EventArgs> DeviceDisposing;
		public event EventHandler<EventArgs> DeviceReset;
		public event EventHandler<EventArgs> DeviceResetting;
	}
}
