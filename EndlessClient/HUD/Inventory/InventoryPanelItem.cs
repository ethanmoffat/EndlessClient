using EndlessClient.Audio;
using EndlessClient.Dialogs;
using EndlessClient.HUD.Controls;
using EndlessClient.HUD.Panels;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Graphics;
using EOLib.IO.Extensions;
using EOLib.IO.Pub;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Input;
using MonoGame.Extended.Input.InputListeners;
using Optional;
using System;
using XNAControls;

namespace EndlessClient.HUD.Inventory;

public class InventoryPanelItem : DraggablePanelItem<EIFRecord>
{
    private readonly IActiveDialogProvider _activeDialogProvider;
    private readonly ISfxPlayer _sfxPlayer;
    private readonly Texture2D _itemGraphic;
    private readonly Texture2D _highlightBackground;
    private readonly XNALabel _nameLabel;

    private int _slot;

    private Option<Vector2> _highlightDrawPosition;

    public int Slot
    {
        get => _slot;
        set
        {
            _slot = value;
            DrawPosition = GetPosition(_slot);
            UpdateNameLabelPosition();
        }
    }

    public InventoryItem InventoryItem { get; set; }

    public string Text
    {
        get => _nameLabel.Text;
        set
        {
            _nameLabel.Text = value;
            _nameLabel.ResizeBasedOnText(16, 9);
            UpdateNameLabelPosition();
        }
    }

    public event EventHandler<EIFRecord> DoubleClick;

    public override Rectangle EventArea => IsDragging ? DrawArea : DrawAreaWithParentOffset;

    // uses absolute coordinates
    protected override Rectangle GridArea => new Rectangle(
        _parentContainer.DrawPositionWithParentOffset.ToPoint() + new Point(12, 8),
        new Point(363, 102));

    public InventoryPanelItem(IItemNameColorService itemNameColorService,
                              InventoryPanel inventoryPanel,
                              IActiveDialogProvider activeDialogProvider,
                              ISfxPlayer sfxPlayer,
                              int slot,
                              InventoryItem inventoryItem,
                              EIFRecord data)
        : base(inventoryPanel)
    {
        _activeDialogProvider = activeDialogProvider;
        _sfxPlayer = sfxPlayer;

        Slot = slot;
        InventoryItem = inventoryItem;
        Data = data;

        _itemGraphic = inventoryPanel.NativeGraphicsManager.TextureFromResource(GFXTypes.Items, 2 * data.Graphic, transparent: true);
        _highlightBackground = new Texture2D(Game.GraphicsDevice, 1, 1);
        _highlightBackground.SetData(new[] { Color.FromNonPremultiplied(200, 200, 200, 60) });

        _nameLabel = new XNALabel(Constants.FontSize08)
        {
            Visible = false,
            AutoSize = false,
            TextAlign = LabelAlignment.MiddleCenter,
            ForeColor = itemNameColorService.GetColorForInventoryDisplay(Data),
            BackColor = Color.FromNonPremultiplied(30, 30, 30, 160),
            Text = string.Empty
        };

        OnMouseEnter += (_, _) => _nameLabel.Visible = _parentContainer.NoItemsDragging() && _activeDialogProvider.PaperdollDialog.Match(d => d.NoItemsDragging(), () => true);
        OnMouseOver += InventoryPanelItem_OnMouseOver;
        OnMouseLeave += (_, _) =>
        {
            _nameLabel.Visible = false;
            _highlightDrawPosition = Option.None<Vector2>();
        };

        DraggingStarted += (_, _) =>
        {
            _nameLabel.Visible = false;
            _sfxPlayer.PlaySfx(SoundEffectID.InventoryPickup);
        };

        var (slotWidth, slotHeight) = Data.Size.GetDimensions();
        SetSize(slotWidth * 26 - 3, slotHeight * 26 - 3);
    }

    public int GetCurrentSlotBasedOnPosition()
    {
        if (!IsDragging)
            return Slot;

        return (int)((DrawPosition.X - OldOffset.X) / 26) + InventoryPanel.InventoryRowSlots * (int)((DrawPosition.Y - OldOffset.Y) / 26);
    }

    public override void Initialize()
    {
        _nameLabel.Initialize();
        _nameLabel.SetParentControl(_parentContainer);
        _nameLabel.ResizeBasedOnText(16, 9);
        //_nameLabel.DrawOrderChanged += (_, e) =>
        //{
        //    _nameLabel.DrawOrder = _parentContainer.DrawOrder + 200
        //};

        base.Initialize();
    }

    protected override void OnDrawControl(GameTime gameTime)
    {
        _spriteBatch.Begin();

        _highlightDrawPosition.MatchSome(drawPosition =>
        {
            if (GridArea.Contains(DrawArea.WithPosition(drawPosition)))
                _spriteBatch.Draw(_highlightBackground, DrawArea.WithPosition(drawPosition), Color.White);
        });

        if (IsDragging)
        {
            _spriteBatch.Draw(_itemGraphic, DrawPosition, Color.FromNonPremultiplied(255, 255, 255, 128));
        }
        else
        {
            _spriteBatch.Draw(_itemGraphic, DrawPositionWithParentOffset, Color.FromNonPremultiplied(255, 255, 255, 255));
        }

        _spriteBatch.End();
        base.OnDrawControl(gameTime);
    }

    private void InventoryPanelItem_OnMouseOver(object sender, MouseStateExtended e)
    {
        if (!GridArea.Contains(e.Position))
            return;

        var currentSlot = GetCurrentSlotBasedOnPosition();
        _highlightDrawPosition = Option.Some(GetPosition(currentSlot) + (IsDragging ? OldOffset : ImmediateParent.DrawPositionWithParentOffset));
    }

    protected override bool HandleDoubleClick(IXNAControl control, MouseEventArgs eventArgs)
    {
        if (IsDragging)
        {
            // roll back the first click in the double click
            base.HandleClick(control, eventArgs);
        }

        DoubleClick?.Invoke(control, Data);

        return true;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _nameLabel.Dispose();
            _highlightBackground.Dispose();
        }

        base.Dispose(disposing);
    }

    private void UpdateNameLabelPosition()
    {
        if (_nameLabel == null)
            return;

        // the name label is parented to the inventory panel so that all name labels draw over all items (see draw orders below)
        // the actual position of the name label needs to be set to this control's draw position
        var actualPosition = DrawPosition;

        if (actualPosition.X + _nameLabel.DrawAreaWithParentOffset.Width + DrawArea.Width > GridArea.Width)
        {
            _nameLabel.DrawPosition = new Vector2(actualPosition.X - _nameLabel.DrawArea.Width, actualPosition.Y);
        }
        else
        {
            _nameLabel.DrawPosition = new Vector2(actualPosition.X + DrawArea.Width, actualPosition.Y);
        }

        DrawOrder = _parentContainer.DrawOrder + 2;
        _nameLabel.DrawOrder = _parentContainer.DrawOrder + 200;
    }

    protected override void OnDraggingFinished(DragCompletedEventArgs<EIFRecord> args)
    {
        base.OnDraggingFinished(args);

        if (args.ContinueDrag)
            return;

        _sfxPlayer.PlaySfx(SoundEffectID.InventoryPlace);

        if (!args.RestoreOriginalSlot)
            Slot = GetCurrentSlotBasedOnPosition();
        else
            DrawPosition = GetPosition(Slot);

        _nameLabel.Visible = false;
        UpdateNameLabelPosition();
    }

    private static Vector2 GetPosition(int slot)
    {
        return new Vector2(13 + 26 * (slot % InventoryPanel.InventoryRowSlots), 9 + 26 * (slot / InventoryPanel.InventoryRowSlots));
    }
}