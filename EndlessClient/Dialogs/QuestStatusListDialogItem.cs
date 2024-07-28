using EOLib;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using XNAControls;

namespace EndlessClient.Dialogs;

public class QuestStatusListDialogItem : ListDialogItem
{
    public enum QuestStatusIcon
    {
        None = 0,
        Talk = 1,
        Item = 2,
        Kill = 3,
        Step = 4,
        Complete = 5,
        None2 = 6, // ?
        None3 = 7, // ?
    };

    private static readonly Vector2 _firstIconPosition = new Vector2(6, 0);
    private static readonly Vector2 _secondIconPosition = new Vector2(151, 0);
    private readonly Vector2 _signalPosition;

    private readonly IXNALabel _progress;
    private readonly Texture2D _iconTexture;
    private readonly QuestPage _page;

    public QuestStatusIcon Icon { get; set; }

    public string QuestName
    {
        get => PrimaryText;
        set => PrimaryText = value;
    }

    public string QuestStep
    {
        get => SubText;
        set => SubText = value;
    }

    public string QuestProgress
    {
        get => _progress.Text;
        set => _progress.Text = value;
    }

    public bool ShowIcons { get; set; }

    public QuestStatusListDialogItem(QuestStatusDialog parent, QuestPage page)
        : base(parent, ListItemStyle.Small)
    {
        _iconTexture = parent.GraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 68, true);
        ShowIcons = true;

        SetSize(427, 16);

        _primaryText.DrawPosition += new Vector2(25, 2);
        _subText.DrawPosition = new Vector2(169, _primaryText.DrawPosition.Y);
        ((DrawableGameComponent)_subText).Visible = true;

        PrimaryText = " ";
        SubText = " ";

        _progress = new XNALabel(Constants.FontSize08pt5)
        {
            DrawPosition = new Vector2(page == QuestPage.Progress ? 353 : 289, _primaryText.DrawPosition.Y),
            AutoSize = true,
            BackColor = _primaryText.BackColor,
            ForeColor = _primaryText.ForeColor,
            Text = " ",
            Visible = true,
        };

        _signalPosition = page == QuestPage.Progress
            ? new Vector2(334, 2)
            : new Vector2(270, 2);

        Icon = page == QuestPage.Progress
            ? QuestStatusIcon.None
            : QuestStatusIcon.Complete;
        _page = page;
    }

    public override void Initialize()
    {
        _progress.Initialize();
        _progress.SetParentControl(this);

        base.Initialize();
    }

    protected override void OnDrawControl(GameTime gameTime)
    {
        base.OnDrawControl(gameTime);

        if (ShowIcons)
        {
            _spriteBatch.Begin();

            _spriteBatch.Draw(_iconTexture,
                DrawPositionWithParentOffset + _firstIconPosition,
                GetIconSourceRectangle(_page == QuestPage.Progress ? QuestStatusIcon.None : QuestStatusIcon.Complete), Color.White);

            if (_page == QuestPage.Progress)
            {
                _spriteBatch.Draw(_iconTexture,
                    DrawPositionWithParentOffset + _secondIconPosition,
                    GetIconSourceRectangle(Icon), Color.White);
            }

            _spriteBatch.Draw(_iconTexture,
                DrawPositionWithParentOffset + _signalPosition,
                GetSignalSourceRectangle(), Color.White);

            _spriteBatch.End();
        }
    }

    private static Rectangle GetIconSourceRectangle(QuestStatusIcon index)
    {
        return new Rectangle((int)index * 15, 0, 15, 15);
    }

    private static Rectangle GetSignalSourceRectangle()
    {
        return new Rectangle(0, 15, 15, 15);
    }
}