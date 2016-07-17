// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient.UIControls
{
    public class DisposablePictureBox : PictureBox
    {
        public DisposablePictureBox(Texture2D displayPicture, XNAControl parent = null)
            : base(displayPicture, parent) { }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _displayPicture.Dispose();
            base.Dispose(disposing);
        }
    }
}
