using AutomaticTypeMapper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EOLib.Graphics
{
    public interface IGraphicsDeviceRepository
    {
        GraphicsDevice GraphicsDevice { get; set; }

        GraphicsDeviceManager GraphicsDeviceManager { get; set; }
    }

    public interface IGraphicsDeviceProvider
    {
        GraphicsDevice GraphicsDevice { get; }

        GraphicsDeviceManager GraphicsDeviceManager { get; }
    }

    [MappedType(BaseType = typeof(IGraphicsDeviceRepository), IsSingleton = true)]
    [MappedType(BaseType = typeof(IGraphicsDeviceProvider), IsSingleton = true)]
    public class GraphicsDeviceRepository : IGraphicsDeviceRepository, IGraphicsDeviceProvider
    {
        public GraphicsDevice GraphicsDevice { get; set; }

        public GraphicsDeviceManager GraphicsDeviceManager { get; set; }
    }
}
