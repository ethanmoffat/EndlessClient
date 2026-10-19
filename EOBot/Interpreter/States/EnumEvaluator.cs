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

            var (result, reason, token) = EvaluateEnumMember(input, out var typeHint);
            if (result == EvalResult.NotMatch)
                (result, reason, token) = EvaluateEnumTypeSpecifier(input, typeHint);

            return Task.FromResult((result, reason, token));
        }

        private static (EvalResult, string, BotToken) EvaluateEnumMember(ProgramState input, out EnumDefinition typeHint)
        {
            typeHint = null;

            var name = input.PeekQualifiedName();
            var nextIndex = input.ExecutionIndex + name.TokenLength;

            if (name.DanglingToken is { } danglingToken)
                return Error(danglingToken, BotTokenType.Identifier);

            if (!name.IsEmpty)
            {
                // a name that resolves to an enum (e.g. 'Outer::Inner') is a type specifier, not a member
                // NotMatch result here allows EvaluateAsync to fall through to type specifier evaluation
                var (result, _, _) = input.SymbolTable.TryResolveQualifiedName(name, out var nameSymbol);
                if (result == EvalResult.Ok && nameSymbol is EnumDefinition enumType)
                {
                    typeHint = enumType;
                    return (EvalResult.NotMatch, string.Empty, input.Current());
                }
            }

            if (!name.IsQualified)
                return (EvalResult.NotMatch, string.Empty, input.Current());

            var enumName = name.Qualifier;
            if (input.SymbolTable.TryResolveQualifiedName(enumName, out var symbol) is { Result: not EvalResult.Ok } res)
                return res;

            if (symbol is ModuleDefinition)
            {
                // the name is a module member (e.g. 'Outer::Nested'), so report on the full name rather than the module
                if (input.SymbolTable.TryResolveQualifiedName(name, out _) is { Result: not EvalResult.Ok } nameRes)
                    return nameRes;

                return (EvalResult.Failed, $"Identifier {name} is not an enum", name.Identifier);
            }

            if (symbol is not EnumDefinition enumDefinition)
                return (EvalResult.Failed, $"Identifier {enumName} is not an enum", enumName.Identifier);

            var memberToken = name.Identifier;
            if (!enumDefinition.TryGetMember(memberToken.TokenValue, out var enumValue))
                return (EvalResult.Failed, $"Enum {enumName} does not have a member named {memberToken.TokenValue}", memberToken);

            input.Goto(nextIndex);
            input.OperationStack.Push(VariableBotToken.FromComputedValue(BotTokenType.Literal, enumValue));

            return Success();
        }

        /// <summary>
        /// Enum names and the 'enum' keyword are treated as type specifiers when they are the right-hand operand of the 'is' operator.
        /// The type hint is the enum the name resolved to, or null for the 'enum' keyword.
        /// </summary>
        private static (EvalResult, string, BotToken) EvaluateEnumTypeSpecifier(ProgramState input, EnumDefinition typeHint)
        {
            var current = input.Current();
            if (input.OperationStack.Count == 0 || !input.OperationStack.Peek().Is(BotTokenType.IsOperator))
                return (EvalResult.NotMatch, string.Empty, current);

            string typeName;
            if (current.Is(BotTokenType.Keyword, Keywords.Enum))
            {
                typeName = current.TokenValue;
                input.SkipToken();
            }
            else if (typeHint != null)
            {
                var name = input.PeekQualifiedName();
                typeName = name;
                input.Goto(input.ExecutionIndex + name.TokenLength);
            }
            else
            {
                return (EvalResult.NotMatch, string.Empty, current);
            }

            input.OperationStack.Push(VariableBotToken.Create(BotTokenType.TypeSpecifier, typeName, EnumVariable.TypeSpecifier(typeHint)));

            return Success();
        }
    }
}
