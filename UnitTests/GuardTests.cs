using System;
using NUnit.Framework;
using TypeMember.Exceptions;
using TypeMember.Guard;
using UnitTests.StubEntities;

namespace UnitTests
{
    public class GuardTests : TestBase
    {
        [Test]
        public void should_throws_argument_null_exception_for_a_null_argument()
        {
            object obj = null;
            Assert.Throws<ArgumentNullException>(() => Guard.IsNotNull(() => obj));
        }

        [Test]
        public void should_not_throws_any_exception_for_a_valid_argument()
        {
            object obj = "value";
            Guard.IsNotNull(() => obj);
        }

        [Test]
        public void if_true_should_throws_an_exception_without_parameters()
        {
            Assert.Throws<Exception>(() => Guard.If(true).Throw<Exception>());
        }

        [Test]
        public void if_null_should_throws_an_exception_without_parameters()
        {
            Assert.Throws<Exception>(() => Guard.IfIsNull(null).Throw<Exception>());
        }

        [Test]
        public void if_true_should_throws_an_exception_with_parameters()
        {
            Assert.Throws<PropertyNotFoundException>(() => Guard.If(true).Throw<PropertyNotFoundException>(typeof(Foo), "PropName"));
        }

        [Test]
        public void if_null_should_throws_an_exception_with_parameters()
        {
            Assert.Throws<PropertyNotFoundException>(() => Guard.IfIsNull(null).Throw<PropertyNotFoundException>(typeof(Foo), "PropName"));
        }
    }
}