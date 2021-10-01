using System.Linq;
using NUnit.Framework;
using TypeMember;
using UnitTests.StubEntities.Blog;

namespace UnitTests
{
    public class PropertyPathTests : TestBase
    {
        [Test]
        public void GetMemberName_WhenSimpleProperty_ReturnsPropertyName()
        {
            var blog = new Blog();

            var propertyName = blog.GetMemberName(b => b.Name);

            Assert.That(propertyName, Is.EqualTo("Name"));
        }

        [Test]
        public void GetMemberNames_WhenMoreThanOneProperty_ReturnsListOfPropertyNames()
        {
            var blog = new Blog();

            var propertyNames = blog.GetMemberNames(b => b.Name, b => b.CreatedOn, b => b.Admin);

            Assert.That(propertyNames.Count, Is.EqualTo(3));
            Assert.That(propertyNames[0], Is.EqualTo("Name"));
            Assert.That(propertyNames[1], Is.EqualTo("CreatedOn"));
            Assert.That(propertyNames[2], Is.EqualTo("Admin"));
        }

        [Test]
        public void GetMemberName_WhenMethod_ReturnsMethodName()
        {
            var blog = new Blog();

            var nameOfTheMethod = blog.GetMemberName(b => b.ToString());

            Assert.That(nameOfTheMethod, Is.EqualTo("ToString"));
        }

        [Test]
        public void GetPropertyPath_WhenSecondLevelProperty_ReturnsPropertyName()
        {
            var blog = new Blog();

            var propertyName = blog.GetPropertyPath(b => b.Admin.Name);

            Assert.That(propertyName, Is.EqualTo("Admin.Name"));
        }

        [Test]
        public void GetPropertyPath_ThirdSecondLevelProperty_ReturnsPropertyName()
        {
            var blog = new Blog();

            var propertyName = blog.GetPropertyPath(b => b.Admin.Address.State);

            Assert.That(propertyName, Is.EqualTo("Admin.Address.State"));
        }

        [Test]
        public void GetMemberName_WhenCollectionProperty_ReturnsPropertyName()
        {
            var blog = new Blog();

            var propertyName = blog.GetMemberName(b => b.Posts);

            Assert.That(propertyName, Is.EqualTo("Posts"));
        }

        [Test]
        public void GetPropertyPath_WhenSecondLevelCollectionProperty_ReturnsPropertyName()
        {
            var propertyName = Reflector.Property.GetPropertyPath<Blog>(b => b.Posts.Select(p => p.Author));

            Assert.That(propertyName, Is.EqualTo("Posts.Author"));
        }

        [Test]
        public void GetPropertyPath_WhenSecondLevelCollectionProperty_WithCollectionSuffix_ReturnsPropertyName()
        {
            var propertyName = Reflector.Property.GetPropertyPath<Blog>(b => b.Posts.Select(p => p.Author), "[]");

            Assert.That(propertyName, Is.EqualTo("Posts[].Author"));
        }

        [Test]
        public void GetPropertyPath_WhenUndefinedLevelCollectionProperty_ReturnsPropertyName()
        {
            var propertyName = Reflector.Property.GetPropertyPath<Blog>(b => b.Posts.Select(p => p.Comments.Select(c => c.Member.UserName)));

            Assert.That(propertyName, Is.EqualTo("Posts.Comments.Member.UserName"));
        }

        [Test]
        public void GetPropertyPath_WhenUndefinedLevelCollectionProperty_WithCollectionSuffix_ReturnsPropertyName()
        {
            var propertyName = Reflector.Property.GetPropertyPath<Blog>(b => b.Posts.Select(p => p.Comments.Select(c => c.Member.UserName)), "[]");

            Assert.That(propertyName, Is.EqualTo("Posts[].Comments[].Member.UserName"));
        }
    }
}