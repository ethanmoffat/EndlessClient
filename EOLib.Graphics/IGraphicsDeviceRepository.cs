// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using Microsoft.Xna.Framework.Graphics;

namespace EOLib.Graphics
{
    public interface IGraphicsDeviceRepository
    {
        GraphicsDevice GraphicsDevice { get; set; }
    }

    public interface IGraphicsDeviceProvider
    {
        GraphicsDevice GraphicsDevice { get; }
    }

    public class GraphicsDeviceRepository : IGraphicsDeviceRepository, IGraphicsDeviceProvider
    {
        public GraphicsDevice GraphicsDevice { get; set; }
    }
}
