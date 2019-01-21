using System.Collections.Generic;

namespace UnitTests.StubEntities.ExampleWebApplication
{
    public class Customer
    {
        public Customer()
        {
            //CustomerCustomerDemos = new List<CustomerCustomerDemo>();
            Orders = new List<Order>();
        }

        public virtual string CustomerID { get; set; }

        public virtual string CompanyName { get; set; }

        public virtual string ContactName { get; set; }

        public virtual string ContactTitle { get; set; }

        public virtual string Address { get; set; }

        public virtual string City { get; set; }

        public virtual string Region { get; set; }

        public virtual string PostalCode { get; set; }

        public virtual string Country { get; set; }

        public virtual string Phone { get; set; }

        public virtual string Fax { get; set; }

        private bool _efBool;
        public virtual bool? EfBool
        {
            get { return _efBool; }
            set { _efBool = value.HasValue && value.Value; }
        }

        public virtual string Bool { get; set; }

        //public virtual IList<CustomerCustomerDemo> CustomerCustomerDemos { get; set; }

        public virtual IList<Order> Orders { get; set; }
    }
}