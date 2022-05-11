using AutomaticTypeMapper;
using EOLib.Logger;
using System;

namespace EOLib.Domain.Chat
{
    public interface IChatLoggerProvider : IDisposable
    {
        ILogger ChatLogger { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class ChatLoggerProvider : IChatLoggerProvider
    {
        public ILogger ChatLogger { get; }

        public ChatLoggerProvider(ILoggerFactory loggerFactory)
        {
            ChatLogger = loggerFactory.CreateLogger<FileLogger>(Constants.ChatLogFile);
        }

        public void Dispose()
        {
            ChatLogger.Dispose();
        }
    }
}
