using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;

namespace EOLib.Graphics.Test
{
    [TestFixture, ExcludeFromCodeCoverage]
    public class LibraryGraphicPairTest
    {
        [Test]
        public void Equals_ReturnsFalse_WhenOtherObjectIsNotLibraryGraphicPair()
        {
            var pair = new LibraryGraphicPair(1, 1);
            Assert.IsFalse(pair.Equals(new object()));
        }

        [Test]
        public void CompareTo_ReturnsNeg1_WhenOtherObjectIsNotLibraryGraphicPair()
        {
            var pair = new LibraryGraphicPair(1, 1);
            Assert.AreEqual(-1, pair.CompareTo(new object()));
        }

        [Test]
        public void CompareTo_ReturnsNeg1_WhenOtherObjectHasDifferentLibraryNumber()
        {
            var pair = new LibraryGraphicPair(1, 1);
            var other = new LibraryGraphicPair(2, 1);

            Assert.AreEqual(-1, pair.CompareTo(other));
        }

        [Test]
        public void CompareTo_ReturnsNeg1_WhenOtherObjectHasDifferentGFXNumber()
        {
            var pair = new LibraryGraphicPair(1, 1);
            var other = new LibraryGraphicPair(1, 2);

            Assert.AreEqual(-1, pair.CompareTo(other));
        }

        [Test]
        public void CompareTo_Returns0_WhenOtherObjectHasSameValues()
        {
            var pair = new LibraryGraphicPair(1, 1);
            var other = new LibraryGraphicPair(1, 1);

            Assert.AreEqual(0, pair.CompareTo(other));
        }
    }
}
