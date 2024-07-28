using AutomaticTypeMapper;

namespace EOLib.Domain.Login
{
    public interface IPlayerInfoRepository
    {
        int LoginAttempts { get; set; }

        string LoggedInAccountName { get; set; }

        string PlayerPassword { get; set; }

        int PlayerID { get; set; }

        bool IsFirstTimePlayer { get; set; }

        bool PlayerIsInGame { get; set; }

        bool PlayerHasAdminCharacter { get; set; }
    }

    public interface IPlayerInfoProvider
    {
        int LoginAttempts { get; }

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
        public int LoginAttempts { get; set; }

        public string LoggedInAccountName { get; set; }

        public string PlayerPassword { get; set; }

        public int PlayerID { get; set; }

        public bool IsFirstTimePlayer { get; set; }

        public bool PlayerIsInGame { get; set; }

        public bool PlayerHasAdminCharacter { get; set; }

        public PlayerInfoRepository() => ResetState();

        public void ResetState()
        {
            LoginAttempts = 0;
            LoggedInAccountName = "";
            PlayerPassword = "";
            PlayerID = 0;
            IsFirstTimePlayer = false;
            PlayerIsInGame = false;
            PlayerHasAdminCharacter = false;
        }
    }
}