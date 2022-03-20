using EndlessClient.Old;
using EOLib;
using EOLib.Graphics;
using EOLib.Localization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls.Old;

namespace EndlessClient.Dialogs.Old
{
    public class QuestHistoryDialogListItem : OldListDialogItem
    {
        public string QuestName
        {
            get { return Text; }
            set { Text = value; }
        }

        private readonly Texture2D m_iconTexture;

        private static readonly Vector2 m_firstIconLocation = new Vector2(6, 0);
        private static readonly Vector2 m_signalLocation = new Vector2(270, 0);

        private readonly bool _constructorFinished;

        public QuestHistoryDialogListItem(OldScrollingListDialog parent, int index = -1)
            : base(parent, ListItemStyle.Small, index)
        {
            m_iconTexture = ((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.PostLoginUI, 68, true);

            _setSize(427, 16);

            m_primaryText.DrawLocation = new Vector2(m_primaryText.DrawLocation.X + 25, m_primaryText.DrawLocation.Y);
            m_secondaryText = new XNALabel(new Rectangle(290, (int)m_primaryText.DrawLocation.Y, 1, 1), Constants.FontSize08pt5)
            {
                AutoSize = true,
                BackColor = m_primaryText.BackColor,
                ForeColor = m_primaryText.ForeColor,
                Text = OldWorld.GetString(EOResourceID.QUEST_COMPLETED)
            };
            m_secondaryText.SetParent(this);

            _constructorFinished = true;
        }

        public override void Draw(GameTime gameTime)
        {
            if (!Visible || !_constructorFinished) return;

            SpriteBatch.Begin();

            SpriteBatch.Draw(m_iconTexture,
                m_firstIconLocation + new Vector2(17 + OffsetX + xOff, OffsetY + yOff + (DrawArea.Height * Index)),
                GetIconSourceRectangle(), Color.White);
            SpriteBatch.Draw(m_iconTexture,
                m_signalLocation + new Vector2(17 + OffsetX + xOff, OffsetY + yOff + (DrawArea.Height * Index)),
                GetSignalSourceRectangle(), Color.White);

            SpriteBatch.End();

            base.Draw(gameTime);
        }

        private static Rectangle GetIconSourceRectangle()
        {
            //always show 'completed' icon
            return new Rectangle(75, 0, 15, 15);
        }

        private static Rectangle GetSignalSourceRectangle()
        {
            return new Rectangle(0, 15, 15, 15);
        }
    }
}
