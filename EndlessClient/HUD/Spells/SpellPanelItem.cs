using System;
using EndlessClient.HUD.Panels;
using EOLib.Domain.Character;
using EOLib.Graphics;
using EOLib.IO.Pub;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace EndlessClient.HUD.Spells
{
    public class SpellPanelItem : BaseSpellPanelItem
    {
        public class SpellDragCompletedEventArgs
        {
            public bool ContinueDragging { get; set; }
        }

        private readonly Texture2D _spellGraphic, _spellLevelColor;

        private Rectangle _spellGraphicSourceRect;
        private DateTime _clickTime;
        private bool _dragging, _followMouse;
        private Rectangle _levelDestinationRectangle;

        private int _lastSlot;
        private IInventorySpell _lastInventorySpell;

        public override IInventorySpell InventorySpell { get; set; }

        public override bool IsBeingDragged => _dragging;

        public override ESFRecord SpellData { get; }

        public SpellPanelItem(ActiveSpellsPanel parent, int slot, IInventorySpell spell, ESFRecord spellData)
            : base(parent, slot)
        {
            InventorySpell = spell;
            SpellData = spellData;

            _spellGraphic = _parentPanel.NativeGraphicsManager.TextureFromResource(GFXTypes.SpellIcons, SpellData.Icon);
            _spellGraphicSourceRect = new Rectangle(0, 0, _spellGraphic.Width / 2, _spellGraphic.Height);

            _spellLevelColor = new Texture2D(Game.GraphicsDevice, 1, 1);
            _spellLevelColor.SetData(new[] { Color.White });

            _clickTime = DateTime.Now;

            OnMouseEnter += (_, _) => SetIconHover(true);
            OnMouseLeave += (_, _) => SetIconHover(false);
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            DoClickAndDragLogic();

            if (_lastSlot != DisplaySlot || _lastInventorySpell != InventorySpell)
            {
                //36 is full width of level bar
                var width = (int)(InventorySpell.Level / 100.0 * 36);
                _levelDestinationRectangle = new Rectangle(DrawAreaWithParentOffset.X + 3, DrawAreaWithParentOffset.Y + 40, width, 6);

                _lastSlot = DisplaySlot;
                _lastInventorySpell = InventorySpell;
            }

            base.OnUpdateControl(gameTime);
        }

        protected override void OnDrawControl(GameTime gameTime)
        {
            _spriteBatch.Begin();
            DrawSpellIcon();
            DrawSpellLevel();
            _spriteBatch.End();

            base.OnDrawControl(gameTime);
        }

        private void SetIconHover(bool hover)
        {
            var halfWidth = _spellGraphic.Width / 2;
            _spellGraphicSourceRect = new Rectangle(hover ? halfWidth : 0, 0, halfWidth, _spellGraphic.Height);
        }

        private void DoClickAndDragLogic()
        {
            if (!_dragging && _parentPanel.AnySpellsDragging())
                return;

            if (LeftButtonDown)
            {
                if (!_dragging)
                {
                    _followMouse = true;
                    _clickTime = DateTime.Now;
                }
                else
                {
                    EndDragging();
                }
            }
            else if (LeftButtonUp)
            {
                if (!_dragging)
                {
                    var clickDelta = (DateTime.Now - _clickTime).TotalMilliseconds;
                    if (clickDelta < 75)
                    {
                        _dragging = true;
                    }
                }
                else
                {
                    EndDragging();
                }
            }

            if (!_dragging && _followMouse && (DateTime.Now - _clickTime).TotalMilliseconds >= 75)
                _dragging = true;
        }

        private bool LeftButtonDown =>
            MouseOver && MouseOverPreviously &&
            CurrentMouseState.LeftButton == ButtonState.Pressed &&
            PreviousMouseState.LeftButton == ButtonState.Released;

        private bool LeftButtonUp =>
            CurrentMouseState.LeftButton == ButtonState.Released &&
            PreviousMouseState.LeftButton == ButtonState.Pressed;

        private void EndDragging()
        {
            _dragging = false;
            _followMouse = false;

            var args = new SpellDragCompletedEventArgs();
            InvokeDragCompleted(args);

            if (args.ContinueDragging)
            {
                _dragging = true;
                _followMouse = true;
            }
        }

        private void DrawSpellIcon()
        {
            Rectangle targetDrawArea;
            Color alphaColor;
            if (!_followMouse)
            {
                targetDrawArea = new Rectangle(
                    DrawAreaWithParentOffset.X + (DrawAreaWithParentOffset.Width - _spellGraphicSourceRect.Width) / 2,
                    DrawAreaWithParentOffset.Y + (DrawAreaWithParentOffset.Height - _spellGraphicSourceRect.Height) / 2,
                    _spellGraphicSourceRect.Width,
                    _spellGraphicSourceRect.Height);
                alphaColor = Color.White;
            }
            else
            {
                targetDrawArea = new Rectangle(
                    Mouse.GetState().X - _spellGraphicSourceRect.Width / 2,
                    Mouse.GetState().Y - _spellGraphicSourceRect.Height / 2,
                    _spellGraphicSourceRect.Width,
                    _spellGraphicSourceRect.Height
                    );
                alphaColor = Color.FromNonPremultiplied(255, 255, 255, 128);
            }

            if (targetDrawArea.Width * targetDrawArea.Height == 0)
                return;

            _spriteBatch.Draw(_spellGraphic, targetDrawArea, _spellGraphicSourceRect, alphaColor);
        }

        private void DrawSpellLevel()
        {
            if (_followMouse || _dragging || _spellLevelColor == null)
                return;

            _spriteBatch.Draw(_spellLevelColor, _levelDestinationRectangle, Color.FromNonPremultiplied(0xc9, 0xb8, 0x9b, 0xff));
        }
    }
}
