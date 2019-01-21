using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FluentAssertions;
using NUnit.Framework;
using TypeMember.Util;
using UnitTests.StubEntities;
using UnitTests.StubEntities.Blog;

namespace UnitTests
{
    public class PropertyPathVisitorTests : TestBase
    {
        [Test]
        public void simple_expression()
        {
            Expression<Func<Blog, object>> expression = x => x.Admin;
            var propertyPathVisitor = new PropertyPathVisitor();
            propertyPathVisitor.Visit(expression);
            propertyPathVisitor.Properties.Should().HaveCount(1);
            propertyPathVisitor.Properties.ElementAt(0).Should().Be("Admin");
        }

        [Test]
        public void simple_expression_with_anonymous_types()
        {
            Expression<Func<Blog, object>> expression = x => new { x.Admin };
            var propertyPathVisitor = new PropertyPathVisitor();
            propertyPathVisitor.Visit(expression);
            propertyPathVisitor.Properties.Should().HaveCount(1);
            propertyPathVisitor.Properties.ElementAt(0).Should().Be("Admin");
        }

        [Test]
        public void two_level_in_a_hierarchy()
        {
            Expression<Func<Blog, object>> expression = x => x.Admin.Address;
            var propertyPathVisitor = new PropertyPathVisitor();
            propertyPathVisitor.Visit(expression);
            propertyPathVisitor.Properties.Should().HaveCount(1);
            propertyPathVisitor.Properties.ElementAt(0).Should().Be("Admin.Address");
        }

        [Test]
        public void two_level_in_a_hierarchy_with_anonymous_types()
        {
            Expression<Func<Blog, object>> expression = x => new { x.Admin.Address };
            var propertyPathVisitor = new PropertyPathVisitor();
            propertyPathVisitor.Visit(expression);
            propertyPathVisitor.Properties.Should().HaveCount(1);
            propertyPathVisitor.Properties.ElementAt(0).Should().Be("Admin.Address");
        }

        [Test]
        public void three_level_in_a_hierarchy()
        {
            Expression<Func<Blog, object>> expression = x => x.Admin.Address.State;
            var propertyPathVisitor = new PropertyPathVisitor();
            propertyPathVisitor.Visit(expression);
            propertyPathVisitor.Properties.Should().HaveCount(1);
            propertyPathVisitor.Properties.ElementAt(0).Should().Be("Admin.Address.State");
        }

        [Test]
        public void three_level_in_a_hierarchy_with_anonymous_types()
        {
            Expression<Func<Blog, object>> expression = x => new { x.Admin.Address.State };
            var propertyPathVisitor = new PropertyPathVisitor();
            propertyPathVisitor.Visit(expression);
            propertyPathVisitor.Properties.Should().HaveCount(1);
            propertyPathVisitor.Properties.ElementAt(0).Should().Be("Admin.Address.State");
        }

        [Test]
        public void using_the_select_method_for_collections()
        {
            Expression<Func<Blog, object>> expression = x => x.Posts.Select(p => p.Comments);
            var propertyPathVisitor = new PropertyPathVisitor();
            propertyPathVisitor.Visit(expression);
            propertyPathVisitor.Properties.Should().HaveCount(1);
            propertyPathVisitor.Properties.ElementAt(0).Should().Be("Posts.Comments");
        }

        [Test]
        public void using_the_select_method_for_collections_with_anonymous_types()
        {
            Expression<Func<Blog, object>> expression = x => new { Prop = x.Posts.Select(p => p.Comments) };
            var propertyPathVisitor = new PropertyPathVisitor();
            propertyPathVisitor.Visit(expression);
            propertyPathVisitor.Properties.Should().HaveCount(1);
            propertyPathVisitor.Properties.ElementAt(0).Should().Be("Posts.Comments");
        }

        [Test]
        public void using_the_select_method_for_sub_collections()
        {
            Expression<Func<Blog, object>> expression = x => x.Posts.SelectMany(p => p.Comments).Select(comment => comment.Member);
            var propertyPathVisitor = new PropertyPathVisitor();
            propertyPathVisitor.Visit(expression);
            propertyPathVisitor.Properties.Should().HaveCount(1);
            propertyPathVisitor.Properties.ElementAt(0).Should().Be("Posts.Comments.Member");
        }

        [Test]
        public void using_the_select_method_for_sub_collections_with_anonymous_types()
        {
            Expression<Func<Blog, object>> expression = x => new { Prop = x.Posts.SelectMany(p => p.Comments).Select(comment => comment.Member) };
            var propertyPathVisitor = new PropertyPathVisitor();
            propertyPathVisitor.Visit(expression);
            propertyPathVisitor.Properties.Should().HaveCount(1);
            propertyPathVisitor.Properties.ElementAt(0).Should().Be("Posts.Comments.Member");
        }

        [Test]
        public void should_support_multi_properties()
        {
            Expression<Func<Blog, object>> expression = x => x.Name != x.Admin.Name;
            var propertyPathVisitor = new PropertyPathVisitor();
            propertyPathVisitor.Visit(expression);
            propertyPathVisitor.Properties.Should().HaveCount(2);
            propertyPathVisitor.Properties.ElementAt(0).Should().Be("Name");
            propertyPathVisitor.Properties.ElementAt(1).Should().Be("Admin.Name");
        }

        [Test]
        public void should_support_multi_properties_with_anonymous_types()
        {
            Expression<Func<Blog, object>> expression = x => new { Prop = x.Name != x.Admin.Name };
            var propertyPathVisitor = new PropertyPathVisitor();
            propertyPathVisitor.Visit(expression);
            propertyPathVisitor.Properties.Should().HaveCount(2);
            propertyPathVisitor.Properties.ElementAt(0).Should().Be("Name");
            propertyPathVisitor.Properties.ElementAt(1).Should().Be("Admin.Name");
        }

        [Test]
        public void should_support_multi_properties_with_constants()
        {
            Expression<Func<Blog, object>> expression = x => "Name: " + x.Name + " " + x.Admin.Name;
            var propertyPathVisitor = new PropertyPathVisitor();
            propertyPathVisitor.Visit(expression);
            propertyPathVisitor.Properties.Should().HaveCount(2);
            propertyPathVisitor.Properties.ElementAt(0).Should().Be("Name");
            propertyPathVisitor.Properties.ElementAt(1).Should().Be("Admin.Name");
        }

        [Test]
        public void should_support_multi_properties_using_the_select_method_for_sub_collections()
        {
            Expression<Func<Blog, object>> expression = x => x.Name + x.Posts.SelectMany(p => p.Comments).Select(comment => comment.Member);
            var propertyPathVisitor = new PropertyPathVisitor();
            propertyPathVisitor.Visit(expression);
            propertyPathVisitor.Properties.Should().HaveCount(2);
            propertyPathVisitor.Properties.ElementAt(0).Should().Be("Name");
            propertyPathVisitor.Properties.ElementAt(1).Should().Be("Posts.Comments.Member");
        }

        [Test]
        public void should_support_multi_properties_using_the_select_method_for_sub_collections_with_anonymous_types()
        {
            Expression<Func<Blog, object>> expression = x => new { Prop = x.Name + x.Posts.SelectMany(p => p.Comments).Select(comment => comment.Member) };
            var propertyPathVisitor = new PropertyPathVisitor();
            propertyPathVisitor.Visit(expression);
            propertyPathVisitor.Properties.Should().HaveCount(2);
            propertyPathVisitor.Properties.ElementAt(0).Should().Be("Name");
            propertyPathVisitor.Properties.ElementAt(1).Should().Be("Posts.Comments.Member");
        }

        [Test]
        public void complex_expression_with_anonymous_types()
        {
            Expression<Func<Foo, object>> selector1 = foo => new
            {
                foo.Name,
                foo.ImInAnEntity,
                foo.Bars,
                foo.UnitPrice,
                foo.UnitsInStock
            };

            Expression<Func<Foo, object>> selector2 = foo => new
            {
                foo.Bar.Bee.Name,
                foo.Bar.ImInAnEntity,
                foo.Bar.Bee.UnitPrice
            };

            var visitor = new PropertyPathVisitor();
            visitor.Visit(selector1);
            visitor.Visit(selector2);

            var properties = visitor.Properties;

            properties.Should().HaveCount(8);
            properties.Where(x => x.Contains('.')).Should().HaveCount(3);
            properties.Where(x => !x.Contains('.')).Should().HaveCount(5);

            var expected = new HashSet<string>
            {
                "Name", "ImInAnEntity", "Bars", "UnitPrice", "UnitsInStock",
                "Bar.Bee.Name", "Bar.ImInAnEntity", "Bar.Bee.UnitPrice"
            };

            for (var index = 0; index < properties.Count; index++)
            {
                properties.ElementAt(index).Should().Be(expected.ElementAt(index));
            }

            properties.Should().Equal(expected);
        }
    }
}