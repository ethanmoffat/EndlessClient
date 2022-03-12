namespace EOLib.Localization
{
    public interface IEDFLoaderService
    {
        IEDFFile LoadFile(string fileName, DataFiles whichFile);

        void SaveFile(string fileName, IEDFFile file);
    }
}