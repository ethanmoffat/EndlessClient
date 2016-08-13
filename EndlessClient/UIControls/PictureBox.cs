// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient.UIControls
{
    public class PictureBox : XNAControl
    {
        protected Texture2D _displayPicture;

        private Rectangle? _src;

        public Rectangle? SourceRectangle
        {
            get { return _src; }
            set
            {
                _src = value;
                SetNewPicture(_displayPicture);
            }
        }

        public PictureBox(Texture2D displayPicture, XNAControl parent = null)
            : base(null, null, parent)
        {
            SetNewPicture(displayPicture);
            if (parent == null)
                Game.Components.Add(this);

            SourceRectangle = null;
        }

        public void SetNewPicture(Texture2D displayPicture)
        {
            _displayPicture = displayPicture;

            if (!SourceRectangle.HasValue)
                _setSize(displayPicture.Width, displayPicture.Height);
            else
                _setSize(((Rectangle)SourceRectangle).Width,
                         ((Rectangle)SourceRectangle).Height);
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch.Begin();
            SpriteBatch.Draw(
                _displayPicture,
                DrawAreaWithOffset,
                SourceRectangle,
                Color.White);
            SpriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
