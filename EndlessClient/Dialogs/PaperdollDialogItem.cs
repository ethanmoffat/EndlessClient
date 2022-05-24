using EndlessClient.Audio;
using EndlessClient.Dialogs.Extensions;
using EndlessClient.HUD.Panels;
using EOLib.Graphics;
using EOLib.IO;
using EOLib.IO.Pub;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Optional;
using System;
using XNAControls;

namespace EndlessClient.Dialogs
{
    public class PaperdollDialogItem : XNAPictureBox
    {
        private readonly ISfxPlayer _sfxPlayer;
        private readonly InventoryPanel _inventoryPanel;
        private readonly PaperdollDialog _paperdollDialog;
        private readonly bool _isMainCharacter;
        private readonly Option<EIFRecord> _itemInfo;

        private bool _beingDragged;

        public EquipLocation EquipLocation { get; }

        public short ItemID => (short)_itemInfo.Match(r => r.ID, () => 0);

        public event EventHandler<EIFRecord> RightClick;

        public bool IsBeingDragged => _beingDragged;

        private bool LeftButtonReleased => CurrentMouseState.LeftButton == ButtonState.Released && PreviousMouseState.LeftButton == ButtonState.Pressed;

        private bool RightButtonReleased => CurrentMouseState.RightButton == ButtonState.Released && PreviousMouseState.RightButton == ButtonState.Pressed;

        private bool LeftButtonHeld => CurrentMouseState.LeftButton == ButtonState.Pressed && PreviousMouseState.LeftButton == ButtonState.Pressed;

        public PaperdollDialogItem(INativeGraphicsManager nativeGraphicsManager,
                                   ISfxPlayer sfxPlayer,
                                   InventoryPanel inventoryPanel,
                                   PaperdollDialog paperdollDialog,
                                   bool isMainCharacter,
                                   EquipLocation location,
                                   Option<EIFRecord> itemInfo)
        {
            _sfxPlayer = sfxPlayer;
            _inventoryPanel = inventoryPanel;
            _paperdollDialog = paperdollDialog;
            _isMainCharacter = isMainCharacter;
            EquipLocation = location;
            _itemInfo = itemInfo;

            _itemInfo.MatchSome(r => Texture = nativeGraphicsManager.TextureFromResource(GFXTypes.Items, r.Graphic * 2, true));
            StretchMode = StretchMode.CenterInFrame;
        }

        public void StartDragging()
        {
            _beingDragged = true;
            SetControlUnparented();
            Game.Components.Add(this);

            _sfxPlayer.PlaySfx(SoundEffectID.InventoryPickup);

            DrawOrder = 1000;
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            if (_isMainCharacter)
            {
                _itemInfo.MatchSome(itemInfo =>
                {
                    if (!_beingDragged && MouseOver && MouseOverPreviously && LeftButtonHeld)
                    {
                        if (_inventoryPanel.NoItemsDragging() && _paperdollDialog.NoItemsDragging())
                        {
                            StartDragging();
                        }
                    }
                    else if (_beingDragged)
                    {
                        DrawPosition = new Vector2(CurrentMouseState.X - (DrawArea.Width / 2), CurrentMouseState.Y - (DrawArea.Height / 2));

                        if (LeftButtonReleased)
                        {
                            if (_inventoryPanel.MouseOver && _inventoryPanel.MouseOverPreviously)
                            {
                                StopDragging();
                                RightClick?.Invoke(this, itemInfo);
                            }
                        }
                        else if (RightButtonReleased)
                        {
                            StopDragging();
                        }
                    }
                    else if (!_beingDragged && MouseOver && RightButtonReleased)
                    {
                        RightClick?.Invoke(this, itemInfo);
                    }
                });
            }

            base.OnUpdateControl(gameTime);
        }

        private void StopDragging()
        {
            _beingDragged = false;
            SetParentControl(_paperdollDialog);
            DrawArea = EquipLocation.GetEquipLocationRectangle();

            _sfxPlayer.PlaySfx(SoundEffectID.InventoryPlace);
        }
    }
}
