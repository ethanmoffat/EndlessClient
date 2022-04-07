using System.Threading.Tasks;

namespace EndlessClient.HUD
{
    public interface IHudButtonController
    {
        void ClickInventory();

        void ClickViewMapToggle();

        void ClickActiveSpells();

        void ClickPassiveSpells();

        void ClickChat();

        void ClickStats();

        Task ClickOnlineList();

        void ClickParty();

        void ClickSettings();

        void ClickHelp();

        Task ClickFriendList();

        Task ClickIgnoreList();

        void ClickUsageAndStats();

        void ClickQuestStatus();
    }
}
