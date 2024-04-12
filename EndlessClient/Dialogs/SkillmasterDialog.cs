using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EndlessClient.Content;
using EndlessClient.Dialogs.Factories;
using EndlessClient.Dialogs.Services;
using EndlessClient.HUD;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Interact.Skill;
using EOLib.Graphics;
using EOLib.IO.Repositories;
using EOLib.Localization;
using Microsoft.Xna.Framework;
using Optional.Collections;
using XNAControls;

namespace EndlessClient.Dialogs
{
    public class SkillmasterDialog : ScrollingListDialog
    {
        private enum SkillState
        {
            Initial,
            Learn,
            Forget,
            ForgetAll
        }

        private SkillState _state;

        private HashSet<Skill> _cachedSkills;
        private HashSet<InventorySpell> _cachedSpells;
        private string _cachedTitle;

        private bool _showingRequirements;
        private readonly ISkillmasterActions _skillmasterActions;
        private readonly IEODialogIconService _dialogIconService;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly IStatusLabelSetter _statusLabelSetter;
        private readonly IEOMessageBoxFactory _messageBoxFactory;
        private readonly ITextInputDialogFactory _textInputDialogFactory;
        private readonly ISkillDataProvider _skillDataProvider;
        private readonly ICharacterProvider _characterProvider;
        private readonly ICharacterInventoryProvider _characterInventoryProvider;
        private readonly IPubFileProvider _pubFileProvider;
        private readonly IContentProvider _contentProvider;

        public SkillmasterDialog(INativeGraphicsManager nativeGraphicsManager,
                                 ISkillmasterActions skillmasterActions,
                                 IEODialogButtonService dialogButtonService,
                                 IEODialogIconService dialogIconService,
                                 ILocalizedStringFinder localizedStringFinder,
                                 IStatusLabelSetter statusLabelSetter,
                                 IEOMessageBoxFactory messageBoxFactory,
                                 ITextInputDialogFactory textInputDialogFactory,
                                 ISkillDataProvider skillDataProvider,
                                 ICharacterProvider characterProvider,
                                 ICharacterInventoryProvider characterInventoryProvider,
                                 IPubFileProvider pubFileProvider,
                                 IContentProvider contentProvider)
            : base(nativeGraphicsManager, dialogButtonService)
        {
            Buttons = ScrollingListDialogButtons.Cancel;
            ListItemType = ListDialogItem.ListItemStyle.Large;

            _cachedSkills = new HashSet<Skill>();
            _cachedSpells = new HashSet<InventorySpell>();
            _cachedTitle = string.Empty;

            BackAction += BackClicked;

            _skillmasterActions = skillmasterActions;
            _dialogIconService = dialogIconService;
            _localizedStringFinder = localizedStringFinder;
            _statusLabelSetter = statusLabelSetter;
            _messageBoxFactory = messageBoxFactory;
            _textInputDialogFactory = textInputDialogFactory;
            _skillDataProvider = skillDataProvider;
            _characterProvider = characterProvider;
            _characterInventoryProvider = characterInventoryProvider;
            _pubFileProvider = pubFileProvider;
            _contentProvider = contentProvider;
            SetState(SkillState.Initial, regen: true);
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            if (_cachedTitle != _skillDataProvider.Title)
            {
                Title = _cachedTitle = _skillDataProvider.Title;
            }

            if (!_cachedSkills.SetEquals(_skillDataProvider.Skills))
            {
                _cachedSkills = _skillDataProvider.Skills.ToHashSet();
                SetState(_state, regen: true);
            }

            if (!_cachedSpells.SetEquals(_characterInventoryProvider.SpellInventory))
            {
                _cachedSpells = _characterInventoryProvider.SpellInventory.ToHashSet();
                SetState(_state, regen: true);
            }

            base.OnUpdateControl(gameTime);
        }

        private void BackClicked(object sender, EventArgs e)
        {
            ListItemType = ListDialogItem.ListItemStyle.Large;

            if (_state == SkillState.Learn && _showingRequirements)
            {
                SetState(SkillState.Learn, regen: true);
                _showingRequirements = false;
            }
            else
            {
                SetState(SkillState.Initial);
            }
        }

        private void SetState(SkillState newState, bool regen = false)
        {
            SkillState old = _state;

            if (old == newState && !regen)
                return;

            int numToLearn = _cachedSkills.Count(x => !_cachedSpells.Any(si => si.ID == x.Id));
            int numToForget = _cachedSpells.Count;

            ClearItemList();

            if (newState == SkillState.Learn && numToLearn == 0)
            {
                var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.SKILL_NOTHING_MORE_TO_LEARN);
                dlg.ShowDialog();
                return;
            }

            switch (newState)
            {
                case SkillState.Initial:
                    {
                        string learnNum = $"{numToLearn}{_localizedStringFinder.GetString(EOResourceID.SKILLMASTER_ITEMS_TO_LEARN)}";
                        string forgetNum = $"{numToForget}{_localizedStringFinder.GetString(EOResourceID.SKILLMASTER_ITEMS_LEARNED)}";

                        var learn = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 0)
                        {
                            PrimaryText = _localizedStringFinder.GetString(EOResourceID.SKILLMASTER_WORD_LEARN),
                            SubText = learnNum,
                            IconGraphic = _dialogIconService.IconSheet,
                            IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.Learn),
                            ShowIconBackGround = false,
                            OffsetY = 45
                        };
                        learn.LeftClick += (_, _) => SetState(SkillState.Learn);
                        learn.RightClick += (_, _) => SetState(SkillState.Learn);
                        learn.SetParentControl(this);

                        var forget = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 1)
                        {
                            PrimaryText = _localizedStringFinder.GetString(EOResourceID.SKILLMASTER_WORD_FORGET),
                            SubText = forgetNum,
                            IconGraphic = _dialogIconService.IconSheet,
                            IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.Forget),
                            ShowIconBackGround = false,
                            OffsetY = 45
                        };
                        forget.LeftClick += (_, _) => SetState(SkillState.Forget);
                        forget.RightClick += (_, _) => SetState(SkillState.Forget);
                        forget.SetParentControl(this);

                        var forgetAll = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 2)
                        {
                            PrimaryText = _localizedStringFinder.GetString(EOResourceID.SKILLMASTER_FORGET_ALL),
                            SubText = _localizedStringFinder.GetString(EOResourceID.SKILLMASTER_RESET_YOUR_CHARACTER),
                            IconGraphic = _dialogIconService.IconSheet,
                            IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.Forget),
                            ShowIconBackGround = false,
                            OffsetY = 45
                        };
                        forgetAll.LeftClick += (_, _) => SetState(SkillState.ForgetAll);
                        forgetAll.RightClick += (_, _) => SetState(SkillState.ForgetAll);
                        forgetAll.SetParentControl(this);

                        SetItemList(new List<ListDialogItem> { learn, forget, forgetAll });

                        Buttons = ScrollingListDialogButtons.Cancel;
                    }
                    break;
                case SkillState.Learn:
                    {
                        foreach (var skill in _cachedSkills.Where(x => !_cachedSpells.Any(y => y.ID == x.Id)))
                        {
                            var skillRef = skill;
                            var spellData = _pubFileProvider.ESFFile[skill.Id];

                            var icon = GraphicsManager.TextureFromResource(GFXTypes.SpellIcons, spellData.Icon);
                            var nextListItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large)
                            {
                                Visible = false,
                                PrimaryText = spellData.Name,
                                SubText = _localizedStringFinder.GetString(EOResourceID.SKILLMASTER_WORD_REQUIREMENTS),
                                IconGraphic = icon,
                                IconGraphicSource = new Rectangle(0, 0, icon.Width / 2, icon.Height),
                                ShowIconBackGround = false,
                                OffsetY = 45
                            };
                            nextListItem.LeftClick += (_, _) => Learn(skillRef);
                            nextListItem.RightClick += (o, e) => Learn(skillRef);
                            nextListItem.OnMouseEnter += (o, e) => ShowRequirementsLabel(skillRef);
                            nextListItem.SetSubtextClickAction((_, _) => ShowRequirements(skillRef));
                            AddItemToList(nextListItem, false);
                        }

                        Buttons = ScrollingListDialogButtons.BackCancel;
                    }
                    break;
                case SkillState.Forget:
                    {
                        var input = _textInputDialogFactory.Create(_localizedStringFinder.GetString(DialogResourceID.SKILL_PROMPT_TO_FORGET), 32);

                        input.DialogClosing += (_, args) =>
                        {
                            if (args.Result == XNADialogResult.Cancel)
                                return;

                            _cachedSpells.SingleOrNone(s => string.Equals(_pubFileProvider.ESFFile[s.ID].Name, input.ResponseText, StringComparison.OrdinalIgnoreCase))
                                .Match(
                                    some: si =>
                                    {
                                        _skillmasterActions.ForgetSkill(si.ID);
                                    },
                                    none: () =>
                                    {
                                        args.Cancel = true;

                                        var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.SKILL_FORGET_ERROR_NOT_LEARNED);
                                        dlg.ShowDialog();
                                    });
                        };

                        input.ShowDialog();

                        //should show initial info in the actual dialog since this uses a pop-up input box
                        //    to select a skill to remove
                        newState = SkillState.Initial;
                        goto case SkillState.Initial;
                    }
                case SkillState.ForgetAll:
                    {
                        ShowForgetAllMessage();
                        Buttons = ScrollingListDialogButtons.BackCancel;
                    }
                    break;
            }

            _state = newState;
        }

        private void Learn(Skill skill)
        {

            bool skillReqsMet = true;
            foreach (var req in skill.SkillRequirements.Where(x => x > 0))
            {
                if (!_characterInventoryProvider.SpellInventory.Any(s => s.ID == req))
                {
                    skillReqsMet = false;
                    break;
                }
            }

            var stats = _characterProvider.MainCharacter.Stats;

            if (!skillReqsMet ||
                stats[CharacterStat.Strength] < skill.StrRequirement || stats[CharacterStat.Intelligence] < skill.IntRequirement || stats[CharacterStat.Wisdom] < skill.WisRequirement ||
                stats[CharacterStat.Agility] < skill.AgiRequirement || stats[CharacterStat.Constitution] < skill.ConRequirement || stats[CharacterStat.Charisma] < skill.ChaRequirement ||
                stats[CharacterStat.Level] < skill.LevelRequirement || !_characterInventoryProvider.ItemInventory.SingleOrNone(x => x.ItemID == 1 && x.Amount >= skill.GoldRequirement).HasValue)
            {
                var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.SKILL_LEARN_REQS_NOT_MET);
                dlg.ShowDialog();
            }
            else if (skill.ClassRequirement > 0 && _characterProvider.MainCharacter.ClassID != skill.ClassRequirement)
            {
                var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.SKILL_LEARN_WRONG_CLASS, $" {_pubFileProvider.ECFFile[skill.ClassRequirement].Name}!");
                dlg.ShowDialog();
                return;
            }
            else
            {
                var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.SKILL_LEARN_CONFIRMATION, $" {_pubFileProvider.ESFFile[skill.Id].Name}?", EODialogButtons.OkCancel);
                dlg.DialogClosing += (o, e) =>
                {
                    if (e.Result == XNADialogResult.OK)
                        _skillmasterActions.LearnSkill(skill.Id);
                };
                dlg.ShowDialog();
            }
        }

        private void ShowRequirements(Skill skill)
        {
            _showingRequirements = true;

            ClearItemList();
            ListItemType = ListDialogItem.ListItemStyle.Small;

            var drawStrings = new List<string>
            {
                _pubFileProvider.ESFFile[skill.Id].Name + (skill.ClassRequirement > 0 ? $" [{_pubFileProvider.ECFFile[skill.ClassRequirement].Name}]" : string.Empty),
                " "
            };

            if (skill.SkillRequirements.Any(x => x != 0))
            {
                drawStrings.AddRange(
                    from req in skill.SkillRequirements
                    where req != 0
                    select _localizedStringFinder.GetString(EOResourceID.SKILLMASTER_WORD_SKILL) + ": " + _pubFileProvider.ESFFile[req].Name);
                drawStrings.Add(" ");
            }

            if (skill.StrRequirement > 0)
                drawStrings.Add($"{skill.StrRequirement} {_localizedStringFinder.GetString(EOResourceID.SKILLMASTER_WORD_STRENGTH)}");
            if (skill.IntRequirement > 0)
                drawStrings.Add($"{skill.IntRequirement} {_localizedStringFinder.GetString(EOResourceID.SKILLMASTER_WORD_INTELLIGENCE)}");
            if (skill.WisRequirement > 0)
                drawStrings.Add($"{skill.WisRequirement} {_localizedStringFinder.GetString(EOResourceID.SKILLMASTER_WORD_WISDOM)}");
            if (skill.AgiRequirement > 0)
                drawStrings.Add($"{skill.AgiRequirement} { _localizedStringFinder.GetString(EOResourceID.SKILLMASTER_WORD_AGILITY)}");
            if (skill.ConRequirement > 0)
                drawStrings.Add($"{skill.ConRequirement} {_localizedStringFinder.GetString(EOResourceID.SKILLMASTER_WORD_CONSTITUTION)}");
            if (skill.ChaRequirement > 0)
                drawStrings.Add($"{skill.ChaRequirement} {_localizedStringFinder.GetString(EOResourceID.SKILLMASTER_WORD_CHARISMA)}");

            drawStrings.Add(" ");
            drawStrings.Add($"{skill.LevelRequirement} {_localizedStringFinder.GetString(EOResourceID.SKILLMASTER_WORD_LEVEL)}");
            drawStrings.Add($"{skill.GoldRequirement} {_pubFileProvider.EIFFile[1].Name}");

            SetItemList(drawStrings.Select(x => new ListDialogItem(this, ListItemType) { PrimaryText = x }).ToList());
        }

        private void ShowRequirementsLabel(Skill skill)
        {
            var full = new StringBuilder();

            full.Append($"{_pubFileProvider.ESFFile[skill.Id].Name} {skill.LevelRequirement} LVL, ");

            if (skill.StrRequirement > 0)
                full.Append($"{skill.StrRequirement} STR, ");
            if (skill.IntRequirement > 0)
                full.Append($"{skill.IntRequirement} INT, ");
            if (skill.WisRequirement > 0)
                full.Append($"{skill.WisRequirement} WIS, ");
            if (skill.AgiRequirement > 0)
                full.Append($"{skill.AgiRequirement} AGI, ");
            if (skill.ConRequirement > 0)
                full.Append($"{skill.ConRequirement} CON, ");
            if (skill.ChaRequirement > 0)
                full.Append($"{skill.ChaRequirement} CHA, ");
            if (skill.GoldRequirement > 0)
                full.Append($"{skill.GoldRequirement} {_pubFileProvider.EIFFile[1].Name}");
            if (skill.ClassRequirement > 0)
                full.Append($", {_pubFileProvider.ECFFile[skill.ClassRequirement].Name}");

            _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_INFORMATION, full.ToString());
        }

        private void ShowForgetAllMessage()
        {
            AddTextAsListItems(_contentProvider.Fonts[Constants.FontSize09],
                () =>
                {
                    var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.SKILL_RESET_CHARACTER_CONFIRMATION, EODialogButtons.OkCancel);
                    dlg.DialogClosing += (_, args) =>
                    {
                        // todo: test how GameServer handles character reset with paperdoll items still equipped
                        if (args.Result == XNADialogResult.OK)
                        {
                            _skillmasterActions.ResetCharacter();
                        }
                    };
                    dlg.ShowDialog();
                },
                _localizedStringFinder.GetString(EOResourceID.SKILLMASTER_FORGET_ALL),
                _localizedStringFinder.GetString(EOResourceID.SKILLMASTER_FORGET_ALL_MSG_1),
                _localizedStringFinder.GetString(EOResourceID.SKILLMASTER_FORGET_ALL_MSG_2),
                _localizedStringFinder.GetString(EOResourceID.SKILLMASTER_FORGET_ALL_MSG_3),
                _localizedStringFinder.GetString(EOResourceID.SKILLMASTER_CLICK_HERE_TO_FORGET_ALL)
            );
        }
    }
}
