using System.Diagnostics;
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
        private int _accountValue;
        public int AccountValue
        {
            get => _accountValue;
            set
            {
                _accountValue = value;
                Debug.WriteLine($"AccountValue updated to: {_accountValue}");
            }
        }

        public int SessionID { get; set; }

        private Option<int> _lockerUpgrades;
        public Option<int> LockerUpgrades
        {
            get => _lockerUpgrades;
            set
            {
                _lockerUpgrades = value;
                Debug.WriteLine($"LockerUpgrades updated: {value.Match(val => val.ToString(), () => "None")}");
            }
        }

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
