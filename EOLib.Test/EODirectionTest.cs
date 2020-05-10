using NUnit.Framework;

namespace EOLib.Test
{
    [TestFixture]
    public class EODirectionTest
    {
        [Test]
        public void OppositeOfLeftIsRight()
        {
            Assert.That(EODirection.Left.Opposite(), Is.EqualTo(EODirection.Right));
        }

        [Test]
        public void OppositeOfRightIsLeft()
        {
            Assert.That(EODirection.Right.Opposite(), Is.EqualTo(EODirection.Left));
        }

        [Test]
        public void OppositeOfDownIsUp()
        {
            Assert.That(EODirection.Down.Opposite(), Is.EqualTo(EODirection.Up));
        }

        [Test]
        public void OppositeOfUpIsDown()
        {
            Assert.That(EODirection.Up.Opposite(), Is.EqualTo(EODirection.Down));
        }
    }
}
