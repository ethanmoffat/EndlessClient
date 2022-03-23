using System;
using EOLib.Graphics;
using EOLib.IO;
using EOLib.IO.Pub;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Optional;
using XNAControls;

namespace EndlessClient.Dialogs
{
    public class PaperdollDialogItem : XNAPictureBox
    {
        private readonly bool _isMainCharacter;
        private readonly Option<EIFRecord> _itemInfo;

        public EquipLocation EquipLocation { get; }

        public short ItemID => (short)_itemInfo.Match(r => r.ID, () => 0);

        public event EventHandler<EIFRecord> RightClick;

        public PaperdollDialogItem(INativeGraphicsManager nativeGraphicsManager,
                                   bool isMainCharacter,
                                   EquipLocation location,
                                   Option<EIFRecord> itemInfo)
        {
            _isMainCharacter = isMainCharacter;
            EquipLocation = location;
            _itemInfo = itemInfo;

            _itemInfo.MatchSome(r => Texture = nativeGraphicsManager.TextureFromResource(GFXTypes.Items, r.Graphic * 2, true));
            StretchMode = StretchMode.CenterInFrame;
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            base.OnUpdateControl(gameTime);

            if (!_isMainCharacter)
                return;

            if (MouseOver && CurrentMouseState.RightButton == ButtonState.Released && PreviousMouseState.RightButton == ButtonState.Pressed)
            {
                _itemInfo.MatchSome(itemInfo =>
                {
                    if (_isMainCharacter)
                    {
                        RightClick?.Invoke(this, itemInfo);
                    }
                });
            }
        }
    }
}
