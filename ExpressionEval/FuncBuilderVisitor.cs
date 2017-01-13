using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ExpressionEval
{
    class FuncBuilderVisitor : ExpressionVisitor
    {
        public List<ParameterExpression> Args { get; set; } = new List<ParameterExpression>();

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if(node.Object != null && node.Object.NodeType == ExpressionType.Constant)
            {
                var arg = CreateArgument(node.Method.DeclaringType);
                var arguments = node.Arguments.Select(Visit).ToArray();
                return Expression.Call(arg, node.Method, arguments);
            }

            return base.VisitMethodCall(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if(node.Expression.NodeType == ExpressionType.Constant)
            {
                var field = node.Member;
                var arg = CreateArgument(field.DeclaringType);
                return Expression.MakeMemberAccess(arg, field);
            }

            var expression = this.Visit(node.Expression);
            return Expression.MakeMemberAccess(expression, node.Member);
        }

        private Expression CreateArgument(Type argType)
        {
            var arg = Expression.Parameter(typeof(object), $"a{Args.Count}");
            Args.Add(arg);
            return Expression.Convert(arg, argType);
        }
    }
}