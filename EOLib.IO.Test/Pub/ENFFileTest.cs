using System.Diagnostics.CodeAnalysis;
using EOLib.IO.Pub;
using NUnit.Framework;

namespace EOLib.IO.Test.Pub
{
    [TestFixture, ExcludeFromCodeCoverage]
    public class ENFFileTest
    {
        [Test]
        public void HasCorrectFileType()
        {
            Assert.That(new ENFFile().FileType, Is.EqualTo("ENF"));
        }
    }
}