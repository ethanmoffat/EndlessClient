namespace EOLib.Domain.Spells
{
    public enum SpellCastValidationResult
    {
        Ok,
        CannotAttackNPC,
        WrongTargetType,
        ExhaustedNoSp,
        ExhaustedNoTp,
    }
}
