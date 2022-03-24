using AutomaticTypeMapper;
using EOLib;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace EndlessClient.Content
{
    public interface IContentProvider
    {
        IReadOnlyDictionary<string, Texture2D> Textures { get; }

        IReadOnlyDictionary<string, SpriteFont> Fonts { get; }

        void SetContentManager(ContentManager content);

        void Load();
    }

    [AutoMappedType(IsSingleton = true)]
    public class ContentProvider : IContentProvider
    {
        private readonly Dictionary<string, Texture2D> _textures;
        private readonly Dictionary<string, SpriteFont> _fonts;

        private ContentManager _content;

        public const string Cursor = "cursor";

        public const string TBBack = "tbBack";
        public const string TBLeft = "tbLeft";
        public const string TBRight = "tbRight";

        public const string ChatTL = @"ChatBubble\TL";
        public const string ChatTM = @"ChatBubble\TM";
        public const string ChatTR = @"ChatBubble\TR";
        public const string ChatML = @"ChatBubble\ML";
        public const string ChatMM = @"ChatBubble\MM";
        public const string ChatMR = @"ChatBubble\MR";
        public const string ChatRL = @"ChatBubble\RL";
        public const string ChatRM = @"ChatBubble\RM";
        public const string ChatRR = @"ChatBubble\RR";
        public const string ChatNUB = @"ChatBubble\NUB";

        public IReadOnlyDictionary<string, Texture2D> Textures => _textures;

        public IReadOnlyDictionary<string, SpriteFont> Fonts => _fonts;

        public ContentProvider()
        {
            _textures = new Dictionary<string, Texture2D>();
            _fonts = new Dictionary<string, SpriteFont>();
        }

        public void SetContentManager(ContentManager content)
        {
            _content = content;
        }

        public void Load()
        {
            RefreshTextures();
            RefreshFonts();
        }

        private void RefreshTextures()
        {
            if (_content == null)
                return;

            _textures[Cursor] = _content.Load<Texture2D>(Cursor);

            _textures[TBBack] = _content.Load<Texture2D>(TBBack);
            _textures[TBLeft] = _content.Load<Texture2D>(TBLeft);
            _textures[TBRight] = _content.Load<Texture2D>(TBRight);

            _textures[ChatTL] = _content.Load<Texture2D>(ChatTL);
            _textures[ChatTM] = _content.Load<Texture2D>(ChatTM);
            _textures[ChatTR] = _content.Load<Texture2D>(ChatTR);
            _textures[ChatML] = _content.Load<Texture2D>(ChatML);
            _textures[ChatMM] = _content.Load<Texture2D>(ChatMM);
            _textures[ChatMR] = _content.Load<Texture2D>(ChatMR);
            _textures[ChatRL] = _content.Load<Texture2D>(ChatRL);
            _textures[ChatRM] = _content.Load<Texture2D>(ChatRM);
            _textures[ChatRR] = _content.Load<Texture2D>(ChatRR);
            _textures[ChatNUB] = _content.Load<Texture2D>(ChatNUB);
        }

        private void RefreshFonts()
        {
            _fonts[Constants.FontSize08] = _content.Load<SpriteFont>(Constants.FontSize08);
        }
    }
}
