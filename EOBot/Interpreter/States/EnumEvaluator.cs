using System.Threading;
using System.Threading.Tasks;
using EOBot.Interpreter.Extensions;
using EOBot.Interpreter.Variables;

namespace EOBot.Interpreter.States
{
    public class EnumEvaluator : BaseEvaluator
    {
        public override Task<(EvalResult, string, BotToken)> EvaluateAsync(ProgramState input, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return Task.FromResult<(EvalResult, string, BotToken)>((EvalResult.Cancelled, string.Empty, null));

            (EvalResult Result, string Reason, BotToken Token) evalRes = EvaluateEnumMember(input);
            if (evalRes.Result == EvalResult.NotMatch)
                evalRes = EvaluateEnumTypeSpecifier(input);

            return Task.FromResult(evalRes);
        }

        private static (EvalResult, string, BotToken) EvaluateEnumMember(ProgramState input)
        {
            var enumToken = input.Current();
            if (!input.ExpectPair(BotTokenType.Identifier, BotTokenType.ScopeResolution))
                return (EvalResult.NotMatch, string.Empty, enumToken);

            var memberToken = input.Current();
            if (!input.Expect(BotTokenType.Identifier))
                return Error(memberToken, BotTokenType.Identifier);

            if (!input.SymbolTable.TryGetValue(enumToken.TokenValue, out var symbol))
                return IdentifierNotFoundError(new IdentifierBotToken(enumToken));

            if (symbol.Identifiable is not EnumDefinition enumDefinition)
                return (EvalResult.Failed, $"Identifier {enumToken.TokenValue} is not an enum", enumToken);

            if (!enumDefinition.TryGetMember(memberToken.TokenValue, out var enumValue))
                return (EvalResult.Failed, $"Enum {enumToken.TokenValue} does not have a member named {memberToken.TokenValue}", memberToken);

            input.OperationStack.Push(new VariableBotToken(BotTokenType.Literal, enumValue.StringValue, enumValue));

            return Success();
        }

        /// <summary>
        /// Enum names and the 'enum' keyword are treated as type specifiers when they are the right-hand operand of the 'is' operator.
        /// </summary>
        private static (EvalResult, string, BotToken) EvaluateEnumTypeSpecifier(ProgramState input)
        {
            var current = input.Current();
            if (input.OperationStack.Count == 0 || !input.OperationStack.Peek().Is(BotTokenType.IsOperator))
                return (EvalResult.NotMatch, string.Empty, current);

            string enumName;
            if (current.Is(BotTokenType.Keyword, BotTokenParser.KEYWORD_ENUM))
            {
                enumName = null;
            }
            else if (current.Is(BotTokenType.Identifier)
                && input.SymbolTable.TryGetValue(current.TokenValue, out var symbol)
                && symbol.Identifiable is EnumDefinition enumDefinition)
            {
                enumName = enumDefinition.Name;
            }
            else
            {
                return (EvalResult.NotMatch, string.Empty, current);
            }

            input.SkipToken();
            input.OperationStack.Push(new VariableBotToken(BotTokenType.TypeSpecifier, current.TokenValue, EnumVariable.TypeSpecifier(enumName)));

            return Success();
        }
    }
}
