using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EOBot.Interpreter;
using NUnit.Framework;

namespace EOBot.Test.Interpreter
{
    [TestFixture]
    public class BotTokenizerTest
    {
        [TestCase("$x = 1 ", "Variable Assign Literal EOF")]
        [TestCase("$x = 1\n  ", "Variable Assign Literal NewLine EOF")]
        [TestCase("$x = 1 /* c */", "Variable Assign Literal EOF")]
        [TestCase("$x = 1 // c", "Variable Assign Literal EOF")]
        [TestCase("$x = 1 // c\n$y = 2", "Variable Assign Literal NewLine Variable Assign Literal EOF")]
        [TestCase("$x = 1 /* c */\n$y = 2", "Variable Assign Literal NewLine Variable Assign Literal EOF")]
        [TestCase("$x = 1 /* a *//* b */\n$y = 2", "Variable Assign Literal NewLine Variable Assign Literal EOF")]
        [TestCase("$x = /**/ 1", "Variable Assign Literal EOF")]
        [TestCase("/* a\nb */ $x = 1", "Variable Assign Literal EOF")]
        public void GetNextToken_WhitespaceAndComments_AreSkipped(string input, string expectedTypes)
        {
            Assert.That(Tokenize(input).Select(x => x.TokenType.ToString().Replace("Operator", string.Empty)), Is.EqualTo(expectedTypes.Split(' ')));
        }

        [Test]
        public void GetNextToken_MultilineBlockComment_TracksLineNumber()
        {
            Assert.That(Tokenize("/* a\nb */ $x = 1").First().LineNumber, Is.EqualTo(2));
        }

        [TestCase("$x = 1 /* c")]
        [TestCase("/*/")]
        public void GetNextToken_UnterminatedBlockComment_ReturnsError(string input)
        {
            Assert.That(Tokenize(input).Last().TokenType, Is.EqualTo(BotTokenType.Error));
        }

        private static List<BotToken> Tokenize(string input)
        {
            using var tokenizer = new BotTokenizer(new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(input))));

            var tokens = new List<BotToken>();
            BotToken token;
            do
            {
                token = tokenizer.GetNextToken();
                tokens.Add(token);
            } while (token.TokenType != BotTokenType.EOF && token.TokenType != BotTokenType.Error);

            return tokens;
        }
    }
}
