using EndlessClient.Dialogs.Services;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Online;
using EOLib.Extensions;
using EOLib.Graphics;
using EOLib.IO.Repositories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using Optional;
using Optional.Unsafe;
using XNAControls;

namespace EndlessClient.Dialogs
{
    public class PlayerInfoDialog : BaseEODialog
    {
        private static readonly Rectangle _iconDrawRect = new Rectangle(227, 258, 44, 21);

        private readonly IPubFileProvider _pubFileProvider;
        private readonly IPaperdollProvider _paperdollProvider;
        protected readonly bool _isMainCharacter;

        private readonly Texture2D _characterIconSheet;
        private Option<Rectangle> _characterIconSourceRect;

        private readonly IXNALabel _name,
            _home,
            _class,
            _partner,
            _title,
            _guild,
            _rank;

        public Character Character { get; }

        private Option<PaperdollData> _paperdollData;

        public PlayerInfoDialog(INativeGraphicsManager graphicsManager,
                                IEODialogButtonService eoDialogButtonService,
                                IPubFileProvider pubFileProvider,
                                IPaperdollProvider paperdollProvider,
                                Character character,
                                bool isMainCharacter)
            : base(graphicsManager, isInGame: true)
        {
            _pubFileProvider = pubFileProvider;
            _paperdollProvider = paperdollProvider;

            Character = character;
            _isMainCharacter = isMainCharacter;

            _characterIconSheet = GraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 32, true);
            _characterIconSourceRect = Option.None<Rectangle>();

            _name = new XNALabel(Constants.FontSize08pt5) { DrawArea = new Rectangle(228, 22, 1, 1), ForeColor = ColorConstants.LightGrayText };
            _name.SetParentControl(this);

            _home = new XNALabel(Constants.FontSize08pt5) { DrawArea = new Rectangle(228, 52, 1, 1), ForeColor = ColorConstants.LightGrayText };
            _home.SetParentControl(this);

            _class = new XNALabel(Constants.FontSize08pt5) { DrawArea = new Rectangle(228, 82, 1, 1), ForeColor = ColorConstants.LightGrayText };
            _class.SetParentControl(this);

            _partner = new XNALabel(Constants.FontSize08pt5) { DrawArea = new Rectangle(228, 112, 1, 1), ForeColor = ColorConstants.LightGrayText };
            _partner.SetParentControl(this);

            _title = new XNALabel(Constants.FontSize08pt5) { DrawArea = new Rectangle(228, 142, 1, 1), ForeColor = ColorConstants.LightGrayText };
            _title.SetParentControl(this);

            _guild = new XNALabel(Constants.FontSize08pt5) { DrawArea = new Rectangle(228, 202, 1, 1), ForeColor = ColorConstants.LightGrayText };
            _guild.SetParentControl(this);

            _rank = new XNALabel(Constants.FontSize08pt5) { DrawArea = new Rectangle(228, 232, 1, 1), ForeColor = ColorConstants.LightGrayText };
            _rank.SetParentControl(this);

            var okButton = new XNAButton(eoDialogButtonService.SmallButtonSheet,
            new Vector2(276, 253),
                eoDialogButtonService.GetSmallDialogButtonOutSource(SmallButton.Ok),
                eoDialogButtonService.GetSmallDialogButtonOverSource(SmallButton.Ok));
            okButton.OnMouseDown += (_, _) => Close(XNADialogResult.OK);
            okButton.Initialize();
            okButton.SetParentControl(this);

            _paperdollData = Option.None<PaperdollData>();
        }

        public override void Initialize()
        {
            _name.Initialize();
            _home.Initialize();
            _class.Initialize();
            _partner.Initialize();
            _title.Initialize();
            _guild.Initialize();
            _rank.Initialize();

            base.Initialize();
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            _paperdollData = _paperdollData.FlatMap(paperdollData =>
                paperdollData.NoneWhen(d => _paperdollProvider.VisibleCharacterPaperdolls.ContainsKey(Character.ID) &&
                                           !_paperdollProvider.VisibleCharacterPaperdolls[Character.ID].Equals(d)));

            _paperdollData.MatchNone(() =>
            {
                if (_paperdollProvider.VisibleCharacterPaperdolls.ContainsKey(Character.ID))
                {
                    var paperdollData = _paperdollProvider.VisibleCharacterPaperdolls[Character.ID];
                    _paperdollData = Option.Some(paperdollData);
                    UpdateDisplayedData(paperdollData);
                }
            });

            base.OnUpdateControl(gameTime);
        }

        protected override void OnDrawControl(GameTime gameTime)
        {
            base.OnDrawControl(gameTime);

            _spriteBatch.Begin();

            _characterIconSourceRect.MatchSome(sourceRect =>
            {
                _spriteBatch.Draw(_characterIconSheet,
                    new Vector2(
                        DrawAreaWithParentOffset.X + _iconDrawRect.X + (_iconDrawRect.Width / 2) - (sourceRect.Width / 2),
                        DrawAreaWithParentOffset.Y + _iconDrawRect.Y + (_iconDrawRect.Height / 2) - (sourceRect.Height / 2)),
                    sourceRect,
                    Color.White);
            });

            _spriteBatch.End();
        }

        protected virtual void UpdateDisplayedData(PaperdollData paperdollData)
        {
            _name.Text = Capitalize(paperdollData.Name);
            _home.Text = Capitalize(paperdollData.Home);

            paperdollData.Class.SomeWhen(x => x != 0)
                .MatchSome(classId => _class.Text = Capitalize(_pubFileProvider.ECFFile[classId].Name));

            _partner.Text = Capitalize(paperdollData.Partner);
            _title.Text = Capitalize(paperdollData.Title);
            _guild.Text = Capitalize(paperdollData.Guild);
            _rank.Text = Capitalize(paperdollData.Rank);

            _characterIconSourceRect = Option.Some(GetOnlineIconSourceRectangle(paperdollData.Icon));
        }

        private static string Capitalize(string input) =>
            string.IsNullOrEmpty(input) ? string.Empty : char.ToUpper(input[0]) + input[1..].ToLower();

        private static Rectangle GetOnlineIconSourceRectangle(CharacterIcon icon)
        {
            var (x, y, width, height) = icon.ToChatIcon().GetChatIconRectangleBounds().ValueOrDefault();
            return new Rectangle(x, y, width, height);
        }
    }
}
