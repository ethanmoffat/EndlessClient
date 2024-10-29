using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using NUnit.Framework;

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

            Assert.That(newRectangle.X, Is.EqualTo(105));
            Assert.That(newRectangle.Y, Is.EqualTo(110));
            Assert.That(newRectangle.Width, Is.EqualTo(15));
            Assert.That(newRectangle.Height, Is.EqualTo(20));
            Assert.That(rectangle, Is.Not.EqualTo(newRectangle));
        }

        [Test]
        public void WithPosition_DoesNotModify_OriginalRectangle()
        {
            var rectangle = new Rectangle(5, 10, 15, 20);
            var newX = 105;
            var newY = 110;

            var newRectangle = rectangle.WithPosition(new Vector2(newX, newY));

            Assert.That(rectangle.X, Is.EqualTo(5));
            Assert.That(rectangle.Y, Is.EqualTo(10));
            Assert.That(rectangle.Width, Is.EqualTo(15));
            Assert.That(rectangle.Height, Is.EqualTo(20));
            Assert.That(rectangle, Is.Not.EqualTo(newRectangle));
        }
    }
}
