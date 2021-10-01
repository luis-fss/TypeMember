using System;
using NUnit.Framework;
using TypeMember.Internal;

namespace UnitTests
{
    public class GuardTests : TestBase
    {
        [Test]
        public void should_throws_argument_null_exception_for_a_null_argument()
        {
            object obj = null;
            Assert.Throws<ArgumentNullException>(() => Guard.ShouldNotBeNull(() => obj), "The parameter 'obj' cannot be null.");
        }

        [Test]
        public void should_not_throws_any_exception_for_a_valid_argument()
        {
            object obj = "value";
            Guard.ShouldNotBeNull(() => obj);
        }
    }
}