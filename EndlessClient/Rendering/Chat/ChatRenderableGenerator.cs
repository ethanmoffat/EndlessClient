﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using System.Linq;
using EndlessClient.HUD.Panels;
using EOLib.Domain.Chat;
using Microsoft.Xna.Framework.Graphics;
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

        private readonly SpriteFont _chatFont;

        public ChatRenderableGenerator(SpriteFont chatFont)
        {
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

            return splitStrings.Select(
                (pair, i) => new NewsChatRenderable(i,
                    "",
                    pair.Text,
                    pair.IsFirstLineOfMultilineMessage && !string.IsNullOrWhiteSpace(pair.Text) ? ChatType.Note : ChatType.None)).ToList();
        }

        private IReadOnlyList<ChatPair> SplitTextIntoLines(string who, IReadOnlyList<string> input)
        {
            var retStrings = new List<ChatPair>();
            who = who ?? "";

            string indentForUserName = "  "; //padding string for additional lines if it is a multi-line message
            if (!string.IsNullOrEmpty(who))
                while (_chatFont.MeasureString(indentForUserName).X < _chatFont.MeasureString(who).X)
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
    }
}