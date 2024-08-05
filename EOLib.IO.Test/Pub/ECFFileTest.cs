using System.Diagnostics.CodeAnalysis;
using EOLib.IO.Pub;
using NUnit.Framework;

namespace EOLib.IO.Test.Pub
{
    [TestFixture, ExcludeFromCodeCoverage]
    public class ECFFileTest
    {
        [Test]
        public void HasCorrectFileType()
        {
            Assert.That(new ECFFile().FileType, Is.EqualTo("ECF"));
        }
    }
}