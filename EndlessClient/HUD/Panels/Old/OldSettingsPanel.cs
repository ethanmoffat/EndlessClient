// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EndlessClient.Dialogs;
using EOLib;
using EOLib.Config;
using EOLib.Graphics;
using EOLib.Localization;
using EOLib.Net;
using Microsoft.Xna.Framework;
using XNAControls;

namespace EndlessClient.HUD.Panels.Old
{
    public class OldEOSettingsPanel : XNAControl
    {
        private readonly XNALabel[] m_leftSide, m_rightSide;
        private readonly XNAButton[] m_buttons;
        private readonly OldWorld w;

        private bool m_soundChanged, m_musicChanged;

        private enum KeyLayout { English, Dutch, Swedish, Azerty }
        private KeyLayout m_keyboard = KeyLayout.English; //this is not stored or loaded

        //parent x,y - 102,330
        public OldEOSettingsPanel(XNAPanel parent)
            : base(null, null, parent)
        {
            _setSize(parent.BackgroundImage.Width, parent.BackgroundImage.Height);

            w = OldWorld.Instance;
            m_leftSide = new XNALabel[5];
            m_rightSide = new XNALabel[5];

            for (int i = 0; i < m_leftSide.Length; ++i)
            {
                m_leftSide[i] = new XNALabel(new Rectangle(117, 25 + (18*i), 100, 15), Constants.FontSize08pt5)
                {
                    ForeColor = ColorConstants.LightGrayText
                };
                m_leftSide[i].SetParent(this);
                m_rightSide[i] = new XNALabel(new Rectangle(356, 25 + (18*i), 100, 15), Constants.FontSize08pt5)
                {
                    ForeColor = ColorConstants.LightGrayText
                };
                m_rightSide[i].SetParent(this);
            }

            _setTextForLanguage();

            m_buttons = new XNAButton[10];
            for (int i = 0; i < m_buttons.Length; ++i)
            {
                m_buttons[i] = new XNAButton(((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.PostLoginUI, 27, true),
                    new Vector2(i < 5 ? 215 : 454, 25 + (18*(i%5))), 
                    new Rectangle(0, 0, 19, 15), 
                    new Rectangle(19, 0, 19, 15));

                m_buttons[i].SetParent(this);
                m_buttons[i].OnClick += _settingChange;
                m_buttons[i].OnMouseOver += (o, e) => EOGame.Instance.Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_BUTTON, DATCONST2.STATUS_LABEL_SETTINGS_CLICK_TO_CHANGE);
            }
        }

        private void _setTextForLanguage()
        {
            m_leftSide[0].Text = OldWorld.GetString(w.SoundEnabled ? DATCONST2.SETTING_ENABLED : DATCONST2.SETTING_DISABLED);
            m_leftSide[1].Text = OldWorld.GetString(w.MusicEnabled ? DATCONST2.SETTING_ENABLED : DATCONST2.SETTING_DISABLED);
            m_leftSide[2].Text = OldWorld.GetString(DATCONST2.SETTING_KEYBOARD_ENGLISH);
            m_leftSide[3].Text = OldWorld.GetString(DATCONST2.SETTING_LANG_CURRENT);
            m_leftSide[4].Text = OldWorld.GetString(w.HearWhispers ? DATCONST2.SETTING_ENABLED : DATCONST2.SETTING_DISABLED);

            m_rightSide[0].Text = OldWorld.GetString(w.ShowChatBubbles ? DATCONST2.SETTING_ENABLED : DATCONST2.SETTING_DISABLED);
            m_rightSide[1].Text = OldWorld.GetString(w.ShowShadows ? DATCONST2.SETTING_ENABLED : DATCONST2.SETTING_DISABLED);
            if (w.StrictFilterEnabled)
                m_rightSide[2].Text = OldWorld.GetString(DATCONST2.SETTING_EXCLUSIVE);
            else if (w.CurseFilterEnabled)
                m_rightSide[2].Text = OldWorld.GetString(DATCONST2.SETTING_NORMAL);
            else
                m_rightSide[2].Text = OldWorld.GetString(DATCONST2.SETTING_DISABLED);

            m_rightSide[3].Text = OldWorld.GetString(w.LogChatToFile ? DATCONST2.SETTING_ENABLED : DATCONST2.SETTING_DISABLED);
            m_rightSide[4].Text = OldWorld.GetString(w.Interaction ? DATCONST2.SETTING_ENABLED : DATCONST2.SETTING_DISABLED);
        }

        private void _settingChange(object sender, EventArgs e)
        {
            if (sender == m_buttons[0])
            {
                if (!m_soundChanged && !w.SoundEnabled)
                {
                    EOMessageBox.Show(DATCONST1.SETTINGS_SOUND_DISABLED, XNADialogButtons.OkCancel, EOMessageBoxStyle.SmallDialogSmallHeader,
                        (o, args) =>
                        {
                            if (args.Result == XNADialogResult.OK)
                            {
                                m_soundChanged = true;
                                w.SoundEnabled = !w.SoundEnabled;
                                OldWorld.Instance.ActiveMapRenderer.PlayOrStopAmbientNoise();
                                m_leftSide[0].Text = OldWorld.GetString(w.SoundEnabled ? DATCONST2.SETTING_ENABLED : DATCONST2.SETTING_DISABLED);
                            }
                        });
                }
                else
                {
                    if (!m_soundChanged)
                        m_soundChanged = true;

                    w.SoundEnabled = !w.SoundEnabled;
                    OldWorld.Instance.ActiveMapRenderer.PlayOrStopAmbientNoise();
                    m_leftSide[0].Text = OldWorld.GetString(w.SoundEnabled ? DATCONST2.SETTING_ENABLED : DATCONST2.SETTING_DISABLED);
                }
            }
            else if (sender == m_buttons[1])
            {
                if (!m_musicChanged && !w.MusicEnabled)
                {
                    EOMessageBox.Show(DATCONST1.SETTINGS_MUSIC_DISABLED, XNADialogButtons.OkCancel, EOMessageBoxStyle.SmallDialogSmallHeader,
                        (o, args) =>
                        {
                            if (args.Result == XNADialogResult.OK)
                            {
                                m_musicChanged = true;
                                w.MusicEnabled = !w.MusicEnabled;
                                OldWorld.Instance.ActiveMapRenderer.PlayOrStopBackgroundMusic();
                                m_leftSide[1].Text = OldWorld.GetString(w.MusicEnabled ? DATCONST2.SETTING_ENABLED : DATCONST2.SETTING_DISABLED);
                            }
                        });
                }
                else
                {
                    if (!m_musicChanged)
                        m_musicChanged = true;

                    w.MusicEnabled = !w.MusicEnabled;
                    OldWorld.Instance.ActiveMapRenderer.PlayOrStopBackgroundMusic();
                    m_leftSide[1].Text = OldWorld.GetString(w.MusicEnabled ? DATCONST2.SETTING_ENABLED : DATCONST2.SETTING_DISABLED);
                }
            }
            else if (sender == m_buttons[2])
            {
                m_keyboard++;
                if (m_keyboard > KeyLayout.Azerty)
                    m_keyboard = 0;
                m_leftSide[2].Text = OldWorld.GetString(DATCONST2.SETTING_KEYBOARD_ENGLISH + (int)m_keyboard);
            }
            else if (sender == m_buttons[3])
            {
                if(w.Language != EOLanguage.Portuguese)
                    w.Language++;
                else
                    w.Language = 0;
                _setTextForLanguage(); //need to reset all strings when language changes
            }
            else if (sender == m_buttons[4])
            {
                w.HearWhispers = !w.HearWhispers;
                m_leftSide[4].Text = OldWorld.GetString(w.HearWhispers ? DATCONST2.SETTING_ENABLED : DATCONST2.SETTING_DISABLED);
                OldPacket pkt = new OldPacket(PacketFamily.Global, w.HearWhispers ? PacketAction.Remove : PacketAction.Player);
                pkt.AddChar(w.HearWhispers ? (byte) 'n' : (byte) 'y');
                w.Client.SendPacket(pkt);
            }
            else if (sender == m_buttons[5])
            {
                w.ShowChatBubbles = !w.ShowChatBubbles;
                m_rightSide[0].Text = OldWorld.GetString(w.ShowChatBubbles ? DATCONST2.SETTING_ENABLED : DATCONST2.SETTING_DISABLED);
            }
            else if (sender == m_buttons[6])
            {
                w.ShowShadows = !w.ShowShadows;
                m_rightSide[1].Text = OldWorld.GetString(w.ShowShadows ? DATCONST2.SETTING_ENABLED : DATCONST2.SETTING_DISABLED);
            }
            else if (sender == m_buttons[7])
            {
                DATCONST2 str;
                if (w.StrictFilterEnabled)
                {
                    w.StrictFilterEnabled = false;
                    str = DATCONST2.SETTING_DISABLED;
                }
                else if (w.CurseFilterEnabled)
                {
                    w.CurseFilterEnabled = false;
                    w.StrictFilterEnabled = true;
                    str = DATCONST2.SETTING_EXCLUSIVE;
                }
                else
                {
                    w.CurseFilterEnabled = true;
                    str = DATCONST2.SETTING_NORMAL;
                }
                m_rightSide[2].Text = OldWorld.GetString(str);
            }
            else if (sender == m_buttons[8])
            {
                w.LogChatToFile = !w.LogChatToFile;
                m_rightSide[3].Text = OldWorld.GetString(w.LogChatToFile ? DATCONST2.SETTING_ENABLED : DATCONST2.SETTING_DISABLED);
            }
            else if (sender == m_buttons[9])
            {
                w.Interaction = !w.Interaction;
                m_rightSide[4].Text = OldWorld.GetString(w.Interaction ? DATCONST2.SETTING_ENABLED : DATCONST2.SETTING_DISABLED);
            }
        }
    }
}
