using AutomaticTypeMapper;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Dialogs.Services
{
    public enum DialogIcon
    {
        Buy = 0,
        Sell,
        JukeboxBrowse = Sell,
        BankDeposit,
        BankWithdraw,
        Craft,
        BankLockerUpgrade,

        JukeboxPlay = 8,
        Registration = 9,

        Learn = 20,
        Forget = 21,
        InnSleep = 22,
        SignUp = 23,
        Unsubscribe = 24
    }

    [AutoMappedType]
    public class EODialogIconService : IEODialogIconService
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;

        public Texture2D IconSheet => _nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 27);

        public EODialogIconService(INativeGraphicsManager nativeGraphicsManager)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
        }

        public Rectangle GetDialogIconSource(DialogIcon whichIcon)
        {
            const int NUM_PER_ROW = 9;
            const int ICON_SIZE = 31;

            return new Rectangle(((int)whichIcon % NUM_PER_ROW) * ICON_SIZE, 291 + ((int)whichIcon / NUM_PER_ROW) * ICON_SIZE, ICON_SIZE, ICON_SIZE);
        }
    }

    public interface IEODialogIconService
    {
        Texture2D IconSheet { get; }

        Rectangle GetDialogIconSource(DialogIcon whichIcon);
    }
}
