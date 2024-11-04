using System;
using EndlessClient.Audio;
using EndlessClient.HUD.Controls;
using EndlessClient.HUD.Panels;
using EOLib.Domain.Character;
using EOLib.Graphics;
using EOLib.IO.Pub;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input.InputListeners;
using XNAControls;

namespace EndlessClient.HUD.Spells
{
    public class SpellPanelItem : DraggablePanelItem<ESFRecord>
    {
        private const int ICON_AREA_WIDTH = 42, ICON_AREA_HEIGHT = 36;

        private readonly ISfxPlayer _sfxPlayer;

        private readonly Texture2D _spellGraphic;
        private Rectangle _spellGraphicSourceRect;

        private readonly Texture2D _whitePixel;

        public int Slot { get; set; }

        private int _displaySlot;
        public int DisplaySlot
        {
            get => _displaySlot;
            set
            {
                _displaySlot = value;
                DrawPosition = GetDisplayPosition(_displaySlot);
            }
        }

        public InventorySpell InventorySpell { get; set; }

        public override Rectangle EventArea => IsDragging ? DrawArea : DrawAreaWithParentOffset;

        // uses absolute coordinates
        protected override Rectangle GridArea => new Rectangle(
            _parentContainer.DrawPositionWithParentOffset.ToPoint() + new Point(98, 6),
            new Point(363, 102));

        public event EventHandler<MouseEventArgs> Click;

        public SpellPanelItem(ActiveSpellsPanel spellPanel,
                              ISfxPlayer sfxPlayer,
                              int slot,
                              InventorySpell spell,
                              ESFRecord data)
            : base(spellPanel)
        {
            _sfxPlayer = sfxPlayer;

            Slot = DisplaySlot = slot;
            InventorySpell = spell;
            Data = data;

            _spellGraphic = spellPanel.NativeGraphicsManager.TextureFromResource(GFXTypes.SpellIcons, Data.Icon);
            _spellGraphicSourceRect = new Rectangle(0, 0, _spellGraphic.Width / 2, _spellGraphic.Height);

            _whitePixel = new Texture2D(Game.GraphicsDevice, 1, 1);
            _whitePixel.SetData(new[] { Color.White });

            SetSize(ICON_AREA_WIDTH, ICON_AREA_HEIGHT);
        }

        public int GetCurrentSlotBasedOnPosition(int scrollOffset)
        {
            if (!IsDragging)
                return Slot;

            // old offset X needs to be adjusted since it assumes parent coordinates are the start of the slot grid
            // this works for inventory without adjustment since the grid goes all the way to the parent panel coordinate
            // however, spell panel has 2 slots worth of padding for the selected slot / level up controls
            var adjustedOffsetX = OldOffset.X + (ICON_AREA_WIDTH * 2);

            return scrollOffset * ActiveSpellsPanel.SpellRowLength +
                (int)((DrawPosition.X - adjustedOffsetX) / ICON_AREA_WIDTH) +
                ActiveSpellsPanel.SpellRowLength * (int)((DrawPosition.Y - OldOffset.Y) / ICON_AREA_HEIGHT);
        }

        protected override void OnDrawControl(GameTime gameTime)
        {
            _spriteBatch.Begin();

            DrawLevelAndHighlight();
            DrawSpellIcon();

            _spriteBatch.End();

            base.OnDrawControl(gameTime);
        }

        protected override bool HandleMouseDown(IXNAControl control, MouseEventArgs eventArgs)
        {
            if (_parentContainer.NoItemsDragging())
                _sfxPlayer.PlaySfx(SoundEffectID.InventoryPickup);

            Click?.Invoke(control, eventArgs);
            return base.HandleMouseDown(control, eventArgs);
        }

        protected override bool HandleDragStart(IXNAControl control, MouseEventArgs eventArgs)
        {
            _sfxPlayer.PlaySfx(SoundEffectID.InventoryPickup);

            return base.HandleDragStart(control, eventArgs);
        }

        protected override void OnDraggingFinished(DragCompletedEventArgs<ESFRecord> args)
        {
            base.OnDraggingFinished(args);

            _sfxPlayer.PlaySfx(SoundEffectID.InventoryPlace);
            DrawPosition = GetDisplayPosition(DisplaySlot);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _whitePixel.Dispose();
            }

            base.Dispose(disposing);
        }

        private void DrawLevelAndHighlight()
        {
            if (!IsDragging)
            {
                var width = (int)(InventorySpell.Level / 100.0 * 36);
                var levelDestinationRectangle = new Rectangle(DrawAreaWithParentOffset.X + 3, DrawAreaWithParentOffset.Y + 40, width, 6);
                _spriteBatch.Draw(_whitePixel, levelDestinationRectangle, Color.FromNonPremultiplied(0xc9, 0xb8, 0x9b, 0xff));
            }

            if (MouseOver)
            {
                if (!IsDragging)
                {
                    _spriteBatch.Draw(_whitePixel, DrawAreaWithParentOffset, Color.FromNonPremultiplied(200, 200, 200, 60));
                }
                else
                {
                    var highlightPosition = GetDisplayPosition(DisplaySlot);
                    var highlightRectangle = new Rectangle((_parentContainer.DrawPositionWithParentOffset + highlightPosition).ToPoint(), DrawArea.Size);

                    if (highlightRectangle.Contains(Mouse.GetState().Position))
                        _spriteBatch.Draw(_whitePixel, highlightRectangle, Color.FromNonPremultiplied(200, 200, 200, 60));
                }
            }
        }

        private void DrawSpellIcon()
        {
            var halfWidth = _spellGraphic.Width / 2;
            _spellGraphicSourceRect = new Rectangle(MouseOver ? halfWidth : 0, 0, halfWidth, _spellGraphic.Height);

            Rectangle targetDrawArea;
            if (!IsDragging)
            {
                targetDrawArea = new Rectangle(
                    DrawAreaWithParentOffset.X + (DrawAreaWithParentOffset.Width - _spellGraphicSourceRect.Width) / 2,
                    DrawAreaWithParentOffset.Y + (DrawAreaWithParentOffset.Height - _spellGraphicSourceRect.Height) / 2,
                    _spellGraphicSourceRect.Width,
                    _spellGraphicSourceRect.Height);
            }
            else
            {
                targetDrawArea = new Rectangle(
                    Mouse.GetState().X - _spellGraphicSourceRect.Width / 2,
                    Mouse.GetState().Y - _spellGraphicSourceRect.Height / 2,
                    _spellGraphicSourceRect.Width,
                    _spellGraphicSourceRect.Height
                    );
            }

            _spriteBatch.Draw(_spellGraphic,
                targetDrawArea,
                _spellGraphicSourceRect,
                Color.FromNonPremultiplied(255, 255, 255, IsDragging ? 127 : 255));
        }

        private static Vector2 GetDisplayPosition(int slot)
        {
            //start pos: 101, 97
            //xdelta: 45; ydelta: 52
            var row = slot / ActiveSpellsPanel.SpellRowLength;
            var col = slot % ActiveSpellsPanel.SpellRowLength;
            return new Vector2(101 + col * 45, 9 + row * 52);
        }
    }
}
