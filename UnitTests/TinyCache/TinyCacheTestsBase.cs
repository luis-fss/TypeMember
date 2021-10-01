using System;
using FluentAssertions;
using NUnit.Framework;
using TypeMember.TinyCache;
using TypeMember.TinyCache.Exceptions;
using UnitTests.StubEntities;

namespace UnitTests.TinyCache
{
    public abstract class TinyCacheTestsBase
    {
        protected abstract ITinyCache<string> GetCache();
            
        [Test]
        public void should_set_one_item()
        {
            const string key = "person";
            var person = new Person("Luis Fernando", 25);
            var cache = GetCache();
            cache.SetItem(key, person);
            cache.Items.Should().HaveCount(1);
            cache.GetItem<Person>(key).Should().Be(person);
        }

        [Test]
        public void should_override_duplicates()
        {
            const string key = "person";
            var person = new Person("Luis Fernando", 25);
            var cache = GetCache();
            cache.SetItem(key, person);
            cache.SetItem(key, person);
            cache.Items.Should().HaveCount(1);
            cache.GetItem<Person>(key).Should().Be(person);
        }

        [Test]
        public void should_get_or_set_one_item()
        {
            const string key = "person";
            var person = new Person("Luis Fernando", 25);
            var cache = GetCache();
            cache.GetOrSetItem(key, () => person).Should().Be(person);
            cache.Items.Should().HaveCount(1);
            cache.GetItem<Person>(key).Should().Be(person);
        }

        [Test]
        public void should_remove_one_item()
        {
            const string key = "person";
            var person = new Person("Luis Fernando", 25);
            var cache = GetCache();
            cache.SetItem(key, person);
            cache.RemoveItem(key);
            cache.Items.Should().HaveCount(0);
            Action action = () => cache.GetItem<Person>(key);
            action.Should().Throw<ItemNotInCacheException>();
        }

        [Test]
        public void should_throw_error_on_invalid_type()
        {
            const string key = "person";
            var person = new Person("Luis Fernando", 25);
            var cache = GetCache();
            cache.SetItem(key, person);
            cache.Items.Should().HaveCount(1);
            Action action = () => cache.GetItem<string>(key);
            action.Should().Throw<ItemTypeIncorrectException>();
        }
    }
}