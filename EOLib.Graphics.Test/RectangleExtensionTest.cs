using Microsoft.Xna.Framework;
using NUnit.Framework;
using System.Diagnostics.CodeAnalysis;

namespace EOLib.Graphics.Test
{
    [TestFixture, ExcludeFromCodeCoverage]
    public class RectangleExtensionTest
    {
        [Test]
        public void WithPosition_ReturnsNewRectangle_WithExpectedSizeAndOffset()
        {
            var rectangle = new Rectangle(5, 10, 15, 20);
            var newX = 105;
            var newY = 110;

            var newRectangle = rectangle.WithPosition(new Vector2(newX, newY));

            Assert.AreEqual(105, newRectangle.X);
            Assert.AreEqual(110, newRectangle.Y);
            Assert.AreEqual(15, newRectangle.Width);
            Assert.AreEqual(20, newRectangle.Height);
            Assert.AreNotEqual(newRectangle, rectangle);
        }

        [Test]
        public void WithPosition_DoesNotModify_OriginalRectangle()
        {
            var rectangle = new Rectangle(5, 10, 15, 20);
            var newX = 105;
            var newY = 110;

            var newRectangle = rectangle.WithPosition(new Vector2(newX, newY));

            Assert.AreEqual(5, rectangle.X);
            Assert.AreEqual(10, rectangle.Y);
            Assert.AreEqual(15, rectangle.Width);
            Assert.AreEqual(20, rectangle.Height);
            Assert.AreNotEqual(newRectangle, rectangle);
        }
    }
}