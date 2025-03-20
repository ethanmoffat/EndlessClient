using System;
using System.Collections;
using System.Collections.Generic;

namespace EOBot.Interpreter.Syntax
{
    public sealed class SyntaxTree : IEnumerable<BotToken>, IEnumerable
    {
        public enum Order
        {
            PreOrder,
            InOrder,
            PostOrder
        }

        private static readonly Dictionary<BotTokenType, int> OperatorPrecedence = new()
        {
            [BotTokenType.Literal] = 9,
            [BotTokenType.Variable] = 9,
            [BotTokenType.TypeSpecifier] = 9,
            [BotTokenType.NotOperator] = 8,
            [BotTokenType.MultiplyOperator] = 6,
            [BotTokenType.DivideOperator] = 6,
            [BotTokenType.ModuloOperator] = 6,
            [BotTokenType.PlusOperator] = 5,
            [BotTokenType.MinusOperator] = 5,
            [BotTokenType.LessThanOperator] = 4,
            [BotTokenType.LessThanEqOperator] = 4,
            [BotTokenType.GreaterThanOperator] = 4,
            [BotTokenType.GreaterThanEqOperator] = 4,
            [BotTokenType.EqualOperator] = 3,
            [BotTokenType.NotEqualOperator] = 3,
            [BotTokenType.StrictEqualOperator] = 3,
            [BotTokenType.StrictNotEqualOperator] = 3,
            [BotTokenType.IsOperator] = 3,
            [BotTokenType.LogicalAndOperator] = 2,
            [BotTokenType.LogicalOrOperator] = 1
        };

        public class Node(BotToken token, Node left, Node right)
        {
            public BotToken Token { get; set; } = token;

            public Node Left { get; set; } = left;

            public Node Right { get; set; } = right;
        }

        public Order VisitOrder { get; set; }

        public Node Root => _root;

        private Node _root;

        public SyntaxTree(Stack<BotToken> tokens)
        {
            while (tokens.Count > 0)
            {
                var next = tokens.Pop();

                if (!OperatorPrecedence.ContainsKey(next.TokenType))
                {
                    tokens.Push(next);
                    break;
                }

                Insert(next);
            }
        }

        private void Insert(BotToken token)
        {
            if (_root == null)
            {
                _root = new Node(token, null, null);
            }
            else
            {
                _root = InsertRecursively(_root, token);
            }
        }

        public IEnumerable<BotToken> Traverse()
        {
            return VisitOrder switch
            {
                Order.PreOrder => PreOrderTraversal(_root),
                Order.InOrder => InOrderTraversal(_root),
                Order.PostOrder => PostOrderTraversal(_root),
                _ => throw new InvalidOperationException("Invalid traversal order")
            };
        }

        private static Node InsertRecursively(Node current, BotToken token)
        {
            if (OperatorPrecedence[token.TokenType] < OperatorPrecedence[current.Token.TokenType])
            {
                return new Node(token, current, null);
            }
            else
            {
                if (current.Right == null)
                {
                    current.Right = new Node(token, null, null);
                    return current;
                }
                else
                {
                    current.Right = InsertRecursively(current.Right, token);
                    return current;
                }
            }
        }

        private static IEnumerable<BotToken> PreOrderTraversal(Node node)
        {
            if (node == null) yield break;

            yield return node.Token;
            foreach (var token in PreOrderTraversal(node.Left))
                yield return token;
            foreach (var token in PreOrderTraversal(node.Right))
                yield return token;
        }

        private static IEnumerable<BotToken> InOrderTraversal(Node node)
        {
            if (node == null) yield break;

            foreach (var token in InOrderTraversal(node.Left))
                yield return token;
            yield return node.Token;
            foreach (var token in InOrderTraversal(node.Right))
                yield return token;
        }

        private static IEnumerable<BotToken> PostOrderTraversal(Node node)
        {
            if (node == null) yield break;

            foreach (var token in PostOrderTraversal(node.Left))
                yield return token;
            foreach (var token in PostOrderTraversal(node.Right))
                yield return token;
            yield return node.Token;
        }

        public IEnumerator<BotToken> GetEnumerator()
        {
            return Traverse().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
