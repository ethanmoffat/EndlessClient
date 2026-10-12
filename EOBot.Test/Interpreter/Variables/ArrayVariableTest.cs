using System.Collections.Generic;
using EOBot.Interpreter.Variables;
using NUnit.Framework;

namespace EOBot.Test.Interpreter.Variables
{
    [TestFixture]
    public class ArrayVariableTest
    {
        [Test]
        public void IndexOf_Primitive_MatchesByValue()
        {
            var array = Array(new IntVariable(1), new StringVariable("a"), new IntVariable(2));

            Assert.That(array.IndexOf(new IntVariable(2)), Is.EqualTo(2));
            Assert.That(array.IndexOf(new StringVariable("a")), Is.EqualTo(1));
        }

        [Test]
        public void IndexOf_EnumAndInt_MatchByValue()
        {
            var array = Array(new IntVariable(0), new EnumVariable("E", "B", 1));

            Assert.That(array.IndexOf(new IntVariable(1)), Is.EqualTo(1));
            Assert.That(array.IndexOf(new EnumVariable("E", "A", 0)), Is.EqualTo(0));
        }

        [Test]
        public void IndexOf_Collection_MatchesByContents()
        {
            var array = Array(
                Array(new IntVariable(1)),
                new DictVariable(new Dictionary<string, IVariable> { ["a"] = new IntVariable(1), ["b"] = new IntVariable(2) }));

            Assert.That(array.IndexOf(Array(new IntVariable(1))), Is.EqualTo(0));
            Assert.That(array.IndexOf(new DictVariable(new Dictionary<string, IVariable> { ["b"] = new IntVariable(2), ["a"] = new IntVariable(1) })), Is.EqualTo(1));
        }

        [Test]
        public void IndexOf_IncomparableElements_AreSkipped()
        {
            var array = Array(new IntVariable(2), Array(new IntVariable(2)), UndefinedVariable.Instance);

            Assert.That(array.IndexOf(Array(new IntVariable(2))), Is.EqualTo(1));
            Assert.That(array.IndexOf(UndefinedVariable.Instance), Is.EqualTo(2));
        }

        [Test]
        public void IndexOf_NoMatch_ReturnsNegativeOne()
        {
            var array = Array(new IntVariable(1), Array(new IntVariable(1)));

            Assert.That(array.IndexOf(new StringVariable("1")), Is.EqualTo(-1));
            Assert.That(array.IndexOf(Array(new IntVariable(2))), Is.EqualTo(-1));
        }

        private static ArrayVariable Array(params IVariable[] elements) => new([.. elements]);
    }
}
