using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FluentAssertions;
using NUnit.Framework;
using TypeMember;
using TypeMember.Util;
using UnitTests.StubEntities;
using UnitTests.StubEntities.ExampleWebApplication;
// ReSharper disable UnusedMember.Local

namespace UnitTests
{
    public class ReflectorTests
    {
        [Test]
        public void value_type_property()
        {
            //Should return "Length", value type property
            var memberName = Reflector.GetMemberName<string>(x => x.Length);
            Assert.AreEqual("Length", memberName);
        }

        [Test]
        public void reference_type_property()
        {
            //Should return "Data", reference type property
            var memberName = Reflector.GetMemberName<Exception>(x => x.Data);
            Assert.AreEqual("Data", memberName);
        }

        [Test]
        public void method_returning_reference_type()
        {
            //Should return "Clone", method returning reference type
            var memberName = Reflector.GetMemberName<string>(x => x.Clone());
            Assert.AreEqual("Clone", memberName);
        }

        [Test]
        public void method_returning_value_type()
        {
            //Should return "GetHashCode", method returning value type
            var memberName = Reflector.GetMemberName<string>(x => x.GetHashCode());
            Assert.AreEqual("GetHashCode", memberName);
        }

        [Test]
        public void void_method()
        {
            //Should return "Reverse", void method
            var memberName = Reflector.GetMemberName<List<string>>(x => x.Reverse());
            Assert.AreEqual("Reverse", memberName);
        }

        [Test]
        public void method_with_parameter()
        {
            //Should return "LastIndexOf", method with parameter
            var memberName = Reflector.GetMemberName<string>(x => x.LastIndexOf(','));
            Assert.AreEqual("LastIndexOf", memberName);
        }

        [Test]
        public void no_type_parameter_required()
        {
            //Should return "Length", no type parameter required
            var memberName = "someString".GetMemberName(x => x.Length);
            Assert.AreEqual("Length", memberName);
        }

        [Test]
        public void value_complex_type_property()
        {
            //Should return "Name", value type property
            var memberName = Reflector.GetMemberName<Foo>(x => x.Bar.Name);
            Assert.AreEqual("Name", memberName);
        }

        [Test]
        public void get_member_name_from_static_type()
        {
            //Should return "Min", value type property
            var memberName = Reflector.GetMemberName(() => Math.Min(0, 1));
            Assert.AreEqual("Min", memberName);
        }

        [Test]
        public void get_memberinfo_for_a_property_that_does_not_exists_should_returns_null()
        {
            //Should return null
            var memberInfo = Reflector.GetMemberInfo<Foo>("property_that_does_not_exists");
            Assert.AreEqual(null, memberInfo);
        }

        [Test]
        public void get_memberinfo_for_a_property()
        {
            var memberInfo = Reflector.GetMemberInfo<Foo>("Bar");
            memberInfo.Should().NotBeNull();
            memberInfo.Name.Should().Be("Bar");
        }

        [Test]
        public void get_memberinfo_for_a_property_case_insensitive()
        {
            var memberInfo = Reflector.GetMemberInfo<Foo>("bar");
            memberInfo.Should().NotBeNull();
            memberInfo.Name.Should().Be("Bar");
        }

        [Test]
        public void get_memberinfo_for_a_nested_property()
        {
            var memberInfo = Reflector.GetMemberInfo<Foo>("Bar.Name");
            memberInfo.Should().NotBeNull();
            memberInfo.Name.Should().Be("Name");
        }

        [Test]
        public void get_memberinfo_for_a_nested_collection()
        {
            var memberInfo = Reflector.GetMemberInfo<Employee>("Orders.OrderID");
            memberInfo.Should().NotBeNull();
            memberInfo.Name.Should().Be("OrderId");
        }

        [Test]
        public void get_memberinfo_for_a_nested_collection_2()
        {
            var memberInfo = Reflector.GetMemberInfo<Employee>("Orders.Customer.City");
            memberInfo.Should().NotBeNull();
            memberInfo.Name.Should().Be("City");
        }

        [Test]
        public void get_memberinfo_for_a_nested_property_case_insensitive()
        {
            var memberInfo = Reflector.GetMemberInfo<Foo>("bar.name");
            memberInfo.Should().NotBeNull();
            memberInfo.Name.Should().Be("Name");
        }

        [Test]
        public void fix_member_path_for_a_property_name()
        {
            var path = Reflector.FixMemberPathCase<Foo>("bar");
            path.Should().NotBeNull();
            path.Should().Be("Bar");
        }

        [Test]
        public void fix_member_path_for_a_nested_property_name()
        {
            var path = Reflector.FixMemberPathCase<Foo>("bar.name");
            path.Should().NotBeNull();
            path.Should().Be("Bar.Name");
        }

        [Test]
        public void fix_member_path_for_a_nested_nested_property_name()
        {
            var path = Reflector.FixMemberPathCase<Foo>("bar.bee.name");
            path.Should().NotBeNull();
            path.Should().Be("Bar.Bee.Name");
        }

        [Test]
        public void set_and_get_property_value_with_reflection()
        {
            const string barName = "BarName";
            var foo = new Foo();
            Reflector.SetPropertyValue(foo, "Bar", new Bar { Name = barName });
            var value = Reflector.GetPropertyValue(foo, "Bar.Name");
            foo.Bar.Name.Should().Be(barName);
            value.Should().Be(barName);
        }

        [Test]
        public void set_and_get_nested_property_value_with_reflection()
        {
            const string barName = "BarName";
            var foo = new Foo();
            Reflector.SetPropertyValue(foo, "Bar.Bee.Name", barName);
            var value = Reflector.GetPropertyValue(foo, "Bar.Bee.Name");
            foo.Bar.Bee.Name.Should().Be(barName);
            value.Should().Be(barName);
        }

        [Test]
        public void set_and_get_nullable_nested_property_value_with_reflection()
        {
            const string barName = "BarName";
            var foo = new Foo { Bar = null };
            Reflector.SetPropertyValue(foo, "Bar.Bee.Name", barName);
            var value = Reflector.GetPropertyValue(foo, "Bar.Bee.Name");
            foo.Bar.Bee.Name.Should().Be(barName);
            value.Should().Be(barName);
        }

        [Test]
        public void get_name_of_nested_property_as_string_value()
        {
            var memberName = ((Expression<Func<Foo, object>>)(x => x.Bar.Name)).GetPropertyPath();
            Assert.AreEqual("Bar.Name", memberName);
        }

        [Test]
        public void should_get_property_path_from_a_binary_expression()
        {
            var propertyPath = ((Expression<Func<Foo, object>>)(x => x.Bar.Name + "hello")).GetPropertyPath();

            propertyPath.Should().Be("Bar.Name");
        }

        [Test]
        public void get_property_expression()
        {
            const string propPath = "Bar.Name";
            var propertyExpression = Reflector.GetPropertyExpression<Foo, string>(propPath);
            Assert.AreEqual(propPath, propertyExpression.GetPropertyPath());
            Assert.AreEqual("foo => foo.Bar.Name", propertyExpression.ToString());
        }

        [Test]
        public void get_property_expression_nested()
        {
            const string propPath = "Bar.Bee.UnitPrice";
            var propertyExpression = Reflector.GetPropertyExpression<Foo, decimal>(propPath);
            Assert.AreEqual(propPath, propertyExpression.GetPropertyPath());
            Assert.AreEqual("foo => foo.Bar.Bee.UnitPrice", propertyExpression.ToString());
        }

        [Test]
        public void get_property_expression_collection()
        {
            Expression<Func<Employee, object>> expected = employee => employee.Orders.Select(order => order.OrderId);

            const string propPath = "Orders.OrderId";
            var propertyExpression = Reflector.GetPropertyExpression<Employee, object>(propPath);

            Assert.AreEqual(propPath, propertyExpression.GetPropertyPath());
            propertyExpression.ToString().Should().Be(expected.ToString());
        }

        [Test]
        public void get_property_expression_collection_2()
        {
            Expression<Func<Employee, object>> expected = employee => employee.Orders.Select(order => order.Customer.City);

            const string propPath = "Orders.Customer.City";
            var propertyExpression = Reflector.GetPropertyExpression<Employee, object>(propPath);
            Assert.AreEqual(propPath, propertyExpression.GetPropertyPath());
            propertyExpression.ToString().Should().Be(expected.ToString());
        }

        #region Get properties paths

        private class Product
        {
            public Category Category { get; set; }
            public string ProductName { get; set; }
            public decimal? ProductPrice { get; set; }
        }

        private class Category
        {
            public MyClass MyClass { get; set; }
            public DateTime DateTime { get; set; }
            public string CategoryName { get; set; }
            public decimal? CategoryPrice { get; set; }
        }

        private class MyClass
        {
            public string MyClassName { get; set; }
            public decimal? MyClassPrice { get; set; }
        }

        [Test]
        public void get_properties_paths_1()
        {
            var dtoProperties = typeof(Product).GetAllPropertiesPaths();
            dtoProperties.Should().HaveCount(7);
        }

        [Test]
        public void get_properties_paths_2()
        {
            var dtoProperties = typeof(Foo).GetAllPropertiesPaths();
            dtoProperties.Should().HaveCount(12);
        }

        [Test]
        public void get_properties_paths_3()
        {
            var dtoProperties = typeof(StubEntities.ExampleWebApplication.Product).GetAllPropertiesPaths();
            dtoProperties.Should().HaveCount(27);
        }

        #endregion
        
        [Test]
        public void get_property_expression_linq_compatible()
        {
            Expression<Func<Employee, object>> expected = employee => employee.City;

            const string propPath = "City";
            
            var propertyExpression = Reflector.GetPropertyExpression<Employee, object>(propPath);

            Assert.AreEqual(propPath, propertyExpression.GetPropertyPath());
            
            propertyExpression.ToString().Should().Be(expected.ToString());
        }
        
        [Test]
        public void get_property_expression_linq_compatible_2()
        {
            Expression<Func<Employee, string>> expected = employee => employee.City;

            const string propPath = "City";
            
            var propertyExpression = Reflector.GetPropertyExpression<Employee, string>(propPath);

            Assert.AreEqual(propPath, propertyExpression.GetPropertyPath());
            
            propertyExpression.ToString().Should().Be(expected.ToString());
        }

        [Test]
        public void get_property_expression_linq_compatible_3_value_types()
        {
            const string propPath = "EmployeeId";

            var propertyExpression = Reflector.GetPropertyExpression<Employee, object>(propPath);

            var propertyPath = Reflector.GetPropertyPath(propertyExpression);

            propertyPath.Should().Be(propPath.Substring(propPath.IndexOf('.') + 1));
        }
    }
}