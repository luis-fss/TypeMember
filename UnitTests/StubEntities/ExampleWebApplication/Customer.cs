using System.Collections.Generic;

namespace UnitTests.StubEntities.ExampleWebApplication
{
    public sealed class Customer
    {
        public Customer()
        {
            //CustomerCustomerDemos = new List<CustomerCustomerDemo>();
            Orders = new List<Order>();
        }

        public string CustomerId { get; set; }

        public string CompanyName { get; set; }

        public string ContactName { get; set; }

        public string ContactTitle { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public string Region { get; set; }

        public string PostalCode { get; set; }

        public string Country { get; set; }

        public string Phone { get; set; }

        public string Fax { get; set; }

        private bool _efBool;
        public bool? EfBool
        {
            get { return _efBool; }
            set { _efBool = value.HasValue && value.Value; }
        }

        public string Bool { get; set; }

        //public virtual IList<CustomerCustomerDemo> CustomerCustomerDemos { get; set; }

        public IList<Order> Orders { get; set; }
    }
}