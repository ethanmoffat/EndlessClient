using System;
using System.Collections.Generic;
using System.Linq;
using EndlessClient.Dialogs.Factories;
using EndlessClient.Dialogs.Services;
using EOLib.Domain.Character;
using EOLib.Domain.Interact.Skill;
using EOLib.Graphics;
using EOLib.Localization;
using Microsoft.Xna.Framework;

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
        private HashSet<IInventorySpell> _cachedSpells;
        private string _cachedTitle;

        private bool _showingRequirements;

        private readonly IEODialogIconService _dialogIconService;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly IEOMessageBoxFactory _messageBoxFactory;
        private readonly ISkillDataProvider _skillDataProvider;
        private readonly ICharacterInventoryProvider _characterInventoryProvider;

        public SkillmasterDialog(INativeGraphicsManager nativeGraphicsManager,
                                 IEODialogButtonService dialogButtonService,
                                 IEODialogIconService dialogIconService,
                                 ILocalizedStringFinder localizedStringFinder,
                                 IEOMessageBoxFactory messageBoxFactory,
                                 ISkillDataProvider skillDataProvider,
                                 ICharacterInventoryProvider characterInventoryProvider)
            : base(nativeGraphicsManager, dialogButtonService)
        {
            Buttons = ScrollingListDialogButtons.Cancel;
            ListItemType = ListDialogItem.ListItemStyle.Large;

            _cachedSkills = new HashSet<Skill>();
            _cachedSpells = new HashSet<IInventorySpell>();
            _cachedTitle = string.Empty;

            BackAction += BackClicked;

            _dialogIconService = dialogIconService;
            _localizedStringFinder = localizedStringFinder;
            _messageBoxFactory = messageBoxFactory;
            _skillDataProvider = skillDataProvider;
            _characterInventoryProvider = characterInventoryProvider;

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

            if (newState == SkillState.Learn && numToLearn == 0)
            {
                var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.SKILL_NOTHING_MORE_TO_LEARN);
                dlg.ShowDialog();
                return;
            }

            ClearItemList();

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
                    //int index = 0;
                    //for (int i = 0; i < m_skills.Count; ++i)
                    //{
                    //    if (OldWorld.Instance.MainPlayer.ActiveCharacter.Spells.FindIndex(_sp => m_skills[i].ID == _sp.ID) >= 0)
                    //        continue;
                    //    int localI = i;

                    //    var spellData = OldWorld.Instance.ESF[m_skills[localI].ID];

                    //    OldListDialogItem nextListItem = new OldListDialogItem(this, OldListDialogItem.ListItemStyle.Large, index++)
                    //    {
                    //        Visible = false,
                    //        Text = spellData.Name,
                    //        SubText = OldWorld.GetString(EOResourceID.SKILLMASTER_WORD_REQUIREMENTS),
                    //        IconGraphic = OldWorld.GetSpellIcon(spellData.Icon, false),
                    //        ShowItemBackGround = false,
                    //        OffsetY = 45,
                    //        ID = m_skills[localI].ID
                    //    };
                    //    nextListItem.OnLeftClick += (o, e) => _learn(m_skills[localI]);
                    //    nextListItem.OnRightClick += (o, e) => _learn(m_skills[localI]);
                    //    nextListItem.OnMouseEnter += (o, e) => _showRequirementsLabel(m_skills[localI]);
                    //    nextListItem.SetSubtextLink(() => _showRequirements(m_skills[localI]));
                    //    AddItemToList(nextListItem, false);
                    //}

                    //_setButtons(ScrollingListDialogButtons.BackCancel);
                }
                    break;
                case SkillState.Forget:
                {
                        //TextInputDialog input = new TextInputDialog(OldWorld.GetString(DialogResourceID.SKILL_PROMPT_TO_FORGET, false), 32);
                        //input.SetAsKeyboardSubscriber();
                        //input.DialogClosing += (sender, args) =>
                        //{
                        //    if (args.Result == XNADialogResult.Cancel) return;
                        //    bool found =
                        //        OldWorld.Instance.MainPlayer.ActiveCharacter.Spells.Any(
                        //            _spell => OldWorld.Instance.ESF[_spell.ID].Name.ToLower() == input.ResponseText.ToLower());

                        //    if (!found)
                        //    {
                        //        args.CancelClose = true;
                        //        EOMessageBox.Show(DialogResourceID.SKILL_FORGET_ERROR_NOT_LEARNED, EODialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
                        //        input.SetAsKeyboardSubscriber();
                        //    }

                        //    if (!m_api.ForgetSpell(
                        //            OldWorld.Instance.MainPlayer.ActiveCharacter.Spells.Find(
                        //                _spell => OldWorld.Instance.ESF[_spell.ID].Name.ToLower() == input.ResponseText.ToLower()).ID))
                        //    {
                        //        Close();
                        //        ((EOGame)Game).DoShowLostConnectionDialogAndReturnToMainMenu();
                        //    }
                        //};

                        ////should show initial info in the actual dialog since this uses a pop-up input box
                        ////    to select a skill to remove
                        //newState = SkillState.Initial;
                        goto case SkillState.Initial;
                    }
                case SkillState.ForgetAll:
                {
                    //_showForgetAllMessage(_forgetAllAction);
                    //_setButtons(ScrollingListDialogButtons.BackCancel);
                }
                    break;
            }

            _state = newState;
        }

        //private void _learn(Skill skill)
        //{
        //    OldCharacter c = OldWorld.Instance.MainPlayer.ActiveCharacter;

        //    bool skillReqsMet = true;
        //    foreach(short x in skill.SkillReq)
        //        if (x != 0 && c.Spells.FindIndex(_sp => _sp.ID == x) < 0)
        //            skillReqsMet = false;

        //    //check the requirements
        //    if (c.Stats.Str < skill.StrReq || c.Stats.Int < skill.IntReq || c.Stats.Wis < skill.WisReq ||
        //        c.Stats.Agi < skill.AgiReq || c.Stats.Con < skill.ConReq || c.Stats.Cha < skill.ChaReq ||
        //        c.Stats.Level < skill.LevelReq || c.Inventory.Find(_ii => _ii.ItemID == 1).Amount < skill.GoldReq || !skillReqsMet)
        //    {
        //        EOMessageBox.Show(DialogResourceID.SKILL_LEARN_REQS_NOT_MET, EODialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
        //        return;
        //    }

        //    if (skill.ClassReq > 0 && c.Class != skill.ClassReq)
        //    {
        //        EOMessageBox.Show(DialogResourceID.SKILL_LEARN_WRONG_CLASS, " " + OldWorld.Instance.ECF[skill.ClassReq].Name + "!", EODialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
        //        return;
        //    }

        //    EOMessageBox.Show(DialogResourceID.SKILL_LEARN_CONFIRMATION, " " + OldWorld.Instance.ESF[skill.ID].Name + "?", EODialogButtons.OkCancel, EOMessageBoxStyle.SmallDialogSmallHeader,
        //        (o, e) =>
        //        {
        //            if (e.Result != XNADialogResult.OK)
        //                return;

        //            if (!m_api.LearnSpell(skill.ID))
        //            {
        //                Close();
        //                ((EOGame)Game).DoShowLostConnectionDialogAndReturnToMainMenu();
        //            }
        //        });
        //}

        //private void _forgetAllAction()
        //{
        //    EOMessageBox.Show(DialogResourceID.SKILL_RESET_CHARACTER_CONFIRMATION, EODialogButtons.OkCancel, EOMessageBoxStyle.SmallDialogSmallHeader,
        //        (sender, args) =>
        //        {
        //            if (args.Result == XNADialogResult.Cancel) return;

        //            if (!m_api.ResetCharacterStatSkill())
        //            {
        //                Close();
        //                ((EOGame) Game).DoShowLostConnectionDialogAndReturnToMainMenu();
        //            }
        //        });
        //}

        //private void _showRequirements(Skill skill)
        //{
        //    m_showingRequirements = true;
        //    ClearItemList();

        //    List<string> drawStrings = new List<string>(15)
        //    {
        //        OldWorld.Instance.ESF[skill.ID].Name + (skill.ClassReq > 0 ? " [" + OldWorld.Instance.ECF[skill.ClassReq].Name + "]" : ""),
        //        " "
        //    };
        //    if (skill.SkillReq.Any(x => x != 0))
        //    {
        //        drawStrings.AddRange(from req in skill.SkillReq where req != 0 select OldWorld.GetString(EOResourceID.SKILLMASTER_WORD_SKILL) + ": " +  OldWorld.Instance.ESF[req].Name);
        //        drawStrings.Add(" ");
        //    }

        //    if(skill.StrReq > 0)
        //        drawStrings.Add(skill.StrReq + " " + OldWorld.GetString(EOResourceID.SKILLMASTER_WORD_STRENGTH));
        //    if (skill.IntReq > 0)
        //        drawStrings.Add(skill.IntReq + " " + OldWorld.GetString(EOResourceID.SKILLMASTER_WORD_INTELLIGENCE));
        //    if (skill.WisReq > 0)
        //        drawStrings.Add(skill.WisReq + " " + OldWorld.GetString(EOResourceID.SKILLMASTER_WORD_WISDOM));
        //    if (skill.AgiReq > 0)
        //        drawStrings.Add(skill.AgiReq + " " + OldWorld.GetString(EOResourceID.SKILLMASTER_WORD_AGILITY));
        //    if (skill.ConReq > 0)
        //        drawStrings.Add(skill.ConReq + " " + OldWorld.GetString(EOResourceID.SKILLMASTER_WORD_CONSTITUTION));
        //    if (skill.ChaReq > 0)
        //        drawStrings.Add(skill.ChaReq + " " + OldWorld.GetString(EOResourceID.SKILLMASTER_WORD_CHARISMA));

        //    drawStrings.Add(" ");
        //    drawStrings.Add(skill.LevelReq + " " + OldWorld.GetString(EOResourceID.SKILLMASTER_WORD_LEVEL));
        //    drawStrings.Add(skill.GoldReq + " " + OldWorld.Instance.EIF[1].Name);

        //    foreach (string s in drawStrings)
        //    {
        //        OldListDialogItem nextLine = new OldListDialogItem(this, OldListDialogItem.ListItemStyle.Small) { Text = s };
        //        AddItemToList(nextLine, false);
        //    }
        //}

        //private void _showRequirementsLabel(Skill skill)
        //{
        //    string full = $"{OldWorld.Instance.ESF[skill.ID].Name} {skill.LevelReq} LVL, ";
        //    if (skill.StrReq > 0)
        //        full += $"{skill.StrReq} STR, ";
        //    if (skill.IntReq > 0)
        //        full += $"{skill.IntReq} INT, ";
        //    if (skill.WisReq > 0)
        //        full += $"{skill.WisReq} WIS, ";
        //    if (skill.AgiReq > 0)
        //        full += $"{skill.AgiReq} AGI, ";
        //    if (skill.ConReq > 0)
        //        full += $"{skill.ConReq} CON, ";
        //    if (skill.ChaReq > 0)
        //        full += $"{skill.ChaReq} CHA, ";
        //    if (skill.GoldReq > 0)
        //        full += $"{skill.GoldReq} Gold";
        //    if (skill.ClassReq > 0)
        //        full += $", {OldWorld.Instance.ECF[skill.ClassReq].Name}";

        //    ((EOGame)Game).Hud.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_INFORMATION, full);
        //}

        //private void _showForgetAllMessage(Action forgetAllAction)
        //{
        //    List<string> drawStrings = new List<string>();

        //    string[] messages =
        //    {
        //        OldWorld.GetString(EOResourceID.SKILLMASTER_FORGET_ALL),
        //        OldWorld.GetString(EOResourceID.SKILLMASTER_FORGET_ALL_MSG_1),
        //        OldWorld.GetString(EOResourceID.SKILLMASTER_FORGET_ALL_MSG_2),
        //        OldWorld.GetString(EOResourceID.SKILLMASTER_FORGET_ALL_MSG_3),
        //        OldWorld.GetString(EOResourceID.SKILLMASTER_CLICK_HERE_TO_FORGET_ALL)
        //    };

        //    TextSplitter ts = new TextSplitter("", Game.Content.Load<SpriteFont>(Constants.FontSize08pt5)) { LineLength = 200 };
        //    foreach (string s in messages)
        //    {
        //        ts.Text = s;
        //        if (!ts.NeedsProcessing)
        //        {
        //            //no text clipping needed
        //            drawStrings.Add(s);
        //            drawStrings.Add(" ");
        //            continue;
        //        }

        //        drawStrings.AddRange(ts.SplitIntoLines());
        //        drawStrings.Add(" ");
        //    }

        //    //now need to take the processed draw strings and make an OldListDialogItem for each one
        //    foreach (string s in drawStrings)
        //    {
        //        string next = s;
        //        bool link = false;
        //        if (next.Length > 0 && next[0] == '*')
        //        {
        //            next = next.Remove(0, 1);
        //            link = true;
        //        }
        //        OldListDialogItem nextItem = new OldListDialogItem(this, OldListDialogItem.ListItemStyle.Small) { Text = next };
        //        if (link) nextItem.SetPrimaryTextLink(forgetAllAction);
        //        AddItemToList(nextItem, false);
        //    }
        //}
    }
}
