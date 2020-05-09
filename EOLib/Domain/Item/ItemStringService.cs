using System;

using AutomaticTypeMapper;
using EOLib.IO.Pub;

namespace EOLib.Domain.Item
{
    [AutoMappedType]
    public class ItemStringService : IItemStringService
    {
        public string GetStringForMapDisplay(EIFRecord record, int amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must not be zero!", nameof(amount));

            if (record.ID == 1)
                return $"{amount} {record.Name}";

            return amount == 1 ? record.Name : $"{record.Name} x{amount}";
        }
    }

    public interface IItemStringService
    {
        string GetStringForMapDisplay(EIFRecord record, int amount);
    }
}
