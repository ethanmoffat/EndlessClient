using AutomaticTypeMapper;

namespace EndlessClient.Rendering
{
    [MappedType(BaseType = typeof(IClientWindowSizeProvider), IsSingleton = true)]
    public class ClientWindowSizeProvider : IClientWindowSizeProvider
    {
        //This could be extended to support adjusting the window size
        //Controls would need to use relative positioning instead of their current absolute coordinates
        //They would also need to support updating layout when the window is resized

        //Supporting dynamic window sizing is NOT a trivial task
        public int Width => 640;
        public int Height => 480;
    }

    public interface IClientWindowSizeProvider
    {
        int Width { get; }
        int Height { get; }
    }
}
