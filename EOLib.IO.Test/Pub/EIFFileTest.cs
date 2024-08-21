using System.Diagnostics.CodeAnalysis;
using EOLib.IO.Pub;
using NUnit.Framework;

namespace EOLib.IO.Test.Pub
{
    [TestFixture, ExcludeFromCodeCoverage]
    public class EIFFileTest
    {
        [Test]
        public void HasCorrectFileType()
        {
            Assert.That(new EIFFile().FileType, Is.EqualTo("EIF"));
        }
    }
}
