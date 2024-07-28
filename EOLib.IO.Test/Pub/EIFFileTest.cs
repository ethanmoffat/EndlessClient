using EOLib.IO.Pub;
using NUnit.Framework;
using System.Diagnostics.CodeAnalysis;

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