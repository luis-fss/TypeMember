using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using TypeMember.Util;
using UnitTests.StubEntities;
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
        public void should_create_a_object_using_activator()
        {
            var foo = ObjectFactory<Foo>.Create(p => p.Name = "Foo", true);
            foo.Name.Should().Be("Foo");
        }

        [Test]
        public void should_hydrate_a_simple_object()
        {
            var foo = ObjectFactory<Foo>.Hydrate();
            foo.Bar.Should().NotBeNull();
        }

        [Test]
        public void should_hydrate_a_complex_object()
        {
            var person = ObjectFactory<Person>.Hydrate();
            person.MyClassProp.Should().NotBeNull();
            person.MyClassProp.Bee.Should().NotBeNull();
            person.MyClassProp.Stub.Bee.Should().NotBeNull();
            person.MyClassProp.Stub.Bees.Should().NotBeNull();
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