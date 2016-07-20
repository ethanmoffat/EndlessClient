// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

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
