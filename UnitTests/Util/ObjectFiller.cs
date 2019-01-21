using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;

namespace UnitTests.Util
{
    public class ObjectFillerTests
    {
        [Test]
        public void sould_fill_a_simple_object()
        {
            var of = new ObjectFiller<MyClass>();
            var values = new Dictionary<string, object>
                             {
                                 { "Prop1", "value1" }, 
                                 { "Prop2", "value2" },
                             };
            var myClass = of.FillObject(values);
            myClass.Prop1.Should().Be("value1");
            myClass.Prop2.Should().Be("value2");
        }

        [Test]
        public void sould_fill_a_complex_object()
        {
            var of = new ObjectFiller<MyClass>();
            var values = new Dictionary<string, object>
                             {
                                 { "Prop1", "value1" }, 
                                 { "Prop2", "value2" },
                                 //{ "MyInnerClass", new MyInnerClass() },
                                 { "MyInnerClass.Prop1", "MyInnerClass.value1" },
                                 { "MyInnerClass.Prop2", "MyInnerClass.value2" },
                             };
            var myClass = of.FillObject(values);
            myClass.Prop1.Should().Be("value1");
            myClass.Prop2.Should().Be("value2");
            //myClass.MyInnerClass.Prop1.Should().Be("MyInnerClass.value1");
            //myClass.MyInnerClass.Prop2.Should().Be("MyInnerClass.value2");
        }

        [Test]
        public void performance_test()
        {
            var of = new ObjectFiller<MyClass>();
            var it = HiResTimer.Start(10);
            for (var i = 0; i < it; i++)
            {
                var values = new Dictionary<string, object>
                                 {
                                     {"Prop1", "value1"},
                                     {"Prop2", "value2"},
                                 };
                of.FillObject(values);
            }
            HiResTimer.Stop();
        }
    }

    class MyClass
    {
        public string Prop1 { get; set; }
        public string Prop2 { get; set; }
        public MyInnerClass MyInnerClass { get; set; }
    }

    class MyInnerClass
    {
        public string Prop1 { get; set; }
        public string Prop2 { get; set; }
    }

    public class ObjectFiller<T> where T : class
    {
        private static Func<IDictionary<string, object>, T> _fillerDelegate;

        private static void Init()
        {
            var obj = Expression.Parameter(typeof(T), "obj");
            var valuesDictionary = Expression.Parameter(typeof(IDictionary<string, object>), "values");

            var create = Expression.Assign(
                obj, Expression.Call(typeof(Activator), "CreateInstance", new[] { typeof(T) }));

            var properties = typeof(T).GetProperties();

            var setters = Expression.Block(properties.Select(p => CreateSetter(p, obj, valuesDictionary)));

            var methodBody = Expression.Block(typeof(T), new[] { obj }, create, setters, obj);

            var fillerExpression = Expression.Lambda<Func<IDictionary<string, object>, T>>(methodBody, valuesDictionary);

            _fillerDelegate = fillerExpression.Compile();
        }

        static Expression CreateSetter(PropertyInfo property, Expression obj, Expression valuesDictionary)
        {
            var indexer = Expression.MakeIndex(
                valuesDictionary,
                typeof(IDictionary<string, object>).GetProperty("Item", new[] { typeof(string) }),
                new[] { Expression.Constant(property.Name) });

            var setter = Expression.Assign(
                Expression.Property(obj, property),
                Expression.Convert(indexer, property.PropertyType));

            var valuesContainsProperty = Expression.Call(
                valuesDictionary, "ContainsKey", null, Expression.Constant(property.Name));

            var condition = Expression.IfThen(valuesContainsProperty, setter);

            return condition;
        }

        public T FillObject(IDictionary<string, object> values)
        {
            if (_fillerDelegate == null)
                Init();

            return _fillerDelegate != null ? _fillerDelegate(values) : null;
        }
    }
}