using AutomaticTypeMapper;
using Microsoft.Xna.Framework;

namespace EndlessClient.Rendering
{
    public interface IGameWindowRepository
    {
        GameWindow Window { get; set; }
    }

    public interface IGameWindowProvider
    {
        GameWindow Window { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class GameWindowRepository : IGameWindowProvider, IGameWindowRepository
    {
        public GameWindow Window { get; set; }
    }
}