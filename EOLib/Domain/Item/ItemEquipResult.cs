namespace EOLib.Domain.Item
{
    public enum ItemEquipResult
    {
        Ok,
        AlreadyEquipped,
        WrongGender,
        StatRequirementNotMet,
        ClassRequirementNotMet,
        NotEquippable
    }
}