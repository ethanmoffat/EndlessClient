using System;
using System.Collections.Generic;
using System.Linq;
using EndlessClient.Services;
using EOLib;
using EOLib.Domain.Chat;
using EOLib.Graphics;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using XNAControls;

namespace EndlessClient.Rendering.Chat
{
    public class ChatRenderableGenerator : IChatRenderableGenerator
    {
        private const int LINE_LEN = 380;

        private class ChatPair
        {
            public string Text { get; set; }
            public bool IsFirstLineOfMultilineMessage { get; set; }
        }

        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IFriendIgnoreListService _friendIgnoreListService;
        private readonly BitmapFont _chatFont;

        public ChatRenderableGenerator(INativeGraphicsManager nativeGraphicsManager,
                                       IFriendIgnoreListService friendIgnoreListService,
                                       BitmapFont chatFont)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _friendIgnoreListService = friendIgnoreListService;
            _chatFont = chatFont;
        }

        public IReadOnlyList<IChatRenderable> GenerateNewsRenderables(IReadOnlyList<string> newsText)
        {
            newsText = newsText.Where(x => !string.IsNullOrEmpty(x)).ToList();

            var newsTextWithBlankLines = new List<string>();
            foreach (var news in newsText)
            {
                newsTextWithBlankLines.Add(news);
                if(news != newsText.Last())
                    newsTextWithBlankLines.Add(" ");
            }

            var splitStrings = SplitTextIntoLines("", newsTextWithBlankLines);
            return splitStrings.Select(CreateNewsRenderableFromChatPair).ToList();
        }

        public IReadOnlyList<IChatRenderable> GenerateChatRenderables(IEnumerable<ChatData> chatData)
        {
            var ignoreList = _friendIgnoreListService.LoadList(Constants.IgnoreListFile);

            var retList = new List<IChatRenderable>();
            foreach (var data in chatData)
            {
                if (ignoreList.Any(x => x.Equals(data.Who, StringComparison.InvariantCultureIgnoreCase)))
                    continue;

                var splitStrings = SplitTextIntoLines(data.Who, new[] {data.Message});
                var renderables = splitStrings.Select(
                    (pair, i) => CreateChatRenderableFromChatPair(pair, i, data))
                    .ToList();
                retList.AddRange(renderables);
            }

            return retList;
        }

        private IReadOnlyList<ChatPair> SplitTextIntoLines(string who, IReadOnlyList<string> input)
        {
            var retStrings = new List<ChatPair>();
            who = who ?? "";

            string indentForUserName = "  "; //padding string for additional lines if it is a multi-line message
            if (!string.IsNullOrEmpty(who))
                while (_chatFont.MeasureString(indentForUserName).Width < _chatFont.MeasureString(who).Width)
                    indentForUserName += " ";

            var splitter = new TextSplitter("", _chatFont)
            {
                LineLength = LINE_LEN,
                LineIndent = indentForUserName
            };

            foreach (var text in input)
            {
                if (string.IsNullOrWhiteSpace(who) && string.IsNullOrWhiteSpace(text))
                {
                    retStrings.Add(new ChatPair {Text = " ", IsFirstLineOfMultilineMessage = true});
                    continue;
                }

                splitter.Text = text;
                if (!splitter.NeedsProcessing)
                {
                    retStrings.Add(new ChatPair { Text = text, IsFirstLineOfMultilineMessage = true });
                    continue;
                }

                var stringsToAdd = splitter.SplitIntoLines()
                                           .Select((str, i) => new ChatPair {Text = str, IsFirstLineOfMultilineMessage = i == 0})
                                           .ToList();
                retStrings.AddRange(stringsToAdd);
            }

            return retStrings;
        }

        private NewsChatRenderable CreateNewsRenderableFromChatPair(ChatPair pair, int i)
        {
            var shouldShowNoteIcon = pair.IsFirstLineOfMultilineMessage && !string.IsNullOrWhiteSpace(pair.Text);
            var chatData = new ChatData(ChatTab.Local, "", pair.Text, shouldShowNoteIcon ? ChatIcon.Note : ChatIcon.None, log: false);
            return new NewsChatRenderable(_nativeGraphicsManager, i, chatData, pair.Text);
        }

        private ChatRenderable CreateChatRenderableFromChatPair(ChatPair pair, int displayIndex, ChatData data)
        {
            var modifiedData = new ChatData(
                data.Tab,
                pair.IsFirstLineOfMultilineMessage ? data.Who : string.Empty,
                data.Message,
                pair.IsFirstLineOfMultilineMessage ? data.Icon : ChatIcon.None,
                data.ChatColor);

            return new ChatRenderable(_nativeGraphicsManager, displayIndex, modifiedData, pair.Text);
        }
    }
}
