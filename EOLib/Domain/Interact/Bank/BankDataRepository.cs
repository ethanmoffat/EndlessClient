using AutomaticTypeMapper;
using Optional;

namespace EOLib.Domain.Interact.Bank
{
    public interface IBankDataRepository : IResettable
    {
        int AccountValue { get; set; }

        int SessionID { get; set; }

        Option<int> LockerUpgrades { get; set; }
    }

    public interface IBankDataProvider : IResettable
    {
        int AccountValue { get; }

        int SessionID { get; }

        Option<int> LockerUpgrades { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class BankDataRepository : IBankDataRepository, IBankDataProvider
    {
        public int AccountValue { get; set; }

        public int SessionID { get; set; }

        public Option<int> LockerUpgrades { get; set; }

        public BankDataRepository()
        {
            ResetState();
        }

        public void ResetState()
        {
            AccountValue = 0;
            SessionID = 0;
            LockerUpgrades = Option.None<int>();
        }
    }
}