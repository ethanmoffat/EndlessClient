using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.Domain.Character
{
    public interface IPlayerInfoData
    {
        string Name { get; }

        string Home { get; }

        string Partner { get; }

        string Title { get; }

        string Guild { get; }

        string Rank { get; }

        int PlayerID { get; }

        int Class { get; }

        int Gender { get; }

        CharacterIcon Icon { get; }
    }
}