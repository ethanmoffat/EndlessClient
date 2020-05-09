using EOLib.IO.Pub;

namespace EOLib.IO.Repositories
{
    public interface IPubFileRepository : IEIFFileRepository, IENFFileRepository, IESFFileRepository, IECFFileRepository
    {
    }

    public interface IEIFFileRepository
    {
        IPubFile<EIFRecord> EIFFile { get; set; }
    }

    public interface IENFFileRepository
    {
        IPubFile<ENFRecord> ENFFile { get; set; }
    }

    public interface IESFFileRepository
    {
        IPubFile<ESFRecord> ESFFile { get; set; }
    }

    public interface IECFFileRepository
    {
        IPubFile<ECFRecord> ECFFile { get; set; }
    }
}
