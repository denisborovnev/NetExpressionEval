using System.Collections.Generic;
using System.Linq.Expressions;

namespace ExpressionEval
{
    class ClosureArgsVisitor : ExpressionVisitor
    {
        public List<object> ArgValues;

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if(node.Object != null && node.Object.NodeType == ExpressionType.Constant)
            {
                AddArgValue(node.Object);
            }
            return base.VisitMethodCall(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if(node.Expression != null && node.Expression.NodeType == ExpressionType.Constant)
            {
                AddArgValue(node.Expression);
            }
            return base.VisitMember(node);
        }

        private void AddArgValue(Expression ex)
        {
            if (ArgValues == null)
            {
                ArgValues = new List<object>();
            }

            ArgValues.Add(((ConstantExpression)ex).Value);
        }
    }
}