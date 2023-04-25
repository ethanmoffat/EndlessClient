using System;
using EndlessClient.HUD.Panels;
using EOLib.Domain.Character;
using EOLib.IO.Pub;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input.InputListeners;
using XNAControls;

using static EndlessClient.HUD.Spells.SpellPanelItem;

namespace EndlessClient.HUD.Spells
{
    public abstract class BaseSpellPanelItem : XNAControl, ISpellPanelItem
    {
        protected const int ICON_AREA_WIDTH = 42, ICON_AREA_HEIGHT = 36;

        public int Slot { get; set; }

        private int _displaySlot;
        public int DisplaySlot
        {
            get => _displaySlot;
            set
            {
                _displaySlot = value;

                //start pos: 101, 97
                //xdelta: 45; ydelta: 52
                var row = _displaySlot / ActiveSpellsPanel.SpellRowLength;
                var col = _displaySlot % ActiveSpellsPanel.SpellRowLength;
                DrawPosition = new Vector2(101 + col * 45, 9 + row * 52);
            }
        }

        public virtual bool IsBeingDragged => false;

        public abstract InventorySpell InventorySpell { get; set; }

        public abstract ESFRecord SpellData { get; }

        public event EventHandler Clicked;

        public abstract event EventHandler<SpellDragCompletedEventArgs> DoneDragging;

        private readonly Texture2D _highlightColor;
        protected readonly ActiveSpellsPanel _parentPanel;

        protected BaseSpellPanelItem(ActiveSpellsPanel parent, int slot)
        {
            SetParentControl(parent);
            _parentPanel = parent;

            Slot = slot;
            DisplaySlot = slot;

            _highlightColor = new Texture2D(Game.GraphicsDevice, 1, 1);
            _highlightColor.SetData(new[] { Color.White });

            SetSize(ICON_AREA_WIDTH, ICON_AREA_HEIGHT);
        }

        protected override void OnDrawControl(GameTime gameTime)
        {
            if (MouseOver && _parentPanel.AnySpellsDragging())
            {
                _spriteBatch.Begin();
                _spriteBatch.Draw(_highlightColor, DrawAreaWithParentOffset, Color.FromNonPremultiplied(200, 200, 200, 60));
                _spriteBatch.End();
            }

            base.OnDrawControl(gameTime);
        }

        protected override bool HandleClick(IXNAControl control, MouseEventArgs eventArgs)
        {
            if (!_parentPanel.AnySpellsDragging())
            {
                Clicked?.Invoke(this, EventArgs.Empty);
                return true;
            }

            return false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _highlightColor.Dispose();

            base.Dispose(disposing);
        }
    }
}
