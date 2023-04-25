using EOLib.IO.Pub;
using System.Collections.Generic;

namespace EOLib.IO.Repositories
{
    public interface IPubFileRepository : IEIFFileRepository, IENFFileRepository, IESFFileRepository, IECFFileRepository
    {
    }

    public interface IEIFFileRepository
    {
        IPubFile<EIFRecord> EIFFile { get; set; }
        List<IPubFile<EIFRecord>> EIFFiles { get; set; }
    }

    public interface IENFFileRepository
    {
        IPubFile<ENFRecord> ENFFile { get; set; }
        List<IPubFile<ENFRecord>> ENFFiles { get; set; }
    }

    public interface IESFFileRepository
    {
        IPubFile<ESFRecord> ESFFile { get; set; }
        List<IPubFile<ESFRecord>> ESFFiles { get; set; }
    }

    public interface IECFFileRepository
    {
        IPubFile<ECFRecord> ECFFile { get; set; }
        List<IPubFile<ECFRecord>> ECFFiles { get; set; }
    }
}
