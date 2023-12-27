using AutomaticTypeMapper;

namespace EOLib.Domain.Login
{
    public interface IPlayerInfoRepository
    {
        string LoggedInAccountName { get; set; }

        string PlayerPassword { get; set; }

        int PlayerID { get; set; }

        bool IsFirstTimePlayer { get; set; }

        bool PlayerIsInGame { get; set; }

        bool PlayerHasAdminCharacter { get; set; }
    }

    public interface IPlayerInfoProvider
    {
        string LoggedInAccountName { get; }

        string PlayerPassword { get; }

        int PlayerID { get; }

        bool IsFirstTimePlayer { get; }

        bool PlayerIsInGame { get; }

        bool PlayerHasAdminCharacter { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public sealed class PlayerInfoRepository : IPlayerInfoRepository, IPlayerInfoProvider, IResettable
    {
        public string LoggedInAccountName { get; set; }

        public string PlayerPassword { get; set; }

        public int PlayerID { get; set; }

        public bool IsFirstTimePlayer { get; set; }

        public bool PlayerIsInGame { get; set; }

        public bool PlayerHasAdminCharacter { get; set; }

        public void ResetState()
        {
            LoggedInAccountName = "";
            PlayerPassword = "";
            PlayerID = 0;
            IsFirstTimePlayer = false;
            PlayerIsInGame = false;
            PlayerHasAdminCharacter = false;
        }
    }
}
