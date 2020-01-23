using System.Collections.Generic;
using AutomaticTypeMapper;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Content
{
    public interface IShaderRepository
    {
        IDictionary<string, Effect> Shaders { get; set; }
    }

    public interface IShaderProvider
    {
        IReadOnlyDictionary<string, Effect> Shaders { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class ShaderRepository : IShaderProvider, IShaderRepository
    {
        public const string HairClip = "HairClip";

        IReadOnlyDictionary<string, Effect> IShaderProvider.Shaders => (IReadOnlyDictionary<string, Effect>)Shaders;

        public IDictionary<string, Effect> Shaders { get; set; }

        public ShaderRepository()
        {
            Shaders = new Dictionary<string, Effect>();
        }
    }
}
