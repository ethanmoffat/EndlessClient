namespace EndlessClient.Rendering.Character
{
    public interface ISpellCaster
    {
        void ShoutSpellPrep(string spellName);

        void ShoutSpellCast();

        void StopShout();
    }
}