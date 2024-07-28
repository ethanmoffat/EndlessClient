using EOLib.IO.Pub;

namespace EOLib.IO.Repositories
{
    public interface IPubFileProvider : IEIFFileProvider, IENFFileProvider, IESFFileProvider, IECFFileProvider
    {
    }

    public interface IEIFFileProvider
    {
        IPubFile<EIFRecord> EIFFile { get; }
    }

    public interface IENFFileProvider
    {
        IPubFile<ENFRecord> ENFFile { get; }
    }

    public interface IESFFileProvider
    {
        IPubFile<ESFRecord> ESFFile { get; }
    }

    public interface IECFFileProvider
    {
        IPubFile<ECFRecord> ECFFile { get; }
    }
}