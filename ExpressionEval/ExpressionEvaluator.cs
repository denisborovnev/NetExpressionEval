using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace ExpressionEval
{
    public class ExpressionEvaluator
    {
        private ConcurrentDictionary<string, Func<object>> _funcsCache0 = new ConcurrentDictionary<string, Func<object>>();
        private ConcurrentDictionary<string, Func<object, object>> _funcsCache1 = new ConcurrentDictionary<string, Func<object, object>>();
        private ConcurrentDictionary<string, Func<object, object, object>> _funcsCache2 = new ConcurrentDictionary<string, Func<object, object, object>>();
        private ConcurrentDictionary<string, Func<object, object, object, object>> _funcsCache3 = new ConcurrentDictionary<string, Func<object, object, object, object>>();

        public object Eval(Expression expression)
        {
            if(expression.NodeType == ExpressionType.Constant)
            {
                return ((ConstantExpression)expression).Value;
            }

            var argsVisitor = new ClosureArgsVisitor();
            argsVisitor.Visit(expression);
            
            if (argsVisitor.ArgValues == null)
            {
                return Invoke_0(expression);
            }

            if (argsVisitor.ArgValues.Count == 1)
            {
                return Invoke_1(expression, argsVisitor);
            }

            if (argsVisitor.ArgValues.Count == 2)
            {
                return Invoke_2(expression, argsVisitor);
            }

            if (argsVisitor.ArgValues.Count == 3)
            {
                return Invoke_3(expression, argsVisitor);
            }

            return Expression.Lambda(expression).Compile().DynamicInvoke();
        }
        
        private object Invoke_0(Expression expression)
        {
            var func = GetOrAddFunc(_funcsCache0, expression);
            return func();
        }

        private object Invoke_1(Expression expression, ClosureArgsVisitor infoVisitor)
        {
            var func = GetOrAddFunc(_funcsCache1, expression);
            return func(infoVisitor.ArgValues[0]);
        }

        private object Invoke_2(Expression expression, ClosureArgsVisitor infoVisitor)
        {
            var func = GetOrAddFunc(_funcsCache2, expression);
            return func(infoVisitor.ArgValues[0], infoVisitor.ArgValues[1]);
        }

        private object Invoke_3(Expression expression, ClosureArgsVisitor infoVisitor)
        {
            var func = GetOrAddFunc(_funcsCache3, expression);
            return func(infoVisitor.ArgValues[0], infoVisitor.ArgValues[1], infoVisitor.ArgValues[2]);
        }

        private T GetOrAddFunc<T>(ConcurrentDictionary<string, T> cache, Expression expression)
        {
            var key = expression.ToString();
            return cache.GetOrAdd(key, (k) =>
            {
                var funcBuilderVisitor = new FuncBuilderVisitor();
                var body = funcBuilderVisitor.Visit(expression);
                body = Expression.Convert(body, typeof(object));
                return Expression.Lambda<T>(body, funcBuilderVisitor.Args.ToArray()).Compile();
            });
        }
    }
}