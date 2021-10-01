using System;
using System.Collections.Concurrent;
using FluentAssertions;
using NUnit.Framework;
using TypeMember.TinyCache;
using UnitTests.StubEntities;

namespace UnitTests.TinyCache
{
    public class TinyCacheTests : TinyCacheTestsBase
    {
        protected override ITinyCache<string> GetCache()
        {
            return new TinyCache<string>();
        }

        [Test]
        public void test_with_type_pair()
        {
            var cache = GetCache().GetOrSetItem("cacheKey-{deead815-2427-4250-99ee-f687067dd01b}",
                () => new ConcurrentDictionary<Type, string>());

            var item = cache.GetOrAdd(typeof(Foo), typeof(Foo).FullName);

            cache.Should().HaveCount(1);
            cache[typeof(Foo)].Should().Be(item);

            for (var i = 0; i < 10; i++)
            {
                item = cache.GetOrAdd(typeof(Foo), typeof(Foo).FullName);

                cache.Should().HaveCount(1);
                cache[typeof(Foo)].Should().Be(item);
            }
        }
    }
}
