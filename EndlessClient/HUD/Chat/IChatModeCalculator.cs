using EndlessClient.UIControls;

namespace EndlessClient.HUD.Chat;

public interface IChatModeCalculator
{
    ChatModePictureBox.ChatMode CalculateMode(string fullTextString);
}