using EndlessClient.Old;
using EOLib;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls.Old;

namespace EndlessClient.Dialogs
{
    public class QuestProgressDialogListItem : ListDialogItem
    {
        public int QuestContextIcon { get; set; }

        public string QuestName
        {
            get { return Text; }
            set { Text = value; }
        }

        public string QuestStep
        {
            get { return SubText; }
            set { SubText = value; }
        }

        private readonly XNALabel m_progress;
        public string QuestProgress
        {
            get { return m_progress.Text; }
            set { m_progress.Text = value; }
        }

        public bool ShowIcons { private get; set; }

        private readonly Texture2D m_iconTexture;

        private static readonly Vector2 m_firstIconLocation = new Vector2(6, 0);
        private static readonly Vector2 m_secondIconLocation = new Vector2(151, 0);
        private static readonly Vector2 m_signalLocation = new Vector2(334, 0);

        private readonly bool _constructorFinished;

        public QuestProgressDialogListItem(ScrollingListDialog parent, int index = -1)
            : base(parent, ListItemStyle.Small, index)
        {
            m_iconTexture = ((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.PostLoginUI, 68, true);
            ShowIcons = true;

            _setSize(427, 16);

            m_primaryText.DrawLocation = new Vector2(m_primaryText.DrawLocation.X + 25, m_primaryText.DrawLocation.Y);
            m_secondaryText = new XNALabel(new Rectangle(169, (int)m_primaryText.DrawLocation.Y, 1, 1), Constants.FontSize08pt5)
            {
                AutoSize = true,
                BackColor = m_primaryText.BackColor,
                ForeColor = m_primaryText.ForeColor,
                Text = " "
            };
            m_secondaryText.SetParent(this);
            m_progress = new XNALabel(new Rectangle(353, (int)m_primaryText.DrawLocation.Y, 1, 1), Constants.FontSize08pt5)
            {
                AutoSize = true,
                BackColor = m_primaryText.BackColor,
                ForeColor = m_primaryText.ForeColor,
                Text = " "
            };
            m_progress.SetParent(this);

            _constructorFinished = true;
        }

        public override void Draw(GameTime gameTime)
        {
            if (!Visible || !_constructorFinished) return;


            if (ShowIcons)
            {
                SpriteBatch.Begin();

                SpriteBatch.Draw(m_iconTexture,
                    m_firstIconLocation + new Vector2(17 + OffsetX + xOff, OffsetY + yOff + (DrawArea.Height * Index)),
                    GetIconSourceRectangle(0), Color.White);
                SpriteBatch.Draw(m_iconTexture,
                    m_secondIconLocation + new Vector2(17 + OffsetX + xOff, OffsetY + yOff + (DrawArea.Height * Index)),
                    GetIconSourceRectangle(QuestContextIcon), Color.White);
                SpriteBatch.Draw(m_iconTexture,
                    m_signalLocation + new Vector2(17 + OffsetX + xOff, OffsetY + yOff + (DrawArea.Height * Index)),
                    GetSignalSourceRectangle(), Color.White);

                SpriteBatch.End();
            }

            base.Draw(gameTime);
        }

        private static Rectangle GetIconSourceRectangle(int index)
        {
            return new Rectangle(index * 15, 0, 15, 15);
        }

        private static Rectangle GetSignalSourceRectangle()
        {
            return new Rectangle(0, 15, 15, 15);
        }
    }
}
