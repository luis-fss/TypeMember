using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using TypeMember.Util;
using UnitTests.StubEntities;
using UnitTests.Util;
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable CollectionNeverUpdated.Local
// ReSharper disable ClassNeverInstantiated.Local

namespace UnitTests
{
    public class ObjectFactoryTests : TestBase
    {
        [Test]
        public void should_create_a_object_using_lambdas()
        {
            var foo = ObjectFactory<Foo>.Create(p => p.Name = "Foo");
            foo.Name.Should().Be("Foo");
        }

        [Test]
        public void performance_test_using_lambdas()
        {
            var it = HiResTimer.Start(10);
            for (var c = 0; c < it; c++)
            {
                ObjectFactory<Foo>.Create(p => p.Name = "Foo");
            }
            HiResTimer.Stop();
        }

        [Test]
        public void should_create_a_object_using_activator()
        {
            var foo = ObjectFactory<Foo>.Create(p => p.Name = "Foo", true);
            foo.Name.Should().Be("Foo");
        }

        [Test]
        public void performance_test_using_activator()
        {
            var it = HiResTimer.Start(10);
            for (var c = 0; c < it; c++)
            {
                ObjectFactory<Foo>.Create(p => p.Name = "Foo", true);
            }
            HiResTimer.Stop();
        }

        [Test]
        public void should_hydrate_a_simple_object()
        {
            var foo = ObjectFactory.Hydrate<Foo>();
            foo.Bar.Should().NotBeNull();
        }

        [Test]
        public void performance_test_hydrate_a_simple_object()
        {
            var it = HiResTimer.Start(10);
            for (var c = 0; c < it; c++)
            {
                ObjectFactory.Hydrate<Foo>();
            }
            HiResTimer.Stop();
        }

        [Test]
        public void should_hydrate_a_complex_object()
        {
            var person = ObjectFactory.Hydrate<Person>();
            person.MyClassProp.Should().NotBeNull();
            person.MyClassProp.Bee.Should().NotBeNull();
            person.MyClassProp.Stub.Bee.Should().NotBeNull();
            person.MyClassProp.Stub.Bees.Should().NotBeNull();
        }

        [Test]
        public void performance_test_hydrate_a_complex_object()
        {
            var it = HiResTimer.Start(10);
            for (var i = 0; i < it; i++)
            {
                ObjectFactory.Hydrate<Person>();
            }
            HiResTimer.Stop();
        }

        class Person
        {
            public MyClass MyClassProp { get; set; }
            public string Name { get; set; }
        }

        class MyClass
        {
            public string MyProp { get; set; }
            public Bee Bee { get; set; }
            public Stub Stub { get; set; }
        }

        class Stub
        {
            public Bee Bee { get; set; }
            public List<Bee> Bees { get; set; }
        }
    }
}