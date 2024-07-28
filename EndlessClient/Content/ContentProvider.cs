using AutomaticTypeMapper;
using EndlessClient.Audio;
using EOLib;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EndlessClient.Content
{
    public interface IContentProvider
    {
        IReadOnlyDictionary<string, Texture2D> Textures { get; }

        IReadOnlyDictionary<string, BitmapFont> Fonts { get; }

        IReadOnlyDictionary<SoundEffectID, SoundEffect> SFX { get; }

        IReadOnlyList<SoundEffect> HarpNotes { get; }

        IReadOnlyList<SoundEffect> GuitarNotes { get; }

        void SetContentManager(ContentManager content);

        void Load();
    }

    [AutoMappedType(IsSingleton = true)]
    public class ContentProvider : IContentProvider
    {
        private readonly Dictionary<string, Texture2D> _textures;
        private readonly Dictionary<string, BitmapFont> _fonts;
        private readonly Dictionary<SoundEffectID, SoundEffect> _sfx;
        private readonly List<SoundEffect> _harpNotes;
        private readonly List<SoundEffect> _guitarNotes;

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

        public const string HPOutline = @"Party\hp-outline";
        public const string HPRed = @"Party\hp-red";
        public const string HPYellow = @"Party\hp-yellow";
        public const string HPGreen = @"Party\hp-green";

        public IReadOnlyDictionary<string, Texture2D> Textures => _textures;

        public IReadOnlyDictionary<string, BitmapFont> Fonts => _fonts;

        public IReadOnlyDictionary<SoundEffectID, SoundEffect> SFX => _sfx;

        public IReadOnlyList<SoundEffect> HarpNotes => _harpNotes;

        public IReadOnlyList<SoundEffect> GuitarNotes => _guitarNotes;

        public ContentProvider()
        {
            _textures = new Dictionary<string, Texture2D>();
            _fonts = new Dictionary<string, BitmapFont>();
            _sfx = new Dictionary<SoundEffectID, SoundEffect>();
            _harpNotes = new List<SoundEffect>();
            _guitarNotes = new List<SoundEffect>();
        }

        public void SetContentManager(ContentManager content)
        {
            _content = content;
        }

        public void Load()
        {
            RefreshTextures();
            RefreshFonts();
            LoadSFX();
            LoadHarp();
            LoadGuitar();
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

            _textures[HPOutline] = _content.Load<Texture2D>(HPOutline);
            _textures[HPRed] = _content.Load<Texture2D>(HPRed);
            _textures[HPYellow] = _content.Load<Texture2D>(HPYellow);
            _textures[HPGreen] = _content.Load<Texture2D>(HPGreen);
        }

        private void RefreshFonts()
        {
            _fonts[Constants.FontSize08] = _content.Load<BitmapFont>(Constants.FontSize08);
            _fonts[Constants.FontSize08pt5] = _content.Load<BitmapFont>(Constants.FontSize08pt5);
            _fonts[Constants.FontSize09] = _content.Load<BitmapFont>(Constants.FontSize09);
        }

        private void LoadSFX()
        {
            var id = (SoundEffectID)0;
            foreach (var sfxFile in GetSoundEffects("sfx*.wav"))
                _sfx[id++] = sfxFile;
            if (_sfx.Count < 81)
                throw new FileNotFoundException("Unexpected number of SFX");
        }

        private void LoadHarp()
        {
            _harpNotes.AddRange(GetSoundEffects("har*.wav"));
            if (_harpNotes.Count != 36)
                throw new FileNotFoundException("Unexpected number of harp SFX");
        }

        private void LoadGuitar()
        {
            _guitarNotes.AddRange(GetSoundEffects("gui*.wav"));
            if (_guitarNotes.Count != 36)
                throw new FileNotFoundException("Unexpected number of guitar SFX");
        }

        private static IEnumerable<SoundEffect> GetSoundEffects(string filter)
        {
            var sfxFiles = Directory.GetFiles(Constants.SfxDirectory, filter).ToList();
            sfxFiles.Sort();

            foreach (var file in sfxFiles)
            {
                using var wavStream = WAVFileValidator.GetStreamWithCorrectLengthHeader(file);
                yield return SoundEffect.FromStream(wavStream);
            }
        }
    }
}