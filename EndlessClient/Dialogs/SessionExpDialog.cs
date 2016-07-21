// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EOLib;
using EOLib.Graphics;
using EOLib.Localization;
using EOLib.Net.API;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient.Dialogs
{
    public class SessionExpDialog : EODialogBase
    {
        private static SessionExpDialog inst;
        public new static void Show()
        {
            if (inst != null) return;

            inst = new SessionExpDialog();
            inst.DialogClosing += (o, e) => inst = null;
        }

        private readonly Texture2D m_icons;
        private readonly Rectangle m_signal;
        private readonly Rectangle m_icon;

        private SessionExpDialog()
            : base((PacketAPI)null)
        {
            bgTexture = ((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.PostLoginUI, 61);
            _setSize(bgTexture.Width, bgTexture.Height);

            m_icons = ((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.PostLoginUI, 68, true);
            m_signal = new Rectangle(0, 15, 15, 15);
            m_icon = new Rectangle(0, 0, 15, 15);

            XNAButton okButton = new XNAButton(smallButtonSheet, new Vector2(98, 214), _getSmallButtonOut(SmallButton.Ok), _getSmallButtonOver(SmallButton.Ok));
            okButton.OnClick += (sender, args) => Close(okButton, XNADialogResult.OK);
            okButton.SetParent(this);

            XNALabel title = new XNALabel(new Rectangle(20, 17, 1, 1), Constants.FontSize08pt5)
            {
                AutoSize = false,
                Text = OldWorld.GetString(EOResourceID.DIALOG_TITLE_PERFORMANCE),
                ForeColor = ColorConstants.LightGrayText
            };
            title.SetParent(this);

            XNALabel[] leftSide = new XNALabel[8], rightSide = new XNALabel[8];
            for (int i = 48; i <= 160; i += 16)
            {
                leftSide[(i - 48) / 16] = new XNALabel(new Rectangle(38, i, 1, 1), Constants.FontSize08pt5)
                {
                    AutoSize = false,
                    ForeColor = ColorConstants.LightGrayText
                };
                leftSide[(i - 48) / 16].SetParent(this);
                rightSide[(i - 48) / 16] = new XNALabel(new Rectangle(158, i, 1, 1), Constants.FontSize08pt5)
                {
                    AutoSize = false,
                    ForeColor = ColorConstants.LightGrayText
                };
                rightSide[(i - 48) / 16].SetParent(this);
            }

            leftSide[0].Text = OldWorld.GetString(EOResourceID.DIALOG_PERFORMANCE_TOTALEXP);
            leftSide[1].Text = OldWorld.GetString(EOResourceID.DIALOG_PERFORMANCE_NEXT_LEVEL);
            leftSide[2].Text = OldWorld.GetString(EOResourceID.DIALOG_PERFORMANCE_EXP_NEEDED);
            leftSide[3].Text = OldWorld.GetString(EOResourceID.DIALOG_PERFORMANCE_TODAY_EXP);
            leftSide[4].Text = OldWorld.GetString(EOResourceID.DIALOG_PERFORMANCE_TOTAL_AVG);
            leftSide[5].Text = OldWorld.GetString(EOResourceID.DIALOG_PERFORMANCE_TODAY_AVG);
            leftSide[6].Text = OldWorld.GetString(EOResourceID.DIALOG_PERFORMANCE_BEST_KILL);
            leftSide[7].Text = OldWorld.GetString(EOResourceID.DIALOG_PERFORMANCE_LAST_KILL);
            Character c = OldWorld.Instance.MainPlayer.ActiveCharacter;
            rightSide[0].Text = string.Format("{0}", c.Stats.Experience);
            rightSide[1].Text = string.Format("{0}", OldWorld.Instance.exp_table[c.Stats.Level + 1]);
            rightSide[2].Text = string.Format("{0}", OldWorld.Instance.exp_table[c.Stats.Level + 1] - c.Stats.Experience);
            rightSide[3].Text = string.Format("{0}", c.TodayExp);
            rightSide[4].Text = string.Format("{0}", (int)(c.Stats.Experience / (c.Stats.Usage / 60.0)));
            int sessionTime = (int)(DateTime.Now - EOGame.Instance.Hud.SessionStartTime).TotalMinutes;
            rightSide[5].Text = string.Format("{0}", sessionTime > 0 ? (c.TodayExp / sessionTime) : 0);
            rightSide[6].Text = string.Format("{0}", c.TodayBestKill);
            rightSide[7].Text = string.Format("{0}", c.TodayLastKill);

            Array.ForEach(leftSide, lbl => lbl.ResizeBasedOnText());
            Array.ForEach(rightSide, lbl => lbl.ResizeBasedOnText());

            Center(Game.GraphicsDevice);
            DrawLocation = new Vector2(DrawLocation.X, 15);
            endConstructor(false);
        }

        public override void Draw(GameTime gt)
        {
            //base draw logic handles drawing the background + child controls
            base.Draw(gt);

            SpriteBatch.Begin();
            //icons next to labels
            SpriteBatch.Draw(m_icons, new Vector2(DrawAreaWithOffset.X + 22, DrawAreaWithOffset.Y + 48), m_icon, Color.White);
            SpriteBatch.Draw(m_icons, new Vector2(DrawAreaWithOffset.X + 22, DrawAreaWithOffset.Y + 64), m_icon, Color.White);
            SpriteBatch.Draw(m_icons, new Vector2(DrawAreaWithOffset.X + 22, DrawAreaWithOffset.Y + 80), m_icon, Color.White);
            SpriteBatch.Draw(m_icons, new Vector2(DrawAreaWithOffset.X + 22, DrawAreaWithOffset.Y + 96), m_icon, Color.White);
            SpriteBatch.Draw(m_icons, new Vector2(DrawAreaWithOffset.X + 22, DrawAreaWithOffset.Y + 112), m_icon, Color.White);
            SpriteBatch.Draw(m_icons, new Vector2(DrawAreaWithOffset.X + 22, DrawAreaWithOffset.Y + 128), m_icon, Color.White);
            SpriteBatch.Draw(m_icons, new Vector2(DrawAreaWithOffset.X + 22, DrawAreaWithOffset.Y + 144), m_icon, Color.White);
            SpriteBatch.Draw(m_icons, new Vector2(DrawAreaWithOffset.X + 22, DrawAreaWithOffset.Y + 160), m_icon, Color.White);

            //signal next to exp labels
            SpriteBatch.Draw(m_icons, new Vector2(DrawAreaWithOffset.X + 142, DrawAreaWithOffset.Y + 48), m_signal, Color.White);
            SpriteBatch.Draw(m_icons, new Vector2(DrawAreaWithOffset.X + 142, DrawAreaWithOffset.Y + 64), m_signal, Color.White);
            SpriteBatch.Draw(m_icons, new Vector2(DrawAreaWithOffset.X + 142, DrawAreaWithOffset.Y + 80), m_signal, Color.White);
            SpriteBatch.End();
        }
    }
}
