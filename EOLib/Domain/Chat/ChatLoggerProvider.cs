using System;
using AutomaticTypeMapper;
using EOLib.Config;
using EOLib.Logger;
using EOLib.Shared;

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

        public ChatLoggerProvider(IConfigurationProvider configurationProvider, ILoggerFactory loggerFactory)
        {
            if (configurationProvider.LogChatToFile)
                ChatLogger = loggerFactory.CreateLogger<FileLogger>(Constants.ChatLogFile);
            else
                ChatLogger = loggerFactory.CreateLogger<NullLogger>();
        }

        public void Dispose()
        {
            ChatLogger.Dispose();
        }
    }
}
