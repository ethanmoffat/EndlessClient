using System;
using System.Linq;
using EndlessClient.Controllers;
using EndlessClient.Dialogs;
using EndlessClient.Dialogs.Factories;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using XNAControls;

namespace EndlessClient.HUD.Panels
{
    public class StatsPanel : XNAPanel, IHudPanel
    {
        private readonly ICharacterProvider _characterProvider;
        private readonly ICharacterInventoryProvider _characterInventoryProvider;
        private readonly IExperienceTableProvider _experienceTableProvider;
        private readonly IEOMessageBoxFactory _messageBoxFactory;
        private readonly ITrainingController _trainingController;
        private const int STR = 0, INT = 1, WIS = 2, AGI = 3, CON = 4, CHA = 5;
        private readonly IXNALabel[] _basicStats;
        private readonly IXNAButton[] _arrowButtons;

        private const int HP = 0, TP = 1, DAM = 2, ACC = 3, ARM = 4, EVA = 5;
        private readonly IXNALabel[] _characterStats;

        private const int NAME = 0, LEVEL = 1, GUILD = 2;
        private readonly IXNALabel[] _characterInfo;

        private const int WEIGHT = 0, STATPTS = 1, SKILLPTS = 2, ELEM = 3,
                          GOLD = 4, EXP = 5, TNL = 6, KARMA = 7;
        private readonly IXNALabel[] _otherInfo;

        private CharacterStats _lastCharacterStats;
        private InventoryItem _lastCharacterGold;
        private bool _confirmedTraining;

        public StatsPanel(INativeGraphicsManager nativeGraphicsManager,
                          ICharacterProvider characterProvider,
                          ICharacterInventoryProvider characterInventoryProvider,
                          IExperienceTableProvider experienceTableProvider,
                          IEOMessageBoxFactory messageBoxFactory,
                          ITrainingController trainingController)
        {
            _characterProvider = characterProvider;
            _characterInventoryProvider = characterInventoryProvider;
            _experienceTableProvider = experienceTableProvider;
            _messageBoxFactory = messageBoxFactory;
            _trainingController = trainingController;

            BackgroundImage = nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 34);
            DrawArea = new Rectangle(102, 330, BackgroundImage.Width, BackgroundImage.Height);

            _basicStats = new IXNALabel[6];
            _arrowButtons = new IXNAButton[6];
            _characterStats = new IXNALabel[6];
            _characterInfo = new IXNALabel[3];
            _otherInfo = new IXNALabel[8];

            var buttonTexture = nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 27, true);
            var outTextureArea = new Rectangle(215, 386, 19, 15);
            var overTextureArea = new Rectangle(234, 386, 19, 15);

            for (int i = 0; i < _basicStats.Length; ++i)
            {
                _basicStats[i] = new XNALabel(Constants.FontSize08pt5)
                {
                    ForeColor = ColorConstants.LightGrayText,
                    AutoSize = false,
                    Text = "",
                    DrawArea = new Rectangle(50, 8 + i*18, 73, 13)
                };

                _arrowButtons[i] = new XNAButton(buttonTexture,
                                                 new Vector2(106, 7 + i*18),
                                                 outTextureArea,
                                                 overTextureArea)
                {
                    Visible = false,
                    FlashSpeed = 500
                };
            }

            for (int i = 0; i < _characterStats.Length; ++i)
            {
                _characterStats[i] = new XNALabel(Constants.FontSize08pt5)
                {
                    ForeColor = ColorConstants.LightGrayText,
                    AutoSize = false,
                    Text = "",
                    DrawArea = new Rectangle(158, 8 + i * 18, 73, 13)
                };
            }

            for (int i = 0; i < _otherInfo.Length; ++i)
            {
                var drawArea = i < 4
                    ? new Rectangle(280, 44 + i%4*18, 60, 13)
                    : new Rectangle(379, 44 + i%4*18, 94, 13);

                _otherInfo[i] = new XNALabel(Constants.FontSize08pt5)
                {
                    ForeColor = ColorConstants.LightGrayText,
                    AutoSize = false,
                    Text = "",
                    DrawArea = drawArea
                };
            }

            _characterInfo[NAME] = new XNALabel(Constants.FontSize08pt5)
            {
                ForeColor = ColorConstants.LightGrayText,
                AutoSize = false,
                Text = "",
                DrawArea = new Rectangle(280, 8, 144, 13)
            };
            _characterInfo[GUILD] = new XNALabel(Constants.FontSize08pt5)
            {
                ForeColor = ColorConstants.LightGrayText,
                AutoSize = false,
                Text = "",
                DrawArea = new Rectangle(280, 26, 193, 13)
            };
            _characterInfo[LEVEL] = new XNALabel(Constants.FontSize08pt5)
            {
                ForeColor = ColorConstants.LightGrayText,
                AutoSize = false,
                Text = "",
                DrawArea = new Rectangle(453, 8, 20, 13)
            };
        }

        public override void Initialize()
        {
            foreach (var control in _arrowButtons)
                control.OnClick += HandleArrowButtonClick;

            var controls = _basicStats.Concat<IXNAControl>(_characterStats)
                                      .Concat(_arrowButtons)
                                      .Concat(_characterInfo)
                                      .Concat(_otherInfo);
            foreach (var control in controls)
            {
                control.SetParentControl(this);
                control.Initialize();
            }

            base.Initialize();
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            if (_lastCharacterStats != _characterProvider.MainCharacter.Stats ||
                _lastCharacterGold != CurrentCharacterGold)
            {
                _lastCharacterStats = _characterProvider.MainCharacter.Stats;
                _lastCharacterGold = CurrentCharacterGold;

                _basicStats[STR].Text = $"{_lastCharacterStats[CharacterStat.Strength]}";
                _basicStats[INT].Text = $"{_lastCharacterStats[CharacterStat.Intelligence]}";
                _basicStats[WIS].Text = $"{_lastCharacterStats[CharacterStat.Wisdom]}";
                _basicStats[AGI].Text = $"{_lastCharacterStats[CharacterStat.Agility]}";
                _basicStats[CON].Text = $"{_lastCharacterStats[CharacterStat.Constituion]}";
                _basicStats[CHA].Text = $"{_lastCharacterStats[CharacterStat.Charisma]}";

                _characterStats[HP].Text = $"{_lastCharacterStats[CharacterStat.HP]}";
                _characterStats[TP].Text = $"{_lastCharacterStats[CharacterStat.TP]}";
                _characterStats[DAM].Text = $"{_lastCharacterStats[CharacterStat.MinDam]} - {_lastCharacterStats[CharacterStat.MaxDam]}";
                _characterStats[ACC].Text = $"{_lastCharacterStats[CharacterStat.Accuracy]}";
                _characterStats[ARM].Text = $"{_lastCharacterStats[CharacterStat.Evade]}";
                _characterStats[EVA].Text = $"{_lastCharacterStats[CharacterStat.Armor]}";

                _otherInfo[WEIGHT].Text = $"{_lastCharacterStats[CharacterStat.Weight]} / {_lastCharacterStats[CharacterStat.MaxWeight]}";
                _otherInfo[STATPTS].Text = $"{_lastCharacterStats[CharacterStat.StatPoints]}";
                _otherInfo[SKILLPTS].Text = $"{_lastCharacterStats[CharacterStat.SkillPoints]}";
                _otherInfo[ELEM].Text = ""; //Elements are not supported by Endless Online :(
                _otherInfo[GOLD].Text = $"{CurrentCharacterGold.Amount}";
                _otherInfo[EXP].Text = $"{_lastCharacterStats[CharacterStat.Experience]}";
                _otherInfo[TNL].Text = $"{ExperienceToNextLevel}";
                _otherInfo[KARMA].Text = $"{_lastCharacterStats.GetKarmaString()}";

                _characterInfo[NAME].Text = $"{_characterProvider.MainCharacter.Name}";
                _characterInfo[GUILD].Text = $"{_characterProvider.MainCharacter.GuildName}";
                _characterInfo[LEVEL].Text = $"{_lastCharacterStats[CharacterStat.Level]}";

                if (_lastCharacterStats.Stats[CharacterStat.StatPoints] > 0)
                {
                    foreach (var button in _arrowButtons.OfType<XNAButton>())
                        button.Visible = true;
                }
                else
                {
                    foreach (var button in _arrowButtons.OfType<XNAButton>())
                        button.Visible = false;
                    _confirmedTraining = false;
                }
            }

            base.OnUpdateControl(gameTime);
        }

        private async void HandleArrowButtonClick(object sender, EventArgs e)
        {
            if (!_confirmedTraining)
            {
                var dialog = _messageBoxFactory.CreateMessageBox("Do you want to train?",
                    "Character training",
                    EODialogButtons.OkCancel);

                var result = await dialog.ShowDialogAsync();
                if (result == XNADialogResult.OK)
                    _confirmedTraining = true;
            }
            else
            {
                var index = _arrowButtons.Select((btn, ndx) => new {btn, ndx})
                                         .Single(x => x.btn == sender).ndx;
                var characterStat = CharacterStat.Strength + index;
                _trainingController.AddStatPoint(characterStat);
            }
        }

        private InventoryItem CurrentCharacterGold
            => _characterInventoryProvider.ItemInventory.Single(x => x.ItemID == 1);

        private int ExperienceToNextLevel =>
            _experienceTableProvider.ExperienceByLevel[
                _characterProvider.MainCharacter.Stats[CharacterStat.Level] + 1
                ] - _lastCharacterStats[CharacterStat.Experience];
    }
}