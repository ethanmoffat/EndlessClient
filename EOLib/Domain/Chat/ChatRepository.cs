using AutomaticTypeMapper;
using EOLib.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EOLib.Domain.Chat
{
    public interface IChatRepository
    {
        IReadOnlyDictionary<ChatTab, IList<ChatData>> AllChat { get; }

        string PMTarget1 { get; set; }

        string PMTarget2 { get; set; }
    }

    public interface IChatProvider
    {
        IReadOnlyDictionary<ChatTab, IReadOnlyList<ChatData>> AllChat { get; }

        string PMTarget1 { get; }

        string PMTarget2 { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class ChatRepository : IChatRepository, IChatProvider, IResettable
    {
        private readonly IConfigurationProvider _configurationProvider;
        private readonly IChatLoggerProvider _chatLoggerProvider;
        private readonly IChatProcessor _chatProcessor;

        public IReadOnlyDictionary<ChatTab, IList<ChatData>> AllChat { get; private set; }

        IReadOnlyDictionary<ChatTab, IReadOnlyList<ChatData>> IChatProvider.AllChat
        {
            get
            {
                return AllChat.ToDictionary(
                    k => k.Key,
                    v => v.Value as IReadOnlyList<ChatData>);
            }
        }

        public string PMTarget1 { get; set; }

        public string PMTarget2 { get; set; }

        public ChatRepository(IConfigurationProvider configurationProvider,
                              IChatLoggerProvider chatLoggerProvider,
                              IChatProcessor chatProcessor)
        {
            _configurationProvider = configurationProvider;
            _chatLoggerProvider = chatLoggerProvider;
            _chatProcessor = chatProcessor;

            ResetState();
        }

        public void ResetState()
        {
            var chat = new Dictionary<ChatTab, IList<ChatData>>();
            foreach (var tab in (ChatTab[])Enum.GetValues(typeof(ChatTab)))
                chat.Add(tab, new LoggingList(_configurationProvider, _chatLoggerProvider, _chatProcessor));

            AllChat = chat;
        }
    }
}