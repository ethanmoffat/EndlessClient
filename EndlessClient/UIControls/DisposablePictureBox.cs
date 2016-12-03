// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient.UIControls
{
    public class DisposablePictureBox : XNAPictureBox
    {
        public DisposablePictureBox(Texture2D displayPicture)
        {
            Texture = displayPicture;
            SetSize(Texture.Width, Texture.Height);
            StretchMode = StretchMode.Stretch;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                Texture.Dispose();
            base.Dispose(disposing);
        }
    }
}
