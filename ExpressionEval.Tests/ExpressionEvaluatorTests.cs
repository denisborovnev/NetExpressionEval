using System;
using System.Diagnostics;
using System.Linq.Expressions;
using NUnit.Framework;

namespace ExpressionEval.Tests
{
    [TestFixture]
    public class ExpressionEvaluatorTests
    {
        private static ExpressionEvaluator _evaluator = new ExpressionEvaluator();

        [Test]
        public void Should_work_with_literals()
        {
            var actual = EvalRightPart<int>(x => x == 5);

            Assert.AreEqual(5, actual);
        }

        [Test]
        public void Should_work_with_inverted_literals()
        {
            var actual = EvalRightPart<bool>(x => x == !true);

            Assert.AreEqual(false, actual);
        }

        [Test]
        public void Should_work_with_value_type_closure()
        {
            var val = 5;

            var actual = EvalRightPart<int>(x => x == val);

            Assert.AreEqual(5, actual);
        }

        [Test]
        public void Should_work_with_not_operator_on_boolean_variable_closure()
        {
            var val = false;

            var actual = EvalRightPart<bool>(x => x == !val);

            Assert.AreEqual(true, actual);
        }

        [Test]
        public void Should_work_with_math_operations_on_value_types()
        {
            var val1 = 5;
            var val2 = 10;

            var actual = EvalRightPart<int>(x => x == (val1 + val2));

            Assert.AreEqual(15, actual);
        }

        [Test]
        public void Should_work_with_property_of_ref_type()
        {
            var instance = new Outer { Val1 = 25 };

            var actual = EvalRightPart<int>(x => x == instance.Val1);

            Assert.AreEqual(25, actual);
        }

        [Test]
        public void Should_work_with_property_chains()
        {
            var instance = new Outer
            {
                Inner = new Inner { Val2 = 5 }
            };

            var actual = EvalRightPart<int>(x => x == instance.Inner.Val2);

            Assert.AreEqual(5, actual);
        }

        [Test]
        public void Should_work_when_bool_property_is_inverted()
        {
            var outer = new Outer { Flag = true };

            var actual = EvalRightPart<bool>(x => x == !outer.Flag);

            Assert.AreEqual(false, actual);
        }

        [Test]
        public void Should_work_with_math_operation_for_property_values()
        {
            var outer = new Outer { Val1 = 3 };
            var inner = new Inner { Val2 = 2 };

            var actual = EvalRightPart<int>(x => x == (outer.Val1 + inner.Val2));

            Assert.AreEqual(5, actual);
        }

        [Test]
        public void Should_work_with_method_calls()
        {
            var outer = new Outer { Val1 = 3 };

            var actual = EvalRightPart<int>(x => x == outer.GetVal1());

            Assert.AreEqual(3, actual);
        }

        [Test]
        public void Should_work_with_method_calls_of_inner_objects()
        {
            var outer = new Outer
            {
                Inner = new Inner { Val2 = 4 }
            };

            var actual = EvalRightPart<int>(x => x == outer.Inner.GetVal2());

            Assert.AreEqual(4, actual);
        }

        [Test]
        public void Should_use_new_value_if_closure_value_is_updated()
        {
            int val = 5;

            Func<object> eval = () =>
            {
                return EvalRightPart<int>(x => x == val);
            };

            Assert.AreEqual(5, eval());

            val = 6;
            Assert.AreEqual(6, eval());
        }

        [Test]
        public void Should_work_with_current_instance_method_call()
        {
            _instanceValue = 5;
            var actual = EvalRightPart<int>(x => x == GetInstanceValue());

            Assert.AreEqual(5, actual);
        }

        [Test]
        public void Should_work_with_current_instance_method_called_with_variable_argument()
        {
            var val = 5;
            var actual = EvalRightPart<int>(x => x == Increment(val));

            Assert.AreEqual(6, actual);
        }

        [Test]
        public void Should_work_with_method_chain_calls()
        {
            _predefinedOuter = new Outer { Val1 = 30 };
            var actual = EvalRightPart<int>(x => x == GetPredefinedOuter().GetVal1());

            Assert.AreEqual(30, actual);
        }

        [Test]
        public void Should_work_with_static_methods()
        {
            var actual = EvalRightPart<int>(x => x == StaticGet5());

            Assert.AreEqual(5, actual);
        }

        [Test]
        public void Should_work_with_static_properties()
        {
            var actual = EvalRightPart<int>(x => x == StaticPropertyWith5Val);

            Assert.AreEqual(5, actual);
        }

        [Test]
        public void Should_work_with_instance_properties()
        {
            InstanceProperty = 30;

            var actual = EvalRightPart<int>(x => x == InstanceProperty);

            Assert.AreEqual(30, actual);
        }

        [Test]
        public void Show_work_with_5_dependencies()
        {
            var x1 = 1;
            var x2 = 2;
            var x3 = 3;
            var x4 = 4;
            var x5 = 5;

            var actual = EvalRightPart<int>(x => x == (x1 + x2 + x3 + x4 + x5));

            Assert.AreEqual(15, actual);
        }

        [Test]
        public void Show_throw_exception_if_there_is_dependency_on_expression_parameter()
        {
            var i = 5;

            Assert.Throws<InvalidOperationException>(() => EvalRightPart<int>(x => 10 == (x + i)));
        }

        [Test]
        [Ignore("")]
        public void Benchmark_for_property()
        {
            var instance = new Outer { Val1 = 5 };
            //warm up
            var result = EvalRightPart<int>(x => x == instance.Val1);

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 100000; i++)
            {
                instance.Val1 = i;
                EvalRightPart<int>(x => x == instance.Val1);
            }

            sw.Stop();
            Console.WriteLine($"Cached takes {sw.ElapsedMilliseconds}ms");

            sw = Stopwatch.StartNew();
            for (int i = 0; i < 100000; i++)
            {
                instance.Val1 = i;
                DynamicEval<int>(x => x == instance.Val1);
            }

            sw.Stop();
            Console.WriteLine($"Recompile takes {sw.ElapsedMilliseconds}ms");
        }

        [Test]
        [Ignore("")]
        public void Benchmark_for_properties_chain()
        {
            var instance = new Outer
            {
                Inner = new Inner { Val2 = 1 }
            };
            //warm up
            var result = EvalRightPart<int>(x => x == instance.Inner.Val2);

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 100000; i++)
            {
                instance.Inner.Val2 = i;
                EvalRightPart<int>(x => x == instance.Inner.Val2);
            }

            sw.Stop();
            Console.WriteLine($"Cached takes {sw.ElapsedMilliseconds}ms");

            sw = Stopwatch.StartNew();
            for (int i = 0; i < 100000; i++)
            {
                instance.Inner.Val2 = i;
                DynamicEval<int>(x => x == instance.Inner.Val2);
            }

            sw.Stop();
            Console.WriteLine($"Recompile takes {sw.ElapsedMilliseconds}ms");
        }

        private int InstanceProperty { get; set; }

        private int _instanceValue;
        private int GetInstanceValue()
        {
            return _instanceValue;
        }

        private Outer _predefinedOuter;
        private Outer GetPredefinedOuter()
        {
            return _predefinedOuter;
        }

        private static int StaticGet5()
        {
            return 5;
        }

        private static int StaticPropertyWith5Val { get { return 5; } }

        private int Increment(int val)
        {
            return val + 1;
        }

        public class Outer
        {
            public int Val1 { get; set; }

            public bool Flag { get; set; }

            public Inner Inner { get; set; }

            public int GetVal1()
            {
                return Val1;
            }
        }

        public class Inner
        {
            public int Val2 { get; set; }

            public int GetVal2()
            {
                return Val2;
            }
        }

        protected object EvalRightPart<TInput>(Expression<Func<TInput, bool>> expression)
        {
            var body = (BinaryExpression)expression.Body;
            return _evaluator.Eval(body.Right);
        }

        private object DynamicEval<TInput>(Expression<Func<TInput, bool>> expression)
        {
            var body = (BinaryExpression)expression.Body;
            return Expression.Lambda(body.Right).Compile().DynamicInvoke();
        }
    }
}