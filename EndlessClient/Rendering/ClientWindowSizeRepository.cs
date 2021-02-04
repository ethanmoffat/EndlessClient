using AutomaticTypeMapper;
using EOLib;
using EOLib.Graphics;
using System;
using System.Collections.Generic;

namespace EndlessClient.Rendering
{
    public interface IClientWindowSizeProvider
    {
        int Width { get; }
        int Height { get; }

        bool Resizable { get; }
    }

    public interface IClientWindowSizeRepository : IResettable
    {
        int Width { get; set; }
        int Height { get; set; }

        bool Resizable { get; set; }

        event EventHandler<EventArgs> GameWindowSizeChanged;
    }

    [AutoMappedType(IsSingleton = true)]
    public class ClientWindowSizeRepository : IClientWindowSizeProvider, IClientWindowSizeRepository, IResettable
    {
        public const int DEFAULT_BACKBUFFER_WIDTH = 640;
        public const int DEFAULT_BACKBUFFER_HEIGHT = 480;

        private readonly IGameWindowRepository _gameWindowRepository;
        private readonly IGraphicsDeviceRepository _graphicsDeviceRepository;

        private readonly List<EventHandler<EventArgs>> _resizeEvents;

        public int Width
        {
            get => _gameWindowRepository.Window.ClientBounds.Width;
            set
            {
                _graphicsDeviceRepository.GraphicsDeviceManager.PreferredBackBufferWidth = value;
                _graphicsDeviceRepository.GraphicsDeviceManager.ApplyChanges();
            }
        }

        public int Height
        {
            get => _gameWindowRepository.Window.ClientBounds.Height;
            set
            {
                _graphicsDeviceRepository.GraphicsDeviceManager.PreferredBackBufferHeight = value;
                _graphicsDeviceRepository.GraphicsDeviceManager.ApplyChanges();
            }
        }

        public bool Resizable
        {
            get => _gameWindowRepository.Window.AllowUserResizing;
            set => _gameWindowRepository.Window.AllowUserResizing = value;
        }

        public event EventHandler<EventArgs> GameWindowSizeChanged
        {
            add => _gameWindowRepository.Window.ClientSizeChanged += value;
            remove => _gameWindowRepository.Window.ClientSizeChanged -= value;
        }

        public ClientWindowSizeRepository(IGameWindowRepository gameWindowRepository,
                                        IGraphicsDeviceRepository graphicsDeviceRepository)
        {
            _gameWindowRepository = gameWindowRepository;
            _graphicsDeviceRepository = graphicsDeviceRepository;
            _resizeEvents = new List<EventHandler<EventArgs>>();
        }

        public void ResetState()
        {
            foreach (var evnt in _resizeEvents)
                GameWindowSizeChanged -= evnt;
            _resizeEvents.Clear();

            Resizable = false;

            Width = DEFAULT_BACKBUFFER_WIDTH;
            Height = DEFAULT_BACKBUFFER_HEIGHT;
        }
    }
}
