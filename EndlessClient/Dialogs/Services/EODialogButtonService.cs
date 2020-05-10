using AutomaticTypeMapper;
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

    [MappedType(BaseType = typeof(IEODialogButtonService))]
    public class EODialogButtonService : IEODialogButtonService
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;

        public Texture2D SmallButtonSheet => _nativeGraphicsManager.TextureFromResource(GFXTypes.PreLoginUI, 15, true);

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
