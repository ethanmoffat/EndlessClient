// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Dialogs.Services
{
    public enum SmallButton
    {
        Connect = 0,
        Cancel,
        Login,
        Delete,
        Ok,
        Back,
        Add,
        Next,
        History,
        Progress,
        NUM_BUTTONS
    }

    public class EODialogButtonService : IEODialogButtonService
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;

        public Texture2D SmallButtonSheet
        {
            get { return _nativeGraphicsManager.TextureFromResource(GFXTypes.PreLoginUI, 15, true); }
        }

        public EODialogButtonService(INativeGraphicsManager nativeGraphicsManager)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
        }

        public Rectangle GetSmallDialogButtonOutSource(SmallButton whichButton)
        {
            var widthDelta = SmallButtonSheet.Width/2;
            var heightDelta = SmallButtonSheet.Height/(int) SmallButton.NUM_BUTTONS;
            return new Rectangle(0, heightDelta*(int) whichButton, widthDelta, heightDelta);
        }

        public Rectangle GetSmallDialogButtonOverSource(SmallButton whichButton)
        {
            var widthDelta = SmallButtonSheet.Width/2;
            var heightDelta = SmallButtonSheet.Height/(int) SmallButton.NUM_BUTTONS;
            return new Rectangle(widthDelta, heightDelta*(int) whichButton, widthDelta, heightDelta);
        }
    }

    public interface IEODialogButtonService
    {
        Texture2D SmallButtonSheet { get; }

        Rectangle GetSmallDialogButtonOutSource(SmallButton whichButton);
        Rectangle GetSmallDialogButtonOverSource(SmallButton whichButton);
    }
}
