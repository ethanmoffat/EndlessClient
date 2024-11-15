using System;
using EndlessClient.Dialogs.Services;
using EOLib.Domain.Character;
using EOLib.Graphics;
using EOLib.Localization;
using EOLib.Shared;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient.Dialogs
{
    public class SessionExpDialog : BaseEODialog
    {
        private static readonly Rectangle _signalSource;
        private static readonly Rectangle _iconSource;

        private readonly Texture2D _icons;

        static SessionExpDialog()
        {
            _signalSource = new Rectangle(0, 15, 15, 15);
            _iconSource = new Rectangle(0, 0, 15, 15);
        }

        public SessionExpDialog(INativeGraphicsManager nativeGraphicsManager,
                                IEODialogButtonService dialogButtonService,
                                ILocalizedStringFinder localizedStringFinder,
                                ICharacterProvider characterProvider,
                                IExperienceTableProvider expTableProvider,
                                ICharacterSessionProvider characterSessionProvider)
            : base(nativeGraphicsManager, isInGame: true)
        {
            BackgroundTexture = GraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 61);

            _icons = GraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 68, true);

            var okButton = new XNAButton(dialogButtonService.SmallButtonSheet,
                new Vector2(98, 214),
                dialogButtonService.GetSmallDialogButtonOutSource(SmallButton.Ok),
                dialogButtonService.GetSmallDialogButtonOverSource(SmallButton.Ok));
            okButton.OnMouseDown += (_, _) => Close(XNADialogResult.OK);
            okButton.SetParentControl(this);
            okButton.Initialize();

            var title = new XNALabel(Constants.FontSize08pt5)
            {
                DrawPosition = new Vector2(20, 16),
                AutoSize = false,
                Text = localizedStringFinder.GetString(EOResourceID.DIALOG_TITLE_PERFORMANCE),
                ForeColor = ColorConstants.LightGrayText
            };
            title.SetParentControl(this);
            title.Initialize();

            XNALabel[] leftSide = new XNALabel[8], rightSide = new XNALabel[8];
            for (int i = 0; i < leftSide.Length; i++)
            {
                leftSide[i] = new XNALabel(Constants.FontSize08pt5)
                {
                    DrawPosition = new Vector2(38, 48 + 16 * i),
                    AutoSize = false,
                    ForeColor = ColorConstants.LightGrayText
                };
                leftSide[i].SetParentControl(this);
                leftSide[i].Initialize();

                rightSide[i] = new XNALabel(Constants.FontSize08pt5)
                {
                    DrawPosition = new Vector2(158, 48 + 16 * i),
                    AutoSize = false,
                    ForeColor = ColorConstants.LightGrayText
                };
                rightSide[i].SetParentControl(this);
                rightSide[i].Initialize();
            }

            leftSide[0].Text = localizedStringFinder.GetString(EOResourceID.DIALOG_PERFORMANCE_TOTALEXP);
            leftSide[1].Text = localizedStringFinder.GetString(EOResourceID.DIALOG_PERFORMANCE_NEXT_LEVEL);
            leftSide[2].Text = localizedStringFinder.GetString(EOResourceID.DIALOG_PERFORMANCE_EXP_NEEDED);
            leftSide[3].Text = localizedStringFinder.GetString(EOResourceID.DIALOG_PERFORMANCE_TODAY_EXP);
            leftSide[4].Text = localizedStringFinder.GetString(EOResourceID.DIALOG_PERFORMANCE_TOTAL_AVG);
            leftSide[5].Text = localizedStringFinder.GetString(EOResourceID.DIALOG_PERFORMANCE_TODAY_AVG);
            leftSide[6].Text = localizedStringFinder.GetString(EOResourceID.DIALOG_PERFORMANCE_BEST_KILL);
            leftSide[7].Text = localizedStringFinder.GetString(EOResourceID.DIALOG_PERFORMANCE_LAST_KILL);

            var c = characterProvider.MainCharacter;
            var level = c.Stats[CharacterStat.Level];
            var exp = c.Stats[CharacterStat.Experience];
            var usage = c.Stats[CharacterStat.Usage];

            rightSide[0].Text = $"{exp}";
            rightSide[1].Text = $"{expTableProvider.ExperienceByLevel[level + 1]}";
            rightSide[2].Text = $"{expTableProvider.ExperienceByLevel[level + 1] - exp}";
            rightSide[3].Text = $"{characterSessionProvider.TodayTotalExp}";
            rightSide[4].Text = $"{(int)(exp / (usage / 60.0))}";
            int sessionTimeMinutes = (int)(DateTime.Now - characterSessionProvider.SessionStartTime).TotalMinutes;
            rightSide[5].Text = $"{(sessionTimeMinutes > 0 ? (int)(characterSessionProvider.TodayTotalExp / (sessionTimeMinutes / 60.0)) : 0)}";
            rightSide[6].Text = $"{characterSessionProvider.BestKillExp}";
            rightSide[7].Text = $"{characterSessionProvider.LastKillExp}";

            Array.ForEach(leftSide, lbl => lbl.ResizeBasedOnText());
            Array.ForEach(rightSide, lbl => lbl.ResizeBasedOnText());

            CenterInGameView();

            if (!Game.Window.AllowUserResizing)
                DrawPosition = new Vector2(DrawPosition.X, 15);
        }

        protected override void OnDrawControl(GameTime gameTime)
        {
            base.OnDrawControl(gameTime);

            _spriteBatch.Begin();

            for (int i = 0; i < 8; i++)
                _spriteBatch.Draw(_icons, new Vector2(DrawPositionWithParentOffset.X + 18, DrawPositionWithParentOffset.Y + 47 + 16 * i), _iconSource, Color.White);

            for (int i = 0; i < 3; i++)
                _spriteBatch.Draw(_icons, new Vector2(DrawPositionWithParentOffset.X + 142, DrawPositionWithParentOffset.Y + 48 + 16 * i), _signalSource, Color.White);

            _spriteBatch.End();
        }
    }
}
