using System;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using TypeMember.TinyCache;
using TypeMember.TinyCache.Exceptions;
using UnitTests.StubEntities;

namespace UnitTests.TinyCache
{
    public class TimedTinyCacheTests : TinyCacheTestsBase
    {
        protected override ITinyCache<string> GetCache()
        {
            return new TimedTinyCache<string>(50, 10);
        }

        [Test]
        public void should_remove_an_item_that_has_expired()
        {
            const string key = "person";
            var person = new Person("Luis Fernando", 25);
            var cache = GetCache();
            cache.SetItem(key, person);
            cache.Items.Should().HaveCount(1);
            Thread.Sleep(60);
            cache.Items.Should().HaveCount(0);
            Action action = () => cache.GetItem<Person>(key);
            action.Should().Throw<ItemNotInCacheException>();
        }
    }
}