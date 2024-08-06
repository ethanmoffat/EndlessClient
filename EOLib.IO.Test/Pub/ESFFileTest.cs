using System.Diagnostics.CodeAnalysis;
using EOLib.IO.Pub;
using NUnit.Framework;

namespace EOLib.IO.Test.Pub
{
    [TestFixture, ExcludeFromCodeCoverage]
    public class ESFFileTest
    {
        [Test]
        public void HasCorrectFileType()
        {
            Assert.That(new ESFFile().FileType, Is.EqualTo("ESF"));
        }
    }
}
