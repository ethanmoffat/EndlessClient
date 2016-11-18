// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EOLib.Graphics.Test
{
    [TestClass, ExcludeFromCodeCoverage]
    public class LibraryGraphicPairTest
    {
        [TestMethod]
        public void Equals_ReturnsFalse_WhenOtherObjectIsNotLibraryGraphicPair()
        {
            var pair = new LibraryGraphicPair(1, 1);
            Assert.IsFalse(pair.Equals(new object()));
        }

        [TestMethod]
        public void CompareTo_ReturnsNeg1_WhenOtherObjectIsNotLibraryGraphicPair()
        {
            var pair = new LibraryGraphicPair(1, 1);
            Assert.AreEqual(-1, pair.CompareTo(new object()));
        }

        [TestMethod]
        public void CompareTo_ReturnsNeg1_WhenOtherObjectHasDifferentLibraryNumber()
        {
            var pair = new LibraryGraphicPair(1, 1);
            var other = new LibraryGraphicPair(2, 1);

            Assert.AreEqual(-1, pair.CompareTo(other));
        }

        [TestMethod]
        public void CompareTo_ReturnsNeg1_WhenOtherObjectHasDifferentGFXNumber()
        {
            var pair = new LibraryGraphicPair(1, 1);
            var other = new LibraryGraphicPair(1, 2);

            Assert.AreEqual(-1, pair.CompareTo(other));
        }

        [TestMethod]
        public void CompareTo_Returns0_WhenOtherObjectHasSameValues()
        {
            var pair = new LibraryGraphicPair(1, 1);
            var other = new LibraryGraphicPair(1, 1);

            Assert.AreEqual(0, pair.CompareTo(other));
        }
    }
}
