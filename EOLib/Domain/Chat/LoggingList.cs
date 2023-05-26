using EOLib.Config;
using EOLib.Localization;
using System;
using System.Collections;
using System.Collections.Generic;

namespace EOLib.Domain.Chat
{
    internal class LoggingList : IList<ChatData>, IReadOnlyList<ChatData>
    {
        private readonly IConfigurationProvider _configurationProvider;
        private readonly IChatLoggerProvider _chatLoggerProvider;
        private readonly IChatProcessor _chatProcessor;

        private readonly List<ChatData> _l;

        public LoggingList(IConfigurationProvider configurationProvider,
                           IChatLoggerProvider chatLoggerProvider,
                           IChatProcessor chatProcessor)
        {
            _configurationProvider = configurationProvider;
            _chatLoggerProvider = chatLoggerProvider;
            _chatProcessor = chatProcessor;

            _l = new List<ChatData>();
        }

        public LoggingList(IConfigurationProvider configurationProvider,
                           IChatLoggerProvider chatLoggerProvider,
                           IChatProcessor chatProcessor,
                           IList<ChatData> source)
            : this(configurationProvider, chatLoggerProvider, chatProcessor)
        {
            _l = new List<ChatData>(source);
        }

        public void Add(ChatData item)
        {
            if (item.Filter && (_configurationProvider.CurseFilterEnabled || _configurationProvider.StrictFilterEnabled))
            {
                var (ok, filtered) = _chatProcessor.FilterCurses(item.Message);
                if (!ok)
                    return;

                item = item.WithMessage(filtered);
            }

            ((ICollection<ChatData>)_l).Add(item);

            if (_configurationProvider.LogChatToFile && item.Log)
            {
                _chatLoggerProvider.ChatLogger.Log($"[{GetChatTabString(item.Tab)}] {(string.IsNullOrEmpty(item.Who) ? string.Empty : $"{item.Who}  ")}{item.Message}");
            }
        }

        private string GetChatTabString(ChatTab tab)
        {
            switch (tab)
            {
                case ChatTab.Private1:
                case ChatTab.Private2: return "W";
                case ChatTab.Global:
                case ChatTab.Group: return "G";
                case ChatTab.Local: return "P";
                case ChatTab.System: return "S";
                default: throw new ArgumentOutOfRangeException(nameof(tab));
            }
        }

        #region Default implementation

        public ChatData this[int index] { get => _l[index]; set => _l[index] = value; }

        public int Count => _l.Count;

        public bool IsReadOnly => ((ICollection<ChatData>)_l).IsReadOnly;

        public void Clear() => _l.Clear();

        public bool Contains(ChatData item) => _l.Contains(item);

        public void CopyTo(ChatData[] array, int arrayIndex) => _l.CopyTo(array, arrayIndex);

        public IEnumerator<ChatData> GetEnumerator() => _l.GetEnumerator();

        public int IndexOf(ChatData item) => _l.IndexOf(item);

        public void Insert(int index, ChatData item) => _l.Insert(index, item);

        public bool Remove(ChatData item) => _l.Remove(item);

        public void RemoveAt(int index) => _l.RemoveAt(index);

        IEnumerator IEnumerable.GetEnumerator() => _l.GetEnumerator();

        #endregion
    }
}
