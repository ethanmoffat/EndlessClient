// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using EOLib;
using EOLib.Localization;
using EOLib.Net.API;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient.Dialogs
{
    public class SkillmasterDialog : ScrollingListDialog
    {
        public static SkillmasterDialog Instance { get; private set; }

        public static void Show(PacketAPI api, short npcIndex)
        {
            if (Instance != null)
                return;

            Instance = new SkillmasterDialog(api);

            if (!api.RequestSkillmaster(npcIndex))
            {
                Instance.Close();
                Instance = null;
                EOGame.Instance.DoShowLostConnectionDialogAndReturnToMainMenu();
            }
        }

        private enum SkillState
        {
            None,
            Initial,
            Learn,
            Forget,
            ForgetAll
        }

        private SkillState m_state;
        private List<Skill> m_skills;
        private bool m_showingRequirements;

        private static Texture2D LearnIcon, ForgetIcon;

        private SkillmasterDialog(PacketAPI api)
            : base(api)
        {
            Buttons = ScrollingListDialogButtons.Cancel;
            ListItemType = ListDialogItem.ListItemStyle.Large;
            DialogClosing += (o, e) =>
            {
                if (e.Result == XNADialogResult.Cancel)
                {
                    Instance = null;
                }
                else if (e.Result == XNADialogResult.Back)
                {
                    e.CancelClose = true;
                    if (m_state == SkillState.Learn && m_showingRequirements)
                    {
                        m_state = SkillState.Initial; //force it to re-generate the list items
                        _setState(SkillState.Learn);
                        m_showingRequirements = false;
                    }
                    else
                        _setState(SkillState.Initial);
                }
            };
            m_state = SkillState.None;

            if (LearnIcon == null || ForgetIcon == null)
            {
                //getDlgIcon
                LearnIcon = _getDlgIcon(ListIcon.Learn);
                ForgetIcon = _getDlgIcon(ListIcon.Forget);
            }
        }

        public void SetSkillmasterData(SkillmasterData data)
        {
            if (Instance == null || this != Instance) return;

            Title = data.Title;
            m_skills = new List<Skill>(data.Skills);

            _setState(SkillState.Initial);
        }

        public void RemoveSkillByIDFromLearnList(short id)
        {
            if (Instance == null || this != Instance) return;

            ListDialogItem itemToRemove = children.OfType<ListDialogItem>().FirstOrDefault(_x => _x.ID == id);
            if(itemToRemove != null)
                RemoveFromList(itemToRemove);
        }

        private void _setState(SkillState newState)
        {
            SkillState old = m_state;

            if (old == newState) return;

            int numToLearn = m_skills.Count(_skill => !OldWorld.Instance.MainPlayer.ActiveCharacter.Spells.Exists(_spell => _spell.ID == _skill.ID));
            int numToForget = OldWorld.Instance.MainPlayer.ActiveCharacter.Spells.Count;

            if (newState == SkillState.Learn && numToLearn == 0)
            {
                EOMessageBox.Show(DialogResourceID.SKILL_NOTHING_MORE_TO_LEARN, XNADialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
                return;
            }

            ClearItemList();
            switch (newState)
            {
                case SkillState.Initial:
                {
                    string learnNum = string.Format("{0}{1}", numToLearn, OldWorld.GetString(EOResourceID.SKILLMASTER_ITEMS_TO_LEARN));
                    string forgetNum = string.Format("{0}{1}", numToForget, OldWorld.GetString(EOResourceID.SKILLMASTER_ITEMS_LEARNED));

                    ListDialogItem learn = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 0)
                    {
                        Text = OldWorld.GetString(EOResourceID.SKILLMASTER_WORD_LEARN),
                        SubText = learnNum,
                        IconGraphic = LearnIcon,
                        ShowItemBackGround = false,
                        OffsetY = 45
                    };
                    learn.OnLeftClick += (o, e) => _setState(SkillState.Learn);
                    learn.OnRightClick += (o, e) => _setState(SkillState.Learn);
                    AddItemToList(learn, false);

                    ListDialogItem forget = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 1)
                    {
                        Text = OldWorld.GetString(EOResourceID.SKILLMASTER_WORD_FORGET),
                        SubText = forgetNum,
                        IconGraphic = ForgetIcon,
                        ShowItemBackGround = false,
                        OffsetY = 45
                    };
                    forget.OnLeftClick += (o, e) => _setState(SkillState.Forget);
                    forget.OnRightClick += (o, e) => _setState(SkillState.Forget);
                    AddItemToList(forget, false);

                    ListDialogItem forgetAll = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 2)
                    {
                        Text = OldWorld.GetString(EOResourceID.SKILLMASTER_FORGET_ALL),
                        SubText = OldWorld.GetString(EOResourceID.SKILLMASTER_RESET_YOUR_CHARACTER),
                        IconGraphic = ForgetIcon,
                        ShowItemBackGround = false,
                        OffsetY = 45
                    };
                    forgetAll.OnLeftClick += (o, e) => _setState(SkillState.ForgetAll);
                    forgetAll.OnRightClick += (o, e) => _setState(SkillState.ForgetAll);
                    AddItemToList(forgetAll, false);

                    _setButtons(ScrollingListDialogButtons.Cancel);
                }
                    break;
                case SkillState.Learn:
                {
                    int index = 0;
                    for (int i = 0; i < m_skills.Count; ++i)
                    {
                        if (OldWorld.Instance.MainPlayer.ActiveCharacter.Spells.FindIndex(_sp => m_skills[i].ID == _sp.ID) >= 0)
                            continue;
                        int localI = i;

                        var spellData = OldWorld.Instance.ESF.Data[m_skills[localI].ID];

                        ListDialogItem nextListItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, index++)
                        {
                            Visible = false,
                            Text = spellData.Name,
                            SubText = OldWorld.GetString(EOResourceID.SKILLMASTER_WORD_REQUIREMENTS),
                            IconGraphic = OldWorld.GetSpellIcon(spellData.Icon, false),
                            ShowItemBackGround = false,
                            OffsetY = 45,
                            ID = m_skills[localI].ID
                        };
                        nextListItem.OnLeftClick += (o, e) => _learn(m_skills[localI]);
                        nextListItem.OnRightClick += (o, e) => _learn(m_skills[localI]);
                        nextListItem.OnMouseEnter += (o, e) => _showRequirementsLabel(m_skills[localI]);
                        nextListItem.SetSubtextLink(() => _showRequirements(m_skills[localI]));
                        AddItemToList(nextListItem, false);
                    }

                    _setButtons(ScrollingListDialogButtons.BackCancel);
                }
                    break;
                case SkillState.Forget:
                {
                    TextInputDialog input = new TextInputDialog(OldWorld.GetString(DialogResourceID.SKILL_PROMPT_TO_FORGET, false), 32);
                    input.SetAsKeyboardSubscriber();
                    input.DialogClosing += (sender, args) =>
                    {
                        if (args.Result == XNADialogResult.Cancel) return;
                        bool found =
                            OldWorld.Instance.MainPlayer.ActiveCharacter.Spells.Any(
                                _spell => OldWorld.Instance.ESF.Data[_spell.ID].Name.ToLower() == input.ResponseText.ToLower());

                        if (!found)
                        {
                            args.CancelClose = true;
                            EOMessageBox.Show(DialogResourceID.SKILL_FORGET_ERROR_NOT_LEARNED, XNADialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
                            input.SetAsKeyboardSubscriber();
                        }

                        if (!m_api.ForgetSpell(
                                OldWorld.Instance.MainPlayer.ActiveCharacter.Spells.Find(
                                    _spell => OldWorld.Instance.ESF.Data[_spell.ID].Name.ToLower() == input.ResponseText.ToLower()).ID))
                        {
                            Close();
                            ((EOGame)Game).DoShowLostConnectionDialogAndReturnToMainMenu();
                        }
                    };

                    //should show initial info in the actual dialog since this uses a pop-up input box
                    //    to select a skill to remove
                    newState = SkillState.Initial;
                    goto case SkillState.Initial;
                }
                case SkillState.ForgetAll:
                {
                    _showForgetAllMessage(_forgetAllAction);
                    _setButtons(ScrollingListDialogButtons.BackCancel);
                }
                    break;
            }

            m_state = newState;
        }

        private void _learn(Skill skill)
        {
            OldCharacter c = OldWorld.Instance.MainPlayer.ActiveCharacter;

            bool skillReqsMet = true;
            foreach(short x in skill.SkillReq)
                if (x != 0 && c.Spells.FindIndex(_sp => _sp.ID == x) < 0)
                    skillReqsMet = false;

            //check the requirements
            if (c.Stats.Str < skill.StrReq || c.Stats.Int < skill.IntReq || c.Stats.Wis < skill.WisReq ||
                c.Stats.Agi < skill.AgiReq || c.Stats.Con < skill.ConReq || c.Stats.Cha < skill.ChaReq ||
                c.Stats.Level < skill.LevelReq || c.Inventory.Find(_ii => _ii.ItemID == 1).Amount < skill.GoldReq || !skillReqsMet)
            {
                EOMessageBox.Show(DialogResourceID.SKILL_LEARN_REQS_NOT_MET, XNADialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
                return;
            }

            if (skill.ClassReq > 0 && c.Class != skill.ClassReq)
            {
                EOMessageBox.Show(DialogResourceID.SKILL_LEARN_WRONG_CLASS, " " + OldWorld.Instance.ECF.Data[skill.ClassReq].Name + "!", XNADialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
                return;
            }

            EOMessageBox.Show(DialogResourceID.SKILL_LEARN_CONFIRMATION, " " + OldWorld.Instance.ESF.Data[skill.ID].Name + "?", XNADialogButtons.OkCancel, EOMessageBoxStyle.SmallDialogSmallHeader,
                (o, e) =>
                {
                    if (e.Result != XNADialogResult.OK)
                        return;

                    if (!m_api.LearnSpell(skill.ID))
                    {
                        Close();
                        ((EOGame)Game).DoShowLostConnectionDialogAndReturnToMainMenu();
                    }
                });
        }

        private void _forgetAllAction()
        {
            EOMessageBox.Show(DialogResourceID.SKILL_RESET_CHARACTER_CONFIRMATION, XNADialogButtons.OkCancel, EOMessageBoxStyle.SmallDialogSmallHeader,
                (sender, args) =>
                {
                    if (args.Result == XNADialogResult.Cancel) return;

                    if (!m_api.ResetCharacterStatSkill())
                    {
                        Close();
                        ((EOGame) Game).DoShowLostConnectionDialogAndReturnToMainMenu();
                    }
                });
        }

        private void _showRequirements(Skill skill)
        {
            m_showingRequirements = true;
            ClearItemList();

            List<string> drawStrings = new List<string>(15)
            {
                OldWorld.Instance.ESF.Data[skill.ID].Name + (skill.ClassReq > 0 ? " [" + OldWorld.Instance.ECF.Data[skill.ClassReq].Name + "]" : ""),
                " "
            };
            if (skill.SkillReq.Any(x => x != 0))
            {
                drawStrings.AddRange(from req in skill.SkillReq where req != 0 select OldWorld.GetString(EOResourceID.SKILLMASTER_WORD_SKILL) + ": " +  OldWorld.Instance.ESF.Data[req].Name);
                drawStrings.Add(" ");
            }

            if(skill.StrReq > 0)
                drawStrings.Add(skill.StrReq + " " + OldWorld.GetString(EOResourceID.SKILLMASTER_WORD_STRENGTH));
            if (skill.IntReq > 0)
                drawStrings.Add(skill.IntReq + " " + OldWorld.GetString(EOResourceID.SKILLMASTER_WORD_INTELLIGENCE));
            if (skill.WisReq > 0)
                drawStrings.Add(skill.WisReq + " " + OldWorld.GetString(EOResourceID.SKILLMASTER_WORD_WISDOM));
            if (skill.AgiReq > 0)
                drawStrings.Add(skill.AgiReq + " " + OldWorld.GetString(EOResourceID.SKILLMASTER_WORD_AGILITY));
            if (skill.ConReq > 0)
                drawStrings.Add(skill.ConReq + " " + OldWorld.GetString(EOResourceID.SKILLMASTER_WORD_CONSTITUTION));
            if (skill.ChaReq > 0)
                drawStrings.Add(skill.ChaReq + " " + OldWorld.GetString(EOResourceID.SKILLMASTER_WORD_CHARISMA));

            drawStrings.Add(" ");
            drawStrings.Add(skill.LevelReq + " " + OldWorld.GetString(EOResourceID.SKILLMASTER_WORD_LEVEL));
            drawStrings.Add(skill.GoldReq + " " + OldWorld.Instance.EIF[1].Name);

            foreach (string s in drawStrings)
            {
                ListDialogItem nextLine = new ListDialogItem(this, ListDialogItem.ListItemStyle.Small) { Text = s };
                AddItemToList(nextLine, false);
            }
        }

        private void _showRequirementsLabel(Skill skill)
        {
            string full = string.Format("{0} {1} LVL, ", OldWorld.Instance.ESF.Data[skill.ID].Name, skill.LevelReq);
            if (skill.StrReq > 0)
                full += string.Format("{0} STR, ", skill.StrReq);
            if (skill.IntReq > 0)
                full += string.Format("{0} INT, ", skill.IntReq);
            if (skill.WisReq > 0)
                full += string.Format("{0} WIS, ", skill.WisReq);
            if (skill.AgiReq > 0)
                full += string.Format("{0} AGI, ", skill.AgiReq);
            if (skill.ConReq > 0)
                full += string.Format("{0} CON, ", skill.ConReq);
            if (skill.ChaReq > 0)
                full += string.Format("{0} CHA, ", skill.ChaReq);
            if (skill.GoldReq > 0)
                full += string.Format("{0} Gold", skill.GoldReq);
            if (skill.ClassReq > 0)
                full += string.Format(", {0}", OldWorld.Instance.ECF.Data[skill.ClassReq].Name);

            ((EOGame)Game).Hud.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_INFORMATION, full);
        }

        private void _showForgetAllMessage(Action forgetAllAction)
        {
            List<string> drawStrings = new List<string>();

            string[] messages =
            {
                OldWorld.GetString(EOResourceID.SKILLMASTER_FORGET_ALL),
                OldWorld.GetString(EOResourceID.SKILLMASTER_FORGET_ALL_MSG_1),
                OldWorld.GetString(EOResourceID.SKILLMASTER_FORGET_ALL_MSG_2),
                OldWorld.GetString(EOResourceID.SKILLMASTER_FORGET_ALL_MSG_3),
                OldWorld.GetString(EOResourceID.SKILLMASTER_CLICK_HERE_TO_FORGET_ALL)
            };

            TextSplitter ts = new TextSplitter("", Game.Content.Load<SpriteFont>(Constants.FontSize08pt5)) { LineLength = 200 };
            foreach (string s in messages)
            {
                ts.Text = s;
                if (!ts.NeedsProcessing)
                {
                    //no text clipping needed
                    drawStrings.Add(s);
                    drawStrings.Add(" ");
                    continue;
                }

                drawStrings.AddRange(ts.SplitIntoLines());
                drawStrings.Add(" ");
            }

            //now need to take the processed draw strings and make an ListDialogItem for each one
            foreach (string s in drawStrings)
            {
                string next = s;
                bool link = false;
                if (next.Length > 0 && next[0] == '*')
                {
                    next = next.Remove(0, 1);
                    link = true;
                }
                ListDialogItem nextItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Small) { Text = next };
                if (link) nextItem.SetPrimaryTextLink(forgetAllAction);
                AddItemToList(nextItem, false);
            }
        }
    }
}
